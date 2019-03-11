using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class ModifyCubeMesh : MonoBehaviour {

    public float newSize = 0.5f;
    public bool activateMeshRenderer = true;
    public bool activateMeshCollider = true;

	void Start () {
        MeshFilter filter = GetComponent<MeshFilter>();
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        MeshCollider collider = GetComponent<MeshCollider>();
        Mesh mesh = filter.mesh;
        Vector3[] vertices = mesh.vertices;
        Vector2[] uv = mesh.uv;

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 vertex = vertices[i];

            if (0.5f == vertex.x)
            {
                vertex.x = newSize;
            }
            if (0.5f == vertex.y)
            {
                vertex.y = newSize;
            }
            if (0.5f == vertex.z)
            {
                vertex.z = newSize;
            }

            if (-0.5f == vertex.x)
            {
                vertex.x = -newSize;
            }
            if (-0.5f == vertex.y)
            {
                vertex.y = -newSize;
            }
            if (-0.5f == vertex.z)
            {
                vertex.z = -newSize;
            }

            vertices[i] = vertex;
        }
        mesh.vertices = vertices;

        for (int i = 0; i < uv.Length; i++)
        {
            Vector2 texCoord = uv[i];
            float newTexModifier = newSize * 2;

            if (1f == texCoord.x)
            {
                texCoord.x = newTexModifier;
            }
            if (1f == texCoord.y)
            {
                texCoord.y = newTexModifier;
            }
            uv[i] = texCoord;
        }
        mesh.uv = uv;

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();

        filter.mesh = mesh;


        if (collider)
        {
            collider.sharedMesh = mesh;
            if (activateMeshCollider)
            {
                collider.enabled = true;
            }
        }

        if (activateMeshRenderer)
        {
            renderer.enabled = true;
        }
    }

    void DebugLogArray<T>(T[] array)
    {
        string output = "";
        string delimiter = "";

        foreach (T element in array)
        {
            output += delimiter + element;
            delimiter = ", ";
        }

        Debug.Log(output);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = GetComponent<MeshRenderer>().sharedMaterial.color;
        Gizmos.DrawCube(transform.position, new Vector3(newSize*2, transform.localScale.y, newSize*2));
    }

}
