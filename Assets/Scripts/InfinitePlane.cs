using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfinitePlane : MonoBehaviour
{
    public Material m_material = null;
    private const float LARGE_NUMBER = 1000;

    private void Start()
    {
        MeshFilter filter = gameObject.AddComponent<MeshFilter>();
        MeshRenderer renderer = gameObject.AddComponent<MeshRenderer>();
        Mesh infiniteMesh = new Mesh();

        Vector3[] vertices = new Vector3[4];
        Vector3[] normals = new Vector3[4];
        int[] tris = new int[6];

        vertices[0] = new Vector3(LARGE_NUMBER, 0.0f, LARGE_NUMBER);
        vertices[1] = new Vector3(LARGE_NUMBER, 0.0f, -LARGE_NUMBER);
        vertices[2] = new Vector3(-LARGE_NUMBER, 0.0f, -LARGE_NUMBER);
        vertices[3] = new Vector3(-LARGE_NUMBER, 0.0f, LARGE_NUMBER);

        for (int normalIndex = 0; normalIndex < 4; normalIndex++)
        {
            normals[normalIndex] = transform.up;
        }

        tris[0] = 0;
        tris[1] = 1;
        tris[2] = 2;

        tris[3] = 0;
        tris[4] = 2;
        tris[5] = 3;

        infiniteMesh.vertices = vertices;
        infiniteMesh.normals = normals;
        infiniteMesh.triangles = tris;
        infiniteMesh.RecalculateTangents();

        filter.sharedMesh = infiniteMesh;

        Material material = Instantiate(m_material);
        renderer.material = material;
    }
}
