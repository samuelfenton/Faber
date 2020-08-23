﻿namespace VoxeltoUnity {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEditor;

	public static class Core_File 
	{


		public static void CreateFileForResult (List<Core_Voxel.Result> resultList, Shader shader, float scale, Vector3 pivot) {
			CreateFileForResult(resultList, new Shader[1] { shader }, null, null, scale, pivot);
		}



		public static void CreateFileForResult (List<Core_Voxel.Result> resultList, Shader[] shaders, string[] shaderKeywords, Vector2[] shaderRemaps, float scale, Vector3 pivot) {

			var diffuseShader = shaders[0];

			for (int index = 0; index < resultList.Count; index++) {

				var result = resultList[index];
				bool lod = result.VoxelModels.Length > 1;
				bool isRig = !lod && result.IsRigged;
				int realLodNum = result.IsRigged ? 1 : result.VoxelModels.Length;

				var root = new GameObject(result.FileName).transform;
				var meshs = new List<Mesh>();
				var materialsMap = new Dictionary<Texture2D, Material[]>();
				Transform[] lodRoots = new Transform[realLodNum];
				
				for (int lodIndex = 0; lodIndex < realLodNum; lodIndex++) {


					var voxelModel = result.VoxelModels[lodIndex];
					var model = CreateModelFrom(voxelModel.RootNode, voxelModel.Materials, root, pivot, ref meshs, ref materialsMap, isRig, result.WithAvatar, shaders, shaderKeywords, shaderRemaps, scale);
					model.name = string.Format("Root{0}", lod ? "_lod " + lodIndex.ToString() : "");
					lodRoots[lodIndex] = model;

					// Rig			 
					if (isRig) {

						Vector3 halfModelSize = voxelModel.ModelSize[0] * 0.5f;
						halfModelSize.x = Mathf.Floor(halfModelSize.x);
						halfModelSize.y = Mathf.Floor(halfModelSize.y);
						halfModelSize.z = Mathf.Floor(halfModelSize.z);

						var skinMR = model.GetComponent<SkinnedMeshRenderer>();
						if (skinMR) {
							Vector3 rootBoneOffset = halfModelSize * scale;
							var boneTFList = new List<Transform>();
							if (voxelModel.RootBones != null) {
								for (int i = 0; i < voxelModel.RootBones.Length; i++) {
									var boneTF = CreateBoneTransform(voxelModel.RootBones[i], model, scale, ref boneTFList);
									if (boneTF) {
										boneTF.localPosition -= rootBoneOffset;
									}
								}
							}

							skinMR.bones = boneTFList.ToArray();
							skinMR.rootBone = model;

							// Bind Poses
							var poses = new Matrix4x4[boneTFList.Count];
							for (int i = 0; i < boneTFList.Count; i++) {
								poses[i] = boneTFList[i].worldToLocalMatrix * model.localToWorldMatrix;
							}
							skinMR.sharedMesh.bindposes = poses;

						}

						// Foot Fix
						model.localPosition = (halfModelSize - voxelModel.FootPoints[lodIndex]) * scale;

					}

				}



				// Lod
				if (lod) {
					LODGroup group = root.gameObject.AddComponent<LODGroup>();
					LOD[] lods = new LOD[realLodNum];
					for (int i = 0; i < realLodNum; i++) {
						lods[i] = new LOD(
							i == realLodNum - 1 ? 0.001f : GetLodRant(result.VoxelModels[i].MaxModelBounds, i),
							lodRoots[i].GetComponentsInChildren<MeshRenderer>(true)
						);
					}
					group.SetLODs(lods);
					group.RecalculateBounds();
				} else if (!isRig && root.childCount > 0) {
					var newRoot = root.GetChild(0);
					newRoot.name = root.name;
					root = newRoot;
				}



				// File
				string path = Util.CombinePaths(
					result.ExportRoot,
					result.ExportSubRoot,
					result.FileName + result.Extension
				);
				path = Util.FixPath(path);
				string parentPath = Util.GetParentPath(path);
				Util.CreateFolder(parentPath);
				List<Object> oldSubObjs = new List<Object>();

				if (result.Extension == ".prefab") {

					Object prefab;

					if (Util.FileExists(path)) {

						// Get Prefab
						prefab = AssetDatabase.LoadAssetAtPath<Object>(path);
						if (prefab as GameObject) {
							var group = (prefab as GameObject).GetComponent<LODGroup>();
							if (group) {
								Object.DestroyImmediate(group, true);
							}
						}

						// Old Sub Objs
						oldSubObjs.AddRange(AssetDatabase.LoadAllAssetRepresentationsAtPath(path));

						foreach (Object o in oldSubObjs) {
							Object.DestroyImmediate(o, true);
						}
						oldSubObjs.Clear();

					} else {
#if UNITY_4 || UNITY_5 || UNITY_2017 || UNITY_2018_1 || UNITY_2018_2
						prefab = PrefabUtility.CreateEmptyPrefab(path);
#else   // 2018.3+
						var tempObject = new GameObject();
						prefab = PrefabUtility.SaveAsPrefabAsset(tempObject, path);
						Object.DestroyImmediate(tempObject, false);
#endif
					}

					if (prefab) 
					{
						var new2old = new Dictionary<Object, Object>();

						// Create Meshs
						for (int meshIndex = 0; meshIndex < meshs.Count; meshIndex++) {
							Mesh currentMesh = meshs[meshIndex];
							currentMesh.name =  "Mesh_" + result.FileName + "_Base" + meshIndex;
							AssetDatabase.AddObjectToAsset(currentMesh, path);
						}

						// Create Textures
						int textureIndex = 0;
						foreach (KeyValuePair<Texture2D, Material[]> texturePair in materialsMap) 
						{
							Texture2D currentTexture = texturePair.Key;
							currentTexture.name = "Texture_" + result.FileName + "_Base" + textureIndex;

							AssetDatabase.AddObjectToAsset(currentTexture, path);

							textureIndex++;
						}

						// Create Materials
						int materialIndex = 0;
						foreach (KeyValuePair<Texture2D, Material[]> texturePair in materialsMap)
						{
							Material[] mats = texturePair.Value;
							for (int i = 0; i < mats.Length; i++) 
							{
								Material currentMaterial = mats[i];
								currentMaterial.name = "Material_" + result.FileName + "_Base" + materialIndex;
								AssetDatabase.AddObjectToAsset(mats[i], path);

								materialIndex++;
							}
						}

						// Replace to Old Instance
						var meshRenderers = root.GetComponentsInChildren<MeshRenderer>(true);
						var skinnnedMeshRenderers = root.GetComponentsInChildren<SkinnedMeshRenderer>(true);
						var meshFilters = root.GetComponentsInChildren<MeshFilter>(true);

						for (int i = 0; i < meshRenderers.Length; i++) {
							var mr = meshRenderers[i];
							if (mr.sharedMaterial && new2old.ContainsKey(mr.sharedMaterial)) 
							{
								var mat = new2old[mr.sharedMaterial] as Material;
								mr.sharedMaterial = mat;
								if (mat.mainTexture && new2old.ContainsKey(mat.mainTexture)) 
								{
									var texture = new2old[mat.mainTexture] as Texture2D;
									mat.mainTexture = texture;
								}
							}
						}

						for (int i = 0; i < meshFilters.Length; i++) {
							var mf = meshFilters[i];
							if (mf.sharedMesh && new2old.ContainsKey(mf.sharedMesh)) 
							{
								mf.sharedMesh = new2old[mf.sharedMesh] as Mesh;
							}
						}

						for (int i = 0; i < skinnnedMeshRenderers.Length; i++) 
						{
							var sr = skinnnedMeshRenderers[i];
							if (sr.sharedMesh && new2old.ContainsKey(sr.sharedMesh)) 
							{
								sr.sharedMesh = new2old[sr.sharedMesh] as Mesh;
							}
							if (sr.sharedMaterial && new2old.ContainsKey(sr.sharedMaterial)) 
							{
								var mat = new2old[sr.sharedMaterial] as Material;
								sr.sharedMaterial = mat;
								if (mat.mainTexture && new2old.ContainsKey(mat.mainTexture)) 
								{
									var texture = new2old[mat.mainTexture] as Texture2D;
									mat.mainTexture = texture;
								}
							}
						}


						// Prefab
						SetStaticHierarchy(root);
#if UNITY_4 || UNITY_5 || UNITY_2017 || UNITY_2018_1 || UNITY_2018_2
						PrefabUtility.ReplacePrefab(root.gameObject, prefab, ReplacePrefabOptions.ReplaceNameBased);
#else  // 2018.3+

						//Mark all as static by default

						prefab = PrefabUtility.SaveAsPrefabAsset(root.gameObject, path);
#endif

					}

				} else { // Obj

					string objFolderPath = Util.CombinePaths(parentPath, result.FileName);
					string textureFolderPath = Util.CombinePaths(objFolderPath, "Textures");
					Util.CreateFolder(objFolderPath);

					VoxelPostprocessor.TheShader = diffuseShader;

					// Assets
					var model = result.VoxelModels[0];
					for (int modelIndex = 0; modelIndex < model.Meshs.Length; modelIndex++) {

						string modelIndexedName = GetIndexedName(result.FileName, modelIndex, model.Meshs.Length);
						string modelPathRoot = Util.CombinePaths(objFolderPath, modelIndexedName);

						// Texture
						string texturePath = Util.CombinePaths(textureFolderPath, modelIndexedName + ".png");
						texturePath = Util.FixPath(texturePath);
						var texture = model.Textures[modelIndex];
						Util.ByteToFile(texture.EncodeToPNG(), texturePath);
						VoxelPostprocessor.AddTexture(texturePath);

						// Meshs
						var uMesh = model.Meshs[modelIndex];
						for (int i = 0; i < uMesh.Count; i++) {
							uMesh[i].name = GetIndexedName("Mesh", i, uMesh.Count);
							string obj = Util.GetObj(uMesh[i]);
							string objPath = GetIndexedName(modelPathRoot, i, uMesh.Count) + ".obj";

							bool hasObjBefore = Util.FileExists(objPath);

							Util.Write(obj, objPath);
							VoxelPostprocessor.AddObj(objPath, texturePath);

							if (hasObjBefore) {
								AssetDatabase.ImportAsset(Util.FixedRelativePath(objPath), ImportAssetOptions.ForceUpdate);
							}

						}

					}

				}


				// Delete Objects
				if (root.parent) {
					Object.DestroyImmediate(root.parent.gameObject, false);
				} else {
					Object.DestroyImmediate(root.gameObject, false);
				}

			}
			AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
			AssetDatabase.SaveAssets();
			Resources.UnloadUnusedAssets();

			EditorApplication.delayCall += VoxelPostprocessor.ClearAsset;

		}

		/// <summary>
		/// Set entire hierachy to be static
		/// Will recursivly loop through each child and its children
		/// </summary>
		/// <param name="p_root">Root transform to start at</param>
		private static void SetStaticHierarchy(Transform p_root)
        {
			p_root.gameObject.isStatic = true;
			foreach (Transform childTransform in p_root)
			{
				childTransform.gameObject.isStatic = true;
				SetStaticHierarchy(childTransform);
			}
		}





		private static Transform CreateModelFrom (
			Core_Voxel.Result.VoxelNode vNode, VoxelData.MaterialData[] materials, Transform parent, Vector3 pivot,
			ref List<Mesh> meshs, ref Dictionary<Texture2D, Material[]> materialsMap,
			bool isRig, bool withAvatar, Shader[] shaders, string[] shaderKeywords, Vector2[] shaderRemaps, float modelScale
		) {

			Quaternion rot = vNode.Rotation;
			Vector3 pivotFixOffset = rot * pivot;
			Vector3 fixedModelSize = rot * vNode.Size.ToVector3();
			pivotFixOffset.Scale(fixedModelSize);
			Vector3 scaleFix = rot * (0.5f * (Vector3.one - vNode.Scale.ToVector3()));
			Vector3 fixedMvPos = vNode.Position.ToVector3();

			for (int i = 0; i < 3; i++) {
				fixedMvPos[i] += (Mathf.RoundToInt(fixedModelSize[i]) % 2) * 0.5f * (scaleFix[i] > 0.5f ? -1 : 1);
			}

			var root = new GameObject().transform;
			root.SetParent(parent);
			root.localRotation = vNode.Rotation;
			root.localScale = vNode.Scale.ToVector3();
			root.gameObject.SetActive(vNode.Active);

			var nodeName = !string.IsNullOrEmpty(vNode.Name) ? vNode.Name : ("Model " + (parent.childCount - 1).ToString());

			if (vNode.Model != null) {
				root.name = nodeName;
				// BAS: FIX rotated position
				fixedModelSize = Util.VectorAbs(fixedModelSize);
				root.localPosition = (fixedMvPos - 0.5f * fixedModelSize + pivotFixOffset) * modelScale;

				// Empty Check
				bool isEmpty = true;
				var uMesh = vNode.Model;
				if (uMesh.Count > 0) {
					for (int i = 0; i < uMesh.Count; i++) {
						if (uMesh[i].vertexCount > 0) {
							isEmpty = false;
							break;
						}
					}
				}

				if (!isEmpty) {

					// Add Assets
					Texture2D texture = vNode.Texture;
					if (!texture) {
						texture = new Texture2D(4, 4);
					}

					Material[] mats;
					if (!materialsMap.ContainsKey(texture)) {
						Dictionary<int, Material> matMap = new Dictionary<int, Material>();
						for (int i = 0; i < uMesh.Count; i++) {
							int matIndex = uMesh.GetMaterialIndexAt(i);
							Material mat;
							if (matMap.ContainsKey(matIndex)) {
								mat = matMap[matIndex];
							} else {
								mat = VoxelData.GetMaterialFrom(materials[matIndex], texture, shaders, shaderKeywords, shaderRemaps);
								matMap.Add(matIndex, mat);
							}
						}
						mats = new Material[matMap.Count];
						foreach (var indexMat in matMap) {
							var mat = indexMat.Value;
							mat.name = GetIndexedName(nodeName, indexMat.Key, matMap.Count);
							mats[indexMat.Key] = mat;
						}
						texture.name = nodeName;
						materialsMap.Add(texture, mats);
					} else {
						mats = materialsMap[texture];
					}

					// Add Mesh To
					if (uMesh.Count == 1) {
						var mesh = uMesh[0];
						int matIndex = uMesh.GetMaterialIndexAt(0);
						if (!meshs.Contains(mesh)) {
							mesh.name = nodeName;
							meshs.Add(mesh);
						}
						AddMeshTo(mesh, root, mats[matIndex], isRig);
					} else {
						for (int i = 0; i < uMesh.Count; i++) {
							var mesh = uMesh[i];
							if (mesh.vertexCount == 0) {
								continue;
							}
							var target = new GameObject("m_" + i.ToString()).transform;
							target.SetParent(root);
							target.SetAsLastSibling();
							target.localPosition = Vector3.zero;
							target.localRotation = Quaternion.identity;
							target.localScale = Vector3.one;
							if (!meshs.Contains(mesh)) {
								mesh.name = GetIndexedName(nodeName, i, uMesh.Count);
								meshs.Add(mesh);
							}
							int matIndex = uMesh.GetMaterialIndexAt(i);
							AddMeshTo(mesh, target, mats[matIndex], false);
						}
					}
				}


			} else if (vNode.Children != null && vNode.Children.Length > 0) {
				// Sub Objects
				root.name = nodeName;
				root.localPosition = vNode.Position.ToVector3() * modelScale;
				for (int i = 0; i < vNode.Children.Length; i++) {
					CreateModelFrom(vNode.Children[i], materials, root, pivot, ref meshs, ref materialsMap, isRig, withAvatar, shaders, shaderKeywords, shaderRemaps, modelScale);
				}
			}
			return root;
		}



		private static Transform CreateBoneTransform (Core_Voxel.Bone rootBone, Transform parent, float modelScale, ref List<Transform> boneTFList) {

			if (rootBone == null || !parent) { return null; }

			// Root
			var boneTF = new GameObject(rootBone.Name).transform;
			boneTF.SetParent(parent, false);
			boneTF.localScale = Vector3.one;

			Quaternion rot = Quaternion.identity;
			Vector3 pos = rootBone.Position;
			if (rootBone.ChildBones.Count > 0) {
				Vector3 childPositionAvage = Vector3.zero;
				for (int i = 0; i < rootBone.ChildBones.Count; i++) {
					childPositionAvage += rootBone.ChildBones[i].Position;
				}
				childPositionAvage /= rootBone.ChildBones.Count;
				rot = Quaternion.LookRotation(childPositionAvage);
			} else {
				rot = Quaternion.LookRotation(pos);
			}
			boneTF.position = parent.position + pos * modelScale;
			boneTF.rotation = rot;

			// TFList
			int index = rootBone.Index;
			if (index >= boneTFList.Count) {
				boneTFList.AddRange(new Transform[index - boneTFList.Count + 1]);
			}
			boneTFList[index] = boneTF;

			// Child
			for (int i = 0; i < rootBone.ChildBones.Count; i++) {
				CreateBoneTransform(rootBone.ChildBones[i], boneTF, modelScale, ref boneTFList);
			}

			return boneTF;

		}



		private static void AddMeshTo (Mesh mesh, Transform target, Material mat, bool skinMesh) {
			if (skinMesh) {
				var sr = target.gameObject.AddComponent<SkinnedMeshRenderer>();
				sr.sharedMesh = mesh;
				sr.material = mat;
			} else {
				var mr = target.gameObject.AddComponent<MeshRenderer>();
				var mf = target.gameObject.AddComponent<MeshFilter>();
				mf.mesh = mesh;
				mr.material = mat;
			}
		}



		private static string GetIndexedName (string name, int index, int count) {
			return name + (count > 1 ? "_" + index.ToString() : "");
		}



		private static float GetLodRant (int modelSize, int lodLevel) {
			float[] LodRant = new float[9]{
				0.004f, 0.002f, 0.001f,
				0.0004f, 0.0002f, 0.0001f,
				0.00004f, 0.00002f, 0.00001f
			};
			return LodRant[lodLevel] * modelSize;
		}



		private static Avatar GetVoxelAvatarInRoot (Transform root) {

			var humanBones = new List<HumanBone>();
			var skeletonBones = new List<SkeletonBone> {
				new SkeletonBone() {
					name = root.name,
					position = root.localPosition,
					rotation = root.localRotation,
					scale = root.localScale,
				}
			};

			var rootChild = root.GetChild(0);
			if (rootChild) {
				skeletonBones.Add(new SkeletonBone() {
					name = rootChild.name,
					position = rootChild.localPosition,
					rotation = rootChild.localRotation,
					scale = rootChild.localScale,
				});
			}

			// Hips
			var hipsTF = LookForHumanBone(root, "Hips", (hips, hipBone, hipSkel) => {
				humanBones.Add(hipBone);
				skeletonBones.Add(hipSkel);

				// Spine
				LookForHumanBone(hips, "Spine", (spine, spineBone, spineSkel) => {
					humanBones.Add(spineBone);
					skeletonBones.Add(spineSkel);

					// Chest
					LookForHumanBone(spine, "Chest", (chest, chestBone, chestSkel) => {
						humanBones.Add(chestBone);
						skeletonBones.Add(chestSkel);

						// _UpperChest
						LookForHumanBone(chest, "UpperChest", (upperChest, upperChestBone, upperChestSkel) => {
							if (upperChest) {
								humanBones.Add(upperChestBone);
								skeletonBones.Add(upperChestSkel);
							}

							var upperChestRoot = upperChest ? upperChest : chest;



							// Neck
							LookForHumanBone(upperChestRoot, "Neck", (neck, neckBone, neckSkel) => {
								humanBones.Add(neckBone);
								skeletonBones.Add(neckSkel);

								// Head
								LookForHumanBone(neck, "Head", (head, headBone, headSkel) => {
									humanBones.Add(headBone);
									skeletonBones.Add(headSkel);

									// _LeftEye
									LookForHumanBone(head, "LeftEye", (leftEye, leftEyeBone, leftEyeSkel) => {
										if (leftEye) {
											humanBones.Add(leftEyeBone);
											skeletonBones.Add(leftEyeSkel);
										}
									}, true);

									// _RightEye
									LookForHumanBone(head, "RightEye", (rightEye, rightEyeBone, rightEyeSkel) => {
										if (rightEye) {
											humanBones.Add(rightEyeBone);
											skeletonBones.Add(rightEyeSkel);
										}
									}, true);

									// _Jaw
									LookForHumanBone(head, "Jaw", (jaw, jawBone, jawSkel) => {
										if (jaw) {
											humanBones.Add(jawBone);
											skeletonBones.Add(jawSkel);
										}
									}, true);
								});




							});



							// _LeftShoulder
							LookForHumanBone(upperChestRoot, "LeftShoulder", (leftShoulder, leftShoulderBone, leftShoulderSkel) => {
								if (leftShoulder) {
									humanBones.Add(leftShoulderBone);
									skeletonBones.Add(leftShoulderSkel);
								}

								var leftShoulderRoot = leftShoulder ? leftShoulder : upperChestRoot;


								// LeftUpperArm
								LookForHumanBone(leftShoulderRoot, "LeftUpperArm", (leftUpperArm, leftUpperArmBone, leftUpperArmSkel) => {
									humanBones.Add(leftUpperArmBone);
									skeletonBones.Add(leftUpperArmSkel);

									// LeftLowerArm
									LookForHumanBone(leftUpperArm, "LeftLowerArm", (leftLowerArm, leftLowerArmBone, leftLowerArmSkel) => {
										humanBones.Add(leftLowerArmBone);
										skeletonBones.Add(leftLowerArmSkel);

										// LeftHand
										LookForHumanBone(leftLowerArm, "LeftHand", (leftHand, leftHandBone, leftHandSkel) => {
											humanBones.Add(leftHandBone);
											skeletonBones.Add(leftHandSkel);

											// Left Thumb Proximal
											LookForHumanBone(leftHand, "Left Thumb Proximal", (leftThumbProximal, leftThumbProximalBone, leftThumbProximalSkel) => {
												humanBones.Add(leftThumbProximalBone);
												skeletonBones.Add(leftThumbProximalSkel);

												// Left Thumb Intermediate
												LookForHumanBone(leftThumbProximal, "Left Thumb Intermediate", (leftThumbIntermediate, leftThumbIntermediateBone, leftThumbIntermediateSkel) => {
													humanBones.Add(leftThumbIntermediateBone);
													skeletonBones.Add(leftThumbIntermediateSkel);

													// Left Thumb Distal
													LookForHumanBone(leftThumbIntermediate, "Left Thumb Distal", (leftThumbDistal, leftThumbDistalBone, leftThumbDistalSkel) => {
														humanBones.Add(leftThumbDistalBone);
														skeletonBones.Add(leftThumbDistalSkel);
													});

												});


											});


											// Left Index Proximal
											LookForHumanBone(leftHand, "Left Index Proximal", (leftIndexProximal, leftIndexProximalBone, leftIndexProximalSkel) => {
												humanBones.Add(leftIndexProximalBone);
												skeletonBones.Add(leftIndexProximalSkel);

												// Left Index Intermediate
												LookForHumanBone(leftIndexProximal, "Left Index Intermediate", (leftIndexIntermediate, leftIndexIntermediateBone, leftIndexIntermediateSkel) => {
													humanBones.Add(leftIndexIntermediateBone);
													skeletonBones.Add(leftIndexIntermediateSkel);

													// Left Index Distal
													LookForHumanBone(leftIndexIntermediate, "Left Index Distal", (leftIndexDistal, leftIndexDistalBone, leftIndexDistalSkel) => {
														humanBones.Add(leftIndexDistalBone);
														skeletonBones.Add(leftIndexDistalSkel);
													});

												});


											});


											// Left Middle Proximal
											LookForHumanBone(leftHand, "Left Middle Proximal", (leftMiddleProximal, leftMiddleProximalBone, leftMiddleProximalSkel) => {
												humanBones.Add(leftMiddleProximalBone);
												skeletonBones.Add(leftMiddleProximalSkel);

												// Left Middle Intermediate
												LookForHumanBone(leftMiddleProximal, "Left Middle Intermediate", (leftMiddleIntermediate, leftMiddleIntermediateBone, leftMiddleIntermediateSkel) => {
													humanBones.Add(leftMiddleIntermediateBone);
													skeletonBones.Add(leftMiddleIntermediateSkel);

													// Left Middle Distal
													LookForHumanBone(leftMiddleIntermediate, "Left Middle Distal", (leftMiddleDistal, leftMiddleDistalBone, leftMiddleDistalSkel) => {
														humanBones.Add(leftMiddleDistalBone);
														skeletonBones.Add(leftMiddleDistalSkel);
													});

												});


											});


											// Left Ring Proximal
											LookForHumanBone(leftHand, "Left Ring Proximal", (leftRingProximal, leftRingProximalBone, leftRingProximalSkel) => {
												humanBones.Add(leftRingProximalBone);
												skeletonBones.Add(leftRingProximalSkel);

												// Left Ring Intermediate
												LookForHumanBone(leftRingProximal, "Left Ring Intermediate", (leftRingIntermediate, leftRingIntermediateBone, leftRingIntermediateSkel) => {
													humanBones.Add(leftRingIntermediateBone);
													skeletonBones.Add(leftRingIntermediateSkel);

													// Left Ring Distal
													LookForHumanBone(leftRingIntermediate, "Left Ring Distal", (leftRingDistal, leftRingDistalBone, leftRingDistalSkel) => {
														humanBones.Add(leftRingDistalBone);
														skeletonBones.Add(leftRingDistalSkel);
													});

												});


											});


											// Left Little Proximal
											LookForHumanBone(leftHand, "Left Little Proximal", (leftLittleProximal, leftLittleProximalBone, leftLittleProximalSkel) => {
												humanBones.Add(leftLittleProximalBone);
												skeletonBones.Add(leftLittleProximalSkel);

												// Left Little Intermediate
												LookForHumanBone(leftLittleProximal, "Left Little Intermediate", (leftLittleIntermediate, leftLittleIntermediateBone, leftLittleIntermediateSkel) => {
													humanBones.Add(leftLittleIntermediateBone);
													skeletonBones.Add(leftLittleIntermediateSkel);

													// Left Little Distal
													LookForHumanBone(leftLittleIntermediate, "Left Little Distal", (leftLittleDistal, leftLittleDistalBone, leftLittleDistalSkel) => {
														humanBones.Add(leftLittleDistalBone);
														skeletonBones.Add(leftLittleDistalSkel);
													});

												});


											});



										});



									});



								});



							}, true);



							// _RightShoulder
							LookForHumanBone(upperChestRoot, "RightShoulder", (leftShoulder, leftShoulderBone, leftShoulderSkel) => {
								if (leftShoulder) {
									humanBones.Add(leftShoulderBone);
									skeletonBones.Add(leftShoulderSkel);
								}

								var leftShoulderRoot = leftShoulder ? leftShoulder : upperChestRoot;


								// RightUpperArm
								LookForHumanBone(leftShoulderRoot, "RightUpperArm", (leftUpperArm, leftUpperArmBone, leftUpperArmSkel) => {
									humanBones.Add(leftUpperArmBone);
									skeletonBones.Add(leftUpperArmSkel);

									// RightLowerArm
									LookForHumanBone(leftUpperArm, "RightLowerArm", (leftLowerArm, leftLowerArmBone, leftLowerArmSkel) => {
										humanBones.Add(leftLowerArmBone);
										skeletonBones.Add(leftLowerArmSkel);

										// RightHand
										LookForHumanBone(leftLowerArm, "RightHand", (leftHand, leftHandBone, leftHandSkel) => {
											humanBones.Add(leftHandBone);
											skeletonBones.Add(leftHandSkel);

											// Right Thumb Proximal
											LookForHumanBone(leftHand, "Right Thumb Proximal", (leftThumbProximal, leftThumbProximalBone, leftThumbProximalSkel) => {
												humanBones.Add(leftThumbProximalBone);
												skeletonBones.Add(leftThumbProximalSkel);

												// Right Thumb Intermediate
												LookForHumanBone(leftThumbProximal, "Right Thumb Intermediate", (leftThumbIntermediate, leftThumbIntermediateBone, leftThumbIntermediateSkel) => {
													humanBones.Add(leftThumbIntermediateBone);
													skeletonBones.Add(leftThumbIntermediateSkel);

													// Right Thumb Distal
													LookForHumanBone(leftThumbIntermediate, "Right Thumb Distal", (leftThumbDistal, leftThumbDistalBone, leftThumbDistalSkel) => {
														humanBones.Add(leftThumbDistalBone);
														skeletonBones.Add(leftThumbDistalSkel);
													});

												});


											});


											// Right Index Proximal
											LookForHumanBone(leftHand, "Right Index Proximal", (leftIndexProximal, leftIndexProximalBone, leftIndexProximalSkel) => {
												humanBones.Add(leftIndexProximalBone);
												skeletonBones.Add(leftIndexProximalSkel);

												// Right Index Intermediate
												LookForHumanBone(leftIndexProximal, "Right Index Intermediate", (leftIndexIntermediate, leftIndexIntermediateBone, leftIndexIntermediateSkel) => {
													humanBones.Add(leftIndexIntermediateBone);
													skeletonBones.Add(leftIndexIntermediateSkel);

													// Right Index Distal
													LookForHumanBone(leftIndexIntermediate, "Right Index Distal", (leftIndexDistal, leftIndexDistalBone, leftIndexDistalSkel) => {
														humanBones.Add(leftIndexDistalBone);
														skeletonBones.Add(leftIndexDistalSkel);
													});

												});


											});


											// Right Middle Proximal
											LookForHumanBone(leftHand, "Right Middle Proximal", (leftMiddleProximal, leftMiddleProximalBone, leftMiddleProximalSkel) => {
												humanBones.Add(leftMiddleProximalBone);
												skeletonBones.Add(leftMiddleProximalSkel);

												// Right Middle Intermediate
												LookForHumanBone(leftMiddleProximal, "Right Middle Intermediate", (leftMiddleIntermediate, leftMiddleIntermediateBone, leftMiddleIntermediateSkel) => {
													humanBones.Add(leftMiddleIntermediateBone);
													skeletonBones.Add(leftMiddleIntermediateSkel);

													// Right Middle Distal
													LookForHumanBone(leftMiddleIntermediate, "Right Middle Distal", (leftMiddleDistal, leftMiddleDistalBone, leftMiddleDistalSkel) => {
														humanBones.Add(leftMiddleDistalBone);
														skeletonBones.Add(leftMiddleDistalSkel);
													});

												});


											});


											// Right Ring Proximal
											LookForHumanBone(leftHand, "Right Ring Proximal", (leftRingProximal, leftRingProximalBone, leftRingProximalSkel) => {
												humanBones.Add(leftRingProximalBone);
												skeletonBones.Add(leftRingProximalSkel);

												// Right Ring Intermediate
												LookForHumanBone(leftRingProximal, "Right Ring Intermediate", (leftRingIntermediate, leftRingIntermediateBone, leftRingIntermediateSkel) => {
													humanBones.Add(leftRingIntermediateBone);
													skeletonBones.Add(leftRingIntermediateSkel);

													// Right Ring Distal
													LookForHumanBone(leftRingIntermediate, "Right Ring Distal", (leftRingDistal, leftRingDistalBone, leftRingDistalSkel) => {
														humanBones.Add(leftRingDistalBone);
														skeletonBones.Add(leftRingDistalSkel);
													});

												});


											});


											// Right Little Proximal
											LookForHumanBone(leftHand, "Right Little Proximal", (leftLittleProximal, leftLittleProximalBone, leftLittleProximalSkel) => {
												humanBones.Add(leftLittleProximalBone);
												skeletonBones.Add(leftLittleProximalSkel);

												// Right Little Intermediate
												LookForHumanBone(leftLittleProximal, "Right Little Intermediate", (leftLittleIntermediate, leftLittleIntermediateBone, leftLittleIntermediateSkel) => {
													humanBones.Add(leftLittleIntermediateBone);
													skeletonBones.Add(leftLittleIntermediateSkel);

													// Right Little Distal
													LookForHumanBone(leftLittleIntermediate, "Right Little Distal", (leftLittleDistal, leftLittleDistalBone, leftLittleDistalSkel) => {
														humanBones.Add(leftLittleDistalBone);
														skeletonBones.Add(leftLittleDistalSkel);
													});

												});


											});



										});



									});



								});



							}, true);



						}, true);


					});

				});

				// LeftUpperLeg
				LookForHumanBone(hips, "LeftUpperLeg", (leftUpperLeg, leftUpperLegBone, leftUpperLegSkel) => {
					humanBones.Add(leftUpperLegBone);
					skeletonBones.Add(leftUpperLegSkel);

					// LeftLowerLeg
					LookForHumanBone(leftUpperLeg, "LeftLowerLeg", (leftLowerLeg, leftLowerLegBone, leftLowerLegSkel) => {
						humanBones.Add(leftLowerLegBone);
						skeletonBones.Add(leftLowerLegSkel);

						// LeftFoot
						LookForHumanBone(leftLowerLeg, "LeftFoot", (leftFoot, leftFootBone, leftFootSkel) => {
							humanBones.Add(leftFootBone);
							skeletonBones.Add(leftFootSkel);

							// _LeftToes
							LookForHumanBone(leftFoot, "LeftToes", (leftToes, leftToesBone, leftToesSkel) => {
								humanBones.Add(leftToesBone);
								skeletonBones.Add(leftToesSkel);
							});

						});

					});


				});

				// RightUpperLeg
				LookForHumanBone(hips, "RightUpperLeg", (leftUpperLeg, leftUpperLegBone, leftUpperLegSkel) => {
					humanBones.Add(leftUpperLegBone);
					skeletonBones.Add(leftUpperLegSkel);

					// RightLowerLeg
					LookForHumanBone(leftUpperLeg, "RightLowerLeg", (leftLowerLeg, leftLowerLegBone, leftLowerLegSkel) => {
						humanBones.Add(leftLowerLegBone);
						skeletonBones.Add(leftLowerLegSkel);

						// RightFoot
						LookForHumanBone(leftLowerLeg, "RightFoot", (leftFoot, leftFootBone, leftFootSkel) => {
							humanBones.Add(leftFootBone);
							skeletonBones.Add(leftFootSkel);

							// _RightToes
							LookForHumanBone(leftFoot, "RightToes", (leftToes, leftToesBone, leftToesSkel) => {
								humanBones.Add(leftToesBone);
								skeletonBones.Add(leftToesSkel);
							});

						});

					});


				});

			});

			if (hipsTF) {
				var avatar = AvatarBuilder.BuildHumanAvatar(root.gameObject, new HumanDescription() {
					human = humanBones.ToArray(),
					skeleton = skeletonBones.ToArray(),
					lowerArmTwist = 0,
					upperArmTwist = 0,
					upperLegTwist = 0,
					lowerLegTwist = 0,
					armStretch = 0,
					legStretch = 0,
					feetSpacing = 0,
					hasTranslationDoF = false,
				});
				avatar.name = "Avatar";
				return avatar;
			} else {
				return null;
			}
		}



		private static Transform LookForHumanBone (Transform tf, string name, System.Action<Transform, HumanBone, SkeletonBone> next = null, bool forceNext = false) {
			Transform result = null;
			int len = tf.childCount;
			for (int i = 0; i < len; i++) {
				var t = tf.GetChild(i);
				if (t.name == name) {
					result = t;
					break;
				}
			}
			if (!result) {
				for (int i = 0; i < len; i++) {
					var t = tf.GetChild(i);
					var res = LookForHumanBone(t, name);
					if (res) {
						result = res;
						break;
					}
				}
			}
			if (next != null && (result || forceNext)) {
				if (result) {
					var hBone = new HumanBone() {
						humanName = name,
						boneName = name,
						limit = new HumanLimit() {
							useDefaultValues = true,
						},
					};
					var kBone = new SkeletonBone() {
						name = name,
						position = result.localPosition,
						rotation = result.localRotation,
						scale = result.localScale,
					};
					next(result, hBone, kBone);
				} else {
					next(null, default(HumanBone), default(SkeletonBone));
				}
			}
			return result;
		}






	}
}