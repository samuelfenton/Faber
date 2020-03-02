using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#if VOXELISER_MATHEMATICS_ENABLED && VOXELISER_BURST_ENABLED && VOXELISER_COLLECTIONS_ENABLED
using Unity.Collections;
using Unity.Jobs;
using UnityEngine.Jobs;
using Unity.Burst;
using Unity.Mathematics;
#endif

[ExecuteAlways]
public class Voxeliser_Burst : MonoBehaviour
{
    #if VOXELISER_MATHEMATICS_ENABLED && VOXELISER_BURST_ENABLED && VOXELISER_COLLECTIONS_ENABLED
    public struct VoxelDetails
    {
        public VoxelDetails(float4 p_pos, float2 p_UV)
        {
            m_pos = p_pos;
            m_UV = p_UV;
        }

        public float4 m_pos;
        public float2 m_UV;
    }

    private const int MAX_MESH_COUNT = 65535; //Mesh can have 65535 verts
    private bool m_running = false;

    [Header("Settings")]
    [Tooltip("Size of each voxel")]
    public float m_voxelSize = 0.5f;

    [SerializeField]
    public enum VOXELISER_TYPE { SOLID, DYNAMIC_SOLID, ANIMATED, STATIC };
    [Tooltip("SOLID = no animation, snaps voxels to grid DYNAMIC_SOLID = solid mesh that will have its vertices modified during runtime ANIMATED = full animation STATIC = converted to single mesh, wont snap at all")]
    public VOXELISER_TYPE m_voxeliserType = VOXELISER_TYPE.SOLID;
    [Tooltip("Perform calculations over as many frames as needed")]
    public bool m_performOverFrames = false;

    //Advanced Settings
    [Header("Advanced Settings")]
    [Tooltip("Gameobject which holds the mesh for the voxeliser, with none assigned, assumption is mesh is on script gameobject")]
    public GameObject m_objectWithMesh = null;
    [Tooltip("Should this wait until end of frame")]
    public bool m_delayedInitialisation = false;

    [Header("Specific Settings")]
    [Tooltip("Allow user to save static mesh at runtime in editor")]
    public bool m_saveStaticMesh = false;

    private GameObject m_voxelObject = null;
    private Mesh m_originalMesh = null;
    private Mesh m_voxelMesh = null;

    //Animated
    private SkinnedMeshRenderer m_skinnedRenderer = null;

    private void Start()
    {
        if (Application.IsPlaying(gameObject))
            StartCoroutine(InitVoxeliser());
    }

    /// <summary>
    /// Handles awake of object, only should restart if already running
    /// </summary>
    private void OnEnable()
    {
        if (Application.IsPlaying(gameObject))
        {
            if(m_running)
            {
                if (m_voxelObject != null)
                    m_voxelObject.SetActive(true);

                switch (m_voxeliserType)
                {
                    case VOXELISER_TYPE.SOLID:
                        StartCoroutine(VoxeliserSolid());
                        break;
                    case VOXELISER_TYPE.DYNAMIC_SOLID:
                        StartCoroutine(VoxeliserDynamicSolid());
                        break;
                    case VOXELISER_TYPE.ANIMATED:
                        StartCoroutine(VoxeliserAnimated());
                        break;
                    case VOXELISER_TYPE.STATIC:
                    default:
                        break;
                }
            }
        }
    }

    /// <summary>
    /// Clean up any jobs, disable voxel object
    /// </summary>
    private void OnDisable()
    {
        StopAllCoroutines();

        m_buildTriJobHandle.Complete();
        m_convertedMeshJobHandle.Complete();

        if (m_voxelObject != null)
            m_voxelObject.SetActive(false);
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (m_saveStaticMesh)
        {
            m_saveStaticMesh = false;
            StartCoroutine(SaveMesh());
        }
    }
#endif


    /// <summary>
    /// Setup of the Voxeliser
    /// Ensure object has all required components atached
    /// Setup required varibles
    /// </summary>
    public IEnumerator InitVoxeliser()
    {
        if (m_delayedInitialisation)
        {
            yield return null;
        }

        //Setup voxel mesh object
        m_voxelObject = new GameObject(name + " Voxel Mesh Holder");
        MeshFilter meshFilter = m_voxelObject.AddComponent<MeshFilter>();
        meshFilter.mesh = new Mesh();
        m_voxelMesh = meshFilter.sharedMesh;

        m_voxelObject.AddComponent<MeshRenderer>();

        if (!VerifyVaribles())
            yield break;

        MeshRenderer voxelMeshRenderer = m_voxelObject.GetComponent<MeshRenderer>();
        voxelMeshRenderer.material = GetMaterial(m_objectWithMesh);

        //Natives Constrution/ Assigning
        m_orginalVerts = new NativeArray<Vector3>(m_originalMesh.vertexCount, Allocator.Persistent);
        m_originalUVs = new NativeArray<float2>(m_originalMesh.vertexCount, Allocator.Persistent);

        for (int i = 0; i < m_originalMesh.uv.Length; i++)
        {
            m_originalUVs[i] = m_originalMesh.uv[i];
        }
        m_originalTris = new NativeArray<int>(m_originalMesh.triangles, Allocator.Persistent);

        m_voxelDetails = new NativeQueue<VoxelDetails>(Allocator.Persistent);

        m_convertedVerts = new NativeList<float4>(Allocator.Persistent);
        m_convertedUVs = new NativeList<float2>(Allocator.Persistent);
        m_convertedTris = new NativeList<int>(Allocator.Persistent);

        //Running of voxeliser
        switch (m_voxeliserType)
        {
            case VOXELISER_TYPE.SOLID:
                InitVoxeliserSolid();
                break;
            case VOXELISER_TYPE.DYNAMIC_SOLID:
                InitVoxeliserDynamicSolid();
                break;
            case VOXELISER_TYPE.ANIMATED:
                InitVoxeliserAnimated();
                break;
            case VOXELISER_TYPE.STATIC:
                InitVoxeliserStatic();
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Intialise the solid version of the object
    /// </summary>
    public void InitVoxeliserSolid()
    {
        DisableMaterial(m_objectWithMesh);

        m_running = true;

        StartCoroutine(VoxeliserSolid());
    }

    /// <summary>
    /// Intialise the solid version of the object
    /// </summary>
    public void InitVoxeliserDynamicSolid()
    {
        DisableMaterial(m_objectWithMesh);

        m_running = true;

        StartCoroutine(VoxeliserDynamicSolid());
    }

    /// <summary>
    /// Intialise the animated version of the object
    /// </summary>
    public void InitVoxeliserAnimated()
    {
        DisableMaterial(m_objectWithMesh);

        m_skinnedRenderer = m_objectWithMesh.GetComponent<SkinnedMeshRenderer>();

        if (m_skinnedRenderer == null)
        {
#if UNITY_EDITOR
            Debug.Log("No skinned mesh renderer was found on animating object " + name);
#endif
            Destroy(gameObject);
            return;
        }
        m_running = true;
        StartCoroutine(VoxeliserAnimated());
    }

    /// <summary>
    /// Intialise the static version of the object
    /// </summary>
    public void InitVoxeliserStatic()
    {
        DisableMaterial(m_objectWithMesh);

        StartCoroutine(VoxeliserStatic());
    }

    /// <summary>
    /// Rather than using Update Voxeliser is Enumerator driven.
    /// </summary>
    /// <returns>null, wait for next frame</returns>
    private IEnumerator VoxeliserSolid()
    {
        if (!VerifyVaribles())
            yield break;

        if (m_performOverFrames)
        {
            Coroutine convertion = StartCoroutine(ConvertToVoxels(m_voxelSize, m_performOverFrames));
            yield return convertion;
        }
        else
        {
            StartCoroutine(ConvertToVoxels(m_voxelSize, m_performOverFrames));
        }

        yield return null;

        StartCoroutine(VoxeliserSolid());
    }

    /// <summary>
    /// Rather than using Update Voxeliser is Enumerator driven.
    /// </summary>
    /// <returns>null, wait for next frame</returns>
    private IEnumerator VoxeliserDynamicSolid()
    {
        if (!VerifyVaribles())
            yield break;

        //Always need to update
        m_originalMesh = GetMesh(m_objectWithMesh);
        m_orginalVerts.CopyFrom(m_originalMesh.vertices);

        if (m_performOverFrames)
        {
            Coroutine convertion = StartCoroutine(ConvertToVoxels(m_voxelSize, m_performOverFrames));
            yield return convertion;
        }
        else
        {
            StartCoroutine(ConvertToVoxels(m_voxelSize, m_performOverFrames));
        }

        yield return null;

        StartCoroutine(VoxeliserDynamicSolid());
    }

    /// <summary>
    /// Rather than using Update Voxeliser is Enumerator driven.
    /// </summary>
    /// <returns>null, wait for next frame</returns>
    private IEnumerator VoxeliserAnimated()
    {
        if (!VerifyVaribles())
            yield break;

        m_originalMesh = GetBakedVerts(m_skinnedRenderer, m_objectWithMesh);

        m_orginalVerts.CopyFrom(m_originalMesh.vertices);

        if (m_performOverFrames)
        {
            Coroutine convertion = StartCoroutine(ConvertToVoxels(m_voxelSize, m_performOverFrames));
            yield return convertion;
        }
        else
        {
            StartCoroutine(ConvertToVoxels(m_voxelSize, m_performOverFrames));
        }

        yield return null;

        StartCoroutine(VoxeliserAnimated());
    }



    /// <summary>
    /// Rather than using Update Voxeliser is Enumerator driven.
    /// </summary>
    /// <returns>null, wait for next frame</returns>
    private IEnumerator VoxeliserStatic()
    {
        if (!VerifyVaribles())
            yield break;

        Coroutine convert = StartCoroutine(ConvertToVoxels(m_voxelSize, false));
        yield return convert;

        m_voxelMesh.Optimize();

        yield break;
    }

    /// <summary>
    /// Get frame voxel positions
    ///     Get transform matrix without the postion assigned
    ///     Get voxel positions
    ///     Get mesh varibles(verts, tris, UVs) 
    /// </summary>
    /// <param name="p_voxelSize">stored value of voxel size</param>
    /// <param name="p_performOverFrames">stored value of if this operation should occur over several frames</param>
    private IEnumerator ConvertToVoxels(float p_voxelSize, bool p_performOverFrames)
    {
        Matrix4x4 localToWorld = m_objectWithMesh.transform.localToWorldMatrix;
        float4x4 localToWorldConverted = localToWorld;

        float voxelSizeRatio = 1.0f / p_voxelSize;

        //Reset Details
        m_orginalVerts.CopyFrom(m_originalMesh.vertices);

        m_voxelDetails.Clear();

        m_convertedVerts.Clear();
        m_convertedUVs.Clear();
        m_convertedTris.Clear();

        //Build hashmap of all voxel details
        BuildTriVoxels triJob = new BuildTriVoxels()
        {
            m_voxelSize = p_voxelSize,
            m_localToWorldTransform = localToWorldConverted,
            m_tris = m_originalTris,
            m_verts = m_orginalVerts,
            m_uvs = m_originalUVs,
            m_voxelDetails = m_voxelDetails.AsParallelWriter(),
            m_voxelSizeRatio = voxelSizeRatio
        };

        m_buildTriJobHandle = triJob.Schedule(m_originalTris.Length / 3, 16);

        GetConvertedMesh convertJob = new GetConvertedMesh()
        {
            m_voxelDetails = m_voxelDetails,
            m_convertedVerts = m_convertedVerts,
            m_convertedUVs = m_convertedUVs,
            m_convertedTris = m_convertedTris,
            m_voxelSize = p_voxelSize
        };

        m_convertedMeshJobHandle = convertJob.Schedule(m_buildTriJobHandle);

        if (p_performOverFrames)
        {
            //Hard limit of not perfomring over 4 frame
            int frameCounter = 0;
            while (!m_buildTriJobHandle.IsCompleted && frameCounter < 3)
            {
                frameCounter++;
                yield return null;
            }
            m_buildTriJobHandle.Complete();

            frameCounter = 0;
            while (!m_convertedMeshJobHandle.IsCompleted && frameCounter < 3)
            {
                frameCounter++;
                yield return null;
            }
            m_convertedMeshJobHandle.Complete();
        }
        else
        {
            m_buildTriJobHandle.Complete();
            m_convertedMeshJobHandle.Complete();
        }

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

        yield break;
    }

    /// <summary>
    /// Cleanup of all natives
    /// Ensure voxelised object is removed too
    /// </summary>
    private void OnDestroy()
    {
        m_buildTriJobHandle.Complete();
        m_convertedMeshJobHandle.Complete();

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

    #region Job system
    //Inputs
    private NativeArray<int> m_originalTris;
    private NativeArray<Vector3> m_orginalVerts;
    private NativeArray<float2> m_originalUVs;

    //Passing Data
    private NativeQueue<VoxelDetails> m_voxelDetails;

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
        public float m_voxelSizeRatio;

        public NativeQueue<VoxelDetails>.ParallelWriter m_voxelDetails;

        /// <summary>
        /// Build the voxel plavcment based off 3 tris
        /// Uses the Bresenham's line algorithum to find points from vert A to vert B
        /// Using the same approach points are calculated from thje previously found points to vert C
        /// </summary>
        /// <param name="index">Triangle index</param>
        public void Execute(int index)
        {
            //Float 4 varients due to matrix math
            int3 vertA = GetSnappedInt3(math.mul(m_localToWorldTransform, new float4(m_verts[m_tris[index * 3]], 1.0f)) * m_voxelSizeRatio);
            int3 vertB = GetSnappedInt3(math.mul(m_localToWorldTransform, new float4(m_verts[m_tris[index * 3 + 1]], 1.0f)) * m_voxelSizeRatio);
            int3 vertC = GetSnappedInt3(math.mul(m_localToWorldTransform, new float4(m_verts[m_tris[index * 3 + 2]], 1.0f)) * m_voxelSizeRatio);

            //Has UV's been set?
            float2 vertAUV = new float2(0,0);
            float2 vertBUV = vertAUV;
            float2 vertCUV = vertAUV;

            if (m_uvs.Length != 0)
            {
                vertAUV = m_uvs[m_tris[index * 3]];
                vertBUV = m_uvs[m_tris[index * 3 + 1]];
                vertCUV = m_uvs[m_tris[index * 3 + 2]];
            }

            NativeList<VoxelDetails> ABDetails = new NativeList<VoxelDetails>(Allocator.Temp);

            //Initial line
            BresenhamDrawEachPoint(vertA, vertB, vertAUV, vertBUV, true, ABDetails);

            for (int ABIndex = 0; ABIndex < ABDetails.Length; ABIndex++)
            {
                BresenhamDrawEachPoint(Float4ToInt3(ABDetails[ABIndex].m_pos), vertC, ABDetails[ABIndex].m_UV, vertCUV, false, ABDetails);
            }
        }

        /// <summary>
        /// Build line of voxels
        /// Using Bresenham's line algorithum draw a line of voxels
        /// Example found here "https://www.mathworks.com/matlabcentral/fileexchange/21057-3d-bresenham-s-line-generation"
        /// </summary>
        /// <param name="p_startingPoint">Tri vert A</param>
        /// <param name="p_finalPoint">Tri vert B</param>
        /// <param name="p_startingUV">UV of point A</param>
        /// <param name="p_finalUV">UV of point B</param>
        /// <param name="p_shouldTempStore">Is this line for temp calc or final</param>
        /// <param name="p_tempStorage">Queue to store verts in.</param>
        private void BresenhamDrawEachPoint(int3 p_startingPoint, int3 p_finalPoint, float2 p_startingUV, float2 p_finalUV, bool p_shouldTempStore, NativeList<VoxelDetails> p_tempStorage)
        {
            float3 vector = p_finalPoint - p_startingPoint;
            float3 vectorAbs = new float3(math.abs(vector.x), math.abs(vector.y), math.abs(vector.z));
            float3 vectorNorm = math.normalize(vector);
            float3 currentPoint = p_startingPoint;

            for (int loopCount = 0; loopCount < MAX_MESH_COUNT; loopCount++)
            {
                float x_next = math.round(currentPoint.x) + (vector.x == 0.0f ? 0.0f : vector.x > 0.0f ? 1 : -1);
                float y_next = math.round(currentPoint.y) + (vector.y == 0.0f ? 0.0f : vector.y > 0.0f ? 1 : -1);
                float z_next = math.round(currentPoint.z) + (vector.z == 0.0f ? 0.0f : vector.z > 0.0f ? 1 : -1);

                float x_diff = currentPoint.x - x_next;
                float y_diff = currentPoint.y - y_next;
                float z_diff = currentPoint.z - z_next;

                float x_diffAbs = x_diff == 0.0f || float.IsNaN(x_diff) ? float.PositiveInfinity : Mathf.Abs(x_diff);
                float y_diffAbs = y_diff == 0.0f || float.IsNaN(y_diff) ? float.PositiveInfinity : Mathf.Abs(y_diff);
                float z_diffAbs = z_diff == 0.0f || float.IsNaN(z_diff) ? float.PositiveInfinity : Mathf.Abs(z_diff);

                if (float.IsInfinity(x_diffAbs) && float.IsInfinity(y_diffAbs) && float.IsInfinity(z_diffAbs))
                {
                    break;
                }

                AddPoint(p_startingPoint, vector, currentPoint, p_startingUV, p_finalUV, p_shouldTempStore, p_tempStorage);

                float movementMagnitude = 0.0f;
                float dominateAbs = GetLowest(x_diffAbs, y_diffAbs, z_diffAbs);
                int dominateAxis = dominateAbs == x_diffAbs ? 0 : dominateAbs == y_diffAbs ? 1 : 2; //Get dominate axis 0 = x-axis, 1 = y-axis, 2 = z-axis

                //Setup intial values
                switch (dominateAxis)
                {
                    case 0: //movnig along x-axis
                        movementMagnitude = x_diffAbs / vectorAbs.x;
                        currentPoint += vectorNorm * movementMagnitude;
                        break;
                    case 1://movnig along y-axis
                        movementMagnitude = y_diffAbs / vectorAbs.y;
                        currentPoint += vectorNorm * movementMagnitude;
                        break;
                    case 2://movnig along z-axis
                        movementMagnitude = z_diffAbs / vectorAbs.z;
                        currentPoint += vectorNorm * movementMagnitude;
                        break;
                }

                if (GetPercent(p_startingPoint, vector, currentPoint) >= 1.0f)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Add a point into voxel details
        /// </summary>
        /// <param name="p_startPoint">Starting point of line</param>
        /// <param name="p_vector">Direciton of line with magnitude</param>
        /// <param name="p_currentPoint">Current position on line</param>
        /// <param name="p_startUV">Starting UV</param>
        /// <param name="p_endUV">Ending UV</param>
        /// <param name="p_shouldTempStore">Is this line for temp calc or final</param>
        /// <param name="p_tempStorage">Queue to store verts in.</param>
        private void AddPoint(float3 p_startPoint, float3 p_vector, float3 p_currentPoint, float2 p_startUV, float2 p_endUV, bool p_shouldTempStore, NativeList<VoxelDetails> p_tempStorage)
        {
            float a2bPercent = GetPercent(p_startPoint, p_vector, p_currentPoint);

            float2 UV = MergeUVs(p_startUV, p_endUV, a2bPercent);

            float3 snappedPoint = GetSnappedFloat3(p_currentPoint);

            if(p_shouldTempStore)
            {
                p_tempStorage.Add(new VoxelDetails(new float4(snappedPoint, 1.0f), UV));
            }
            else
            {
                m_voxelDetails.Enqueue(new VoxelDetails(new float4(snappedPoint, 1.0f), UV));
            }
        }


        #region Supporting Fuctions

        /// <summary>
        /// Get a Vector3Int that has been rounded to closest, not down
        /// </summary>
        /// <param name="p_vector">Vector to round to Vector3Int</param>
        /// <returns>final Vector3Int</returns>
        private float3 GetSnappedFloat3(float4 p_vector)
        {
            return new float3(math.round(p_vector.x), math.round(p_vector.y), math.round(p_vector.z));
        }

        /// <summary>
        /// Get a Vector3Int that has been rounded to closest, not down
        /// </summary>
        /// <param name="p_vector">Vector to round to Vector3Int</param>
        /// <returns>final Vector3Int</returns>
        private float3 GetSnappedFloat3(float3 p_vector)
        {
            return new float3(math.round(p_vector.x), math.round(p_vector.y), math.round(p_vector.z));
        }

        /// <summary>
        /// Get a Vector3Int that has been rounded to closest, not down
        /// </summary>
        /// <param name="p_vector">Vector to round to Vector3Int</param>
        /// <returns>final Vector3Int</returns>
        private int3 GetSnappedInt3(float4 p_vector)
        {
            return new int3((int)math.round(p_vector.x), (int)math.round(p_vector.y), (int)math.round(p_vector.z));
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
        /// Get lowest of 3 values
        /// </summary>
        /// <param name="p_val1">First to compare with</param>
        /// <param name="p_val2">Second to compare with</param>
        /// <param name="p_val3">Third to compare with</param>
        /// <returns>Lowest of the three values</returns>
        private float GetLowest(float p_val1, float p_val2, float p_val3)
        {
            //current lowest = p_val2 > p_val3 ? p_val2 : p_val3
            return p_val1 < (p_val2 < p_val3 ? p_val2 : p_val3) ? p_val1 : (p_val2 < p_val3 ? p_val2 : p_val3);
        }

        /// <summary>
        /// Get lowest of 3 values
        /// </summary>
        /// <param name="p_startPoint">Starting point of vector</param>
        /// <param name="p_vector">Vector</param>
        /// <param name="p_currentPoint">What current value is</param>
        /// <returns>How far the current point is from start, 0 = 0%, 1 = 100%</returns>
        private float GetPercent(float3 p_startPoint, float3 p_vector, float3 p_currentPoint)
        {
            float3 currentVector = p_currentPoint - p_startPoint;
            return GetFloat3Mag(currentVector) / GetFloat3Mag(p_vector);
        }

        /// <summary>
        /// Get magnitude of a float3
        /// </summary>
        /// <param name="p_val">Float 3 to bet magnitde of</param>
        /// <returns>Magnitude of a float 3</returns>
        private float GetFloat3Mag(float3 p_val)
        {
            return math.sqrt(p_val.x * p_val.x + p_val.y * p_val.y + p_val.z * p_val.z);
        }

        /// <summary>
        /// Convert float3 to int3
        /// </summary>
        /// <param name="p_val">Float 3 to convert</param>
        /// <returns>int3</returns>
        private int3 Float4ToInt3(float4 p_val)
        {
            return new int3((int)p_val.x, (int)p_val.y, (int)p_val.z);
        }

        #endregion
    }


    [BurstCompile]
    private struct GetConvertedMesh : IJob
    {
        [ReadOnly]
        public float m_voxelSize;
        public NativeQueue<VoxelDetails> m_voxelDetails;

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
            float4 right = new float4(m_voxelSize / 2.0f, 0.0f, 0.0f, 0.0f) ; // r = right l = left
            float4 up = new float4(0.0f, m_voxelSize / 2.0f, 0.0f, 0.0f); // u = up, d = down
            float4 forward = new float4(0.0f, 0.0f, m_voxelSize / 2.0f, 0.0f); // f = forward b = backward

            //Get all unique
            NativeList<float4> uniquePositions = new NativeList<float4>(Allocator.Temp);
            NativeList<float2> uniqueUVs = new NativeList<float2>(Allocator.Temp);

            while (m_voxelDetails.Count > 0)
            {
                VoxelDetails details = m_voxelDetails.Dequeue();

                if (!uniquePositions.Contains(details.m_pos))
                {
                    uniquePositions.Add(details.m_pos);
                    uniqueUVs.Add(details.m_UV);
                }
            }


            NativeArray<int> indexArray = new NativeArray<int>(8, Allocator.Temp);

            for (int i = 0; i < uniquePositions.Length; i++)
            {
                float4 voxelPos = new float4(uniquePositions[i].x * m_voxelSize, uniquePositions[i].y * m_voxelSize, uniquePositions[i].z * m_voxelSize, uniquePositions[i].w);

                //Verts
                m_convertedVerts.Add(voxelPos - right - up - forward);
                m_convertedVerts.Add(voxelPos + right - up - forward);
                m_convertedVerts.Add(voxelPos + right + up - forward);
                m_convertedVerts.Add(voxelPos - right + up - forward);
                m_convertedVerts.Add(voxelPos - right + up + forward);
                m_convertedVerts.Add(voxelPos + right + up + forward);
                m_convertedVerts.Add(voxelPos + right - up + forward);
                m_convertedVerts.Add(voxelPos - right - up + forward);

                //UVs
                m_convertedUVs.Add(uniqueUVs[i]);

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


    #region Varible Verification/Render Functions

    /// <summary>
    /// Ensure all varibles are setup correctly
    /// When varibles are not setup, attempt to fix, otherwise remove object
    /// </summary>
    /// <returns>true when all varibels are setup correctly</returns>
    private bool VerifyVaribles()
    {
        if (m_voxelSize <= 0.0f)
        {
#if UNITY_EDITOR
            Debug.Log("Voxel size on " + name + " is set to less than or equal to 0, defaulting to 1");
#endif
            m_voxelSize = 1.0f;
        }

        //Original Object
        if (m_objectWithMesh == null)
        {
            m_objectWithMesh = gameObject;
            if (m_objectWithMesh == null)
            {
#if UNITY_EDITOR
                Debug.Log(name + " orginal object is missing");
#endif
                m_running = false;

                return false;
            }
        }
        //Original Mesh
        if (m_originalMesh == null ||  m_originalMesh.vertexCount == 0)
        {
            m_originalMesh = GetMesh(m_objectWithMesh);
            if (m_originalMesh == null || m_originalMesh.vertexCount == 0)
            {
#if UNITY_EDITOR
                Debug.Log(name + " orginal mesh is missing or mesh has no vertices");
#endif
                m_running = false;

                return false;
            }
        }

        //Voxel Object
        if (m_voxelObject == null)
        {
#if UNITY_EDITOR
            Debug.Log(name + " voxel object is missing");
#endif
            m_running = false;

            return false;
        }

        if (m_voxeliserType == VOXELISER_TYPE.ANIMATED)
        {
            if (m_skinnedRenderer == null)
            {
                m_skinnedRenderer = m_objectWithMesh.GetComponent<SkinnedMeshRenderer>();
                if (m_skinnedRenderer == null)
                {
#if UNITY_EDITOR
                    Debug.Log(name + " skinned mesh renderer is missing, maybe its intended to be solid or static?");
#endif
                    m_running = false;

                    return false;
                }
            }

        }
        return true;
    }

#if UNITY_EDITOR
    /// <summary>
    /// Saving of static meshes
    /// </summary>
    private IEnumerator SaveMesh()
    {
        //Saving of static mesh
        if (m_voxeliserType == VOXELISER_TYPE.STATIC)
        {
            //Verify as static
            Coroutine convert = StartCoroutine(InitVoxeliser());
            yield return convert;

            MeshFilter meshFilter = m_voxelObject.GetComponent<MeshFilter>();

            if (meshFilter != null)
            {
                //Get mesh
                Mesh savingMesh = meshFilter.sharedMesh;
                savingMesh.name = "Voxelised_" + savingMesh.name;

                string path = EditorUtility.SaveFilePanel("Save Voxel Mesh: " + name, "Assets/", "Voxelised-" + name, "asset");
                if (!string.IsNullOrEmpty(path))
                {
                    path = FileUtil.GetProjectRelativePath(path);

                    MeshUtility.Optimize(savingMesh);

                    AssetDatabase.CreateAsset(savingMesh, path);
                    AssetDatabase.SaveAssets();
                }
            }
        }
        else
        {
            Debug.Log("Can only save when set to static");
        }

        enabled = false;

        yield break;
    }
#endif


    /// <summary>
    /// Get mesh from eith skinned mesh renderer or mesh renderer
    /// </summary>
    /// <param name="p_object">object to get mesh for</param>
    /// <returns>correct mesh based off avalibilty of renderers in children</returns>
    private static Mesh GetMesh(GameObject p_object)
    {
        if (p_object == null) //Early breakout
            return null;

        Mesh orignal = new Mesh();
        Mesh instanceMesh = new Mesh();

        MeshFilter meshFilter = p_object.GetComponent<MeshFilter>();
        if (meshFilter != null)
        {
            orignal = meshFilter.sharedMesh;
        }
        else
        {
            SkinnedMeshRenderer skinnedMeshRenderer = p_object.GetComponent<SkinnedMeshRenderer>();
            if (skinnedMeshRenderer != null)
            {
                orignal = GetBakedVerts(skinnedMeshRenderer, p_object);
            }
        }

        instanceMesh.vertices = orignal.vertices;
        instanceMesh.uv = orignal.uv;
        instanceMesh.triangles = orignal.triangles;
        return instanceMesh;
    }

    /// <summary>
    /// Remove all materials attached to an object
    /// </summary>
    /// <param name="p_object">object to remove material from</param>
    private static void DisableMaterial(GameObject p_object)
    {
        if (p_object == null)
            return;

        MeshRenderer meshRenderer = p_object.GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.materials = new Material[0];
        }
        else
        {
            SkinnedMeshRenderer skinnedMeshRenderer = p_object.GetComponent<SkinnedMeshRenderer>();
            if (skinnedMeshRenderer != null)
                skinnedMeshRenderer.materials = new Material[0];
        }
    }

    /// <summary>
    /// Remove all materials attached to an object
    /// </summary>
    /// <param name="p_object">object to remove material from</param>
    /// <returns></returns>
    private static Material GetMaterial(GameObject p_object)
    {
        MeshRenderer meshRenderer = p_object.GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            return meshRenderer.sharedMaterial;
        }
        else
        {
            SkinnedMeshRenderer skinnedMeshRenderer = p_object.GetComponent<SkinnedMeshRenderer>();
            if (skinnedMeshRenderer != null)
                return skinnedMeshRenderer.sharedMaterial;
        }

        return null;
    }

    /// <summary>
    /// Due to how baking works, we want to remove all scale transforms, as future functions assume this is the case
    /// </summary>
    /// <param name="p_skinnedMeshRenderer">Renderer to bake</param>
    /// <param name="p_object">Object which contains the transforms</param>
    /// <returns>A baked mesh without any transforms applied</returns>
    private static Mesh GetBakedVerts(SkinnedMeshRenderer p_skinnedMeshRenderer, GameObject p_object)
    {
        Mesh bakedMesh = new Mesh();

        p_skinnedMeshRenderer.BakeMesh(bakedMesh);

        Vector3[] verts = bakedMesh.vertices;

        Vector3 lossyScale = p_object.transform.lossyScale;
        //Invert
        lossyScale.x = 1.0f / lossyScale.x;
        lossyScale.y = 1.0f / lossyScale.y;
        lossyScale.z = 1.0f / lossyScale.z;

        Matrix4x4 scaleMatrix = Matrix4x4.Scale(lossyScale);
        //Convert
        for (int vertIndex = 0; vertIndex < verts.Length; vertIndex++)
        {
            verts[vertIndex] = scaleMatrix * verts[vertIndex];
        }
        bakedMesh.vertices = verts;

        return bakedMesh;
    }

    #endregion
    #endif
}


