using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebuggingColorTrianglesByNormalYShader : MonoBehaviour
{
	Mesh mesh = null;
	Vector3 rotation = new Vector3();
	// Start is called before the first frame update
	void Start()
	{
		mesh = GetComponent<MeshFilter>().mesh;

		Vector3[] newVerticesArray = new Vector3[mesh.triangles.Length];
		Vector3[] newNormals = new Vector3[mesh.triangles.Length];
		int[] newTriangles = new int[mesh.triangles.Length];

		int[] triangles = mesh.triangles;
		Vector3[] oldVertices = mesh.vertices;
		Vector3[] oldNormals = mesh.normals;

		for (int i = 0; i < newVerticesArray.Length; i += 3)
		{
			newVerticesArray[i] = oldVertices[triangles[i]]; ;
			newVerticesArray[i + 1] = oldVertices[triangles[i + 1]]; ;
			newVerticesArray[i + 2] = oldVertices[triangles[i + 2]]; ;

			newNormals[i] = oldNormals[triangles[i]]; ;
			newNormals[i + 1] = oldNormals[triangles[i + 1]]; ;
			newNormals[i + 2] = oldNormals[triangles[i + 2]]; ;

			newTriangles[i] = i;
			newTriangles[i + 1] = i + 1;
			newTriangles[i + 2] = i + 2;

		}

		mesh.vertices = newVerticesArray;
		mesh.triangles = newTriangles;
		mesh.normals = newNormals;



		recolorMesh();


		rotation = transform.rotation.eulerAngles;
	}

	private void Update()
	{
		if(rotation != transform.rotation.eulerAngles)
		{
			recolorMesh();
			rotation = transform.rotation.eulerAngles;
		}
	}

	void recolorMesh()
	{
		Color32[] newColors = new Color32[mesh.triangles.Length];
		for (int i = 0; i < newColors.Length; i++)
		{
			Quaternion rotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z);
			Vector3 normal = rotation * mesh.normals[i];

            //newColors[i] = new Color32(255, 165, 0, 255);
            
            if (normal.y <= -0.5f)
            {
                // ceiling
                // orange
                newColors[i] = new Color32(255, 165, 0, 255);
            }
            else if (normal.y >= -0.174f && normal.y < -0.09f)
            {
                // dark blue wall facing down
                newColors[i] = new Color32(0, 0, 139, 255);
            }
            else  if (normal.y >= -0.09f && normal.y <= 0.09f)
            {
                // wall
                // blue
                newColors[i] = new Color32(0, 191, 255, 255);
            }
            else if (normal.y > 0.09f && normal.y <= 0.174f)
            {
                // light blue wall facing up
                newColors[i] = new Color32(135, 206, 250, 255);
            }
            else if(normal.y >= 0.707f)
            {
                // regular floor
                // light green
                newColors[i] = new Color32(124, 252, 0, 255);
            }
            else if(normal.y < 0.707f && normal.y >= 0.5f)
            {
                // slippery floor
                // dark green
                newColors[i] = new Color32(0, 100, 0, 255);
            }
            else
            {
                // bad triangular face
                // red
                newColors[i] = new Color32(255, 0, 0, 255);
            }

            /*
			if(normal.y < -0.09f){
				newColors[i] = Color.red;
			}
			else if(normal.y < 0.09f){
				newColors[i] = Color.blue;
			}
			else if(normal.y < 0.5f){
				newColors[i] = Color.red;
			}
			else{
				newColors[i] = Color.green;
			}
            */
		}
        
		mesh.colors32 = newColors;
	}
}
