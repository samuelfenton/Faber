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

    private const int MAX_MESH_COUNT = 65535; //Mesh can have 65535 verts
    private const int MAX_VOXEL_COUNT = 8191; //Mesh can have 65535 verts
    private bool m_runFlag = false;
    private bool m_processingFlag = false;

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
    [Tooltip("Should each voxel be its own 'Object' allowing for hard textures, vs more of a blend")]
    public bool m_separatedVoxels = true;
    [Tooltip("Gameobject which holds the mesh for the voxeliser, with none assigned, assumption is mesh is on script gameobject")]
    public GameObject m_objectWithMesh = null;
    [Tooltip("Should this wait until end of frame")]
    public bool m_delayedInitialisation = false;

    [Header("Specific Settings")]
    [Tooltip("Allow user to save static mesh at runtime in editor")]
    public bool m_saveStaticMesh = false;
    [Tooltip("Should the rotation be reset when saving?")]
    public bool m_resetRotation = false;

    private GameObject m_voxelObject = null;
    private Mesh m_originalMesh = null;
    private Mesh m_voxelMesh = null;

    //Animated
    private SkinnedMeshRenderer m_skinnedRenderer = null;
    private Material[] m_orginalMats = new Material[0];

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
            if(m_runFlag)
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
        if (!Application.IsPlaying(gameObject))
        {
            if (m_saveStaticMesh && !m_processingFlag)
            {
                m_saveStaticMesh = false;
                m_processingFlag = true;
                StartCoroutine(SaveMesh());
            }
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
        
        m_orginalVerts.CopyFrom(m_originalMesh.vertices);

        m_voxelDetails = new NativeHashMap<int3, float2>(MAX_VOXEL_COUNT, Allocator.Persistent);
        m_ABPoints = new NativeQueue<int3>(Allocator.Persistent);
        m_ABUVs = new NativeQueue<float2>(Allocator.Persistent);

        m_convertedVerts = new NativeList<float4>(Allocator.Persistent);
        m_convertedUVs = new NativeList<float2>(Allocator.Persistent);
        m_convertedTris = new NativeList<int>(Allocator.Persistent);

        MeshRenderer meshRenderer = m_objectWithMesh.GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            m_orginalMats = meshRenderer.sharedMaterials;

        }
        else
        {
            SkinnedMeshRenderer skinnedMeshRenderer = m_objectWithMesh.GetComponent<SkinnedMeshRenderer>();
            if (skinnedMeshRenderer != null)
            {
                m_orginalMats = skinnedMeshRenderer.sharedMaterials;

            }
        }

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
        ToggleMaterial(m_objectWithMesh, false);

        m_runFlag = true;

        StartCoroutine(VoxeliserSolid());
    }

    /// <summary>
    /// Intialise the solid version of the object
    /// </summary>
    public void InitVoxeliserDynamicSolid()
    {
        ToggleMaterial(m_objectWithMesh, false);

        m_runFlag = true;

        StartCoroutine(VoxeliserDynamicSolid());
    }

    /// <summary>
    /// Intialise the animated version of the object
    /// </summary>
    public void InitVoxeliserAnimated()
    {
        ToggleMaterial(m_objectWithMesh, false);

        m_skinnedRenderer = m_objectWithMesh.GetComponent<SkinnedMeshRenderer>();

        if (m_skinnedRenderer == null)
        {
#if UNITY_EDITOR
            Debug.Log("No skinned mesh renderer was found on animating object " + name);
#endif
            Destroy(gameObject);
            return;
        }
        m_runFlag = true;
        StartCoroutine(VoxeliserAnimated());
    }

    /// <summary>
    /// Intialise the static version of the object
    /// </summary>
    public void InitVoxeliserStatic()
    {
        ToggleMaterial(m_objectWithMesh, false);

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
            Coroutine convertion = StartCoroutine(ConvertToVoxels(m_voxelSize, m_separatedVoxels, m_performOverFrames));
            yield return convertion;
        }
        else
        {
            StartCoroutine(ConvertToVoxels(m_voxelSize, m_separatedVoxels, m_performOverFrames));
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
            Coroutine convertion = StartCoroutine(ConvertToVoxels(m_voxelSize, m_separatedVoxels, m_performOverFrames));
            yield return convertion;
        }
        else
        {
            StartCoroutine(ConvertToVoxels(m_voxelSize, m_separatedVoxels, m_performOverFrames));
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

        GetBakedVerts();

        m_orginalVerts.CopyFrom(m_originalMesh.vertices);

        if (m_performOverFrames)
        {
            Coroutine convertion = StartCoroutine(ConvertToVoxels(m_voxelSize, m_separatedVoxels, m_performOverFrames));
            yield return convertion;
        }
        else
        {
            StartCoroutine(ConvertToVoxels(m_voxelSize, m_separatedVoxels, m_performOverFrames));
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

        Coroutine convert = StartCoroutine(ConvertToVoxels(m_voxelSize, m_separatedVoxels, false));
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
    /// <param name="p_seperatedVoxels">stored value of if the voxels will have shared verts</param>
    /// <param name="p_performOverFrames">stored value of if this operation should occur over several frames</param>
    private IEnumerator ConvertToVoxels(float p_voxelSize, bool p_seperatedVoxels, bool p_performOverFrames)
    {
        Matrix4x4 localToWorld = m_objectWithMesh.transform.localToWorldMatrix;
        float4x4 localToWorldConverted = localToWorld;

        float voxelSizeRatio = 1.0f / p_voxelSize;

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
            m_voxelSizeRatio = voxelSizeRatio,
            m_voxelDetails = m_voxelDetails,
            m_ABPoints = m_ABPoints,
            m_ABUVs = m_ABUVs
        };

        m_buildTriJobHandle = triJob.Schedule();

        GetConvertedMesh convertJob = new GetConvertedMesh()
        {
            m_voxelDetails = m_voxelDetails,
            m_convertedVerts = m_convertedVerts,
            m_convertedUVs = m_convertedUVs,
            m_convertedTris = m_convertedTris,
            m_voxelSize = p_voxelSize,
            m_seperatedVoxels = p_seperatedVoxels
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
            Vector2 UV = new Vector2(tempUVs[i].x, tempUVs[i].y);
            convertedUVs[i] = UV;
        }

        int[] tempTris = m_convertedTris.ToArray();
        //Tris
        for (int i = 0; i < triCount; i++)
        {
            convertedTris[i] = tempTris[i];
        }

        //Build new mesh
        m_voxelMesh.Clear(false);

        m_voxelMesh.SetVertices(new List<Vector3>(convertedVerts));
        m_voxelMesh.SetUVs(0, new List<Vector2>(convertedUVs));
        m_voxelMesh.SetTriangles(convertedTris, 0);

        m_voxelMesh.Optimize();
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
        if (m_ABPoints.IsCreated)
            m_ABPoints.Dispose(); 
        if (m_ABUVs.IsCreated)
            m_ABUVs.Dispose();

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
    private NativeHashMap<int3, float2> m_voxelDetails;
    private NativeQueue<int3> m_ABPoints;
    private NativeQueue<float2> m_ABUVs;
    //Outputs
    private NativeList<float4> m_convertedVerts; // unknown size
    private NativeList<float2> m_convertedUVs; //size of verts
    private NativeList<int> m_convertedTris; //3 x 12 x the size of positions

    private JobHandle m_buildTriJobHandle;
    private JobHandle m_convertedMeshJobHandle;

    [BurstCompile]
    private struct BuildTriVoxels : IJob
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

        public NativeHashMap<int3, float2> m_voxelDetails;
        public NativeQueue<int3> m_ABPoints;
        public NativeQueue<float2> m_ABUVs;

        /// <summary>
        /// Build the voxel plavcment based off 3 tris
        /// Uses the Bresenham's line algorithum to find points from vert A to vert B
        /// Using the same approach points are calculated from thje previously found points to vert C
        /// </summary>
        public void Execute()
        {
            for (int triIndex = 0; triIndex < m_tris.Length/3; triIndex++)
            {
                if(m_voxelDetails.Length >= m_voxelDetails.Capacity - 1)
                    return;

                //Float 4 varients due to matrix math
                float4 localVertA = math.mul(m_localToWorldTransform, new float4(m_verts[m_tris[triIndex * 3]], 1.0f)) * m_voxelSizeRatio;
                float4 localVertB = math.mul(m_localToWorldTransform, new float4(m_verts[m_tris[triIndex * 3 + 1]], 1.0f)) * m_voxelSizeRatio;
                float4 localVertC = math.mul(m_localToWorldTransform, new float4(m_verts[m_tris[triIndex * 3 + 2]], 1.0f)) * m_voxelSizeRatio;

                int3 vertA = GetSnappedInt3(new float3(localVertA.x, localVertA.y, localVertA.z));
                int3 vertB = GetSnappedInt3(new float3(localVertB.x, localVertB.y, localVertB.z));
                int3 vertC = GetSnappedInt3(new float3(localVertC.x, localVertC.y, localVertC.z));

                //Has UV's been set?
                float2 vertAUV = new float2(0, 0);
                float2 vertBUV = vertAUV;
                float2 vertCUV = vertAUV;

                if (m_uvs.Length != 0)
                {
                    vertAUV = m_uvs[m_tris[triIndex * 3]];
                    vertBUV = m_uvs[m_tris[triIndex * 3 + 1]];
                    vertCUV = m_uvs[m_tris[triIndex * 3 + 2]];
                }

                //Initial line
                BresenhamDrawEachPoint(vertA, vertB, vertAUV, vertBUV, true);

                while(m_ABPoints.Count > 0)
                {
                    int3 ABPos = m_ABPoints.Dequeue();
                    float2 ABUV = m_ABUVs.Dequeue();
                    
                    BresenhamDrawEachPoint(ABPos, vertC, ABUV, vertCUV, false);
                }
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
        private void BresenhamDrawEachPoint(int3 p_startingPoint, int3 p_finalPoint, float2 p_startingUV, float2 p_finalUV, bool p_shouldTempStore)
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

                AddPoint(p_startingPoint, vector, currentPoint, p_startingUV, p_finalUV, p_shouldTempStore);

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
        private void AddPoint(float3 p_startPoint, float3 p_vector, float3 p_currentPoint, float2 p_startUV, float2 p_endUV, bool p_shouldTempStore)
        {
            float a2bPercent = GetPercent(p_startPoint, p_vector, p_currentPoint);

            float2 UV = MergeUVs(p_startUV, p_endUV, a2bPercent);

            if(p_shouldTempStore)
            {
                m_ABPoints.Enqueue(GetSnappedInt3(p_currentPoint));
                m_ABUVs.Enqueue(UV);
            }
            else
            {
                m_voxelDetails.TryAdd(GetSnappedInt3(p_currentPoint), UV);
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
        private int3 GetSnappedInt3(float3 p_vector)
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
        #endregion
    }


    [BurstCompile]
    private struct GetConvertedMesh : IJob
    {
        [ReadOnly]
        public float m_voxelSize;
        public bool m_seperatedVoxels;
        public NativeHashMap<int3, float2> m_voxelDetails;

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
            //Get all unique
            NativeArray<int3> uniquePositions = m_voxelDetails.GetKeyArray(Allocator.Temp);
            NativeArray<float2> uniqueUVs = m_voxelDetails.GetValueArray(Allocator.Temp);

            NativeArray<int> indexArray = new NativeArray<int>(8, Allocator.Temp);
            if (m_seperatedVoxels)
            {
                for (int i = 0; i < uniquePositions.Length; i++)
                {
                    float4 voxelPos = new float4(uniquePositions[i].x * m_voxelSize, uniquePositions[i].y * m_voxelSize, uniquePositions[i].z * m_voxelSize, 1.0f);

                    float4 right = new float4(m_voxelSize / 2.0f, 0.0f, 0.0f, 0.0f); // r = right l = left
                    float4 up = new float4(0.0f, m_voxelSize / 2.0f, 0.0f, 0.0f); // u = up, d = down
                    float4 forward = new float4(0.0f, 0.0f, m_voxelSize / 2.0f, 0.0f); // f = forward b = backward

                    //Verts
                    m_convertedVerts.Add(voxelPos - right - up - forward);
                    m_convertedVerts.Add(voxelPos + right - up - forward);
                    m_convertedVerts.Add(voxelPos + right + up - forward);
                    m_convertedVerts.Add(voxelPos - right + up - forward);
                    m_convertedVerts.Add(voxelPos - right + up + forward);
                    m_convertedVerts.Add(voxelPos + right + up + forward);
                    m_convertedVerts.Add(voxelPos + right - up + forward);
                    m_convertedVerts.Add(voxelPos - right - up + forward);

                    //UVs Add in 8 for each vert added
                    float2 UV = uniqueUVs[i];
                    for (int UVIndex = 0; UVIndex < 8; UVIndex++)
                    {
                        m_convertedUVs.Add(UV);
                    }

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
            else
            {
                NativeHashMap<int3, int> addedVerts = new NativeHashMap<int3, int>(MAX_MESH_COUNT, Allocator.Temp);
                int currentIndex = 0;

                for (int i = 0; i < uniquePositions.Length; i++)
                {
                    if (addedVerts.Length + 8 >= MAX_MESH_COUNT)
                        break;

                    int3 voxelPos = uniquePositions[i];
                    float2 UV = uniqueUVs[i];
                    int3 right = new int3(1, 0, 0); // r = right l = left
                    int3 up = new int3(0, 1, 0); // u = up, d = down
                    int3 forward = new int3(0, 0, 1); // f = forward b = backward

                    indexArray[0] = AddJointVert(ref addedVerts, voxelPos, ref currentIndex, UV);
                    indexArray[1] = AddJointVert(ref addedVerts, voxelPos + right, ref currentIndex, UV);
                    indexArray[2] = AddJointVert(ref addedVerts, voxelPos + right + up, ref currentIndex, UV);
                    indexArray[3] = AddJointVert(ref addedVerts, voxelPos + up, ref currentIndex, UV);
                    indexArray[4] = AddJointVert(ref addedVerts, voxelPos + up + forward, ref currentIndex, UV);
                    indexArray[5] = AddJointVert(ref addedVerts, voxelPos + right + up + forward, ref currentIndex, UV);
                    indexArray[6] = AddJointVert(ref addedVerts, voxelPos + right + forward, ref currentIndex, UV);
                    indexArray[7] = AddJointVert(ref addedVerts, voxelPos + forward, ref currentIndex, UV);

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

                NativeArray<float4> verts = new NativeArray<float4>(addedVerts.Length, Allocator.Temp);

                NativeArray<int3> vertPos = addedVerts.GetKeyArray(Allocator.Temp);
                NativeArray<int> vertindex = addedVerts.GetValueArray(Allocator.Temp);

                for(int i = 0; i < vertPos.Length; i++)
                {
                    verts[vertindex[i]] = new float4(vertPos[i].x * m_voxelSize, vertPos[i].y * m_voxelSize, vertPos[i].z * m_voxelSize, 1.0f);
                }

                m_convertedVerts.AddRange(verts);
            }
        }

            /// <summary>
    /// Add in a joint vert to the dictionary
    /// </summary>
    /// <param name="p_addedVerts">Dictionary of already added in verts</param>
    /// <param name="p_position">current vert postion</param>
    /// <param name="p_index">current index</param>
    /// <param name="p_UV">The UV of the given voxel</param>
    /// <returns>What index is the vert found or been made</returns>
    private int AddJointVert(ref NativeHashMap<int3, int> p_addedVerts, int3 p_position, ref int p_index, float2 p_UV)
        {
            if (p_addedVerts.TryGetValue(p_position, out int index)) //Already added
            {
                return index;
            }
            else
            {
                p_addedVerts.TryAdd(p_position, p_index);
                m_convertedUVs.Add(p_UV);
                p_index++;
                return p_index - 1;
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
                m_runFlag = false;

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
                m_runFlag = false;

                return false;
            }
        }

        //Voxel Object
        if (m_voxelObject == null)
        {
#if UNITY_EDITOR
            Debug.Log(name + " voxel object is missing");
#endif
            m_runFlag = false;

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
                    m_runFlag = false;

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
            if (m_objectWithMesh == null)
                m_objectWithMesh = gameObject;

            //Store transform
            Vector3 orginalPos = m_objectWithMesh.transform.position;
            Quaternion orginalRot = m_objectWithMesh.transform.rotation;

            m_objectWithMesh.transform.position = Vector3.zero;
            if (m_resetRotation)
                m_objectWithMesh.transform.rotation = Quaternion.identity;

            Coroutine saveConvert = StartCoroutine(InitVoxeliser());

            yield return saveConvert;

            m_voxelMesh.Optimize();

            m_objectWithMesh.transform.position = orginalPos;
            if (m_resetRotation)
                m_objectWithMesh.transform.rotation = orginalRot;

            //Verify as static
            MeshFilter meshFilter = m_voxelObject.GetComponent<MeshFilter>();

            if (meshFilter != null)
            {
                //Get mesh
                Mesh savingMesh = meshFilter.sharedMesh;
                savingMesh.name = "Voxelised_" + savingMesh.name;
                string path = EditorUtility.SaveFilePanel("Save Voxel Mesh: " + name, "Assets/", "Voxelised-" + name + ".mesh", "mesh");
                if (!string.IsNullOrEmpty(path))
                {
                    path = FileUtil.GetProjectRelativePath(path);

                    MeshUtility.Optimize(savingMesh);

                    AssetDatabase.CreateAsset(savingMesh, path);
                    AssetDatabase.SaveAssets();
                }
            }

            m_saveStaticMesh = false;

            DestroyImmediate(m_voxelObject);
            ToggleMaterial(m_objectWithMesh, true);
        }
        else
        {
            Debug.Log("Can only save when set to static");
        }

        m_processingFlag = false;

        yield break;
    }
#endif


    /// <summary>
    /// Get mesh from eith skinned mesh renderer or mesh renderer
    /// </summary>
    /// <param name="p_object">object to get mesh for</param>
    /// <returns>correct mesh based off avalibilty of renderers in children</returns>
    private Mesh GetMesh(GameObject p_object)
    {
        if (p_object == null) //Early breakout
            return null;

        Mesh instanceMesh = new Mesh();

        MeshFilter meshFilter = p_object.GetComponent<MeshFilter>();
        if (meshFilter != null)
        {
            m_originalMesh = meshFilter.sharedMesh;
        }
        else
        {
            if(m_skinnedRenderer == null)
                m_skinnedRenderer = p_object.GetComponent<SkinnedMeshRenderer>();
            if (m_skinnedRenderer != null)
            {
                GetBakedVerts();
            }
        }

        instanceMesh.vertices = m_originalMesh.vertices;
        instanceMesh.uv = m_originalMesh.uv;
        instanceMesh.triangles = m_originalMesh.triangles;
        return instanceMesh;
    }

    /// <summary>
    /// Remove all materials attached to an object
    /// </summary>
    /// <param name="p_object">object to remove material from</param>
    /// <param name="p_val">Value to give material</param>
    private void ToggleMaterial(GameObject p_object, bool p_val)
    {
        if (p_object == null)
            return;

        MeshRenderer meshRenderer = p_object.GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            if (p_val)
            {
                meshRenderer.materials = m_orginalMats;
            }
            else
            {
                meshRenderer.materials = new Material[0];
            }
        }
        else
        {
            SkinnedMeshRenderer skinnedMeshRenderer = p_object.GetComponent<SkinnedMeshRenderer>();
            if (skinnedMeshRenderer != null)
            {
                if (p_val)
                {
                    skinnedMeshRenderer.materials = m_orginalMats;
                }
                else
                {
                    skinnedMeshRenderer.materials = new Material[0];
                }
            }
        }
    }

    /// <summary>
    /// Remove all materials attached to an object
    /// </summary>
    /// <param name="p_object">object to remove material from</param>
    /// <returns></returns>
    private Material GetMaterial(GameObject p_object)
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
    private void GetBakedVerts()
    {
        if(m_originalMesh == null)
            m_originalMesh = new Mesh();
        m_originalMesh.Clear();

        m_skinnedRenderer.BakeMesh(m_originalMesh);

        Vector3[] verts = m_originalMesh.vertices;

        Vector3 lossyScale = m_objectWithMesh.transform.lossyScale;

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
        m_originalMesh.vertices = verts;
    }

    #endregion
    #endif
}


