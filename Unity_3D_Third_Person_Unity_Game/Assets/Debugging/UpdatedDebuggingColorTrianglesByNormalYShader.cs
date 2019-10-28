using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdatedDebuggingColorTrianglesByNormalYShader : MonoBehaviour
{
    Mesh mesh = null;
    Vector3 rotation = new Vector3();
    Vector3 scale = new Vector3();



    // all of these values are the angle of the normal from the vertical plane,clockwise
    public float regularFloorStart = 0;
    public float regularFloorSemiSlipperyFloor = 30; // anything below is normal, above is semi slippery
    public float SemiSlipperyFloorSlipperyFloor = 60; // anything below is semi slippery, above is slippery
    public float SlipperyFloorWall = 80; // anything below is slippery, above is wall
    public float wallCeiling = 100; // anything below is wall, above is ceiling
    public float ceiling = 180;

    public float regularFloorStartArc;
    public float regularFloorSemiSlipperyFloorArc;
    public float semiSlipperyFloorSlipperyFloorArc;
    public float SlipperyFloorWallArc;
    public float wallCeilingArc;
    public float ceilingArc;

    public float arcRatio = (2 * Mathf.PI / 360);


    // a regular floor is defined as any surface with normal.y between 0.866 and 1.0
    public float floorUpperLimitNormalY;// = 1.0f; // <=
    public float floorLowerLimitNormalY;// = 0.866f; // >=
    // a semi slippery floor is defined as any surface with normal.y between 0.5 and 0.866
    public float semiSlipperyFloorUpperLimitNormalY; // = 0.866f; // <
    public float semiSlipperyFloorLowerLimitNormalY; // = 0.5f; // <
    // a slippery floor is defined as any surface with normal.y between 0.174 and 0.5
    public float slipperyFloorUpperLimitNormalY;// = 0.5f; // <
    public float slipperyFloorLowerLimitNormalY;// = 0.174f; // >=
    // a wall is defined as any surface with normal.y between -0.174 and 0.174
    public float wallUpperLimitNormalY;// = 0.174f; // <=
    public float wallLowerLimitNormalY;// = -0.174f; // >=
    // a ceiling is defined as any surface with normal.y between -1.0 and -0.174
    public float ceilingUpperLimitNormalY;// = -0.174; // <=
    // -1.0f is the minimum value for normal.y so we don't need to use this value for ceilings
    
    // Start is called before the first frame update
    void Start()
    {







        regularFloorStartArc = regularFloorStart * arcRatio;
        regularFloorSemiSlipperyFloorArc = regularFloorSemiSlipperyFloor * arcRatio;
        semiSlipperyFloorSlipperyFloorArc = SemiSlipperyFloorSlipperyFloor * arcRatio;
        SlipperyFloorWallArc = SlipperyFloorWall * arcRatio;
        wallCeilingArc = wallCeiling * arcRatio;
        ceilingArc = ceiling * arcRatio;


        // a regular floor is defined as any surface with normal.y between   1.000  and  0.707
        // a slippery floor is defined as any surface with normal.y between  0.707  and  0.174
        // a wall is defined as any surface with normal.y between            0.174  and -0.174
        // a ceiling is defined as any surface with normal.y between        -0.174  and -1.000

        floorUpperLimitNormalY = 1.0f; // floor is <= 1.0
        floorLowerLimitNormalY = Mathf.Sin((90 - regularFloorSemiSlipperyFloor) * arcRatio); // floor is >= 0.866


        semiSlipperyFloorUpperLimitNormalY = floorLowerLimitNormalY; // semi slippery is < 0.866f
        semiSlipperyFloorLowerLimitNormalY = Mathf.Sin((90 - SemiSlipperyFloorSlipperyFloor) * arcRatio); // semi slippery is >= 0.5f

        slipperyFloorUpperLimitNormalY = semiSlipperyFloorLowerLimitNormalY; // slippery is < 0.5
        slipperyFloorLowerLimitNormalY = Mathf.Sin((90 - SlipperyFloorWall) * arcRatio); // slippery is > 0.174

        wallUpperLimitNormalY = slipperyFloorLowerLimitNormalY; // wall is <= 0.174
        wallLowerLimitNormalY = -wallUpperLimitNormalY; // wall is >= -0.174

        ceilingUpperLimitNormalY = Mathf.Sin((90 - wallCeiling) * arcRatio); // ceiling is < -0.174













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
        scale = transform.localScale;
    }

    private void Update()
    {
        if (rotation != transform.rotation.eulerAngles || scale != transform.localScale)
        {
            recolorMesh();
            rotation = transform.rotation.eulerAngles;
            scale = transform.localScale;
            Debug.Log(scale);
        }
    }

    void recolorMesh()
    {
        Color32[] newColors = new Color32[mesh.triangles.Length];
        for (int i = 0; i < newColors.Length; i++)
        {
            Quaternion rotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z);
            Vector3 normal = rotation * mesh.normals[i];
            normal.x /= transform.localScale.x;
            normal.y /= transform.localScale.y;
            normal.z /= transform.localScale.z;
            normal = normal.normalized;

            // should include "super" edges (floors and walls that are perfect

            if (normal.y < ceilingUpperLimitNormalY /*- 0.5f*/)
            {
                // ceiling
                // orange
                newColors[i] = new Color32(255, 165, 0, 255);
            }
            else if (normal.y >= wallLowerLimitNormalY /*- 0.174f*/ && normal.y < -0.09f)
            {
                // dark blue wall facing down
                newColors[i] = new Color32(0, 0, 139, 255);
            }
            else if (normal.y >= -0.09f && normal.y <= 0.09f)
            {
                // wall
                // blue
                newColors[i] = new Color32(0, 191, 255, 255);
            }
            else if (normal.y > 0.09f && normal.y <= wallUpperLimitNormalY /*0.174f*/)
            {
                // light blue wall facing up
                newColors[i] = new Color32(135, 206, 250, 255);
            }
            else if (normal.y >= floorLowerLimitNormalY /*0.707f*/)
            {
                // regular floor
                // light green
                newColors[i] = new Color32(144, 238, 144, 255);
            }
            else if (normal.y < semiSlipperyFloorUpperLimitNormalY && normal.y >= semiSlipperyFloorLowerLimitNormalY)
            {
                // semi slippery floor
                // green
                newColors[i] = new Color32(0, 255, 0, 255);
            }
            else if (normal.y < slipperyFloorUpperLimitNormalY /*0.707f*/ && normal.y > slipperyFloorLowerLimitNormalY /*0.5f*/)
            {
                // slippery floor
                // dark green
                newColors[i] = new Color32(0, 100, 0, 255);
            }
            else
            {
                // bad triangular face, error
                // red
                newColors[i] = new Color32(255, 0, 0, 255);
            }

        }

        mesh.colors32 = newColors;
    }
}
