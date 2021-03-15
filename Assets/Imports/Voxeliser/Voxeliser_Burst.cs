#if VOXELISER_MATHEMATICS_ENABLED && VOXELISER_BURST_ENABLED && VOXELISER_COLLECTIONS_ENABLED

//#define VOXELISER_DEBUG_TOOLTIPS

using System.Collections;
using UnityEngine;
using Unity.Jobs;
using UnityEngine.Jobs;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Collections;

//Faster array copy
using Unity.Collections.LowLevel.Unsafe;


public class Voxeliser_Burst : MonoBehaviour
{
    //Building Voxel stuff
    private const int MAX_VERTS_PER_MESH_COUNT = 65535; //Mesh can have 65535 verts

    private const int VERTS_PER_VOXEL_HARDEDGE = 24; //How many verts on a single voxel for a hard edge
    private const int VERTS_PER_VOXEL_SOFTEDGE = 8; //How many verts on a single voxel for a soft edge

    private const int VOXELS_PER_MESH_HARDEDGE = MAX_VERTS_PER_MESH_COUNT / VERTS_PER_VOXEL_HARDEDGE;
    private const int VOXELS_PER_MESH_SOFTEDGE = MAX_VERTS_PER_MESH_COUNT / VERTS_PER_VOXEL_SOFTEDGE;

    private const int MAX_VOXEL_COUNT = 1000000; //Smaller is better, keep below int32.maxvalue

    private const int TRIS_PER_VOXEL = 36;

    [SerializeField]
    public enum VOXELISER_TYPE {SOLID, STATIC, ANIMATED};
    [Tooltip("SOLID = no animation, snaps voxels to grid DYNAMIC_SOLID = solid mesh that will have its vertices modified during runtime ANIMATED = full animation STATIC = converted to single mesh, wont snap at all")]
    public VOXELISER_TYPE m_voxeliserType = VOXELISER_TYPE.SOLID;
    public enum VERT_TYPE { HARD_EDGE, SOFT_EDGE }
    [Tooltip("What kind of verts will be used?")]
    public VERT_TYPE m_vertType = VERT_TYPE.HARD_EDGE;

    private bool m_storedIsHardEdge = true;
    private float m_storedVoxelSize = 0.0f;
    //End

    //Editor
    [Tooltip("How large each voxel is")]
    public float m_voxelSize = 1.0f;
    [Tooltip("Where can we find the mesh desired to be voxelised, if on the same object as the script, this can be ignored")]
    public GameObject m_objectWithMesh = null;
    [Header("Advanced")]
    [Range(1,100)]
    [Tooltip("How many meshes should exist, used for large meshes")]
    public int m_meshesToUse = 1;
    [Tooltip("Should this voxelised object run via itself")]
    public bool m_autoInitilise = false;

    private bool m_editorBeenChangedFlag = true;
    private bool m_initilisedFlag = false;
    //End


    //Native Job system 
    private struct PassThroughData
    {
        public int3 m_position;
        public double2 m_UV;

        public PassThroughData(int3? p_nullPosition, double2? p_nullUV = null)
        {
            m_position = p_nullPosition == null ? int3.zero : p_nullPosition.Value;
            m_UV = p_nullUV == null ? double2.zero : p_nullUV.Value;
        }
    }

    private struct VoxelData
    {
        public int3 m_position;
        public double2 m_UV;

        public VoxelData(int3? p_nullPosition, double2? p_nullUV = null)
        {
            m_position = p_nullPosition == null ? int3.zero : p_nullPosition.Value;
            m_UV = p_nullUV == null ? double2.zero : p_nullUV.Value;
        }

        public void SetVoxelData(int3? p_nullPosition, double2? p_nullUV)
        {
            m_position = p_nullPosition == null ? int3.zero : p_nullPosition.Value;
            m_UV = p_nullUV == null ? double2.zero : p_nullUV.Value;
        }
    }

    private NativeHashMap<int3, double2> m_nativePassThroughTriData;

    private NativeArray<Vector3> m_nativeReturnVerts;
    private NativeArray<int> m_nativeReturnTris;
    private NativeArray<Vector2> m_nativeReturnUVs;
    private NativeArray<Vector3> m_nativeReturnNormals;

    private NativeArray<Vector3> m_nativeVerts;
    private NativeArray<int> m_nativeTris;
    private NativeArray<Vector2> m_nativeUVs;

    private JobHandle m_calcTrisJobHandle;
    private JobHandle m_buildMeshJobHandle;
    //End

    //Mesh stuff
    private GameObject m_voxelisedObject = null;
    private Mesh m_originalMesh = null;

    private SkinnedMeshRenderer m_skinnedMeshRenderer = null;

    private Mesh[] m_voxelisedMeshes;

    private void Start()
    {
        if (m_autoInitilise && !m_initilisedFlag)
            InitVoxeliser();
    }

    //End
    public void InitVoxeliser()
    {
        m_initilisedFlag = true;
        
        if (m_objectWithMesh == null)//Atempt to find a mesh renderer, may be the wrong one
        {
#if UNITY_EDITOR && VOXELISER_DEBUG_TOOLTIPS
            Debug.LogWarning("No mesh assigned, attempting to find one in child " + name);
#endif
            MeshRenderer meshRenderer = GetComponentInChildren<MeshRenderer>();
            if (meshRenderer != null)
                m_objectWithMesh = meshRenderer.gameObject;
            else
            {
                SkinnedMeshRenderer skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
                if (skinnedMeshRenderer != null)
                    m_objectWithMesh = skinnedMeshRenderer.gameObject;
            }
        }

        m_originalMesh = GetMesh(m_objectWithMesh);

        if (m_originalMesh == null)
        {
            Destroy(this);
            return;
        }

        //Build mesh stuff
        Vector3[] verts = m_originalMesh.vertices;
        int[] tris = m_originalMesh.triangles;
        Vector2[] UVs = m_originalMesh.uv;

        m_voxelisedMeshes = new Mesh[m_meshesToUse];
        m_voxelisedObject = new GameObject(gameObject.name + "-Voxelised");
        Material material = GetMaterial(m_objectWithMesh);

        for (int meshIndex = 0; meshIndex < m_meshesToUse; meshIndex++)
        {
            GameObject meshObject = new GameObject(m_voxelisedObject.name + "(meshObject" + meshIndex + ")");
            meshObject.transform.SetParent(m_voxelisedObject.transform);

            MeshFilter newFilter = meshObject.AddComponent<MeshFilter>();
            Mesh newMesh = new Mesh();
            m_voxelisedMeshes[meshIndex] = newMesh;
            newFilter.mesh = newMesh;

            MeshRenderer newMeshRenderer = meshObject.AddComponent<MeshRenderer>();
            newMeshRenderer.material = material;
        }

        //Setup Natives
        m_nativePassThroughTriData = new NativeHashMap<int3, double2>(MAX_VOXEL_COUNT, Allocator.Persistent);
        
        m_nativeVerts = new NativeArray<Vector3>(verts.Length, Allocator.Persistent);
        m_nativeTris = new NativeArray<int>(tris.Length, Allocator.Persistent);
        m_nativeUVs = new NativeArray<Vector2>(UVs.Length, Allocator.Persistent);

        Convert(m_nativeVerts, verts);
        Convert(m_nativeTris, tris);
        Convert(m_nativeUVs, UVs);

        RebuildVaribles();

        switch (m_voxeliserType)
        {
            case VOXELISER_TYPE.SOLID:
                InitVoxeliserSolid();
                break;
            case VOXELISER_TYPE.STATIC:
                InitVoxeliserSolid();
                break;
            case VOXELISER_TYPE.ANIMATED:
                InitVoxeliserAnimated();
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Intialise the solid version of the object
    /// </summary>
    private void InitVoxeliserSolid()
    {
        ToggleMaterial(m_objectWithMesh, false);

        VoxeliserUpdateSolid();
    }

    /// <summary>
    /// Intialise the animated version of the object
    /// </summary>
    private void InitVoxeliserAnimated()
    {
        ToggleMaterial(m_objectWithMesh, false);

        m_skinnedMeshRenderer = m_objectWithMesh.GetComponent<SkinnedMeshRenderer>();

        if (m_skinnedMeshRenderer == null)
        {
#if UNITY_EDITOR
            Debug.Log("No skinned mesh renderer was found on animating object " + name);
#endif
            Destroy(gameObject);
            return;
        }

        VoxeliserUpdateAnimated();
    }

    private void VoxeliserUpdateSolid()
    {
        //Solid does nothing
        StartCoroutine(VoxeliserUpdate());
    }

    private void VoxeliserUpdateAnimated()
    {
        if (m_originalMesh == null)
            m_originalMesh = new Mesh();
        m_originalMesh.Clear();

        m_skinnedMeshRenderer.BakeMesh(m_originalMesh);

        Vector3[] verts = m_originalMesh.vertices;

        Convert(m_nativeVerts, verts);

        StartCoroutine(VoxeliserUpdate());
    }

    private IEnumerator VoxeliserUpdate()
    {
        //Clean up variables
        if (m_editorBeenChangedFlag) //Rebuild what needs to be
        {
            m_editorBeenChangedFlag = false;
            RebuildVaribles();
        }

        m_nativePassThroughTriData.Clear();

        Matrix4x4 currentTransform = m_objectWithMesh.transform.localToWorldMatrix;
        double4x4 nativeCurrentTransform = FloatMatrixToDoubleMatrix(currentTransform);

        //Do job stuff
        IJob_CalulateTris calcTrisJob = new IJob_CalulateTris
        {
            m_passThroughTriData = m_nativePassThroughTriData.AsParallelWriter(),
            m_nativeVerts = m_nativeVerts,
            m_nativeTris = m_nativeTris,
            m_nativeUVs = m_nativeUVs,
            m_transformMatrix = nativeCurrentTransform,
            m_voxelSize = m_voxelSize
        };

        m_calcTrisJobHandle = calcTrisJob.Schedule(m_nativeTris.Length / 3, 16);

        m_calcTrisJobHandle.Complete();

        int voxelCount = m_nativePassThroughTriData.Count();
        int maxPossibleVoxels = Mathf.Min((m_storedIsHardEdge ? VOXELS_PER_MESH_HARDEDGE : VOXELS_PER_MESH_SOFTEDGE) * m_meshesToUse, voxelCount); //how many we want to do, vs how many we can do

        int voxelsPerMesh = maxPossibleVoxels / m_meshesToUse;

        if (m_nativeReturnVerts.IsCreated)
            m_nativeReturnVerts.Dispose();
        if (m_nativeReturnUVs.IsCreated)
            m_nativeReturnUVs.Dispose();
        if (m_nativeReturnTris.IsCreated)
            m_nativeReturnTris.Dispose();
        if (m_nativeReturnNormals.IsCreated)
            m_nativeReturnNormals.Dispose();

        m_nativeReturnVerts = new NativeArray<Vector3>(m_storedIsHardEdge ? maxPossibleVoxels * VERTS_PER_VOXEL_HARDEDGE : maxPossibleVoxels * VERTS_PER_VOXEL_SOFTEDGE, Allocator.Persistent);
        m_nativeReturnUVs = new NativeArray<Vector2>(m_storedIsHardEdge ? maxPossibleVoxels * VERTS_PER_VOXEL_HARDEDGE : maxPossibleVoxels * VERTS_PER_VOXEL_SOFTEDGE, Allocator.Persistent);
        m_nativeReturnTris = new NativeArray<int>(maxPossibleVoxels * TRIS_PER_VOXEL, Allocator.Persistent);
        m_nativeReturnNormals = new NativeArray<Vector3>(m_storedIsHardEdge ? maxPossibleVoxels * VERTS_PER_VOXEL_HARDEDGE : maxPossibleVoxels * VERTS_PER_VOXEL_SOFTEDGE, Allocator.Persistent);

        IJob_BuildMeshes buildMeshJob = new IJob_BuildMeshes
        {
            m_passThroughTriData = m_nativePassThroughTriData,
            m_returnVerts = m_nativeReturnVerts,
            m_returnTris = m_nativeReturnTris,
            m_returnUVs = m_nativeReturnUVs,
            m_returnNormals = m_nativeReturnNormals,
            m_voxelSize = m_storedVoxelSize,
            m_hardEdges = m_storedIsHardEdge,
            m_voxelsPerMesh = voxelsPerMesh,
            m_meshCount = m_meshesToUse
        };

        m_buildMeshJobHandle = buildMeshJob.Schedule();

        m_buildMeshJobHandle.Complete();

        Vector3[] allDoubleVerts = m_nativeReturnVerts.ToArray();
        Vector2[] allDoubleUVs = m_nativeReturnUVs.ToArray();
        int[] allTris = m_nativeReturnTris.ToArray();
        Vector3[] allDoubleNormals = m_nativeReturnNormals.ToArray();

        Vector3[] meshVerts = new Vector3[voxelsPerMesh * (m_storedIsHardEdge ? VERTS_PER_VOXEL_HARDEDGE : VERTS_PER_VOXEL_SOFTEDGE)];
        Vector2[] meshUVs = new Vector2[voxelsPerMesh * (m_storedIsHardEdge ? VERTS_PER_VOXEL_HARDEDGE : VERTS_PER_VOXEL_SOFTEDGE)];
        int[] meshTris = new int[voxelsPerMesh * 36];
        Vector3[] meshNormals = new Vector3[voxelsPerMesh * (m_storedIsHardEdge ? VERTS_PER_VOXEL_HARDEDGE : VERTS_PER_VOXEL_SOFTEDGE)];

        int vertsPerVoxel = (m_storedIsHardEdge ? VERTS_PER_VOXEL_HARDEDGE : VERTS_PER_VOXEL_SOFTEDGE);
        int vertsPerMesh = vertsPerVoxel * voxelsPerMesh;
        int trisPerMesh = 36 * voxelsPerMesh;

        //Get voxel
        for (int meshIndex = 0; meshIndex < m_meshesToUse; meshIndex++)
        {
            m_voxelisedMeshes[meshIndex].Clear();
            
            int vertStartingIndex = meshIndex * vertsPerMesh;
            int triStartingIndex = meshIndex * trisPerMesh;

            System.Array.Copy(allDoubleVerts, vertStartingIndex, meshVerts, 0, vertsPerMesh);
            System.Array.Copy(allDoubleUVs, vertStartingIndex, meshUVs, 0, vertsPerMesh);
            System.Array.Copy(allDoubleNormals, vertStartingIndex, meshNormals, 0, vertsPerMesh);

            System.Array.Copy(allTris, triStartingIndex, meshTris, 0, trisPerMesh);

            m_voxelisedMeshes[meshIndex].vertices = meshVerts;
            m_voxelisedMeshes[meshIndex].uv = meshUVs;
            m_voxelisedMeshes[meshIndex].triangles = meshTris;
            m_voxelisedMeshes[meshIndex].normals = meshNormals;
        }

        yield return null;

#if UNITY_EDITOR
        if (m_storedVoxelSize != m_voxelSize || m_storedIsHardEdge != (m_vertType == VERT_TYPE.HARD_EDGE))
            m_editorBeenChangedFlag = true;
#endif

        switch (m_voxeliserType)
        {
            case VOXELISER_TYPE.SOLID:
                VoxeliserUpdateSolid();
                break;
            case VOXELISER_TYPE.STATIC: //Just runs once
                break;
            case VOXELISER_TYPE.ANIMATED:
                VoxeliserUpdateAnimated();
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Clean up natives
    /// </summary>
    private void OnDestroy()
    {
        m_calcTrisJobHandle.Complete();
        m_buildMeshJobHandle.Complete();

        if (m_nativePassThroughTriData.IsCreated)
            m_nativePassThroughTriData.Dispose();

        if (m_nativeReturnVerts.IsCreated)
            m_nativeReturnVerts.Dispose();
        if (m_nativeReturnTris.IsCreated)
            m_nativeReturnTris.Dispose(); 
        if (m_nativeReturnUVs.IsCreated)
            m_nativeReturnUVs.Dispose();
        if (m_nativeReturnNormals.IsCreated)
            m_nativeReturnNormals.Dispose();

        if (m_nativeVerts.IsCreated)
            m_nativeVerts.Dispose();
        if (m_nativeTris.IsCreated)
            m_nativeTris.Dispose();
        if (m_nativeUVs.IsCreated)
            m_nativeUVs.Dispose();
    }

    /// <summary>
    /// Rebuild pre calculated varibes and native containers as needed when variables change
    /// </summary>
    private void RebuildVaribles()
    {
        if(m_voxelSize <= 0.0f)
        {
            m_storedVoxelSize = 1.0f;
#if UNITY_EDITOR
            Debug.LogWarning("Voxel size on " + name + " is out of range, should be greater then 0.0f. Defualting to 1.0f");
#endif
        }
        else
        {
            m_storedVoxelSize = m_voxelSize;
        }

        m_storedIsHardEdge = m_vertType == VERT_TYPE.HARD_EDGE;
    }

#region Job Systems
    [BurstCompile]
    private struct IJob_CalulateTris : IJobParallelFor
    {
        private const double ALLOWABLE_DISTANCE_TO_PLANE = 0.55;
        private const double ALLOWABLE_AREA_GROWTH = 1.05;

        public NativeHashMap<int3, double2>.ParallelWriter m_passThroughTriData;

        [ReadOnly]
        public NativeArray<Vector3> m_nativeVerts;
        [ReadOnly]
        public NativeArray<int> m_nativeTris;
        [ReadOnly]
        public NativeArray<Vector2> m_nativeUVs;
        [ReadOnly]
        public double4x4 m_transformMatrix;
        [ReadOnly]
        public double m_voxelSize;

        public void Execute(int index)
        {
            //Setup transform matrix
            double4 vertA = math.mul(m_transformMatrix, new double4(Convert(m_nativeVerts[m_nativeTris[index * 3]]), 1.0f));
            double4 vertB = math.mul(m_transformMatrix, new double4(Convert(m_nativeVerts[m_nativeTris[index * 3 + 1]]), 1.0f));
            double4 vertC = math.mul(m_transformMatrix, new double4(Convert(m_nativeVerts[m_nativeTris[index * 3 + 2]]), 1.0f));

            vertA = SnapToIncrement(vertA);
            vertB = SnapToIncrement(vertB);
            vertC = SnapToIncrement(vertC);

            double2 UVA = Convert(m_nativeUVs[m_nativeTris[index * 3]]);
            double2 UVB = Convert(m_nativeUVs[m_nativeTris[index * 3 + 1]]);
            double2 UVC = Convert(m_nativeUVs[m_nativeTris[index * 3 + 2]]);

            int3 vertAGridPos = PositionToGridPosition(vertA);
            int3 vertBGridPos = PositionToGridPosition(vertB);
            int3 vertCGridPos = PositionToGridPosition(vertC);

            if (SameInt3(vertAGridPos, vertBGridPos) && SameInt3(vertAGridPos, vertBGridPos) && SameInt3(vertBGridPos, vertCGridPos)) //All same value, add once and end
            {
                m_passThroughTriData.TryAdd(vertAGridPos, UVA);
                return;
            }

            //Build max extents of grid
            int3 lowerBounds = new int3(GetLowestOf3(vertAGridPos.x, vertBGridPos.x, vertCGridPos.x), GetLowestOf3(vertAGridPos.y, vertBGridPos.y, vertCGridPos.y), GetLowestOf3(vertAGridPos.z, vertBGridPos.z, vertCGridPos.z));
            int3 upperBounds = new int3(GetHighestOf3(vertAGridPos.x, vertBGridPos.x, vertCGridPos.x), GetHighestOf3(vertAGridPos.y, vertBGridPos.y, vertCGridPos.y), GetHighestOf3(vertAGridPos.z, vertBGridPos.z, vertCGridPos.z));

            //Run through all possible positions given the extent

            //Area Calc
            //Using plane equation for distance https://mathinsight.org/distance_point_plane_examples
            double4 planeEquation = GetPlaneEquation(vertAGridPos, vertBGridPos, vertCGridPos);
            double planeMagnitude = math.sqrt(planeEquation.x * planeEquation.x + planeEquation.y * planeEquation.y + planeEquation.z * planeEquation.z);

            //Determining Closest point
            double denominator = planeEquation.x * planeEquation.x + planeEquation.y * planeEquation.y + planeEquation.z * planeEquation.z;

            //For within plane equations
            double areaABC = GetArea(vertAGridPos, vertBGridPos, vertCGridPos);

            for (int xIndex = lowerBounds.x; xIndex <= upperBounds.x; xIndex++)
            {
                for (int yIndex = lowerBounds.y; yIndex <= upperBounds.y; yIndex++)
                {
                    for (int zIndex = lowerBounds.z; zIndex <= upperBounds.z; zIndex++)
                    {
                        //Get distance, is it smaller then the size of a voxel?, example found here "https://mathinsight.org/distance_point_plane_examples"
                        double3 currentPoint = new double3(xIndex, yIndex, zIndex);

                        double distanceToPlane = math.abs(planeEquation.x * currentPoint.x + planeEquation.y * currentPoint.y + planeEquation.z * currentPoint.z + planeEquation.w) / planeMagnitude;

                        if (distanceToPlane < ALLOWABLE_DISTANCE_TO_PLANE) //lying on plane, determine if within the triangle
                        {
                            double numerator = -planeEquation.w - (currentPoint.x * planeEquation.x + currentPoint.y * planeEquation.y + currentPoint.z * planeEquation.z);
                            double cValue = numerator / denominator;

                            double3 pointOnPlane = GetClosestPoint(currentPoint, planeEquation, cValue);

                            double pointToVertArea = GetArea(currentPoint, vertAGridPos, vertBGridPos) + GetArea(currentPoint, vertAGridPos, vertCGridPos) + GetArea(currentPoint, vertBGridPos, vertCGridPos);

                            if(pointToVertArea/areaABC <= ALLOWABLE_AREA_GROWTH)
                            {
                                int3 intValuePoint = new int3(xIndex, yIndex, zIndex);
                                m_passThroughTriData.TryAdd(intValuePoint, CalculateUV(intValuePoint, vertAGridPos, vertBGridPos, vertCGridPos, UVA, UVB, UVC));
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Convert a world position into a grid position
        /// This new position acts in two ways
        /// 1: It allows for better storage of varibles, doubles in hashmap, although the same may have slight variations, int3 wont have this issue.
        /// 2: Builds a clear and defined grid for the voxels to work on
        /// </summary>
        /// <param name="p_point">Point to convert</param>
        /// <returns>Int3 value</returns>
        private int3 PositionToGridPosition(double4 p_point)
        {
            return new int3((int)(p_point.x / m_voxelSize), (int)(p_point.y / m_voxelSize), (int)(p_point.z / m_voxelSize));
        }

        /// <summary>
        /// Snap a value to an increment
        /// 1.5 of an increment of 0.2 will be 1.4
        /// -0.1 of an incrment of 0.2 wil be -0.2
        /// </summary>
        /// <param name="p_val">Value to snap</param>
        /// <returns>Snapped value</returns>
        private double4 SnapToIncrement(double4 p_val)
        {
            p_val.x = p_val.x - (p_val.x >= 0.0f ? p_val.x % m_voxelSize : p_val.x % m_voxelSize + m_voxelSize);
            p_val.y = p_val.y - (p_val.y >= 0.0f ? p_val.y % m_voxelSize : p_val.y % m_voxelSize + m_voxelSize);
            p_val.z = p_val.z - (p_val.z >= 0.0f ? p_val.z % m_voxelSize : p_val.z % m_voxelSize + m_voxelSize);

            return p_val;
        }

        /// <summary>
        /// Determine if 2 int3's are the same value
        /// </summary>
        /// <param name="p_val1">First int3</param>
        /// <param name="p_val2">Second int3</param>
        /// <returns>True when x,y, z components match</returns>
        private bool SameInt3(int3 p_val1, int3 p_val2)
        {
            return p_val1.x == p_val2.x && p_val1.y == p_val2.y & p_val1.z == p_val2.z;
        }

        /// <summary>
        /// Given 3 ints, find the lowest
        /// </summary>
        /// <param name="p_val1">Value 1</param>
        /// <param name="p_val2">Value 2</param>
        /// <param name="p_val3">Value 3</param>
        /// <returns>Lowest of the three</returns>
        private int GetLowestOf3(int p_val1, int p_val2, int p_val3)
        {
            ///    1 smaller then 2, so 1 or 3, 1 smaller then 3 => 1/3    2 smaller, so cant be 1, check for 2 or 3      
            return p_val1 < p_val2 ? (p_val1 < p_val3 ? p_val1 : p_val3) : (p_val2 < p_val3 ? p_val2 : p_val3);
        }

        /// <summary>
        /// Given 3 ints, find the highest
        /// </summary>
        /// <param name="p_val1">Value 1</param>
        /// <param name="p_val2">Value 2</param>
        /// <param name="p_val3">Value 3</param>
        /// <returns>Highest of the three</returns>
        private int GetHighestOf3(int p_val1, int p_val2, int p_val3)
        {
            ///    1 larger then 2, so 1 or 3, 1 larger then 3 => 1/3    2 larger, so cant be 1, check for 2 or 3      
            return p_val1 > p_val2 ? (p_val1 > p_val3 ? p_val1 : p_val3) : (p_val2 > p_val3 ? p_val2 : p_val3);
        }

        /// <summary>
        /// Building of plane equation
        /// Example here http://pi.math.cornell.edu/~froh/231f08e1a.pdf
        /// </summary>
        /// <param name="p_val1"></param>
        /// <param name="p_val2"></param>
        /// <param name="p_val3"></param>
        /// <returns></returns>
        private double4 GetPlaneEquation(int3 p_val1, int3 p_val2, int3 p_val3)
        {
            double3 vector1 = p_val1 - p_val2;
            double3 vector2 = p_val3 - p_val2;

            double3 normal = CrossProduct(vector1, vector2);

            double dValue = normal.x * p_val1.x + normal.y * p_val1.y + normal.z * p_val1.z;

            return new double4(normal, -dValue); //Invert d value to move to other side of equation
        }

        /// <summary>
        /// Get the cross product between 2 vectors
        /// Note: CrossProduct(Val1, Val2) != CrossProduct(Val2, Val1)
        /// Example found here https://www.mathsisfun.com/algebra/vectors-cross-product.html
        /// </summary>
        /// <param name="p_val1">Value 1</param>
        /// <param name="p_val2">Value 2</param>
        /// <returns>Cross product</returns>
        private double3 CrossProduct(double3 p_val1, double3 p_val2)
        {
            // Ay*Bz - Az*By, Az*Bx - Ax*Bz, Ax*By - Ay*Bx
            return new double3(p_val1.y * p_val2.z - p_val1.z * p_val2.y, p_val1.z * p_val2.x - p_val1.x * p_val2.z, p_val1.x * p_val2.y - p_val1.y * p_val2.x);
        }

        /// <summary>
        /// Get the area between 3 points
        /// </summary>
        /// <param name="p_pointA"></param>
        /// <param name="p_pointB"></param>
        /// <param name="p_pointC"></param>
        /// <returns>Total area</returns>
        private double GetArea(double3 p_pointA, double3 p_pointB, double3 p_pointC)
        {
            double3 ABVec = p_pointB - p_pointA;
            double3 ACVec = p_pointC - p_pointA;

            return 0.5 * Magnitude(CrossProduct(ABVec, ACVec));
        }

        /// <summary>
        /// Get the magnitude between two int3's
        /// </summary>
        /// <param name="p_val1"></param>
        /// <returns>Magnitude</returns>
        private double Magnitude(double3 p_val1)
        {
            return math.sqrt(p_val1.x * p_val1.x + p_val1.y * p_val1.y + p_val1.z * p_val1.z);
        }

        /// <summary>
        /// 
        /// example found at https://math.stackexchange.com/a/723966
        /// </summary>
        /// <param name="p_orginalPoint"></param>
        /// <param name="p_planeEquation"></param>
        /// <returns></returns>
        private double3 GetClosestPoint(double3 p_orginalPoint, double4 p_planeEquation, double p_cValue)
        {
            return p_orginalPoint - p_cValue * new double3(p_planeEquation.x, p_planeEquation.y, p_planeEquation.z);
        }

        /// <summary>
        /// Calculate UV based off points distance to each vert
        /// </summary>
        /// <param name="p_pointPos">Point position</param>
        /// <param name="p_vertAGridPos">VertA position</param>
        /// <param name="p_vertBGridPos">VertB position</param>
        /// <param name="p_vertCGridPos">VertC position</param>
        /// <param name="p_UVA">A verts UV</param>
        /// <param name="p_UVB">B verts UV</param>
        /// <param name="p_UVC">C verts UV</param>
        /// <returns>UV basde off distance</returns>
        private double2 CalculateUV(int3 p_pointPos, int3 p_vertAGridPos, int3 p_vertBGridPos, int3 p_vertCGridPos, double2 p_UVA, double2 p_UVB, double2 p_UVC)
        {
            int vertADistance = SqrMagnitude(p_pointPos, p_vertAGridPos);
            int vertBDistance = SqrMagnitude(p_pointPos, p_vertBGridPos);
            int vertCDistance = SqrMagnitude(p_pointPos, p_vertCGridPos);

            int total = vertADistance + vertBDistance + vertCDistance;

            return (double)vertADistance / total * p_UVA + (double)vertBDistance / total * p_UVB + (double)vertCDistance / total * p_UVC;

        }

        /// <summary>
        /// Get the square distance between two int3's
        /// Faster the true distance
        /// </summary>
        /// <param name="p_val1"></param>
        /// <param name="p_val2"></param>
        /// <returns>Square magnitude</returns>
        private int SqrMagnitude(int3 p_val1, int3 p_val2)
        {
            int3 val1ToVal2Vector = p_val2 - p_val1;
            return val1ToVal2Vector.x * val1ToVal2Vector.x + val1ToVal2Vector.y * val1ToVal2Vector.y + val1ToVal2Vector.z * val1ToVal2Vector.z;
        }

        /// <summary>
        /// Convert from Vector2 to double2
        /// </summary>
        /// <param name="p_val">Value to convert</param>
        /// <returns>Converted Value</returns>
        private double2 Convert(Vector2 p_val)
        {
            return new double2(p_val.x, p_val.y);
        }

        /// <summary>
        /// Convert from Vector3 to double3
        /// </summary>
        /// <param name="p_val">Value to convert</param>
        /// <returns>Converted Value</returns>
        private double3 Convert(Vector3 p_val)
        {
            return new double3(p_val.x, p_val.y, p_val.z);
        }
    }                                              

    [BurstCompile]
    private struct IJob_BuildMeshes : IJob
    {
        private const float NORMAL_COMP_LENGHT = 0.5774f;

        public NativeArray<Vector3> m_returnVerts;
        public NativeArray<Vector2> m_returnUVs;
        public NativeArray<int> m_returnTris;
        public NativeArray<Vector3> m_returnNormals;

        [ReadOnly]
        public NativeHashMap<int3, double2> m_passThroughTriData;

        [ReadOnly]
        public float m_voxelSize;
        [ReadOnly]
        public bool m_hardEdges;
        [ReadOnly]
        public int m_voxelsPerMesh;
        [ReadOnly]
        public int m_meshCount;

        private Vector3 m_rightVector;
        private Vector3 m_upVector;
        private Vector3 m_forwardVector;

        //Normal generation
        private Vector3 m_rightNormal;
        private Vector3 m_leftNormal;
        private Vector3 m_upNormal;
        private Vector3 m_downNormal;
        private Vector3 m_forwardNormal;
        private Vector3 m_backwardNormal;

        private Vector3 m_rightUpForwardNormal;
        private Vector3 m_rightUpBackwardNormal;
        private Vector3 m_rightDownForwardNormal;
        private Vector3 m_rightDownBackwardNormal;
        private Vector3 m_leftUpForwardNormal;
        private Vector3 m_leftUpBackwardNormal;
        private Vector3 m_leftDownForwardNormal;
        private Vector3 m_leftDownBackwardNormal;

        public void Execute()
        {
            m_rightVector = new Vector3(m_voxelSize / 2.0f, 0.0f, 0.0f);
            m_upVector = new Vector3(0.0f, m_voxelSize / 2.0f, 0.0f);
            m_forwardVector = new Vector3(0.0f, 0.0f, m_voxelSize / 2.0f);

            if(m_hardEdges)
            {
                m_rightNormal = new Vector3(1.0f, 0.0f, 0.0f);
                m_leftNormal = new Vector3(-1.0f, 0.0f, 0.0f);
                m_upNormal = new Vector3(0.0f, 1.0f, 0.0f);
                m_downNormal = new Vector3(0.0f, -1.0f, 0.0f);
                m_forwardNormal = new Vector3(0.0f, 0.0f, 1.0f);
                m_backwardNormal = new Vector3(0.0f, 0.0f, -1.0f);
            }
            else
            {
                m_rightUpForwardNormal = new Vector3(NORMAL_COMP_LENGHT, NORMAL_COMP_LENGHT, NORMAL_COMP_LENGHT);
                m_rightUpBackwardNormal = new Vector3(NORMAL_COMP_LENGHT, NORMAL_COMP_LENGHT, -NORMAL_COMP_LENGHT);
                m_rightDownForwardNormal = new Vector3(NORMAL_COMP_LENGHT, -NORMAL_COMP_LENGHT, NORMAL_COMP_LENGHT);
                m_rightDownBackwardNormal = new Vector3(NORMAL_COMP_LENGHT, -NORMAL_COMP_LENGHT, -NORMAL_COMP_LENGHT);
                m_leftUpForwardNormal = new Vector3(-NORMAL_COMP_LENGHT, NORMAL_COMP_LENGHT, NORMAL_COMP_LENGHT);
                m_leftUpBackwardNormal = new Vector3(-NORMAL_COMP_LENGHT, NORMAL_COMP_LENGHT, -NORMAL_COMP_LENGHT);
                m_leftDownForwardNormal = new Vector3(-NORMAL_COMP_LENGHT, -NORMAL_COMP_LENGHT, NORMAL_COMP_LENGHT);
                m_leftDownBackwardNormal = new Vector3(-NORMAL_COMP_LENGHT, -NORMAL_COMP_LENGHT, -NORMAL_COMP_LENGHT);
            }

            //Convert queue into unique array
            NativeArray<int3> uniqueVoxelPositions = m_passThroughTriData.GetKeyArray(Allocator.Temp);
            NativeArray<double2> uniqueVoxelUV = m_passThroughTriData.GetValueArray(Allocator.Temp);

            int meshVoxelIndex = 0;
            for (int meshIndex = 0; meshIndex < m_meshCount; meshIndex++)
            {

                for (int voxelIndex = 0; voxelIndex < m_voxelsPerMesh; voxelIndex++, meshVoxelIndex++)
                {
                    if (meshVoxelIndex >= uniqueVoxelPositions.Length)
                        break;

                    BuildVoxel(uniqueVoxelPositions[meshVoxelIndex], uniqueVoxelUV[meshVoxelIndex], voxelIndex, meshIndex);
                }
            }
        }

        /// <summary>
        /// Build the mesh verts/tri/UV data as needed per voxel
        /// </summary>
        /// <param name="p_position">Voxel grid position</param>
        /// <param name="p_UV">Voxel UV</param>
        /// <param name="voxelIndex">Current voxelIndex</param>
        /// <param name="p_meshIndex">meshIndex, used in assigning to multihashmap</param>
        private void BuildVoxel(double3 p_position, double2 p_UV, int voxelIndex, int p_meshIndex)
        {
            Vector3 voxelPos = Convert(p_position);
            Vector2 voxelUV = Convert(p_UV);

            voxelPos *= m_voxelSize;

            int vertsPerVoxel = (m_hardEdges ? VERTS_PER_VOXEL_HARDEDGE : VERTS_PER_VOXEL_SOFTEDGE);

            int vertStartingIndex = p_meshIndex * m_voxelsPerMesh * vertsPerVoxel + voxelIndex * vertsPerVoxel;
            int triArrayStartingIndex = p_meshIndex * m_voxelsPerMesh * 36  + voxelIndex * 36;
            int triVoxelStartIndex = voxelIndex * vertsPerVoxel;
            if (m_hardEdges)
            {
                //build verts, looking at face, start bottom left and move up, right, down. => Bottom Left, Top Left, Top Right, Bottom Right
                //Front face
                m_returnVerts[vertStartingIndex + 0] = voxelPos + (m_rightVector - m_upVector + m_forwardVector);
                m_returnVerts[vertStartingIndex + 1] = voxelPos + (m_rightVector + m_upVector + m_forwardVector);
                m_returnVerts[vertStartingIndex + 2] = voxelPos + (-m_rightVector + m_upVector + m_forwardVector);
                m_returnVerts[vertStartingIndex + 3] = voxelPos + (-m_rightVector - m_upVector + m_forwardVector);

                //Back 
                m_returnVerts[vertStartingIndex + 4] = voxelPos + (-m_rightVector - m_upVector - m_forwardVector);
                m_returnVerts[vertStartingIndex + 5] = voxelPos + (-m_rightVector + m_upVector - m_forwardVector);
                m_returnVerts[vertStartingIndex + 6] = voxelPos + (m_rightVector + m_upVector - m_forwardVector);
                m_returnVerts[vertStartingIndex + 7] = voxelPos + (m_rightVector - m_upVector - m_forwardVector);

                //Right face
                m_returnVerts[vertStartingIndex + 8] = voxelPos + (m_rightVector - m_upVector - m_forwardVector);
                m_returnVerts[vertStartingIndex + 9] = voxelPos + (m_rightVector + m_upVector - m_forwardVector);
                m_returnVerts[vertStartingIndex + 10] = voxelPos + (m_rightVector + m_upVector + m_forwardVector);
                m_returnVerts[vertStartingIndex + 11] = voxelPos + (m_rightVector - m_upVector + m_forwardVector);

                //Left face        
                m_returnVerts[vertStartingIndex + 12] = voxelPos + (-m_rightVector - m_upVector + m_forwardVector);
                m_returnVerts[vertStartingIndex + 13] = voxelPos + (-m_rightVector + m_upVector + m_forwardVector);
                m_returnVerts[vertStartingIndex + 14] = voxelPos + (-m_rightVector + m_upVector - m_forwardVector);
                m_returnVerts[vertStartingIndex + 15] = voxelPos + (-m_rightVector - m_upVector - m_forwardVector);

                //Top face    
                m_returnVerts[vertStartingIndex + 16] = voxelPos + (-m_rightVector + m_upVector - m_forwardVector);
                m_returnVerts[vertStartingIndex + 17] = voxelPos + (-m_rightVector + m_upVector + m_forwardVector);
                m_returnVerts[vertStartingIndex + 18] = voxelPos + (m_rightVector + m_upVector + m_forwardVector);
                m_returnVerts[vertStartingIndex + 19] = voxelPos + (m_rightVector + m_upVector - m_forwardVector);

                //Bottom face      
                m_returnVerts[vertStartingIndex + 20] = voxelPos + (-m_rightVector - m_upVector + m_forwardVector);
                m_returnVerts[vertStartingIndex + 21] = voxelPos + (-m_rightVector - m_upVector - m_forwardVector);
                m_returnVerts[vertStartingIndex + 22] = voxelPos + (m_rightVector - m_upVector - m_forwardVector);
                m_returnVerts[vertStartingIndex + 23] = voxelPos + (m_rightVector - m_upVector + m_forwardVector);

                //Tris, Goes Bottom left, Top Left, Top Right : Bottom Left, Top Right, Bottom Right
                //Minus vert starting index due to having multiple meshes 
                //Front
                m_returnTris[triArrayStartingIndex + 0] = triVoxelStartIndex + 0;
                m_returnTris[triArrayStartingIndex + 1] = triVoxelStartIndex + 1;
                m_returnTris[triArrayStartingIndex + 2] = triVoxelStartIndex + 2;
                                              
                m_returnTris[triArrayStartingIndex + 3] = triVoxelStartIndex + 0;
                m_returnTris[triArrayStartingIndex + 4] = triVoxelStartIndex + 2;
                m_returnTris[triArrayStartingIndex + 5] = triVoxelStartIndex + 3;
                //Back                        
                m_returnTris[triArrayStartingIndex + 6] = triVoxelStartIndex + 4;
                m_returnTris[triArrayStartingIndex + 7] = triVoxelStartIndex + 5;
                m_returnTris[triArrayStartingIndex + 8] = triVoxelStartIndex + 6;

                m_returnTris[triArrayStartingIndex + 9] = triVoxelStartIndex + 4;
                m_returnTris[triArrayStartingIndex + 10] = triVoxelStartIndex + 6;
                m_returnTris[triArrayStartingIndex + 11] = triVoxelStartIndex + 7;
                //Right                       
                m_returnTris[triArrayStartingIndex + 12] = triVoxelStartIndex + 8;
                m_returnTris[triArrayStartingIndex + 13] = triVoxelStartIndex + 9;
                m_returnTris[triArrayStartingIndex + 14] = triVoxelStartIndex + 10;

                m_returnTris[triArrayStartingIndex + 15] = triVoxelStartIndex + 8;
                m_returnTris[triArrayStartingIndex + 16] = triVoxelStartIndex + 10;
                m_returnTris[triArrayStartingIndex + 17] = triVoxelStartIndex + 11;
                //Left                        
                m_returnTris[triArrayStartingIndex + 18] = triVoxelStartIndex + 12;
                m_returnTris[triArrayStartingIndex + 19] = triVoxelStartIndex + 13;
                m_returnTris[triArrayStartingIndex + 20] = triVoxelStartIndex + 14;

                m_returnTris[triArrayStartingIndex + 21] = triVoxelStartIndex + 12;
                m_returnTris[triArrayStartingIndex + 22] = triVoxelStartIndex + 14;
                m_returnTris[triArrayStartingIndex + 23] = triVoxelStartIndex + 15;
                //Top                         
                m_returnTris[triArrayStartingIndex + 24] = triVoxelStartIndex + 16;
                m_returnTris[triArrayStartingIndex + 25] = triVoxelStartIndex + 17;
                m_returnTris[triArrayStartingIndex + 26] = triVoxelStartIndex + 18;

                m_returnTris[triArrayStartingIndex + 27] = triVoxelStartIndex + 16;
                m_returnTris[triArrayStartingIndex + 28] = triVoxelStartIndex + 18;
                m_returnTris[triArrayStartingIndex + 29] = triVoxelStartIndex + 19;
                //Bottom                      
                m_returnTris[triArrayStartingIndex + 30] = triVoxelStartIndex + 20;
                m_returnTris[triArrayStartingIndex + 31] = triVoxelStartIndex + 21;
                m_returnTris[triArrayStartingIndex + 32] = triVoxelStartIndex + 22;

                m_returnTris[triArrayStartingIndex + 33] = triVoxelStartIndex + 20;
                m_returnTris[triArrayStartingIndex + 34] = triVoxelStartIndex + 22;
                m_returnTris[triArrayStartingIndex + 35] = triVoxelStartIndex + 23;

                //UVS
                for (int vertIndex = 0; vertIndex < VERTS_PER_VOXEL_HARDEDGE; vertIndex++)
                {
                    m_returnUVs[vertStartingIndex + vertIndex] = voxelUV;
                }

                //Normals
                //Front
                m_returnNormals[vertStartingIndex + 0] = m_forwardNormal;
                m_returnNormals[vertStartingIndex + 1] = m_forwardNormal;
                m_returnNormals[vertStartingIndex + 2] = m_forwardNormal;
                m_returnNormals[vertStartingIndex + 3] = m_forwardNormal;

                //Back
                m_returnNormals[vertStartingIndex + 4] = m_backwardNormal;
                m_returnNormals[vertStartingIndex + 5] = m_backwardNormal;
                m_returnNormals[vertStartingIndex + 6] = m_backwardNormal;
                m_returnNormals[vertStartingIndex + 7] = m_backwardNormal;

                //Right
                m_returnNormals[vertStartingIndex + 8] = m_rightNormal;
                m_returnNormals[vertStartingIndex + 9] = m_rightNormal;
                m_returnNormals[vertStartingIndex + 10] = m_rightNormal;
                m_returnNormals[vertStartingIndex + 11] = m_rightNormal;

                //Left
                m_returnNormals[vertStartingIndex + 12] = m_leftNormal;
                m_returnNormals[vertStartingIndex + 13] = m_leftNormal;
                m_returnNormals[vertStartingIndex + 14] = m_leftNormal;
                m_returnNormals[vertStartingIndex + 15] = m_leftNormal;

                //Up
                m_returnNormals[vertStartingIndex + 16] = m_upNormal;
                m_returnNormals[vertStartingIndex + 17] = m_upNormal;
                m_returnNormals[vertStartingIndex + 18] = m_upNormal;
                m_returnNormals[vertStartingIndex + 19] = m_upNormal;

                //Down
                m_returnNormals[vertStartingIndex + 20] = m_downNormal;
                m_returnNormals[vertStartingIndex + 21] = m_downNormal;
                m_returnNormals[vertStartingIndex + 22] = m_downNormal;
                m_returnNormals[vertStartingIndex + 23] = m_downNormal;
            }
            else
            {
                //build verts, looking at Front then back face, start bottom left and move up, right, down. => Bottom Left, Top Left, Top Right, Bottom Right
                m_returnVerts[vertStartingIndex + 0] = voxelPos + (m_rightVector - m_upVector + m_forwardVector);
                m_returnVerts[vertStartingIndex + 1] = voxelPos + (m_rightVector + m_upVector + m_forwardVector);
                m_returnVerts[vertStartingIndex + 2] = voxelPos + (-m_rightVector + m_upVector + m_forwardVector);
                m_returnVerts[vertStartingIndex + 3] = voxelPos + (-m_rightVector - m_upVector + m_forwardVector);

                m_returnVerts[vertStartingIndex + 4] = voxelPos + (-m_rightVector - m_upVector - m_forwardVector);
                m_returnVerts[vertStartingIndex + 5] = voxelPos + (-m_rightVector + m_upVector - m_forwardVector);
                m_returnVerts[vertStartingIndex + 6] = voxelPos + (m_rightVector + m_upVector - m_forwardVector);
                m_returnVerts[vertStartingIndex + 7] = voxelPos + (m_rightVector - m_upVector - m_forwardVector);

                //Tris, Goes Bottom left, Top Left, Top Right : Bottom Left, Top Right, Bottom Right
                //Front
                m_returnTris[triArrayStartingIndex + 0] = triVoxelStartIndex + 0;
                m_returnTris[triArrayStartingIndex + 1] = triVoxelStartIndex + 1;
                m_returnTris[triArrayStartingIndex + 2] = triVoxelStartIndex + 2;
                                                          
                m_returnTris[triArrayStartingIndex + 3] = triVoxelStartIndex + 0;
                m_returnTris[triArrayStartingIndex + 4] = triVoxelStartIndex + 2;
                m_returnTris[triArrayStartingIndex + 5] = triVoxelStartIndex + 3;
                                                          
                //Back                                    
                m_returnTris[triArrayStartingIndex + 6] = triVoxelStartIndex + 4;
                m_returnTris[triArrayStartingIndex + 7] = triVoxelStartIndex + 5;
                m_returnTris[triArrayStartingIndex + 8] = triVoxelStartIndex + 6;
                                                          
                m_returnTris[triArrayStartingIndex + 9] = triVoxelStartIndex + 4;
                m_returnTris[triArrayStartingIndex + 10] = triVoxelStartIndex + 6;
                m_returnTris[triArrayStartingIndex + 11] = triVoxelStartIndex + 7;
                //Right                                    
                m_returnTris[triArrayStartingIndex + 12] = triVoxelStartIndex + 7;
                m_returnTris[triArrayStartingIndex + 13] = triVoxelStartIndex + 6;
                m_returnTris[triArrayStartingIndex + 14] = triVoxelStartIndex + 1;
                                                           
                m_returnTris[triArrayStartingIndex + 15] = triVoxelStartIndex + 7;
                m_returnTris[triArrayStartingIndex + 16] = triVoxelStartIndex + 1;
                m_returnTris[triArrayStartingIndex + 17] = triVoxelStartIndex + 0;
                //Left                                     
                m_returnTris[triArrayStartingIndex + 18] = triVoxelStartIndex + 3;
                m_returnTris[triArrayStartingIndex + 19] = triVoxelStartIndex + 2;
                m_returnTris[triArrayStartingIndex + 20] = triVoxelStartIndex + 5;
                                                           
                m_returnTris[triArrayStartingIndex + 21] = triVoxelStartIndex + 3;
                m_returnTris[triArrayStartingIndex + 22] = triVoxelStartIndex + 5;
                m_returnTris[triArrayStartingIndex + 23] = triVoxelStartIndex + 4;
                //Top                                      
                m_returnTris[triArrayStartingIndex + 24] = triVoxelStartIndex + 5;
                m_returnTris[triArrayStartingIndex + 25] = triVoxelStartIndex + 2;
                m_returnTris[triArrayStartingIndex + 26] = triVoxelStartIndex + 1;
                                                           
                m_returnTris[triArrayStartingIndex + 27] = triVoxelStartIndex + 5;
                m_returnTris[triArrayStartingIndex + 28] = triVoxelStartIndex + 1;
                m_returnTris[triArrayStartingIndex + 29] = triVoxelStartIndex + 6;
                                                           
                //Bottom                                   
                m_returnTris[triArrayStartingIndex + 30] = triVoxelStartIndex + 3;
                m_returnTris[triArrayStartingIndex + 31] = triVoxelStartIndex + 4;
                m_returnTris[triArrayStartingIndex + 32] = triVoxelStartIndex + 7;
                                                           
                m_returnTris[triArrayStartingIndex + 33] = triVoxelStartIndex + 3;
                m_returnTris[triArrayStartingIndex + 34] = triVoxelStartIndex + 7;
                m_returnTris[triArrayStartingIndex + 35] = triVoxelStartIndex + 0;

                //UVS
                for (int vertIndex = 0; vertIndex < VERTS_PER_VOXEL_SOFTEDGE; vertIndex++)
                {
                    m_returnUVs[vertStartingIndex + vertIndex] = voxelUV;
                }

                //Normals
                //Front Face
                m_returnNormals[vertStartingIndex + 0] = m_rightDownForwardNormal; //RDF
                m_returnNormals[vertStartingIndex + 1] = m_rightUpForwardNormal; //RUF
                m_returnNormals[vertStartingIndex + 2] = m_leftUpForwardNormal; //LUF
                m_returnNormals[vertStartingIndex + 3] = m_leftDownForwardNormal; //LDF

                //Back Face
                m_returnNormals[vertStartingIndex + 4] = m_leftDownBackwardNormal; //LDB
                m_returnNormals[vertStartingIndex + 5] = m_leftUpBackwardNormal; //LUB
                m_returnNormals[vertStartingIndex + 6] = m_rightUpBackwardNormal; //RUB
                m_returnNormals[vertStartingIndex + 7] = m_rightDownBackwardNormal; //RDB
            }
        }

        /// <summary>
        /// Convert from double2 to Vector2
        /// </summary>
        /// <param name="p_val">Value to convert</param>
        /// <returns>Converted Value</returns>
        private Vector2 Convert(double2 p_val)
        {
            return new Vector2((float)p_val.x, (float)p_val.y);
        }

        /// <summary>
        /// Convert from double3 to Vector3
        /// </summary>
        /// <param name="p_val">Value to convert</param>
        /// <returns>Converted Value</returns>
        private Vector3 Convert(double3 p_val)
        {
            return new Vector3((float)p_val.x, (float)p_val.y, (float)p_val.z);
        }
    }

#endregion

#region Mesh Functions
    /// <summary>
    /// Attempt to get the mesh on a given object
    /// </summary>
    /// <param name="p_objectWithMesh">Object that should contain the mesh</param>
    /// <returns>mesh found, null when no mesh found</returns>
    private Mesh GetMesh(GameObject p_objectWithMesh)
    {
        if (p_objectWithMesh == null)
            return null;

        MeshFilter meshFilter = p_objectWithMesh.GetComponent<MeshFilter>();
        if(meshFilter != null)
        {
            return meshFilter.mesh;
        }
        SkinnedMeshRenderer skinnedMeshRenderer = p_objectWithMesh.GetComponent<SkinnedMeshRenderer>();
        if (skinnedMeshRenderer != null)
        {
            Mesh bakedMesh = new Mesh();
            skinnedMeshRenderer.BakeMesh(bakedMesh);
            return bakedMesh;
        }
        return null;
    }

    /// <summary>
    /// Attempt to get the mesh on a given object
    /// </summary>
    /// <param name="p_objectWithMesh">Object that should contain the material</param>
    /// <returns>mesh found, null when no mesh found</returns>
    private Material GetMaterial(GameObject p_objectWithMesh)
    {
        if (p_objectWithMesh == null)
            return null;

        MeshRenderer meshRenderer = p_objectWithMesh.GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            return meshRenderer.material;
        }
        else
        {
            SkinnedMeshRenderer skinnedMeshRenderer = p_objectWithMesh.GetComponent<SkinnedMeshRenderer>();
            if (skinnedMeshRenderer != null)
            {
                return skinnedMeshRenderer.material;
            }
        }

        return null;
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
            meshRenderer.materials = new Material[0];
        }
        else
        {
            SkinnedMeshRenderer skinnedMeshRenderer = p_object.GetComponent<SkinnedMeshRenderer>();
            if (skinnedMeshRenderer != null)
            {
                skinnedMeshRenderer.materials = new Material[0];
            }
        }
    }
    #endregion

#region Native Maths

    /// <summary>
    /// Convert Array to NativeArray
    /// See here for full example "https://gist.github.com/LotteMakesStuff/c2f9b764b15f74d14c00ceb4214356b4"
    /// </summary>
    /// <param name="p_fillingArray">Native array to fill</param>
    /// <param name="p_filledFromArray">Array filling from. Should be same size</param>
    unsafe private static void Convert(NativeArray<Vector3> p_fillingArray, Vector3[] p_filledFromArray)
    {
        fixed (void* vertexBufferPointer = p_filledFromArray)
        {
            // ...and use memcpy to copy the Vector3[] into a NativeArray<floar3> without casting. whould be fast!
            UnsafeUtility.MemCpy(NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(p_fillingArray), vertexBufferPointer, p_filledFromArray.Length * (long)UnsafeUtility.SizeOf<float3>());
        }
    }

    /// <summary>
    /// Convert Array to NativeArray
    /// See here for full example "https://gist.github.com/LotteMakesStuff/c2f9b764b15f74d14c00ceb4214356b4"
    /// </summary>
    /// <param name="p_fillingArray">Native array to fill</param>
    /// <param name="p_filledFromArray">Array filling from. Should be same size</param>
    unsafe private static void Convert(NativeArray<int> p_fillingArray, int[] p_filledFromArray)
    {
        fixed (void* vertexBufferPointer = p_filledFromArray)
        {
            // ...and use memcpy to copy the Vector3[] into a NativeArray<floar3> without casting. whould be fast!
            UnsafeUtility.MemCpy(NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(p_fillingArray), vertexBufferPointer, p_filledFromArray.Length * (long)UnsafeUtility.SizeOf<int>());
        }
    }

    /// <summary>
    /// Convert Array to NativeArray
    /// See here for full example "https://gist.github.com/LotteMakesStuff/c2f9b764b15f74d14c00ceb4214356b4"
    /// </summary>
    /// <param name="p_fillingArray">Native array to fill</param>
    /// <param name="p_filledFromArray">Array filling from. Should be same size</param>
    unsafe private static void Convert(NativeArray<Vector2> p_fillingArray, Vector2[] p_filledFromArray)
    {
        fixed (void* vertexBufferPointer = p_filledFromArray)
        {
            // ...and use memcpy to copy the Vector3[] into a NativeArray<floar3> without casting. whould be fast!
            UnsafeUtility.MemCpy(NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(p_fillingArray), vertexBufferPointer, p_filledFromArray.Length * (long)UnsafeUtility.SizeOf<float2>());
        }
    }

    /// <summary>
    /// Convert Matrix4x4 to double4x4
    /// </summary>
    /// <param name="p_floatMatrix">Matrix4x4 to convert</param>
    /// <returns>double4x4 with values copied over</returns>
    private static double4x4 FloatMatrixToDoubleMatrix(Matrix4x4 p_floatMatrix)
    {
        double4x4 doubleMatrix = new double4x4(0.0);

        for (int rowIndex = 0; rowIndex < 4; rowIndex++)
        {
            for (int colIndex = 0; colIndex < 4; colIndex++)
            {
                doubleMatrix[rowIndex][colIndex] = p_floatMatrix[colIndex, rowIndex];
            }
        }

        return doubleMatrix;
    }
#endregion
}

#endif
