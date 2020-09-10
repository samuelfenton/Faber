using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[ExecuteAlways]
public class Voxeliser : MonoBehaviour
{

    private const int MAX_MESH_COUNT = 65535; //Mesh can have 65535 verts

    private const int HARDEDGE_VERTS_PER_VOXEL = 24; //How many verts on a single voxel for a hard edge
    private const int SOFTEDGE_VERTS_PER_VOXEL = 8; //How many verts on a single voxel for a soft edge

    private const int MAX_MESH_VOXEL_HARDEDGE = MAX_MESH_COUNT / HARDEDGE_VERTS_PER_VOXEL;
    private const int MAX_MESH_VOXEL_SOFTEDGE = MAX_MESH_COUNT / SOFTEDGE_VERTS_PER_VOXEL;

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
    [Tooltip("Gameobject which holds the mesh for the voxeliser, with none assigned, assumption is mesh is on script gameobject")]
    public GameObject m_objectWithMesh = null;
    [Tooltip("How many meshes should exist, used for large meshes")]
    public int m_totalMeshCount = 1;
    public enum VERT_TYPE { HARD_EDGE, SOFT_EDGE }
    [Tooltip("What kind of verts will be used?")]
    public VERT_TYPE m_vertType = VERT_TYPE.HARD_EDGE;

    //Stored variblesdue to this neededing to be intialised at start.
    private bool m_storedIsHardEdge = true;
    private int m_storedTotalMeshCount = 0;
    private int m_storedMeshVoxelCount = 0;
    private int m_storedVertPerVoxel = 0;
    private int m_storedMaxVoxels = 0;

    [Header("Specific Settings")]
    [Tooltip("Allow user to save static mesh at runtime in editor")]
    public bool m_saveStaticMesh = false;
    [Tooltip("Should the rotation be reset when saving?")]
    public bool m_resetRotation = false;

    //Passing Data
    private Dictionary<Vector3Int, Vector2> m_voxelIntDetails = new Dictionary<Vector3Int, Vector2>();
    private int m_calcVoxelsPerMesh = 0;

    //Inputs
    private List<Vector3> m_orginalVerts = new List<Vector3>();
    private List<Vector2> m_originalUVs = new List<Vector2>();
    private List<int> m_originalTris = new List<int>();

    //Outputs
    private Vector3[] m_convertedVerts; // unknown size
    private Vector2[] m_convertedUVs; //size of verts
    private int[] m_convertedTris; //3 x 12 x the size of positions

    //Shared
    private GameObject m_parentVoxelObject = null;
    private GameObject[] m_voxelObjects = new GameObject[0];
    private Mesh m_originalMesh = null;
    private Mesh[] m_voxelMeshs = new Mesh[0];

    //Animated
    private SkinnedMeshRenderer m_skinnedRenderer = null;
    private Material[] m_orginalMats = new Material[0];

    private void Awake()
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
            if (m_runFlag)
            {
                if (m_parentVoxelObject != null)
                    m_parentVoxelObject.SetActive(true);

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
    /// Disable voxel object
    /// </summary>
    private void OnDisable()
    {
        StopAllCoroutines();

        if (m_parentVoxelObject != null)
            m_parentVoxelObject.SetActive(false);
    }

#if UNITY_EDITOR
    private void OnValidate()
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
    private IEnumerator InitVoxeliser()
    {
        yield return null;

        m_storedIsHardEdge = m_vertType == VERT_TYPE.HARD_EDGE;
        m_storedTotalMeshCount = Mathf.Max(1, m_totalMeshCount);
        m_storedMeshVoxelCount = m_vertType == VERT_TYPE.HARD_EDGE ? MAX_MESH_VOXEL_HARDEDGE : MAX_MESH_VOXEL_SOFTEDGE;
        m_storedVertPerVoxel = m_vertType == VERT_TYPE.HARD_EDGE ? HARDEDGE_VERTS_PER_VOXEL : SOFTEDGE_VERTS_PER_VOXEL;

        m_storedMaxVoxels = m_storedMeshVoxelCount * m_storedTotalMeshCount;

        //Setup voxel mesh object
        //Setup voxel mesh object
        //Original Object
        if (m_objectWithMesh == null)
            m_objectWithMesh = gameObject;

        m_voxelObjects = new GameObject[m_storedTotalMeshCount];
        m_voxelMeshs = new Mesh[m_storedTotalMeshCount];

        m_parentVoxelObject = new GameObject(name + " Voxel Mesh Holder");

        for (int meshIndex = 0; meshIndex < m_storedTotalMeshCount; meshIndex++)
        {
            m_voxelObjects[meshIndex] = new GameObject("Mesh Section: " + meshIndex);
            m_voxelObjects[meshIndex].transform.SetParent(m_parentVoxelObject.transform);

            MeshFilter meshFilter = m_voxelObjects[meshIndex].AddComponent<MeshFilter>();
            meshFilter.mesh = new Mesh();
            m_voxelMeshs[meshIndex] = meshFilter.sharedMesh;
            m_voxelObjects[meshIndex].AddComponent<MeshRenderer>();
            MeshRenderer voxelMeshRenderer = m_voxelObjects[meshIndex].GetComponent<MeshRenderer>();
            voxelMeshRenderer.material = GetMaterial(m_objectWithMesh);
        }

        if (!VerifyVaribles())
            yield break;

        //Natives Constrution/ Assigning
        m_orginalVerts.AddRange(m_originalMesh.vertices);
        m_originalTris.AddRange(m_originalMesh.triangles);
        m_originalUVs.AddRange(m_originalMesh.uv);

        m_convertedVerts = new Vector3[m_storedTotalMeshCount * m_storedMeshVoxelCount * m_storedVertPerVoxel];
        m_convertedUVs = new Vector2[m_storedTotalMeshCount * m_storedMeshVoxelCount * m_storedVertPerVoxel];
        m_convertedTris = new int[m_storedTotalMeshCount * m_storedMeshVoxelCount * 36];

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
    /// Intialise a dynamic solid version of the object
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

        m_orginalVerts.Clear();
        m_orginalVerts.AddRange(m_originalMesh.vertices);

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

        GetBakedVerts();

        //Reset verts
        m_orginalVerts.Clear();
        m_orginalVerts.AddRange(m_originalMesh.vertices);

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

        foreach (Mesh voxelMesh in m_voxelMeshs)
        {
            if (voxelMesh != null)
                voxelMesh.Optimize();
        }

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
        //Reset old details
        m_voxelIntDetails.Clear();

        //Varible setup
        Matrix4x4 localToWorld = BuildGlobalLocalToWorldMat4x4(m_objectWithMesh);
        float voxelSizeRatio = 1.0f / p_voxelSize;

        //Build new mesh
        if (p_performOverFrames)
        {
            Coroutine buildTris = StartCoroutine(BuildTriVoxels(voxelSizeRatio, localToWorld));
            yield return buildTris;
            Coroutine getConverted = StartCoroutine(GetConvertedMesh(p_voxelSize));
            yield return getConverted;
        }
        else
        {
            StartCoroutine(BuildTriVoxels(voxelSizeRatio, localToWorld));
            StartCoroutine(GetConvertedMesh(p_voxelSize));
        }

        //Build new mesh
        for (int meshIndex = 0; meshIndex < m_storedTotalMeshCount; meshIndex++)
        {
            int vertsCount = m_calcVoxelsPerMesh * m_storedVertPerVoxel; //24 verts per voxel
            int triCount = m_calcVoxelsPerMesh * 36; // 36 tris per voxel

            Vector3[] convertedVerts = new Vector3[vertsCount];
            Vector2[] convertedUVs = new Vector2[vertsCount];
            int[] convertedTris = new int[triCount];

            //Verts/UVs
            int initialVoxelIndex = meshIndex * m_calcVoxelsPerMesh;
            int initialVertIndex = initialVoxelIndex * m_storedVertPerVoxel;
            int initialTriIndex = initialVoxelIndex * 36;

            for (int vertIndex = 0; vertIndex < vertsCount; vertIndex++)
            {
                convertedVerts[vertIndex] = m_convertedVerts[initialVertIndex + vertIndex];
                convertedUVs[vertIndex] = m_convertedUVs[initialVertIndex + vertIndex];
            }

            //Tris
            for (int triIndex = 0; triIndex < triCount; triIndex++)
            {
                convertedTris[triIndex] = m_convertedTris[initialTriIndex + triIndex]; //Use the minus due to "reseting" on new mesh
            }

            m_voxelMeshs[meshIndex].Clear(false);

            m_voxelMeshs[meshIndex].SetVertices(new List<Vector3>(convertedVerts));
            m_voxelMeshs[meshIndex].SetUVs(0, new List<Vector2>(convertedUVs));
            m_voxelMeshs[meshIndex].SetTriangles(convertedTris, 0);

            m_voxelMeshs[meshIndex].Optimize();
            m_voxelMeshs[meshIndex].RecalculateNormals();
        }
        yield break;
    }

    /// <summary>
    /// Build the voxel plavcments based off all tris
    /// Uses the Bresenham's line algorithum to find points from vert A to vert B
    /// Using the same approach points are calculated from thje previously found points to vert C
    /// </summary>
    /// <param name="p_voxelSizeRatio">Conversion ratio to get 1:1 scale of tri</param>
    /// <param name="p_localToWorld">Matrix for local to world, ignores position</param>
    private IEnumerator BuildTriVoxels(float p_voxelSizeRatio, Matrix4x4 p_localToWorld)
    {
        int triCount = m_originalTris.Count / 3;

        for (int triIndex = 0; triIndex < triCount; triIndex++)
        {
            StartCoroutine(BuildTri(p_voxelSizeRatio, triIndex, p_localToWorld));
        }

        yield break;
    }

    /// <summary>
    /// Build tri
    /// </summary>
    /// <param name="p_voxelSizeRatio">stored value of voxel size</param>
    /// <param name="p_tirIndex">Index of tri</param>
    /// <param name="p_localToWorld">Matrix for local to world, ignores position</param>
    private IEnumerator BuildTri(float p_voxelSizeRatio, int p_tirIndex, Matrix4x4 p_localToWorld)
    {
        //Float 4 varients due to matrix math
        Vector3Int vertA = GetVector3Int(p_localToWorld.MultiplyPoint3x4(m_orginalVerts[m_originalTris[p_tirIndex * 3]]) * p_voxelSizeRatio);
        Vector3Int vertB = GetVector3Int(p_localToWorld.MultiplyPoint3x4(m_orginalVerts[m_originalTris[p_tirIndex * 3 + 1]]) * p_voxelSizeRatio);
        Vector3Int vertC = GetVector3Int(p_localToWorld.MultiplyPoint3x4(m_orginalVerts[m_originalTris[p_tirIndex * 3 + 2]]) * p_voxelSizeRatio);

        //Has UV's been set?
        Vector2 vertAUV = Vector2.zero;
        Vector2 vertBUV = Vector2.zero;
        Vector2 vertCUV = Vector2.zero;

        if (m_originalUVs.Count != 0)
        {
            vertAUV = m_originalUVs[m_originalTris[p_tirIndex * 3]];
            vertBUV = m_originalUVs[m_originalTris[p_tirIndex * 3 + 1]];
            vertCUV = m_originalUVs[m_originalTris[p_tirIndex * 3 + 2]];
        }

        Dictionary<Vector3Int, Vector2> ABPoints = new Dictionary<Vector3Int, Vector2>();

        BresenhamDrawEachPoint(vertA, vertB, vertAUV, vertBUV, ABPoints);

        foreach (KeyValuePair<Vector3Int, Vector2> voxelDetails in ABPoints)
        {
            if (m_voxelIntDetails.Count >= m_storedMaxVoxels)
                break;

            BresenhamDrawEachPoint(voxelDetails.Key, vertC, voxelDetails.Value, vertCUV, m_voxelIntDetails);
        }

        yield break;
    }

    #region Supporting Building Voxels
    /// <summary>
    /// Build line of voxels
    /// Using Bresenham's line algorithum draw a line of voxels
    /// Example found here "https://www.mathworks.com/matlabcentral/fileexchange/21057-3d-bresenham-s-line-generation"
    /// </summary>
    /// <param name="p_startingPoint">Tri vert A</param>
    /// <param name="p_finalPoint">Tri vert B</param>
    /// <param name="p_startingUV">UV of point A</param>
    /// <param name="p_finalUV">UV of point B</param>
    /// <param name="p_storedDictionary">Dictionary to store verts in.</param>
    private void BresenhamDrawEachPoint(Vector3Int p_startingPoint, Vector3Int p_finalPoint, Vector2 p_startingUV, Vector2 p_finalUV, Dictionary<Vector3Int, Vector2> p_storedDictionary)
    {
        Vector3 vector = p_finalPoint - p_startingPoint;
        Vector3 vectorAbs = new Vector3(Mathf.Abs(vector.x), Mathf.Abs(vector.y), Mathf.Abs(vector.z));
        Vector3 vectorNorm = vector.normalized;
        Vector3 currentPoint = p_startingPoint;

        for (int loopCount = 0; loopCount < MAX_MESH_COUNT; loopCount++)
        {
            float x_next = Mathf.RoundToInt(currentPoint.x) + (vector.x == 0.0f ? 0.0f : vector.x > 0.0f ? 1 : -1);
            float y_next = Mathf.RoundToInt(currentPoint.y) + (vector.y == 0.0f ? 0.0f : vector.y > 0.0f ? 1 : -1);
            float z_next = Mathf.RoundToInt(currentPoint.z) + (vector.z == 0.0f ? 0.0f : vector.z > 0.0f ? 1 : -1);

            float x_diff = currentPoint.x - x_next;
            float y_diff = currentPoint.y - y_next;
            float z_diff = currentPoint.z - z_next;

            float x_diffAbs = x_diff == 0.0f || float.IsNaN(x_diff) ? Mathf.Infinity : Mathf.Abs(x_diff);
            float y_diffAbs = y_diff == 0.0f || float.IsNaN(y_diff) ? Mathf.Infinity : Mathf.Abs(y_diff);
            float z_diffAbs = z_diff == 0.0f || float.IsNaN(z_diff) ? Mathf.Infinity : Mathf.Abs(z_diff);

            if (float.IsInfinity(x_diffAbs) && float.IsInfinity(y_diffAbs) && float.IsInfinity(z_diffAbs))
            {
                break;
            }

            if (AddPoint(p_startingPoint, vector, currentPoint, p_startingUV, p_finalUV, p_storedDictionary))
                break;
            
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
    /// <param name="p_storedDictionary">Dictionary to store verts in.</param>
    /// <returns>Have we hit voxel limit</returns>
    private bool AddPoint(Vector3 p_startPoint, Vector3 p_vector, Vector3 p_currentPoint, Vector2 p_startUV, Vector2 p_endUV, Dictionary<Vector3Int, Vector2> p_storedDictionary)
    {
        if (p_storedDictionary.Count >= m_storedMaxVoxels)
            return true;

        float a2bPercent = GetPercent(p_startPoint, p_vector, p_currentPoint);

        Vector2 UV = MergeUVs(p_startUV, p_endUV, a2bPercent);

        Vector3Int snappedPoint = GetVector3Int(p_currentPoint);

        if (!p_storedDictionary.ContainsKey(snappedPoint))
            p_storedDictionary.Add(snappedPoint, UV);

        return false;
    }

    /// <summary>
    /// Get a Vector3Int that has been rounded to closest, not down
    /// </summary>
    /// <param name="p_vector">Vector to round to Vector3Int</param>
    /// <returns>final Vector3Int</returns>
    private Vector3Int GetVector3Int(Vector3 p_vector)
    {
        return new Vector3Int(Mathf.RoundToInt(p_vector.x), Mathf.RoundToInt(p_vector.y), Mathf.RoundToInt(p_vector.z));
    }

    /// <summary>
    /// Lerp between two UVs
    /// </summary>
    /// <param name="p_UVA">First UV</param>
    /// <param name="p_UVB">Second UV</param>
    /// <param name="p_percent">How far from UV1 to UV2</param>
    /// <returns>new merged UV</returns>
    private Vector2 MergeUVs(Vector2 p_UVA, Vector2 p_UVB, float p_percent)
    {
        return p_UVB * p_percent + p_UVA * (1 - p_percent);
    }

    /// <summary>
    /// Get lowest of 3 values
    /// </summary>
    /// <param name="p_startPoint">Starting point of vector</param>
    /// <param name="p_vector">Vector</param>
    /// <param name="p_currentPoint">What current value is</param>
    /// <returns>How far the current point is from start, 0 = 0%, 1 = 100%</returns>
    private float GetPercent(Vector3 p_startPoint, Vector3 p_vector, Vector3 p_currentPoint)
    {
        Vector3 currentVector = p_currentPoint - p_startPoint;
        return currentVector.magnitude / p_vector.magnitude;
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
    #endregion

    /// <summary>
    /// Converting of a single point where the voxel is, into the 8 points of a cube
    /// Vertices will overlap, this is ensure each "Voxel" has its own flat colour
    /// </summary>
    /// <param name="p_voxelSize">Actual VoxelSize</param>
    private IEnumerator GetConvertedMesh(float p_voxelSize)
    {
        int voxelCount = m_voxelIntDetails.Count;
        int arrayVoxelIndex = 0;
        //Get all unique
        Vector3Int[] uniquePositions = new Vector3Int[voxelCount];
        Vector2[] uniqueUVs = new Vector2[voxelCount];

        //Get arrays
        foreach (KeyValuePair<Vector3Int, Vector2> VoxelDetails in m_voxelIntDetails)
        {
            uniquePositions[arrayVoxelIndex] = VoxelDetails.Key;
            uniqueUVs[arrayVoxelIndex] = VoxelDetails.Value;
            arrayVoxelIndex++;
        }

        m_calcVoxelsPerMesh = uniquePositions.Length / m_totalMeshCount;

        for (int meshIndex = 0; meshIndex < m_totalMeshCount; meshIndex++)
        {
            int firstVoxelIndex = meshIndex * m_calcVoxelsPerMesh;

            for (int voxelIndex = 0; voxelIndex < m_calcVoxelsPerMesh; voxelIndex++)
            {
                Vector3 voxelPos = new Vector3(uniquePositions[firstVoxelIndex + voxelIndex].x * p_voxelSize, uniquePositions[firstVoxelIndex + voxelIndex].y * p_voxelSize, uniquePositions[firstVoxelIndex + voxelIndex].z * p_voxelSize);

                Vector3 right = new Vector3(p_voxelSize / 2.0f, 0.0f, 0.0f); // r = right l = left
                Vector3 up = new Vector3(0.0f, p_voxelSize / 2.0f, 0.0f); // u = up, d = down
                Vector3 forward = new Vector3(0.0f, 0.0f, p_voxelSize / 2.0f); // f = forward b = backward

                if (m_storedIsHardEdge) //Soft edge, 24 verts in total
                {
                    int currentVertIndex = firstVoxelIndex * 24 + voxelIndex * 24; //24 verts per Voxel

                    //Take each face, and face towards, up being up, or backwards
                    //start bottom left corner
                    //Back face
                    m_convertedVerts[currentVertIndex + 0] = voxelPos + right - up + forward;
                    m_convertedVerts[currentVertIndex + 1] = voxelPos + right + up + forward;
                    m_convertedVerts[currentVertIndex + 2] = voxelPos - right + up + forward;
                    m_convertedVerts[currentVertIndex + 3] = voxelPos - right - up + forward;
                    //Front - Flip above order
                    m_convertedVerts[currentVertIndex + 4] = voxelPos - right - up - forward;
                    m_convertedVerts[currentVertIndex + 5] = voxelPos - right + up - forward;
                    m_convertedVerts[currentVertIndex + 6] = voxelPos + right + up - forward;
                    m_convertedVerts[currentVertIndex + 7] = voxelPos + right - up - forward;
                    //Right face
                    m_convertedVerts[currentVertIndex + 8] = voxelPos + right - up - forward;
                    m_convertedVerts[currentVertIndex + 9] = voxelPos + right + up - forward;
                    m_convertedVerts[currentVertIndex + 10] = voxelPos + right + up + forward;
                    m_convertedVerts[currentVertIndex + 11] = voxelPos + right - up + forward;
                    //Left face        
                    m_convertedVerts[currentVertIndex + 12] = voxelPos - right - up + forward;
                    m_convertedVerts[currentVertIndex + 13] = voxelPos - right + up + forward;
                    m_convertedVerts[currentVertIndex + 14] = voxelPos - right + up - forward;
                    m_convertedVerts[currentVertIndex + 15] = voxelPos - right - up - forward;
                    //Top face    
                    m_convertedVerts[currentVertIndex + 16] = voxelPos - right + up - forward;
                    m_convertedVerts[currentVertIndex + 17] = voxelPos - right + up + forward;
                    m_convertedVerts[currentVertIndex + 18] = voxelPos + right + up + forward;
                    m_convertedVerts[currentVertIndex + 19] = voxelPos + right + up - forward;
                    //Bottom face      
                    m_convertedVerts[currentVertIndex + 20] = voxelPos + right - up - forward;
                    m_convertedVerts[currentVertIndex + 21] = voxelPos + right - up + forward;
                    m_convertedVerts[currentVertIndex + 22] = voxelPos - right - up + forward;
                    m_convertedVerts[currentVertIndex + 23] = voxelPos - right - up - forward;

                    //UVs Add in 24 for each vert added
                    Vector2 UV = uniqueUVs[firstVoxelIndex + voxelIndex];
                    for (int UVIndex = 0; UVIndex < 24; UVIndex++)
                    {
                        m_convertedUVs[currentVertIndex + UVIndex] = UV;
                    }

                    int startTriIndex = firstVoxelIndex * 36 + voxelIndex * 36; //36 tris per Voxel
                    int startPreaddedVertIndex = voxelIndex * 24; // Preaddeded as later on it will be split up
                                                                  //Tris
                                                                  //Back
                    m_convertedTris[startTriIndex + 0] = startPreaddedVertIndex + 0;
                    m_convertedTris[startTriIndex + 1] = startPreaddedVertIndex + 1;
                    m_convertedTris[startTriIndex + 2] = startPreaddedVertIndex + 2;

                    m_convertedTris[startTriIndex + 3] = startPreaddedVertIndex + 0;
                    m_convertedTris[startTriIndex + 4] = startPreaddedVertIndex + 2;
                    m_convertedTris[startTriIndex + 5] = startPreaddedVertIndex + 3;
                    //Front                              
                    m_convertedTris[startTriIndex + 6] = startPreaddedVertIndex + 4;
                    m_convertedTris[startTriIndex + 7] = startPreaddedVertIndex + 5;
                    m_convertedTris[startTriIndex + 8] = startPreaddedVertIndex + 6;

                    m_convertedTris[startTriIndex + 9] = startPreaddedVertIndex + 4;
                    m_convertedTris[startTriIndex + 10] = startPreaddedVertIndex + 6;
                    m_convertedTris[startTriIndex + 11] = startPreaddedVertIndex + 7;
                    //Right                               
                    m_convertedTris[startTriIndex + 12] = startPreaddedVertIndex + 8;
                    m_convertedTris[startTriIndex + 13] = startPreaddedVertIndex + 9;
                    m_convertedTris[startTriIndex + 14] = startPreaddedVertIndex + 10;

                    m_convertedTris[startTriIndex + 15] = startPreaddedVertIndex + 8;
                    m_convertedTris[startTriIndex + 16] = startPreaddedVertIndex + 10;
                    m_convertedTris[startTriIndex + 17] = startPreaddedVertIndex + 11;
                    //Left                                
                    m_convertedTris[startTriIndex + 18] = startPreaddedVertIndex + 12;
                    m_convertedTris[startTriIndex + 19] = startPreaddedVertIndex + 13;
                    m_convertedTris[startTriIndex + 20] = startPreaddedVertIndex + 14;

                    m_convertedTris[startTriIndex + 21] = startPreaddedVertIndex + 12;
                    m_convertedTris[startTriIndex + 22] = startPreaddedVertIndex + 14;
                    m_convertedTris[startTriIndex + 23] = startPreaddedVertIndex + 15;
                    //Top                                 
                    m_convertedTris[startTriIndex + 24] = startPreaddedVertIndex + 16;
                    m_convertedTris[startTriIndex + 25] = startPreaddedVertIndex + 17;
                    m_convertedTris[startTriIndex + 26] = startPreaddedVertIndex + 18;

                    m_convertedTris[startTriIndex + 27] = startPreaddedVertIndex + 16;
                    m_convertedTris[startTriIndex + 28] = startPreaddedVertIndex + 18;
                    m_convertedTris[startTriIndex + 29] = startPreaddedVertIndex + 19;
                    //Bottom                              
                    m_convertedTris[startTriIndex + 30] = startPreaddedVertIndex + 20;
                    m_convertedTris[startTriIndex + 31] = startPreaddedVertIndex + 21;
                    m_convertedTris[startTriIndex + 32] = startPreaddedVertIndex + 22;

                    m_convertedTris[startTriIndex + 33] = startPreaddedVertIndex + 20;
                    m_convertedTris[startTriIndex + 34] = startPreaddedVertIndex + 22;
                    m_convertedTris[startTriIndex + 35] = startPreaddedVertIndex + 23;
                }
                else //Soft edge, 8 verts in total
                {
                    int currentVertIndex = firstVoxelIndex * 8 + voxelIndex * 8; //24 verts per Voxel

                    //Take each face, and face towards, start bottom left corner
                    //Back face
                    m_convertedVerts[currentVertIndex + 0] = voxelPos + right - up + forward;
                    m_convertedVerts[currentVertIndex + 1] = voxelPos + right + up + forward;
                    m_convertedVerts[currentVertIndex + 2] = voxelPos - right + up + forward;
                    m_convertedVerts[currentVertIndex + 3] = voxelPos - right - up + forward;
                    //Front - Flip above order
                    m_convertedVerts[currentVertIndex + 4] = voxelPos - right - up - forward;
                    m_convertedVerts[currentVertIndex + 5] = voxelPos - right + up - forward;
                    m_convertedVerts[currentVertIndex + 6] = voxelPos + right + up - forward;
                    m_convertedVerts[currentVertIndex + 7] = voxelPos + right - up - forward;

                    //UVs Add in 8 for each vert added
                    Vector2 UV = uniqueUVs[firstVoxelIndex + voxelIndex];
                    for (int UVIndex = 0; UVIndex < 8; UVIndex++)
                    {
                        m_convertedUVs[currentVertIndex + UVIndex] = UV;
                    }

                    int startTriIndex = firstVoxelIndex * 36 + voxelIndex * 36; //36 tris per Voxel
                    int startPreaddedVertIndex = voxelIndex * 8; // Preaddeded as later on it will be split up
                                                                 //Tris
                                                                 //Back
                    m_convertedTris[startTriIndex + 0] = startPreaddedVertIndex + 0;
                    m_convertedTris[startTriIndex + 1] = startPreaddedVertIndex + 1;
                    m_convertedTris[startTriIndex + 2] = startPreaddedVertIndex + 2;

                    m_convertedTris[startTriIndex + 3] = startPreaddedVertIndex + 0;
                    m_convertedTris[startTriIndex + 4] = startPreaddedVertIndex + 2;
                    m_convertedTris[startTriIndex + 5] = startPreaddedVertIndex + 3;
                    //Front                              
                    m_convertedTris[startTriIndex + 6] = startPreaddedVertIndex + 4;
                    m_convertedTris[startTriIndex + 7] = startPreaddedVertIndex + 5;
                    m_convertedTris[startTriIndex + 8] = startPreaddedVertIndex + 6;

                    m_convertedTris[startTriIndex + 9] = startPreaddedVertIndex + 4;
                    m_convertedTris[startTriIndex + 10] = startPreaddedVertIndex + 6;
                    m_convertedTris[startTriIndex + 11] = startPreaddedVertIndex + 7;
                    //Right                               
                    m_convertedTris[startTriIndex + 12] = startPreaddedVertIndex + 7;
                    m_convertedTris[startTriIndex + 13] = startPreaddedVertIndex + 6;
                    m_convertedTris[startTriIndex + 14] = startPreaddedVertIndex + 1;

                    m_convertedTris[startTriIndex + 15] = startPreaddedVertIndex + 7;
                    m_convertedTris[startTriIndex + 16] = startPreaddedVertIndex + 1;
                    m_convertedTris[startTriIndex + 17] = startPreaddedVertIndex + 0;
                    //Left                                
                    m_convertedTris[startTriIndex + 18] = startPreaddedVertIndex + 3;
                    m_convertedTris[startTriIndex + 19] = startPreaddedVertIndex + 2;
                    m_convertedTris[startTriIndex + 20] = startPreaddedVertIndex + 5;

                    m_convertedTris[startTriIndex + 21] = startPreaddedVertIndex + 3;
                    m_convertedTris[startTriIndex + 22] = startPreaddedVertIndex + 5;
                    m_convertedTris[startTriIndex + 23] = startPreaddedVertIndex + 4;
                    //Top                                 
                    m_convertedTris[startTriIndex + 24] = startPreaddedVertIndex + 5;
                    m_convertedTris[startTriIndex + 25] = startPreaddedVertIndex + 2;
                    m_convertedTris[startTriIndex + 26] = startPreaddedVertIndex + 1;

                    m_convertedTris[startTriIndex + 27] = startPreaddedVertIndex + 5;
                    m_convertedTris[startTriIndex + 28] = startPreaddedVertIndex + 1;
                    m_convertedTris[startTriIndex + 29] = startPreaddedVertIndex + 3;
                    //Bottom                              
                    m_convertedTris[startTriIndex + 30] = startPreaddedVertIndex + 7;
                    m_convertedTris[startTriIndex + 31] = startPreaddedVertIndex + 0;
                    m_convertedTris[startTriIndex + 32] = startPreaddedVertIndex + 3;

                    m_convertedTris[startTriIndex + 33] = startPreaddedVertIndex + 7;
                    m_convertedTris[startTriIndex + 34] = startPreaddedVertIndex + 3;
                    m_convertedTris[startTriIndex + 35] = startPreaddedVertIndex + 4;
                }

            }
        }
        yield break;
    }

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
        if (m_originalMesh == null || m_originalMesh.vertexCount == 0)
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
        foreach (GameObject voxelObject in m_voxelObjects)
        {
            if (voxelObject == null)
            {
#if UNITY_EDITOR
                Debug.Log(name + " voxel object is missing");
#endif
                m_runFlag = false;

                return false;
            }
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
            Quaternion orginalRot = m_objectWithMesh.transform.rotation;

            if (m_resetRotation)
                m_objectWithMesh.transform.rotation = Quaternion.identity;

            Coroutine saveConvert = StartCoroutine(InitVoxeliser());

            yield return saveConvert;

            if (m_resetRotation)
                m_objectWithMesh.transform.rotation = orginalRot;

            //Get mesh
            string fileName = "Voxelised - " + name;
            string meshPath = "Assets/" + fileName + ".mesh";

            Mesh topMesh = m_voxelMeshs[0];

            AssetDatabase.CreateAsset(topMesh, meshPath);
            AssetDatabase.SetMainObject(topMesh, meshPath);

            //Add in any additional meshes
            for (int meshIndex = 1; meshIndex < m_voxelMeshs.Length; meshIndex++)
            {
                Mesh additionalMesh = m_voxelMeshs[meshIndex];
                additionalMesh.name = "Additional mesh:" + meshIndex;
                AssetDatabase.AddObjectToAsset(additionalMesh, topMesh);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(meshPath);

            //Create prefab
            string prefabPath = "Assets/" + fileName + ".prefab";
            GameObject parentObject = new GameObject();
            parentObject.name = fileName;
            //Main asset first
            Object meshObject = AssetDatabase.LoadMainAssetAtPath(meshPath);
            Mesh meshSection = (Mesh)meshObject;
            if (meshSection != null)
            {
                GameObject meshSectionObject = new GameObject("Mesh Section: " + 0);
                MeshFilter meshFilter = meshSectionObject.AddComponent<MeshFilter>();
                MeshRenderer meshRenderer = meshSectionObject.AddComponent<MeshRenderer>();
                meshFilter.sharedMesh = meshSection;
                meshRenderer.materials = m_orginalMats;
                meshSectionObject.transform.parent = parentObject.transform;
            }

            //Each sub asset
            Object[] meshObjects = AssetDatabase.LoadAllAssetRepresentationsAtPath(meshPath);
            for (int meshIndex = 0; meshIndex < meshObjects.Length; meshIndex++)
            {
                meshSection = (Mesh)meshObjects[meshIndex];

                if (meshSection != null)
                {
                    GameObject meshSectionObject = new GameObject("Mesh Section: " + meshIndex + 1); //Increment as main is 0
                    MeshFilter meshFilter = meshSectionObject.AddComponent<MeshFilter>();
                    MeshRenderer meshRenderer = meshSectionObject.AddComponent<MeshRenderer>();
                    meshFilter.sharedMesh = meshSection;
                    meshRenderer.materials = m_orginalMats;
                    meshSectionObject.transform.parent = parentObject.transform;
                }
            }

            PrefabUtility.SaveAsPrefabAssetAndConnect(parentObject, prefabPath, InteractionMode.UserAction);

            DestroyImmediate(parentObject);
        }
        else
        {
            Debug.Log("Can only save when set to static");
        }

        ToggleMaterial(m_objectWithMesh, true);
        DestroyImmediate(m_parentVoxelObject);
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
            if (m_skinnedRenderer == null)
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
        if (m_originalMesh == null)
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

    /// <summary>
    /// Build a global local to world matrix
    /// </summary>
    /// <param name="p_gameObject">Object to build</param>
    /// <returns>TRS matrix contianing pos, rot and scale</returns>
    private Matrix4x4 BuildGlobalLocalToWorldMat4x4(GameObject p_gameObject)
    {
        Matrix4x4 positionMatrix = Matrix4x4.Translate(p_gameObject.transform.position);
        Matrix4x4 rotationMatrix = Matrix4x4.Rotate(p_gameObject.transform.rotation);
        Matrix4x4 scaleMatrix = Matrix4x4.Scale(p_gameObject.transform.lossyScale);
        return positionMatrix * rotationMatrix * scaleMatrix;
    }

    #endregion
}
