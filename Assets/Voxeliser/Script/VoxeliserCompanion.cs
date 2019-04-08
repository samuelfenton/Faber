using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxeliserCompanion : MonoBehaviour
{
    [Tooltip("Set how many voxels will be used, in the case of missing voxels increase this number. Too high will cause lag")]
    public int VOXEL_COUNT = 1000;

    [Tooltip("How often this voxeliser will update, it is this varible + 2. So N = 1, updates every 3 frames")]
    public int UPDATE_N_FRAMES = 1;

    [Tooltip("Should the material be disabled on play")]
    public bool DISABLE_MATERIAL = false;

    [Tooltip("Gameobject which holds the mesh for the voxeliser, with none assigned, assumption is mesh is on script gameobject")]
    public GameObject m_objectWithMesh = null;

    protected VoxeliserHandler m_voxelHandler = null;

    [HideInInspector]
    public Mesh m_modelMesh;

    protected Texture2D m_mainTexture = null;
    protected Color m_backupColor = Color.white;

    private float m_voxelSize = 1.0f;

    VectorDouble[] m_positionArray;
    Vector3[] m_colorArray;

    int[] m_meshTris;
    Vector3[] m_vertColors;

    //--------------------
    //  Setup of the Voxeliser
    //      Ensure object has all required components atached
    //      Setup required varibles
    //--------------------
    protected virtual void Start ()
    {
        if (m_objectWithMesh == null)
            m_objectWithMesh = gameObject;

        m_modelMesh = CustomMeshHandeling.GetMesh(m_objectWithMesh);

        if (m_modelMesh == null)
        {
#if UNITY_EDITOR
            Debug.Log("Unable to get mesh on " + gameObject.name);
#endif
            Destroy(this);
            return;
        }

        m_voxelHandler = GetComponent<VoxeliserHandler>();
        if (m_voxelHandler == null)
        {
#if UNITY_EDITOR
            Debug.Log("Unable to get valid voxel handler on " + gameObject.name);
#endif
            Destroy(this);
            return;
        }

        m_voxelSize = m_voxelHandler.m_voxelSize;

        if (VOXEL_COUNT <= 0)
        {
            VOXEL_COUNT = 1;
#if UNITY_EDITOR
            Debug.Log("Voxel Count on " + gameObject.name + " shouldn't be set as less or equal to 0");
#endif
        }

        if (UPDATE_N_FRAMES < 0)
        {
            UPDATE_N_FRAMES = 0;
#if UNITY_EDITOR
            Debug.Log("Frame update on " + gameObject.name + " shouldn't be set as less than 0");
#endif
        }

        m_meshTris = m_modelMesh.triangles;
        m_vertColors = SetupMeshColor(m_modelMesh.vertices);

        //Voxel handler
        m_voxelHandler.InitVoxels(VOXEL_COUNT);

        if (DISABLE_MATERIAL)
        {
            CustomMeshHandeling.DisableMaterial(m_objectWithMesh);
        }

        StartCoroutine(VoxeliserUpdate());
    }

    //--------------------
    //  Rather than using Update Voxeliser is Enumerator driven.
    //  Run logic every run every second frame
    //--------------------
    protected virtual IEnumerator VoxeliserUpdate()
    {
        if (Time.frameCount % (2 + UPDATE_N_FRAMES) == 0) //Run every third frame
        {
            foreach (VoxeliserCompanion_Child voxelChild in GetComponentsInChildren<VoxeliserCompanion_Child>())
            {
                StartCoroutine(voxelChild.VoxeliserUpdate());
            }

            UpdateMesh();
            StartCoroutine(ConvertToVoxels());
        }

        yield return null;
        StartCoroutine(VoxeliserUpdate());
    }

    //--------------------
    //  Update the mesh used in determining vertex position
    //  Overriden by skinned renderer/ mesh renderer derrived classes
    //--------------------
    protected virtual void UpdateMesh()
    {
    }

    //--------------------
    //  Get frame voxel positions
    //      Get transform matrix without the postion assigned
    //      Get Voxel positions
    //      Pass data to voxel handler
    //--------------------
    protected IEnumerator ConvertToVoxels()
    {
        Vector3[] meshVerts = m_modelMesh.vertices;
        HashSet<VectorDouble> voxelPositions = new HashSet<VectorDouble>(); 
        List<Vector3> voxelColors = new List<Vector3>();

        //Build tris
        int coroutineIndex = 0;
        int totalTris = m_meshTris.Length / 3;
        int coroutineLoopCount = UPDATE_N_FRAMES == 0 ? totalTris : totalTris / UPDATE_N_FRAMES;

        int loopIndex = 0;

        do
        {
            ///Getting position Logic
            for (loopIndex = coroutineIndex * coroutineLoopCount; loopIndex < (coroutineIndex + 1) * coroutineLoopCount && loopIndex < totalTris; loopIndex++)
            {
                //Set up vert index/positions
                int vertAIndex = m_meshTris[loopIndex * 3];
                int vertBIndex = m_meshTris[loopIndex * 3 + 1];
                int vertCIndex = m_meshTris[loopIndex * 3 + 2];
                VectorDouble vertA = VectorDouble.GetVectorDouble(transform.localToWorldMatrix * meshVerts[vertAIndex]);
                VectorDouble vertB = VectorDouble.GetVectorDouble(transform.localToWorldMatrix * meshVerts[vertBIndex]);
                VectorDouble vertC = VectorDouble.GetVectorDouble(transform.localToWorldMatrix * meshVerts[vertCIndex]);

                //Build orginal vertA to vertB line
                List<Vector3> a2bColors = new List<Vector3>();
                List<VectorDouble> a2bPoints = BresenhamDrawEachPoint(vertA, vertB, m_vertColors[vertAIndex], m_vertColors[vertBIndex], a2bColors);

                for (int originIndex = 0; originIndex < a2bPoints.Count; originIndex++)
                {
                    //Building a2b points to vert C
                    List<Vector3> ab2cColors = new List<Vector3>();
                    List<VectorDouble> voxelPoints = BresenhamDrawEachPoint(a2bPoints[originIndex], vertC, a2bColors[originIndex], m_vertColors[vertCIndex], ab2cColors);

                    for (int voxelIndex = 0; voxelIndex < voxelPoints.Count; voxelIndex++)
                    {
                        if (!voxelPositions.Contains(voxelPoints[voxelIndex]))
                        {
                            voxelPositions.Add(voxelPoints[voxelIndex]);
                            voxelColors.Add(m_vertColors[voxelIndex]);
                        }
                    }
                }
            }


            //Coroutine Logic
            coroutineIndex++;
            if (coroutineIndex < UPDATE_N_FRAMES)
                yield return null;

        }
        while (coroutineIndex < UPDATE_N_FRAMES);

        yield return null;

        StartCoroutine(m_voxelHandler.HandleVoxels(voxelPositions, voxelColors));
    }

    //--------------------
    //  Setup vert colors
    //      Attempt to get main texture
    //      In case of no texture use base material color
    //      Otherwise use vertex uv's to get color from texture.
    //  params:
    //      p_meshVerts - Array of mesh verts, used to determine color
    //--------------------
    private Vector3[] SetupMeshColor(Vector3[] p_meshVerts)
    {
        Vector3[] vertColors = new Vector3[p_meshVerts.Length];

        //Attempt to get MAIN_TEX
        MeshRenderer meshRenderer = m_objectWithMesh.GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            m_mainTexture = (Texture2D)meshRenderer.material.mainTexture;
            m_backupColor = meshRenderer.material.color;
        }
        else
        {
            SkinnedMeshRenderer skinnedMeshRenderer = m_objectWithMesh.GetComponent<SkinnedMeshRenderer>();
            if (skinnedMeshRenderer != null)
            {
                m_mainTexture = (Texture2D)skinnedMeshRenderer.material.mainTexture;
                m_backupColor = skinnedMeshRenderer.material.color;
            }
        }

        //Build colour based off main texture/basecolor
        if (m_mainTexture == null) //No texture supplied
        {
            Vector3 vertColor = new Vector3(m_backupColor.r, m_backupColor.g, m_backupColor.b); ;
            for (int i = 0; i < p_meshVerts.Length; i++)
            {
                vertColors[i] = vertColor;
            }
        }
        else
        {
            Vector2[] uvs;

            uvs = m_modelMesh.uv;

            for (int i = 0; i < p_meshVerts.Length; i++)
            {
                Color vertColor = m_mainTexture.GetPixel((int)uvs[i].x, (int)uvs[i].y);
                vertColors[i] = new Vector3(vertColor.r, vertColor.g, vertColor.b);
            }
        }
        return vertColors;
    }

    //--------------------
    //  Build line of voxels
    //  Using Bresenham's line algorithum draw a line of voxels
    //  Example found here "https://www.mathworks.com/matlabcentral/fileexchange/21057-3d-bresenham-s-line-generation", ":https://github.com/ssloy/tinyrenderer/wiki/Lesson-2:-Triangle-rasterization-and-back-face-culling"
    //  params:
    //      p_pointA - Tri vert 1
    //      p_pointB - Tri vert 2
    //      p_pointAColor - Color of point A
    //      p_pointBColor - Color of point B
    //      p_pointColors - List to store colors in.
    //  return:
    //      List<VectorDouble> - Points calculated along Bresenhams line
    //--------------------
    private List<VectorDouble> BresenhamDrawEachPoint(VectorDouble p_pointA, VectorDouble p_pointB, Vector3 p_pointAColor, Vector3 p_pointBColor, List<Vector3> p_pointColors)
    {
        List<VectorDouble> linePoints = new List<VectorDouble>();
        //Basic Values
        double x_diff = p_pointB.m_x - p_pointA.m_x;
        double y_diff = p_pointB.m_y - p_pointA.m_y;
        double z_diff = p_pointB.m_z - p_pointA.m_z;

        double x_abs = Math.Abs(x_diff);
        double y_abs = Math.Abs(y_diff);
        double z_abs = Math.Abs(z_diff);

        double dominateAbs = GetHighest(x_abs, y_abs, z_abs);
        int dominateAxis = dominateAbs == x_abs ? 0 : dominateAbs == y_abs ? 1 : 2; //Get dominate axis 0 = x-axis, 1 = y-axis, 2 = z-axis

        int steps = (int)(dominateAbs * 1.5 / m_voxelSize);

        double dominateCurr = 0, dominateStep = 0;

        double axisACurr = 0, axisAStep = 0;
        double axisBCurr = 0, axisBStep = 0;

        switch (dominateAxis)
        {
            case 0:
                dominateCurr = p_pointA.m_x;
                dominateStep = x_diff / steps;

                axisACurr = p_pointA.m_y;
                axisAStep = y_diff / steps;

                axisBCurr = p_pointA.m_z;
                axisBStep = z_diff / steps;
                break;
            case 1:
                dominateCurr = p_pointA.m_y;
                dominateStep = y_diff / steps;

                axisACurr = p_pointA.m_x;
                axisAStep = x_diff / steps;

                axisBCurr = p_pointA.m_z;
                axisBStep = z_diff / steps;
                break;
            case 2:
                dominateCurr = p_pointA.m_z;
                dominateStep = z_diff / steps;

                axisACurr = p_pointA.m_x;
                axisAStep = x_diff / steps;

                axisBCurr = p_pointA.m_y;
                axisBStep = y_diff / steps;
                break;
        }

        for (int i = 0; i < steps; i++)
        {
            //DrawPoint
            VectorDouble newPoint = new VectorDouble();
            switch (dominateAxis)
            {
                case 0:
                    newPoint = new VectorDouble(dominateCurr, axisACurr, axisBCurr);
                    break;
                case 1:
                    newPoint = new VectorDouble(axisACurr, dominateCurr, axisBCurr);
                    break;
                case 2:
                    newPoint = new VectorDouble(axisACurr, axisBCurr, dominateCurr);
                    break;
            }

            axisACurr += axisAStep;

            axisBCurr += axisBStep;

            dominateCurr += dominateStep;

            linePoints.Add(SnapToGrid(newPoint, m_voxelSize));
            //Calc color
            float a2bPercent = steps == 0 ? 0 : (float)(i) / steps;
            p_pointColors.Add(MergeColors(p_pointAColor, p_pointBColor, a2bPercent));
        }

        return linePoints;
    }

    //--------------------
    //  Snap to postion grid
    //  params:
    //      p_position - World position
    //      p_gridSize - size between grid lines
    //  return:
    //      Vector3 - Postioned "Snapped" to a world grid with increments of p_gridSize 
    //--------------------
    Vector3 SnapToGrid(Vector3 p_position, float p_gridSize)
    {
        p_position.x -= p_position.x < 0 ? p_position.x % -p_gridSize : p_position.x % p_gridSize;
        p_position.y -= p_position.y < 0 ? p_position.y % -p_gridSize : p_position.y % p_gridSize;
        p_position.z -= p_position.z < 0 ? p_position.z % -p_gridSize : p_position.z % p_gridSize;
        
        return p_position;
    }

    //--------------------
    //  Snap to postion grid
    //  params:
    //      p_position - World position
    //      p_gridSize - size between grid lines
    //  return:
    //      VectorDouble - Postioned "Snapped" to a world grid with increments of p_gridSize 
    //--------------------
    private VectorDouble SnapToGrid(VectorDouble p_position, float p_gridSize)
    {
        p_position.m_x -= p_position.m_x < 0 ? p_position.m_x % -p_gridSize : p_position.m_x % p_gridSize;
        p_position.m_y -= p_position.m_y < 0 ? p_position.m_y % -p_gridSize : p_position.m_y % p_gridSize;
        p_position.m_z -= p_position.m_x < 0 ? p_position.m_z % -p_gridSize : p_position.m_z % p_gridSize;

        return p_position;
    }

    //--------------------
    //  Merge 2 colors, rather than average get percent based merge
    //  params:
    //      p_colorA - First color
    //      p_colorB - Second color
    //      p_a2bRatio - Percent from color A to color B
    //  return:
    //      float - new merged color
    //--------------------
    private Vector3 MergeColors(Vector3 p_colorA, Vector3 p_colorB, float p_a2bRatio)
    {
        p_a2bRatio = p_a2bRatio < 0 ? 0 : p_a2bRatio > 1 ? 1 : p_a2bRatio; //Snap between 0 and 1
        Vector3 colorDiff = new Vector3(p_colorB.x - p_colorA.x, p_colorB.y - p_colorA.y, p_colorB.z - p_colorA.z);
        return p_colorA + new Vector3(colorDiff.x * p_a2bRatio, colorDiff.y * p_a2bRatio, colorDiff.z * p_a2bRatio);
    }

    //--------------------
    //  Get relative sign of given number
    //  params:
    //      p_val - Value to get sign from
    //      p_pointB - Tri vert 2
    //      p_pointC - Tri vert 3
    //  return:
    //      val<0 = negitive(-1), val>0 = positive(1), val == 0 = netural(0)
    //--------------------
    private int GetSign(double p_val)
    {
        return p_val < 0 ? -1 : p_val > 0 ? 1 : 0;
    }

    //--------------------
    //  Get highest of 3 values
    //  params:
    //      p_val1 - First to compare with
    //      p_val2 - Second to compare with
    //      p_val3 - Third to compare with
    //  return:
    //      float - Highest of the three values
    //--------------------
    private double GetHighest(double p_val1, double p_val2, double p_val3)
    {
        //current highest = p_val2 > p_val3 ? p_val2 : p_val3
        return p_val1 > (p_val2 > p_val3 ? p_val2 : p_val3) ? p_val1 : (p_val2 > p_val3 ? p_val2 : p_val3);
    }

}
