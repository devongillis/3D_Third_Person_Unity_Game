using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class shader : MonoBehaviour
{
    void Start () {
    Mesh mesh = GetComponent<MeshFilter>().mesh;        
    SplitMesh(mesh);
    SetColors(mesh);
}

void SplitMesh(Mesh mesh)
{
    int[] triangles = mesh.triangles; 
    Vector3[] verts = mesh.vertices;
    Vector3[] normals = mesh.normals;
    Vector2[] uvs = mesh.uv;

    Vector3[] newVerts;
    Vector3[] newNormals;
    Vector2[] newUvs;

    int n = triangles.Length;
    newVerts   = new Vector3[n];
    newNormals = new Vector3[n];
    newUvs     = new Vector2[n];

    for(int i = 0; i < n; i++)
    {
			Debug.Log("help");
        newVerts[i] = verts[triangles[i]];
        newNormals[i] = normals[triangles[i]];
        if (uvs.Length > 0)
        {
            newUvs[i] = uvs[triangles[i]];
        }
        triangles[i] = i; 
    }        
    mesh.vertices = newVerts;
    mesh.normals = newNormals;
    mesh.uv = newUvs;        
    mesh.triangles = triangles;
		Debug.Log("yes");
}   
void SetColors(Mesh mesh)
{
    Color[] colors = new Color[mesh.vertexCount];
    for (int i = 0; i < colors.Length; i+=3)
    {
        colors[i] = Color.red;
        colors[i + 1] = Color.green;
        colors[i + 2] = Color.blue;
    }
    mesh.colors = colors;
}
}
