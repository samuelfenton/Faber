#if VOXELISER_MATHEMATICS_ENABLED && VOXELISER_BURST_ENABLED && VOXELISER_COLLECTIONS_ENABLED

using System.Collections;
using UnityEngine;
using Unity.Jobs;
using UnityEngine.Jobs;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Collections;

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

    private bool m_editorBeenChangedFlag = true;
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

    private NativeArray<double3> m_nativeReturnVerts;
    private NativeArray<int> m_nativeReturnTris;
    private NativeArray<double2> m_nativeReturnUVs;

    private NativeArray<double3> m_nativeVerts;
    private NativeArray<int> m_nativeTris;
    private NativeArray<double2> m_nativeUVs;

    private JobHandle m_calcTrisJobHandle;
    private JobHandle m_buildMeshJobHandle;
    //End

    //Mesh stuff
    private GameObject m_voxelisedObject = null;
    private Mesh m_originalMesh = null;

    private SkinnedMeshRenderer m_skinnedMeshRenderer = null;

    private Mesh[] m_voxelisedMeshes;

    //End
    private void Start()
    {
        if (m_objectWithMesh == null)
            m_objectWithMesh = gameObject;

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
        m_nativeVerts = new NativeArray<double3>(verts.Length, Allocator.Persistent);
        m_nativeTris = new NativeArray<int>(tris.Length, Allocator.Persistent);
        m_nativeUVs = new NativeArray<double2>(UVs.Length, Allocator.Persistent);

        m_nativePassThroughTriData = new NativeHashMap<int3, double2>(MAX_VOXEL_COUNT, Allocator.Persistent);

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

        m_nativeReturnVerts = new NativeArray<double3>(m_storedIsHardEdge ? maxPossibleVoxels * VERTS_PER_VOXEL_HARDEDGE : maxPossibleVoxels * VERTS_PER_VOXEL_SOFTEDGE, Allocator.Persistent);
        m_nativeReturnUVs = new NativeArray<double2>(m_storedIsHardEdge ? maxPossibleVoxels * VERTS_PER_VOXEL_HARDEDGE : maxPossibleVoxels * VERTS_PER_VOXEL_SOFTEDGE, Allocator.Persistent);
        m_nativeReturnTris = new NativeArray<int>(maxPossibleVoxels * TRIS_PER_VOXEL, Allocator.Persistent);

        IJob_BuildMeshes buildMeshJob = new IJob_BuildMeshes
        {
            m_passThroughTriData = m_nativePassThroughTriData,
            m_returnVerts = m_nativeReturnVerts,
            m_returnTris = m_nativeReturnTris,
            m_returnUVs = m_nativeReturnUVs,
            m_voxelSize = m_storedVoxelSize,
            m_hardEdges = m_storedIsHardEdge,
            m_voxelsPerMesh = voxelsPerMesh,
            m_meshCount = m_meshesToUse
        };

        m_buildMeshJobHandle = buildMeshJob.Schedule();

        m_buildMeshJobHandle.Complete();

        double3[] allDoubleVerts = m_nativeReturnVerts.ToArray();
        double2[] allDoubleUVs = m_nativeReturnUVs.ToArray();
        int[] allTris = m_nativeReturnTris.ToArray();

        Vector3[] meshVerts = new Vector3[voxelsPerMesh * (m_storedIsHardEdge ? VERTS_PER_VOXEL_HARDEDGE : VERTS_PER_VOXEL_SOFTEDGE)];
        Vector2[] meshUVs = new Vector2[voxelsPerMesh * (m_storedIsHardEdge ? VERTS_PER_VOXEL_HARDEDGE : VERTS_PER_VOXEL_SOFTEDGE)];
        int[] meshTris = new int[voxelsPerMesh * 36];

        int vertsPerVoxel = (m_storedIsHardEdge ? VERTS_PER_VOXEL_HARDEDGE : VERTS_PER_VOXEL_SOFTEDGE);
        int vertsPerMesh = vertsPerVoxel * voxelsPerMesh;
        int trisPerMesh = 36 * voxelsPerMesh;

        //Get voxel
        for (int meshIndex = 0; meshIndex < m_meshesToUse; meshIndex++)
        {
            m_voxelisedMeshes[meshIndex].Clear();
            
            int vertStartingIndex = meshIndex * vertsPerMesh;
            int triStartingIndex = meshIndex * trisPerMesh;

            for (int vertIndex = 0; vertIndex < vertsPerMesh; vertIndex++)
            {
                meshVerts[vertIndex] = Convert(allDoubleVerts[vertStartingIndex + vertIndex]);
                meshUVs[vertIndex] = Convert(allDoubleUVs[vertStartingIndex + vertIndex]);
            }

            for (int triIndex = 0; triIndex < trisPerMesh; triIndex++)
            {
                meshTris[triIndex] = allTris[triStartingIndex + triIndex] - vertStartingIndex;
            }

            m_voxelisedMeshes[meshIndex].vertices = meshVerts;
            m_voxelisedMeshes[meshIndex].uv = meshUVs;
            m_voxelisedMeshes[meshIndex].triangles = meshTris;

            m_voxelisedMeshes[meshIndex].RecalculateNormals();
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
        public NativeHashMap<int3, double2>.ParallelWriter m_passThroughTriData;

        [ReadOnly]
        public NativeArray<double3> m_nativeVerts;
        [ReadOnly]
        public NativeArray<int> m_nativeTris;
        [ReadOnly]
        public NativeArray<double2> m_nativeUVs;
        [ReadOnly]
        public double4x4 m_transformMatrix;
        [ReadOnly]
        public double m_voxelSize;

        public void Execute(int index)
        {
            //Setup transform matrix
            double4 vertA = math.mul(m_transformMatrix, new double4(m_nativeVerts[m_nativeTris[index * 3]], 1.0f));
            double4 vertB = math.mul(m_transformMatrix, new double4(m_nativeVerts[m_nativeTris[index * 3 + 1]], 1.0f));
            double4 vertC = math.mul(m_transformMatrix, new double4(m_nativeVerts[m_nativeTris[index * 3 + 2]], 1.0f));

            vertA = SnapToIncrement(vertA);
            vertB = SnapToIncrement(vertB);
            vertC = SnapToIncrement(vertC);

            double2 UVA = m_nativeUVs[m_nativeTris[index * 3]];
            double2 UVB = m_nativeUVs[m_nativeTris[index * 3 + 1]];
            double2 UVC = m_nativeUVs[m_nativeTris[index * 3 + 2]];

            int3 vertAGridPos = PositionToGridPosition(vertA);
            int3 vertBGridPos = PositionToGridPosition(vertB);
            int3 vertCGridPos = PositionToGridPosition(vertC);

            //Build line from Vert A to Vert B, as verts on line are found, build line from line vert to Vert C
            int3 ABVector = new int3(vertBGridPos.x - vertAGridPos.x, vertBGridPos.y - vertAGridPos.y, vertBGridPos.z - vertAGridPos.z);

            if (ABVector.x == 0 && ABVector.y == 0 && ABVector.z == 0)
            {
                m_passThroughTriData.TryAdd(vertAGridPos, UVA);
                return;
            }

            int3 absABVector = math.abs(ABVector);

            int steps = absABVector.x > absABVector.y ? (absABVector.x > absABVector.z ? absABVector.x : absABVector.z) : (absABVector.y > absABVector.z ? absABVector.y : absABVector.z); //Get largest out of 3 values
            int3 currentPathOnABVector = int3.zero;

            int3 signedABVector = new int3(ABVector.x >= 0 ? 1 : -1, ABVector.y >= 0 ? 1 : -1, ABVector.z >= 0 ? 1 : -1);
            double3 increaseThreshold = new double3((double)absABVector.x / steps, (double)absABVector.y / steps, (double)absABVector.z / steps);
            double3 currentThreshold = new double3(0.5, 0.5, 0.5); //Added in 0.5 as voxel should be placed in center of grid

            BuildVertOnLineToVertCEdge(vertAGridPos, vertCGridPos, UVA, UVC);

            //Move along the line
            //Steps will move on "grid" every time
            //Use threshold to dertermine when youve translated to next grid square
            for (int alongGridIndex = 1; alongGridIndex < steps; alongGridIndex++)
            {
                currentThreshold.x += increaseThreshold.x;
                if (currentThreshold.x >= 1)
                {
                    currentPathOnABVector.x += signedABVector.x;
                    currentThreshold.x -= 1.0;
                }

                currentThreshold.y += increaseThreshold.y;
                if (currentThreshold.y >= 1)
                {
                    currentPathOnABVector.y += signedABVector.y;
                    currentThreshold.y -= 1.0;
                }

                currentThreshold.z += increaseThreshold.z;
                if (currentThreshold.z >= 1)
                {
                    currentPathOnABVector.z += signedABVector.z;
                    currentThreshold.z -= 1.0;
                }

                BuildVertOnLineToVertCEdge(vertAGridPos + currentPathOnABVector, vertCGridPos, MergeUV(UVA, UVB, (double)alongGridIndex / steps), UVC);
            }
        }

        /// <summary>
        /// Build a queue for the line form VertA to vertB
        /// </summary>
        /// <param name="p_vertOnLine">Vert found on line</param>
        /// <param name="p_vertC">VertC</param>
        /// <param name="p_voxelSize">Size of a voxel</param>
        private void BuildVertOnLineToVertCEdge(int3 p_vertOnLine, int3 p_vertC, double2 p_vertOnLineUV, double2 p_vertCUV)
        {
            //Build line from Vert A to Vert B, as verts on line are found, build line from line vert to Vert C
            int3 lineToCVector = new int3(p_vertC.x - p_vertOnLine.x, p_vertC.y - p_vertOnLine.y, p_vertC.z - p_vertOnLine.z);

            if (lineToCVector.x == 0 && lineToCVector.y == 0 && lineToCVector.z == 0)
            {
                m_passThroughTriData.TryAdd(p_vertOnLine, p_vertOnLineUV);
                return;
            }

            int3 absLineToCVector = math.abs(lineToCVector);

            int steps = absLineToCVector.x > absLineToCVector.y ? (absLineToCVector.x > absLineToCVector.z ? absLineToCVector.x : absLineToCVector.z) : (absLineToCVector.y > absLineToCVector.z ? absLineToCVector.y : absLineToCVector.z); //Get largest out of 3 values
            int3 currentPathOnLineToCVector = int3.zero;

            int3 signedLineToCVector = new int3(lineToCVector.x >= 0 ? 1 : -1, lineToCVector.y >= 0 ? 1 : -1, lineToCVector.z >= 0 ? 1 : -1);
            double3 increaseThreshold = new double3((double)absLineToCVector.x / steps, (double)absLineToCVector.y / steps, (double)absLineToCVector.z / steps);
            double3 currentThreshold = new double3(0.5, 0.5, 0.5); //Added in 0.5 as voxel should be placed in center of grid

            m_passThroughTriData.TryAdd(p_vertOnLine, p_vertOnLineUV);
            
            //Move along the line
            //Steps will move on "grid" every time
            //Use threshold to dertermine when youve translated to next grid square
            for (int alongGridIndex = 1; alongGridIndex < steps; alongGridIndex++)
            {
                currentThreshold.x += increaseThreshold.x;
                if (currentThreshold.x >= 1)
                {
                    currentPathOnLineToCVector.x += signedLineToCVector.x ;
                    currentThreshold.x -= 1.0;
                }

                currentThreshold.y += increaseThreshold.y;
                if (currentThreshold.y >= 1)
                {
                    currentPathOnLineToCVector.y += signedLineToCVector.y;
                    currentThreshold.y -= 1.0;
                }

                currentThreshold.z += increaseThreshold.z;
                if (currentThreshold.z >= 1)
                {
                    currentPathOnLineToCVector.z += signedLineToCVector.z;
                    currentThreshold.z -= 1.0;
                }

                m_passThroughTriData.TryAdd(p_vertOnLine + currentPathOnLineToCVector, MergeUV(p_vertOnLineUV, p_vertCUV, (double)alongGridIndex / steps));
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
        /// Merge two given UVs based off how much one is from another
        /// </summary>
        /// <param name="p_UVA">UV for vertA</param>
        /// <param name="p_UVB">UV for vertB</param>
        /// <param name="p_percent">Percent through towards UVB</param>
        /// <returns></returns>
        private double2 MergeUV(double2 p_UVA, double2 p_UVB, double p_percent)
        {
            double2 UVVector = p_UVB - p_UVA;

            return p_UVA + UVVector * p_percent;
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
    }

    [BurstCompile]
    private struct IJob_BuildMeshes : IJob
    {
        public NativeArray<double3> m_returnVerts;
        public NativeArray<double2> m_returnUVs;
        public NativeArray<int> m_returnTris;
        
        [ReadOnly]
        public NativeHashMap<int3, double2> m_passThroughTriData;

        [ReadOnly]
        public double m_voxelSize;
        [ReadOnly]
        public bool m_hardEdges;
        [ReadOnly]
        public int m_voxelsPerMesh;
        [ReadOnly]
        public int m_meshCount;

        private double3 m_rightVector;
        private double3 m_upVector;
        private double3 m_forwardVector;

        public void Execute()
        {
            m_rightVector = new double3(m_voxelSize / 2.0, 0.0, 0.0);
            m_upVector = new double3(0.0, m_voxelSize / 2.0, 0.0);
            m_forwardVector = new double3(0.0, 0.0, m_voxelSize / 2.0);

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
            double3 voxelPos = p_position;
            voxelPos *= m_voxelSize;

            int vertsPerVoxel = (m_hardEdges ? VERTS_PER_VOXEL_HARDEDGE : VERTS_PER_VOXEL_SOFTEDGE);

            int vertStartingIndex = p_meshIndex * m_voxelsPerMesh * vertsPerVoxel + voxelIndex * vertsPerVoxel;
            int triStartingIndex = p_meshIndex * m_voxelsPerMesh * 36  + voxelIndex * 36;

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
                //Front
                m_returnTris[triStartingIndex + 0] = vertStartingIndex + 0;
                m_returnTris[triStartingIndex + 1] = vertStartingIndex + 1;
                m_returnTris[triStartingIndex + 2] = vertStartingIndex + 2;
                                              
                m_returnTris[triStartingIndex + 3] = vertStartingIndex + 0;
                m_returnTris[triStartingIndex + 4] = vertStartingIndex + 2;
                m_returnTris[triStartingIndex + 5] = vertStartingIndex + 3;
                //Back                        
                m_returnTris[triStartingIndex + 6] = vertStartingIndex + 4;
                m_returnTris[triStartingIndex + 7] = vertStartingIndex + 5;
                m_returnTris[triStartingIndex + 8] = vertStartingIndex + 6;

                m_returnTris[triStartingIndex + 9] = vertStartingIndex + 4;
                m_returnTris[triStartingIndex + 10] = vertStartingIndex + 6;
                m_returnTris[triStartingIndex + 11] = vertStartingIndex + 7;
                //Right                       
                m_returnTris[triStartingIndex + 12] = vertStartingIndex + 8;
                m_returnTris[triStartingIndex + 13] = vertStartingIndex + 9;
                m_returnTris[triStartingIndex + 14] = vertStartingIndex + 10;

                m_returnTris[triStartingIndex + 15] = vertStartingIndex + 8;
                m_returnTris[triStartingIndex + 16] = vertStartingIndex + 10;
                m_returnTris[triStartingIndex + 17] = vertStartingIndex + 11;
                //Left                        
                m_returnTris[triStartingIndex + 18] = vertStartingIndex + 12;
                m_returnTris[triStartingIndex + 19] = vertStartingIndex + 13;
                m_returnTris[triStartingIndex + 20] = vertStartingIndex + 14;

                m_returnTris[triStartingIndex + 21] = vertStartingIndex + 12;
                m_returnTris[triStartingIndex + 22] = vertStartingIndex + 14;
                m_returnTris[triStartingIndex + 23] = vertStartingIndex + 15;
                //Top                         
                m_returnTris[triStartingIndex + 24] = vertStartingIndex + 16;
                m_returnTris[triStartingIndex + 25] = vertStartingIndex + 17;
                m_returnTris[triStartingIndex + 26] = vertStartingIndex + 18;

                m_returnTris[triStartingIndex + 27] = vertStartingIndex + 16;
                m_returnTris[triStartingIndex + 28] = vertStartingIndex + 18;
                m_returnTris[triStartingIndex + 29] = vertStartingIndex + 19;
                //Bottom                      
                m_returnTris[triStartingIndex + 30] = vertStartingIndex + 20;
                m_returnTris[triStartingIndex + 31] = vertStartingIndex + 21;
                m_returnTris[triStartingIndex + 32] = vertStartingIndex + 22;

                m_returnTris[triStartingIndex + 33] = vertStartingIndex + 20;
                m_returnTris[triStartingIndex + 34] = vertStartingIndex + 22;
                m_returnTris[triStartingIndex + 35] = vertStartingIndex + 23;

                //UVS
                for (int vertIndex = 0; vertIndex < VERTS_PER_VOXEL_HARDEDGE; vertIndex++)
                {
                    m_returnUVs[vertStartingIndex + vertIndex] = p_UV;
                }
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
                m_returnTris[triStartingIndex + 0] = vertStartingIndex + 0;
                m_returnTris[triStartingIndex + 1] = vertStartingIndex + 1;
                m_returnTris[triStartingIndex + 2] = vertStartingIndex + 2;
                                              
                m_returnTris[triStartingIndex + 3] = vertStartingIndex + 0;
                m_returnTris[triStartingIndex + 4] = vertStartingIndex + 2;
                m_returnTris[triStartingIndex + 5] = vertStartingIndex + 3;

                //Back
                m_returnTris[triStartingIndex + 6] = vertStartingIndex + 4;
                m_returnTris[triStartingIndex + 7] = vertStartingIndex + 5;
                m_returnTris[triStartingIndex + 8] = vertStartingIndex + 6;

                m_returnTris[triStartingIndex + 9] = vertStartingIndex + 4;
                m_returnTris[triStartingIndex + 10] = vertStartingIndex + 6;
                m_returnTris[triStartingIndex + 11] = vertStartingIndex + 7;
                //Right                               
                m_returnTris[triStartingIndex + 12] = vertStartingIndex + 7;
                m_returnTris[triStartingIndex + 13] = vertStartingIndex + 6;
                m_returnTris[triStartingIndex + 14] = vertStartingIndex + 1;
                                                      
                m_returnTris[triStartingIndex + 15] = vertStartingIndex + 7;
                m_returnTris[triStartingIndex + 16] = vertStartingIndex + 1;
                m_returnTris[triStartingIndex + 17] = vertStartingIndex + 0;
                //Left                                
                m_returnTris[triStartingIndex + 18] = vertStartingIndex + 3;
                m_returnTris[triStartingIndex + 19] = vertStartingIndex + 2;
                m_returnTris[triStartingIndex + 20] = vertStartingIndex + 5;
                                                      
                m_returnTris[triStartingIndex + 21] = vertStartingIndex + 3;
                m_returnTris[triStartingIndex + 22] = vertStartingIndex + 5;
                m_returnTris[triStartingIndex + 23] = vertStartingIndex + 4;
                //Top                    
                m_returnTris[triStartingIndex + 24] = vertStartingIndex + 5;
                m_returnTris[triStartingIndex + 25] = vertStartingIndex + 2;
                m_returnTris[triStartingIndex + 26] = vertStartingIndex + 1;

                m_returnTris[triStartingIndex + 27] = vertStartingIndex + 5;
                m_returnTris[triStartingIndex + 28] = vertStartingIndex + 1;
                m_returnTris[triStartingIndex + 29] = vertStartingIndex + 6;
                                                      
                //Bottom                              
                m_returnTris[triStartingIndex + 30] = vertStartingIndex + 3;
                m_returnTris[triStartingIndex + 31] = vertStartingIndex + 4;
                m_returnTris[triStartingIndex + 32] = vertStartingIndex + 7;
                                                      
                m_returnTris[triStartingIndex + 33] = vertStartingIndex + 3;
                m_returnTris[triStartingIndex + 34] = vertStartingIndex + 7;
                m_returnTris[triStartingIndex + 35] = vertStartingIndex + 0;

                //UVS
                for (int vertIndex = 0; vertIndex < VERTS_PER_VOXEL_SOFTEDGE; vertIndex++)
                {
                    m_returnUVs[vertStartingIndex + vertIndex] = p_UV;
                }
            }
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
    /// Convert Array to NativeArray, float3 type
    /// </summary>
    /// <param name="p_fillingArray">Native array to fill</param>
    /// <param name="p_filledFromArray">Array filling from. Should be same size</param>
    private static void Convert(NativeArray<double3> p_fillingArray, Vector3[] p_filledFromArray)
    {
        for (int arrayIndex = 0; arrayIndex < p_filledFromArray.Length; arrayIndex++)
        {
            Vector3 currentArrayVal = p_filledFromArray[arrayIndex];
            p_fillingArray[arrayIndex] = new double3(currentArrayVal.x, currentArrayVal.y, currentArrayVal.z);
        }
    }

    /// <summary>
    /// Convert Array to NativeArray, int type
    /// </summary>
    /// <param name="p_fillingArray">Native array to fill</param>
    /// <param name="p_filledFromArray">Array filling from. Should be same size</param>
    private static void Convert(NativeArray<int> p_fillingArray, int[] p_filledFromArray)
    {
        for (int arrayIndex = 0; arrayIndex < p_filledFromArray.Length; arrayIndex++)
        {
            p_fillingArray[arrayIndex] = p_filledFromArray[arrayIndex];
        }
    }

    /// <summary>
    /// Convert Array to NativeArray, double2 type
    /// </summary>
    /// <param name="p_fillingArray">Native array to fill</param>
    /// <param name="p_filledFromArray">Array filling from. Should be same size</param>
    private static void Convert(NativeArray<double2> p_fillingArray, Vector2[] p_filledFromArray)
    {
        for (int arrayIndex = 0; arrayIndex < p_filledFromArray.Length; arrayIndex++)
        {
            Vector2 currentArrayVal = p_filledFromArray[arrayIndex];
            p_fillingArray[arrayIndex] = new double2(currentArrayVal.x, currentArrayVal.y);
        }
    }

    private static Vector3 Convert(double3 p_val)
    {
        return new Vector3((float)p_val.x, (float)p_val.y, (float)p_val.z);
    }

    private static Vector2 Convert(double2 p_val)
    {
        return new Vector2((float)p_val.x, (float)p_val.y);
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
