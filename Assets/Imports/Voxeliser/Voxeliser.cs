using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[ExecuteAlways]
public class Voxeliser : MonoBehaviour
{
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

    //Passing Data
    private Dictionary<Vector3Int, Vector2> m_voxelIntDetails = new Dictionary<Vector3Int, Vector2>();

    //Inputs
    private List<Vector3> m_orginalVerts = new List<Vector3>();
    private List<Vector2> m_originalUVs = new List<Vector2>();
    private List<int> m_originalTris = new List<int>();

    //Outputs
    private List<Vector3> m_convertedVerts = new List<Vector3>(); // unknown size
    private List<Vector2> m_convertedUVs = new List<Vector2>(); //size of verts
    private List<int> m_convertedTris = new List<int>(); //3 x 12 x the size of positions

    //Shared
    private GameObject m_voxelObject = null;
    private Mesh m_originalMesh = null; //A copy of orginal mesh
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
            if (m_running)
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
    /// Disable voxel object
    /// </summary>
    private void OnDisable()
    {
        StopAllCoroutines();

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
    private IEnumerator InitVoxeliser()
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
        m_orginalVerts.AddRange(m_originalMesh.vertices);
        m_originalTris.AddRange(m_originalMesh.triangles);
        m_originalUVs.AddRange(m_originalMesh.uv);

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
    /// Intialise a dynamic solid version of the object
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

        m_originalMesh = GetBakedVerts(m_skinnedRenderer, m_objectWithMesh);

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

        m_voxelMesh.Optimize();

#if UNITY_EDITOR
        if (m_saveStaticMesh)
        {
            SaveMesh();
        }
#endif
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
        //Reset Details
        m_voxelIntDetails.Clear();

        float voxelSizeRatio = 1.0f / p_voxelSize;

        //Build new mesh
        if (p_performOverFrames)
        {
            Coroutine buildTris = StartCoroutine(BuildTriVoxels(voxelSizeRatio));
            yield return buildTris;
            Coroutine getConverted = StartCoroutine(GetConvertedMesh(p_voxelSize));
            yield return getConverted;
        }
        else
        {
            StartCoroutine(BuildTriVoxels(voxelSizeRatio));
            StartCoroutine(GetConvertedMesh(p_voxelSize));
        }

        //Build new mesh
        m_voxelMesh.Clear();

        m_voxelMesh.SetVertices(m_convertedVerts);
        m_voxelMesh.SetUVs(0, m_convertedUVs);
        m_voxelMesh.SetTriangles(m_convertedTris, 0);

        m_voxelMesh.RecalculateNormals();

        yield break;
    }

    /// <summary>
    /// Build the voxel plavcments based off all tris
    /// Uses the Bresenham's line algorithum to find points from vert A to vert B
    /// Using the same approach points are calculated from thje previously found points to vert C
    /// </summary>
    /// <param name="p_sizeConverter">Conversion ratio to get 1:1 scale of tri</param>
    private IEnumerator BuildTriVoxels(float p_sizeConverter)
    {
        int triCount = m_originalTris.Count / 3;

        for (int triIndex = 0; triIndex < triCount; triIndex++)
        {
            StartCoroutine(BuildTri(p_sizeConverter, triIndex));
        }


        yield break;
    }

    /// <summary>
    /// Build tri
    /// </summary>
    /// <param name="p_voxelSizeConveter">stored value of voxel size</param>
    /// <param name="p_tirIndex">Index of tri</param>
    private IEnumerator BuildTri(float p_voxelSizeConveter, int p_tirIndex)
    {
        //Float 4 varients due to matrix math
        Vector3Int vertA = GetVector3Int(m_objectWithMesh.transform.TransformPoint(m_orginalVerts[m_originalTris[p_tirIndex * 3]]) * p_voxelSizeConveter);
        Vector3Int vertB = GetVector3Int(m_objectWithMesh.transform.TransformPoint(m_orginalVerts[m_originalTris[p_tirIndex * 3 + 1]]) * p_voxelSizeConveter);
        Vector3Int vertC = GetVector3Int(m_objectWithMesh.transform.TransformPoint(m_orginalVerts[m_originalTris[p_tirIndex * 3 + 2]]) * p_voxelSizeConveter);

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

            AddPoint(p_startingPoint, vector, currentPoint, p_startingUV, p_finalUV, p_storedDictionary);
            
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
    private void AddPoint(Vector3 p_startPoint, Vector3 p_vector, Vector3 p_currentPoint, Vector2 p_startUV, Vector2 p_endUV, Dictionary<Vector3Int, Vector2> p_storedDictionary)
    {
        float a2bPercent = GetPercent(p_startPoint, p_vector, p_currentPoint);

        Vector2 UV = MergeUVs(p_startUV, p_endUV, a2bPercent);

        Vector3Int snappedPoint = GetVector3Int(p_currentPoint);

        if (!p_storedDictionary.ContainsKey(snappedPoint))
            p_storedDictionary.Add(snappedPoint, UV);
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
        return p_UVA * p_percent + p_UVB * (1 - p_percent);
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
        float halfVoxel = m_voxelSize / 2.0f;

        int voxelCount = m_voxelIntDetails.Count;
        m_convertedVerts.Clear();//8 verts per voxel
        m_convertedUVs.Clear();//one per vert
        m_convertedTris.Clear();// 3 per tri, 12  tris per voxel

        Vector3 right = new Vector3(halfVoxel, 0.0f, 0.0f); // r = right l = left
        Vector3 up = new Vector3(0.0f, halfVoxel, 0.0f); // u = up, d = down
        Vector3 forward = new Vector3(0.0f, 0.0f, halfVoxel); // f = forward b = backward

        int[] indexArray = new int[8];

        foreach (KeyValuePair<Vector3Int, Vector2> voxel in m_voxelIntDetails)
        {
            //Vert indexes, if positon doesnt exiosts, new index, otherwise old index
            int indexStart = m_convertedVerts.Count;
            indexArray[0] = indexStart;
            indexArray[1] = indexStart + 1;
            indexArray[2] = indexStart + 2;
            indexArray[3] = indexStart + 3;
            indexArray[4] = indexStart + 4;
            indexArray[5] = indexStart + 5;
            indexArray[6] = indexStart + 6;
            indexArray[7] = indexStart + 7;

            Vector3 voxelPos = new Vector3(voxel.Key.x * p_voxelSize, voxel.Key.y * p_voxelSize, voxel.Key.z * p_voxelSize);

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
            for (int i = 0; i < 8; i++)
            {
                m_convertedUVs.Add(voxel.Value);
            }

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
                m_running = false;

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

        if(m_voxeliserType == VOXELISER_TYPE.ANIMATED)
        {
            if(m_skinnedRenderer == null)
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
            m_saveStaticMesh = false;
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
}
