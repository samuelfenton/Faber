using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxeliserCompanion : MonoBehaviour
{
    public const int MAX_SIZE = 64;
    public const int N_FRAMES_PER_UPDATE = 1;

    public const int MAX_VOXEL_COUNT = 6000;

    public GameObject m_voxel = null;
    public float m_voxelSize = 1;

    public ComputeShader m_voxelComputeShader = null;
    protected int m_voxelKernal = 0;

    protected int m_meshTriCount = 0;

    protected Mesh m_modelMesh = null;

    protected ComputeBuffer m_inBufferMeshVert = null;
    protected ComputeBuffer m_inBufferMeshTri = null;

    protected ComputeBuffer m_outBufferVoxelCount = null;
    protected ComputeBuffer m_outBufferVoxelPositions = null;

    protected Queue<GameObject> m_voxelsAvailable = new Queue<GameObject>();
    protected Queue<GameObject> m_voxelsInUse = new Queue<GameObject>();
    protected HashSet<Vector3> m_voxelPositions= new HashSet<Vector3>();

    private Vector3 m_poolingLocation = new Vector3(-2000, -2000, -2000);

    protected virtual void Start ()
    {
        //Voxel pooling
        for (int i = 0; i < MAX_VOXEL_COUNT; i++)
        {
            GameObject newVoxel = Instantiate(m_voxel);
            newVoxel.transform.localScale = new Vector3(m_voxelSize, m_voxelSize, m_voxelSize);
            newVoxel.transform.position = m_poolingLocation;
            //newVoxel.SetActive(false);
            m_voxelsAvailable.Enqueue(newVoxel);
        }

        m_voxelComputeShader = Instantiate(m_voxelComputeShader);

        MeshInit();

        if (m_modelMesh == null)
        {
            Debug.Log("Unable to get mesh");
            Destroy(this);
        }
        
        Vector3[] meshVerts = m_modelMesh.vertices;
        int[] meshTris = m_modelMesh.triangles;
        m_meshTriCount = meshTris.Length;

        m_voxelKernal = m_voxelComputeShader.FindKernel("Voxelise");

        //Init
        m_inBufferMeshVert = new ComputeBuffer(meshVerts.Length, sizeof(float) * 3);
        m_inBufferMeshTri = new ComputeBuffer(meshTris.Length, sizeof(int));

        m_outBufferVoxelCount = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
        m_outBufferVoxelPositions = new ComputeBuffer(MAX_VOXEL_COUNT, sizeof(float) * 3, ComputeBufferType.Append);

        //Setup
        m_inBufferMeshVert.SetData(meshVerts);
        m_inBufferMeshTri.SetData(meshTris);
        m_outBufferVoxelPositions.SetData(new Vector3[MAX_VOXEL_COUNT]);

        //Join to shader
        m_voxelComputeShader.SetFloat("PARAM_gridSize", m_voxelSize);

        m_voxelComputeShader.SetMatrix("IN_transformMatrix", transform.localToWorldMatrix);

        m_voxelComputeShader.SetBuffer(m_voxelKernal, "IN_verts", m_inBufferMeshVert);
        m_voxelComputeShader.SetBuffer(m_voxelKernal, "IN_tris", m_inBufferMeshTri);

        m_voxelComputeShader.SetBuffer(m_voxelKernal, "OUT_voxelPositions", m_outBufferVoxelPositions);
        m_outBufferVoxelPositions.SetCounterValue(0);

        StartCoroutine(VoxeliserUpdate());
    }

    protected void OnDestroy()
    {
        if (m_inBufferMeshVert != null) m_inBufferMeshVert.Dispose();
        if (m_inBufferMeshTri != null) m_inBufferMeshTri.Dispose();

        if (m_outBufferVoxelCount != null) m_outBufferVoxelCount.Dispose();
        if (m_outBufferVoxelPositions != null) m_outBufferVoxelPositions.Dispose();
    }

    protected virtual void MeshInit()
    {

    }

    protected virtual IEnumerator VoxeliserUpdate()
    {
        yield return null;
        if (Time.frameCount % (N_FRAMES_PER_UPDATE) == 0) //Run every N frames
        {
            UpdateMesh();
            ConvertToVoxels();
        }
        StartCoroutine(VoxeliserUpdate());
    }

    protected virtual void UpdateMesh()
    {
    }

    private int[] counter = new int[1];
    Vector3[] outputArray = new Vector3[MAX_VOXEL_COUNT];
    protected void ConvertToVoxels()
    {
        Vector3[] meshVerts = m_modelMesh.vertices;

        //Setup data
        //Update Shader
        m_voxelComputeShader.SetMatrix("IN_transformMatrix", transform.localToWorldMatrix);

        m_inBufferMeshVert.SetData(meshVerts);

        m_outBufferVoxelPositions.SetCounterValue(0);

        //Shader time
        //m_voxelComputeShader.Dispatch(m_voxelKernal, m_meshTriCount / (16 * 3), m_meshTriCount / (16 * 3), 1);
        m_voxelComputeShader.Dispatch(m_voxelKernal, m_meshTriCount / (8 * 3), 1, 1);

        //Get back data
        ComputeBuffer.CopyCount(m_outBufferVoxelPositions, m_outBufferVoxelCount, 0);
        m_outBufferVoxelCount.GetData(counter);

        m_outBufferVoxelPositions.GetData(outputArray, 0, 0, counter[0]);

        //Debug.Log(counter[0]);

        //Build voxel postions
        m_voxelPositions.Clear();

        int perCoroutineCount = counter[0] / N_FRAMES_PER_UPDATE;

        for (int coroutineIndex = 0; coroutineIndex < N_FRAMES_PER_UPDATE; coroutineIndex++)
        {
            for (int i = 0; i < perCoroutineCount; i++)
            {
                Vector3 newPosition = outputArray[coroutineIndex * perCoroutineCount + i];

                if (!m_voxelPositions.Contains(newPosition))//Hasnt already been added
                {
                    m_voxelPositions.Add(newPosition);
                }
            }
            //yield return null;
        }

        ClearVoxels();
        AddVoxels();
    }

    protected void ClearVoxels()
    {
        foreach (GameObject currentVoxel in m_voxelsInUse)
        {
            m_voxelsAvailable.Enqueue(currentVoxel);
            currentVoxel.transform.position = m_poolingLocation;
            //currentVoxel.SetActive(false);
        }

        m_voxelsInUse.Clear();
    }

    protected void AddVoxels()
    {
        foreach (Vector3 position in m_voxelPositions)
        {
            if (m_voxelsAvailable.Count <= 0)//Early breakout if not enough voxels
                break;

            GameObject newVoxel = m_voxelsAvailable.Dequeue();
            newVoxel.transform.position = position;
            //newVoxel.SetActive(true);

            m_voxelsInUse.Enqueue(newVoxel);
        }
    }
}
