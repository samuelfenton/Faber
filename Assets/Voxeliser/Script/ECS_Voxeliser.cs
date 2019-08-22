using Unity.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using UnityEngine.Jobs;
using Unity.Burst;
using Unity.Mathematics;

public class ECS_Voxeliser : MonoBehaviour
{
    #region Editor Varibles/Normal MonoBehaviour varibles

    public float VOXEL_SIZE = 0.1f;

    //Advanced Settings
    [Header("Advanced Settings")]
    [Tooltip("Gameobject which holds the mesh for the voxeliser, with none assigned, assumption is mesh is on script gameobject")]
    public GameObject m_objectWithMesh = null;
    [Tooltip("Resolution of trianlge formation, if 'gaps' start to show in the model increase this varible. Higher Values caluse lag. Keep between 1-50")]
    public int m_stepModifier = 5;
    [Tooltip("Set how many voxels will be used, in the case of missing voxels increase this number. Too high will cause lag")]
    public int m_maxVoxelCount = 10000;

    [HideInInspector]
    public Mesh m_modelMesh = null;
    protected GameObject m_voxelObject = null;
    protected Mesh m_voxelMesh = null;

    protected Texture2D m_mainTexture = null;
    #endregion

    #region Job system
    //Inputs
    private NativeArray<int> m_originalTris;
    private NativeArray<Vector3> m_orginalVerts;
    private NativeArray<float2> m_originalUVs;

    //Passing Data
    private NativeHashMap<float4, float2> m_voxelDetails;

    //Outputs
    private NativeList<float4> m_convertedVerts; // unknown size
    private NativeList<float2> m_convertedUVs; //size of verts
    private NativeList<int> m_convertedTris; //3 x 12 x the size of positions

    private JobHandle m_buildTriJobHandle;
    private JobHandle m_convertedMeshJobHandle;

    [BurstCompile]
    private struct BuildTriVoxels : IJobParallelFor
    {
        [ReadOnly]
        public float m_voxelSize;
        [ReadOnly]
        public float4x4 m_localToWorldTransform;
        [ReadOnly]
        public NativeArray<int> m_tris;
        [ReadOnly]
        public NativeArray<Vector3> m_verts;
        [ReadOnly]
        public NativeArray<float2> m_uvs;

        //Advanced
        [ReadOnly]
        public int m_stepModifier;

        [WriteOnly]
        public NativeHashMap<float4, float2>.ParallelWriter m_voxelDetailsConcurrent;

        /// <summary>
        /// Build the voxel plavcment based off 3 tris
        /// Uses the Bresenham's line algorithum to find points from vert A to vert B
        /// Using the same approach points are calculated from thje previously found points to vert C
        /// </summary>
        /// <param name="index">Triangle index</param>
        public void Execute(int index)
        {
            //Float 4 varients due to matrix math
            float4 vertA = math.mul(m_localToWorldTransform, new float4(m_verts[m_tris[index * 3]], 1));
            float4 vertB = math.mul(m_localToWorldTransform, new float4(m_verts[m_tris[index * 3 + 1]], 1));
            float4 vertC = math.mul(m_localToWorldTransform, new float4(m_verts[m_tris[index * 3 + 2]], 1));

            float2 vertAUV = m_uvs[m_tris[index * 3]];
            float2 vertBUV = m_uvs[m_tris[index * 3 + 1]];
            float2 vertCUV = m_uvs[m_tris[index * 3 + 2]];

            NativeList<float4> points = new NativeList<float4>(Allocator.Temp);
            NativeList<float2> pointUVs = new NativeList<float2>(Allocator.Temp);

            //Initial line
            InitialBresenhamDrawEachPoint(vertA, vertB, vertAUV, vertBUV, points, pointUVs);

            //Draw from intial line to vertC
            for (int i = 0; i < points.Length; i++)
            {
                m_voxelDetailsConcurrent.TryAdd(points[i], pointUVs[i]);
                BresenhamDrawEachPoint(points[i], vertC, pointUVs[i], vertCUV);
            }
        }

        /// <summary>
        /// Build line of voxels
        /// Using Bresenham's line algorithum draw a line of voxels
        /// Example found here "https://www.mathworks.com/matlabcentral/fileexchange/21057-3d-bresenham-s-line-generation", ":https://github.com/ssloy/tinyrenderer/wiki/Lesson-2:-Triangle-rasterization-and-back-face-culling"
        /// All points made are stored in list, and NOT snapped to the grid
        /// </summary>
        /// <param name="p_pointA">Tri vert A</param>
        /// <param name="p_pointB">Tri vert B</param>
        /// <param name="p_pointAUV">UV of point A</param>
        /// <param name="p_pointBUV">UV of point B</param>
        /// <param name="p_points">List to store verts in.</param>
        /// <param name="p_pointUVs">List to store UVs in.</param>
        private void InitialBresenhamDrawEachPoint(float4 p_pointA, float4 p_pointB, float2 p_pointAUV, float2 p_pointBUV, NativeList<float4> p_points, NativeList<float2> p_pointUVs)
        {
            //Basic Values
            float x_diff = p_pointB.x - p_pointA.x;
            float y_diff = p_pointB.y - p_pointA.y;
            float z_diff = p_pointB.z - p_pointA.z;

            float x_abs = math.abs(x_diff);
            float y_abs = math.abs(y_diff);
            float z_abs = math.abs(z_diff);

            float dominateAbs = GetHighest(x_abs, y_abs, z_abs);
            int dominateAxis = dominateAbs == x_abs ? 0 : dominateAbs == y_abs ? 1 : 2; //Get dominate axis 0 = x-axis, 1 = y-axis, 2 = z-axis

            //Incease step count by mukltiplier, ensure steps occurs at least once
            int steps = math.max(1, (int)(dominateAbs / m_voxelSize) * m_stepModifier);

            float dominateCurr = 0, dominateStep = 0;

            float axisACurr = 0, axisAStep = 0;
            float axisBCurr = 0, axisBStep = 0;

            //Setup intial values
            switch (dominateAxis)
            {
                case 0:
                    dominateCurr = p_pointA.x;
                    dominateStep = x_diff / steps;

                    axisACurr = p_pointA.y;
                    axisAStep = y_diff / steps;

                    axisBCurr = p_pointA.z;
                    axisBStep = z_diff / steps;
                    break;
                case 1:
                    dominateCurr = p_pointA.y;
                    dominateStep = y_diff / steps;

                    axisACurr = p_pointA.x;
                    axisAStep = x_diff / steps;

                    axisBCurr = p_pointA.z;
                    axisBStep = z_diff / steps;
                    break;
                case 2:
                    dominateCurr = p_pointA.z;
                    dominateStep = z_diff / steps;

                    axisACurr = p_pointA.x;
                    axisAStep = x_diff / steps;

                    axisBCurr = p_pointA.y;
                    axisBStep = y_diff / steps;
                    break;
            }

            for (int i = 0; i < steps; i++)
            {
                //DrawPoint
                float4 newPoint = new float4();

                float a2bPercent = steps == 0 ? 0 : (float)(i) / steps;
                float2 UV = MergeUVs(p_pointAUV, p_pointBUV, a2bPercent);

                switch (dominateAxis)
                {
                    case 0:
                        newPoint = new float4(dominateCurr, axisACurr, axisBCurr, 1);
                        break;
                    case 1:
                        newPoint = new float4(axisACurr, dominateCurr, axisBCurr, 1);
                        break;
                    case 2:
                        newPoint = new float4(axisACurr, axisBCurr, dominateCurr, 1);
                        break;
                }

                //Add to list for next loop usage
                p_points.Add(SnapToGrid(newPoint));
                p_pointUVs.Add(UV);

                axisACurr += axisAStep;
                axisBCurr += axisBStep;
                dominateCurr += dominateStep;
            }
        }

        /// <summary>
        /// Build line of voxels
        /// Using Bresenham's line algorithum draw a line of voxels
        /// Example found here "https://www.mathworks.com/matlabcentral/fileexchange/21057-3d-bresenham-s-line-generation", ":https://github.com/ssloy/tinyrenderer/wiki/Lesson-2:-Triangle-rasterization-and-back-face-culling"
        /// All points made are stored in dictionary, and ARE snapped to the grid
        /// </summary>
        /// <param name="p_pointOnLine">Tri vert on Line</param>
        /// <param name="p_pointC">Tri vert C</param>
        /// <param name="p_pointOnLineUV">UV of point on Line</param>
        /// <param name="p_pointCUV">UV of point C</param>
        private void BresenhamDrawEachPoint(float4 p_pointOnLine, float4 p_pointC, float2 p_pointOnLineUV, float2 p_pointCUV)
        {
            //Basic Values
            float x_diff = p_pointC.x - p_pointOnLine.x;
            float y_diff = p_pointC.y - p_pointOnLine.y;
            float z_diff = p_pointC.z - p_pointOnLine.z;

            float x_abs = math.abs(x_diff);
            float y_abs = math.abs(y_diff);
            float z_abs = math.abs(z_diff);

            float dominateAbs = GetHighest(x_abs, y_abs, z_abs);
            int dominateAxis = dominateAbs == x_abs ? 0 : dominateAbs == y_abs ? 1 : 2; //Get dominate axis 0 = x-axis, 1 = y-axis, 2 = z-axis

            //Incease step count by mukltiplier, ensure steps occurs at least once
            int steps = math.max(1, (int)(dominateAbs / m_voxelSize) * m_stepModifier);

            float dominateCurr = 0, dominateStep = 0;

            float axisACurr = 0, axisAStep = 0;
            float axisBCurr = 0, axisBStep = 0;

            //Setup intial values
            switch (dominateAxis)
            {
                case 0:
                    dominateCurr = p_pointOnLine.x;
                    dominateStep = x_diff / steps;

                    axisACurr = p_pointOnLine.y;
                    axisAStep = y_diff / steps;

                    axisBCurr = p_pointOnLine.z;
                    axisBStep = z_diff / steps;
                    break;
                case 1:
                    dominateCurr = p_pointOnLine.y;
                    dominateStep = y_diff / steps;

                    axisACurr = p_pointOnLine.x;
                    axisAStep = x_diff / steps;

                    axisBCurr = p_pointOnLine.z;
                    axisBStep = z_diff / steps;
                    break;
                case 2:
                    dominateCurr = p_pointOnLine.z;
                    dominateStep = z_diff / steps;

                    axisACurr = p_pointOnLine.x;
                    axisAStep = x_diff / steps;

                    axisBCurr = p_pointOnLine.y;
                    axisBStep = y_diff / steps;
                    break;
            }

            for (int i = 0; i < steps; i++)
            {
                //DrawPoint
                float4 newPoint = new float4();
                switch (dominateAxis)
                {
                    case 0:
                        newPoint = new float4(dominateCurr, axisACurr, axisBCurr, 1);
                        break;
                    case 1:
                        newPoint = new float4(axisACurr, dominateCurr, axisBCurr, 1);
                        break;
                    case 2:
                        newPoint = new float4(axisACurr, axisBCurr, dominateCurr, 1);
                        break;
                }

                axisACurr += axisAStep;

                axisBCurr += axisBStep;

                dominateCurr += dominateStep;

                float a2bPercent = steps == 0 ? 0 : (float)(i) / steps;
                m_voxelDetailsConcurrent.TryAdd(SnapToGrid(newPoint), MergeUVs(p_pointOnLineUV, p_pointCUV, a2bPercent));
            }
        }

        #region Supporting Fuctions

        /// <summary>
        ///  Snap to postion grid
        /// </summary>
        /// <param name="p_position">World position</param>
        /// <returns>Postioned "Snapped" to a world grid with increments of p_gridSize </returns>
        float4 SnapToGrid(float4 p_position)
        {
            p_position.x -= p_position.x % m_voxelSize;
            p_position.y -= p_position.y % m_voxelSize;
            p_position.z -= p_position.z % m_voxelSize;

            return p_position;
        }

        /// <summary>
        /// Lerp between two UVs
        /// </summary>
        /// <param name="p_UVA">First UV</param>
        /// <param name="p_UVB">Second UV</param>
        /// <param name="p_percent">How far from UV1 to UV2</param>
        /// <returns>new merged UV</returns>
        private float2 MergeUVs(float2 p_UVA, float2 p_UVB, float p_percent)
        {
            return p_UVA * p_percent + p_UVB * (1 - p_percent);
        }

        /// <summary>
        /// Get highest of 3 values
        /// </summary>
        /// <param name="p_val1">First to compare with</param>
        /// <param name="p_val2">Second to compare with</param>
        /// <param name="p_val3">Third to compare with</param>
        /// <returns>Highest of the three values</returns>
        private float GetHighest(float p_val1, float p_val2, float p_val3)
        {
            //current highest = p_val2 > p_val3 ? p_val2 : p_val3
            return p_val1 > (p_val2 > p_val3 ? p_val2 : p_val3) ? p_val1 : (p_val2 > p_val3 ? p_val2 : p_val3);
        }
        #endregion
    }


    [BurstCompile]
    private struct GetConvertedMesh : IJob
    {
        [ReadOnly]
        public float m_voxelSizeHalf;
        [ReadOnly]
        public NativeHashMap<float4, float2> m_voxelDetails;

        [WriteOnly]
        public NativeList<float4> m_convertedVerts; // unknown size
        [WriteOnly]
        public NativeList<float2> m_convertedUVs; //size of verts
        [WriteOnly]
        public NativeList<int> m_convertedTris; //3 x 12 x the size of positions

        /// <summary>
        /// Converting of a single point where the voxel is, into the 8 points of a cube
        /// Vertices will overlap, this is ensure each "Voxel" has its own flat colour
        /// </summary>
        public void Execute()
        {
            float4 right = new float4(m_voxelSizeHalf, 0.0f, 0.0f, 0.0f); // r = right l = left
            float4 up = new float4(0.0f, m_voxelSizeHalf, 0.0f, 0.0f); // u = up, d = down
            float4 forward = new float4(0.0f, 0.0f, m_voxelSizeHalf, 0.0f); // f = forward b = backward

            NativeArray<float4> tempAccessPostions = m_voxelDetails.GetKeyArray(Allocator.Temp); //Voxel center
            NativeArray<float2> tempAccessUVs = m_voxelDetails.GetValueArray(Allocator.Temp); //Voxel UV

            NativeArray<int> indexArray = new NativeArray<int>(8, Allocator.Temp);

            for (int i = 0; i < tempAccessPostions.Length; i++)
            {
                //Verts
                m_convertedVerts.Add(tempAccessPostions[i] - right - up - forward);
                m_convertedVerts.Add(tempAccessPostions[i] + right - up - forward);
                m_convertedVerts.Add(tempAccessPostions[i] + right + up - forward);
                m_convertedVerts.Add(tempAccessPostions[i] - right + up - forward);
                m_convertedVerts.Add(tempAccessPostions[i] - right + up + forward);
                m_convertedVerts.Add(tempAccessPostions[i] + right + up + forward);
                m_convertedVerts.Add(tempAccessPostions[i] + right - up + forward);
                m_convertedVerts.Add(tempAccessPostions[i] - right - up + forward);

                //UVs
                m_convertedUVs.Add(tempAccessUVs[i]);

                //Vert indexes, if positon doesnt exiosts, new index, otherwise old index
                int indexStart = i * 8;
                indexArray[0] = indexStart;
                indexArray[1] = indexStart + 1;
                indexArray[2] = indexStart + 2;
                indexArray[3] = indexStart + 3;
                indexArray[4] = indexStart + 4;
                indexArray[5] = indexStart + 5;
                indexArray[6] = indexStart + 6;
                indexArray[7] = indexStart + 7;

                //Build tris
                //Front Face
                m_convertedTris.Add(indexArray[0]);
                m_convertedTris.Add(indexArray[2]);
                m_convertedTris.Add(indexArray[1]);

                m_convertedTris.Add(indexArray[0]);
                m_convertedTris.Add(indexArray[3]);
                m_convertedTris.Add(indexArray[2]);

                //Top Face  
                m_convertedTris.Add(indexArray[2]);
                m_convertedTris.Add(indexArray[3]);
                m_convertedTris.Add(indexArray[4]);

                m_convertedTris.Add(indexArray[2]);
                m_convertedTris.Add(indexArray[4]);
                m_convertedTris.Add(indexArray[5]);

                //Right Face       
                m_convertedTris.Add(indexArray[1]);
                m_convertedTris.Add(indexArray[2]);
                m_convertedTris.Add(indexArray[5]);

                m_convertedTris.Add(indexArray[1]);
                m_convertedTris.Add(indexArray[5]);
                m_convertedTris.Add(indexArray[6]);

                //Left Face           
                m_convertedTris.Add(indexArray[0]);
                m_convertedTris.Add(indexArray[7]);
                m_convertedTris.Add(indexArray[4]);

                m_convertedTris.Add(indexArray[0]);
                m_convertedTris.Add(indexArray[4]);
                m_convertedTris.Add(indexArray[3]);

                //Back Face         
                m_convertedTris.Add(indexArray[5]);
                m_convertedTris.Add(indexArray[4]);
                m_convertedTris.Add(indexArray[7]);

                m_convertedTris.Add(indexArray[5]);
                m_convertedTris.Add(indexArray[7]);
                m_convertedTris.Add(indexArray[6]);

                //Down Face          
                m_convertedTris.Add(indexArray[0]);
                m_convertedTris.Add(indexArray[6]);
                m_convertedTris.Add(indexArray[7]);

                m_convertedTris.Add(indexArray[0]);
                m_convertedTris.Add(indexArray[1]);
                m_convertedTris.Add(indexArray[6]);
            }
        }
    }
    #endregion

    #region Initial Setup

    /// <summary>
    /// Setup of the Voxeliser
    /// Ensure object has all required components atached
    /// Setup required varibles
    /// </summary>
    protected virtual void Start()
    {
        //Grabbing varibles
        if (m_objectWithMesh == null)
            m_objectWithMesh = gameObject;

        m_modelMesh = CustomMeshHandeling.GetMesh(m_objectWithMesh);

        if (m_modelMesh == null)
        {
#if UNITY_EDITOR
            Debug.Log("Unable to get mesh on " + gameObject.name);
#endif
            Destroy(gameObject);
            return;
        }

        //Attempt to get MAIN_TEX
        MeshRenderer meshRenderer = m_objectWithMesh.GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            m_mainTexture = (Texture2D)meshRenderer.material.mainTexture;
        }
        else
        {
            SkinnedMeshRenderer skinnedMeshRenderer = m_objectWithMesh.GetComponent<SkinnedMeshRenderer>();
            if (skinnedMeshRenderer != null)
            {
                m_mainTexture = (Texture2D)skinnedMeshRenderer.materials[0].mainTexture;
            }
        }

        //Natives Constrution/ Assigning
        m_orginalVerts = new NativeArray<Vector3>(m_modelMesh.vertexCount, Allocator.Persistent);
        m_originalUVs = new NativeArray<float2>(m_modelMesh.vertexCount, Allocator.Persistent);

        for (int i = 0; i < m_modelMesh.uv.Length; i++)
        {
            m_originalUVs[i] = m_modelMesh.uv[i];
        }
        m_originalTris = new NativeArray<int>(m_modelMesh.triangles, Allocator.Persistent);


        m_voxelDetails = new NativeHashMap<float4, float2>(m_maxVoxelCount, Allocator.Persistent);

        m_convertedVerts = new NativeList<float4>(Allocator.Persistent);
        m_convertedUVs = new NativeList<float2>(Allocator.Persistent);
        m_convertedTris = new NativeList<int>(Allocator.Persistent);

        //New object that holds the mesh
        m_voxelObject = new GameObject(name + " Voxel Mesh Holder");
        MeshFilter voxelMeshFilter = m_voxelObject.AddComponent<MeshFilter>();
        m_voxelMesh = voxelMeshFilter.mesh;

        MeshRenderer voxelMeshRenderer = m_voxelObject.AddComponent<MeshRenderer>();
        voxelMeshRenderer.material = CustomMeshHandeling.GetMaterial(m_objectWithMesh);

        CustomMeshHandeling.DisableMaterial(m_objectWithMesh);

        StartCoroutine(VoxeliserUpdate());
    }

    #endregion

    /// <summary>
    /// Rather than using Update Voxeliser is Enumerator driven.
    /// Update mesh used
    /// Run voxeliser
    /// </summary>
    /// <returns>null, wait for next frame</returns>
    protected virtual IEnumerator VoxeliserUpdate()
    {
        yield return null;

        UpdateMesh();
        ConvertToVoxels();

        StartCoroutine(VoxeliserUpdate());
    }

    /// <summary>
    /// Update the mesh used in determining vertex position
    /// Overriden by skinned renderer/ mesh renderer derrived classes
    /// </summary>
    protected virtual void UpdateMesh()
    {
       
    }


    /// <summary>
    /// Get frame voxel positions
    ///     Get transform matrix without the postion assigned
    ///     Get voxel positions
    ///     Get mesh varibles(verts, tris, UVs) 
    /// </summary>
    protected void ConvertToVoxels()
    {
        Matrix4x4 localToWorld = transform.localToWorldMatrix;
        float4x4 localToWorldConverted = localToWorld;

        //Reset Details
        m_orginalVerts.CopyFrom(m_modelMesh.vertices);

        m_voxelDetails.Clear();

        m_convertedVerts.Clear();
        m_convertedUVs.Clear();
        m_convertedTris.Clear();

        //Build hashmap of all voxel details
        BuildTriVoxels triJob = new BuildTriVoxels()
        {
            m_voxelSize = VOXEL_SIZE,
            m_localToWorldTransform = localToWorldConverted,
            m_tris = m_originalTris,
            m_verts = m_orginalVerts,
            m_uvs = m_originalUVs,
            m_voxelDetailsConcurrent = m_voxelDetails.AsParallelWriter(),
            m_stepModifier = m_stepModifier
        };

        m_buildTriJobHandle = triJob.Schedule(m_originalTris.Length/3, 64);

        GetConvertedMesh convertJob = new GetConvertedMesh()
        {
            m_voxelSizeHalf = VOXEL_SIZE / 2.0f,
            m_voxelDetails = m_voxelDetails,
            m_convertedVerts = m_convertedVerts,
            m_convertedUVs = m_convertedUVs,
            m_convertedTris = m_convertedTris
        };

        m_convertedMeshJobHandle = convertJob.Schedule(m_buildTriJobHandle);

        m_buildTriJobHandle.Complete();
        m_convertedMeshJobHandle.Complete();

        //Build new mesh

        //Varibles to set
        //Verts
        int vertsCount = m_convertedVerts.Length;
        int UVsCount = m_convertedUVs.Length;
        int triCount = m_convertedTris.Length;

        Vector3[] convertedVerts = new Vector3[vertsCount];//Same size
        Vector2[] convertedUVs = new Vector2[vertsCount];//Same Size
        int[] convertedTris = new int[triCount];

        float4[] tempVerts = m_convertedVerts.ToArray();
        //Verts
        for (int i = 0; i < vertsCount; i++)
        {
            float4 vert = tempVerts[i];
            convertedVerts[i] = new Vector3(vert.x, vert.y, vert.z);
        }

        float2[] tempUVs = m_convertedUVs.ToArray();
        //UVs
        for (int i = 0; i < UVsCount; i++)
        {
            int startingIndex = i * 8;
            Vector2 UV = new Vector2(tempUVs[i].x, tempUVs[i].y);
            //UV shared by 8
            for (int vertIndex = 0; vertIndex < 8; vertIndex++)
            {
                convertedUVs[startingIndex + vertIndex] = UV;
            }
        }

        int[] tempTris = m_convertedTris.ToArray();
        //Tris
        for (int i = 0; i < triCount; i++)
        {
            convertedTris[i] = tempTris[i];
        }

        //Build new mesh
        m_voxelMesh.Clear();

        m_voxelMesh.SetVertices(new List<Vector3>(convertedVerts));
        m_voxelMesh.SetUVs(0, new List<Vector2>(convertedUVs));
        m_voxelMesh.SetTriangles(convertedTris, 0);

        //m_voxelMesh.Optimize();
        m_voxelMesh.RecalculateNormals();

        //Useful debug mesages when not enough voxels
#if UNITY_EDITOR
        if (m_maxVoxelCount < m_voxelDetails.Length)
            Debug.Log("The voxeliser on " + name + " is trying to place more voxels than intiliased, think about increasing the count");
#endif
    }

    /// <summary>
    /// Cleanup of all natives
    /// Ensure voxelised object is removed too
    /// </summary>
    private void OnDestroy()
    {
        if (m_orginalVerts.IsCreated)
            m_orginalVerts.Dispose();
        if (m_originalUVs.IsCreated)
            m_originalUVs.Dispose();
        if (m_originalTris.IsCreated)
            m_originalTris.Dispose();

        if (m_voxelDetails.IsCreated)
            m_voxelDetails.Dispose();

        if (m_convertedVerts.IsCreated)
            m_convertedVerts.Dispose();
        if (m_convertedUVs.IsCreated)
            m_convertedUVs.Dispose();
        if (m_convertedTris.IsCreated)
            m_convertedTris.Dispose();

        if (m_voxelObject != null)
            DestroyImmediate(m_voxelObject);
    }
}
