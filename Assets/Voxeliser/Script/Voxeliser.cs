using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class Voxeliser : MonoBehaviour
{
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

    //Inputs
    private List<Vector3> m_orginalVerts = new List<Vector3>();
    private List<Vector2> m_originalUVs = new List<Vector2>();
    private List<int> m_originalTris = new List<int>();

    //Passing Data
    private Dictionary<Vector3, Vector2> m_voxelDetails = new Dictionary<Vector3, Vector2>();

    //Outputs
    private List<Vector3> m_convertedVerts = new List<Vector3>(); // unknown size
    private List<Vector2> m_convertedUVs = new List<Vector2>(); //size of verts
    private List<int> m_convertedTris = new List<int>(); //3 x 12 x the size of positions

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
        m_modelMesh.GetVertices(m_orginalVerts);
        m_modelMesh.GetUVs(0, m_originalUVs);
        m_modelMesh.GetTriangles(m_originalTris, 0);

        //New object that holds the mesh
        m_voxelObject = new GameObject(name + " Voxel Mesh Holder");
        MeshFilter voxelMeshFilter = m_voxelObject.AddComponent<MeshFilter>();
        m_voxelMesh = voxelMeshFilter.mesh;

        MeshRenderer voxelMeshRenderer = m_voxelObject.AddComponent<MeshRenderer>();
        voxelMeshRenderer.material = CustomMeshHandeling.GetMaterial(m_objectWithMesh);

        CustomMeshHandeling.DisableMaterial(m_objectWithMesh);

        StartCoroutine(VoxeliserUpdate());
    }

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
        //Reset Details
        m_modelMesh.GetVertices(m_orginalVerts);

        m_voxelDetails.Clear();

        //Build new mesh

        //Varibles to set
        //Verts
        BuildTriVoxels();
        GetConvertedMesh();

        //Build new mesh
        m_voxelMesh.Clear();

        m_voxelMesh.SetVertices(m_convertedVerts);
        m_voxelMesh.SetUVs(0,m_convertedUVs);
        m_voxelMesh.SetTriangles(m_convertedTris, 0);
        
        //m_voxelMesh.Optimize();
        m_voxelMesh.RecalculateNormals();

        //Useful debug mesages when not enough voxels
#if UNITY_EDITOR
        if (m_maxVoxelCount < m_voxelDetails.Count)
            Debug.Log("The voxeliser on " + name + " is trying to place more voxels than intiliased, think about increasing the count");
#endif
    }

    /// <summary>
    /// Build the voxel plavcments based off all tris
    /// Uses the Bresenham's line algorithum to find points from vert A to vert B
    /// Using the same approach points are calculated from thje previously found points to vert C
    /// </summary>
    public void BuildTriVoxels()
    {
        int triCount = m_originalTris.Count / 3;
        for (int triIndex = 0; triIndex < triCount; triIndex++)
        {
            //Float 4 varients due to matrix math
            Vector3 vertA = m_objectWithMesh.transform.TransformPoint(m_orginalVerts[m_originalTris[triIndex * 3]]);
            Vector3 vertB = m_objectWithMesh.transform.TransformPoint(m_orginalVerts[m_originalTris[triIndex * 3 + 1]]);
            Vector3 vertC = m_objectWithMesh.transform.TransformPoint(m_orginalVerts[m_originalTris[triIndex * 3 + 2]]);

            Vector2 vertAUV = m_originalUVs[m_originalTris[triIndex * 3]];
            Vector2 vertBUV = m_originalUVs[m_originalTris[triIndex * 3 + 1]];
            Vector2 vertCUV = m_originalUVs[m_originalTris[triIndex * 3 + 2]];

            List<Vector3> points = new List<Vector3>();
            List<Vector2> pointUVs = new List<Vector2>();

            //Initial line
            InitialBresenhamDrawEachPoint(vertA, vertB, vertAUV, vertBUV, points, pointUVs);

            //Draw from intial line to vertC
            for (int i = 0; i < points.Count; i++)
            {
                BresenhamDrawEachPoint(points[i], vertC, pointUVs[i], vertCUV);
            }
        }
    }

    #region Supporting Building Voxels
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
    private void InitialBresenhamDrawEachPoint(Vector3 p_pointA, Vector3 p_pointB, Vector2 p_pointAUV, Vector2 p_pointBUV, List<Vector3> p_points, List<Vector2> p_pointUVs)
    {
        //Basic Values
        float x_diff = p_pointB.x - p_pointA.x;
        float y_diff = p_pointB.y - p_pointA.y;
        float z_diff = p_pointB.z - p_pointA.z;

        float x_abs = Mathf.Abs(x_diff);
        float y_abs = Mathf.Abs(y_diff);
        float z_abs = Mathf.Abs(z_diff);

        float dominateAbs = GetHighest(x_abs, y_abs, z_abs);
        int dominateAxis = dominateAbs == x_abs ? 0 : dominateAbs == y_abs ? 1 : 2; //Get dominate axis 0 = x-axis, 1 = y-axis, 2 = z-axis

        //Incease step count by mukltiplier, ensure steps occurs at least once
        int steps = Mathf.Max(1, (int)(dominateAbs / VOXEL_SIZE) * m_stepModifier);

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
            Vector3 newPoint = new Vector3();

            float a2bPercent = steps == 0 ? 0 : (float)(i) / steps;
            Vector2 UV = MergeUVs(p_pointAUV, p_pointBUV, a2bPercent);

            switch (dominateAxis)
            {
                case 0:
                    newPoint = new Vector3(dominateCurr, axisACurr, axisBCurr);
                    break;
                case 1:
                    newPoint = new Vector3(axisACurr, dominateCurr, axisBCurr);
                    break;
                case 2:
                    newPoint = new Vector3(axisACurr, axisBCurr, dominateCurr);
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
    private void BresenhamDrawEachPoint(Vector3 p_pointOnLine, Vector3 p_pointC, Vector2 p_pointOnLineUV, Vector2 p_pointCUV)
    {
        //Basic Values
        float x_diff = p_pointC.x - p_pointOnLine.x;
        float y_diff = p_pointC.y - p_pointOnLine.y;
        float z_diff = p_pointC.z - p_pointOnLine.z;

        float x_abs = Mathf.Abs(x_diff);
        float y_abs = Mathf.Abs(y_diff);
        float z_abs = Mathf.Abs(z_diff);

        float dominateAbs = GetHighest(x_abs, y_abs, z_abs);
        int dominateAxis = dominateAbs == x_abs ? 0 : dominateAbs == y_abs ? 1 : 2; //Get dominate axis 0 = x-axis, 1 = y-axis, 2 = z-axis

        //Incease step count by mukltiplier, ensure steps occurs at least once
        int steps = Mathf.Max(1, (int)(dominateAbs / VOXEL_SIZE) * m_stepModifier);

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
            Vector3 newPoint = new Vector3();
            switch (dominateAxis)
            {
                case 0:
                    newPoint = new Vector3(dominateCurr, axisACurr, axisBCurr);
                    break;
                case 1:
                    newPoint = new Vector3(axisACurr, dominateCurr, axisBCurr);
                    break;
                case 2:
                    newPoint = new Vector3(axisACurr, axisBCurr, dominateCurr);
                    break;
            }

            axisACurr += axisAStep;

            axisBCurr += axisBStep;

            dominateCurr += dominateStep;

            float a2bPercent = steps == 0 ? 0 : (float)(i) / steps;
            Vector3 snappedPoint = SnapToGrid(newPoint);

            if (!m_voxelDetails.ContainsKey(snappedPoint))
                m_voxelDetails.Add(SnapToGrid(newPoint), MergeUVs(p_pointOnLineUV, p_pointCUV, a2bPercent));
        }
    }

    /// <summary>
    ///  Snap to postion grid
    /// </summary>
    /// <param name="p_position">World position</param>
    /// <returns>Postioned "Snapped" to a world grid with increments of p_gridSize </returns>
    Vector3 SnapToGrid(Vector3 p_position)
    {
        p_position.x -= p_position.x % VOXEL_SIZE;
        p_position.y -= p_position.y % VOXEL_SIZE;
        p_position.z -= p_position.z % VOXEL_SIZE;

        return p_position;
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

    /// <summary>
    /// Converting of a single point where the voxel is, into the 8 points of a cube
    /// Vertices will overlap, this is ensure each "Voxel" has its own flat colour
    /// </summary>
    public void GetConvertedMesh()
    {
        float halfVoxel = VOXEL_SIZE / 2.0f;

        int voxelCount = m_voxelDetails.Count;
        m_convertedVerts.Clear();//8 verts per voxel
        m_convertedUVs.Clear();//one per vert
        m_convertedTris.Clear();// 3 per tri, 12  tris per voxel

        Vector3 right = new Vector3(halfVoxel, 0.0f, 0.0f); // r = right l = left
        Vector3 up = new Vector3(0.0f, halfVoxel, 0.0f); // u = up, d = down
        Vector3 forward = new Vector3(0.0f, 0.0f, halfVoxel); // f = forward b = backward

        int[] indexArray = new int[8];

        foreach (KeyValuePair<Vector3,Vector2> voxel in m_voxelDetails)
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

            //Verts
            m_convertedVerts.Add(voxel.Key - right - up - forward);
            m_convertedVerts.Add(voxel.Key + right - up - forward);
            m_convertedVerts.Add(voxel.Key + right + up - forward);
            m_convertedVerts.Add(voxel.Key - right + up - forward);
            m_convertedVerts.Add(voxel.Key - right + up + forward);
            m_convertedVerts.Add(voxel.Key + right + up + forward);
            m_convertedVerts.Add(voxel.Key + right - up + forward);
            m_convertedVerts.Add(voxel.Key - right - up + forward);

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
    }

    /// <summary>
    /// Ensure voxelised object is removed too
    /// </summary>
    private void OnDestroy()
    {
        if (m_voxelObject != null)
            DestroyImmediate(m_voxelObject);
    }
}
