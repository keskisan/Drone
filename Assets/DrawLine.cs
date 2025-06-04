using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(1)]
public class DrawLine : MonoBehaviour
{
    [SerializeField]
    DrawGeometry drawGeometry;

    [SerializeField]
    float thickness = 0.1f;

    List<List<Vector3>> lineList = new List<List<Vector3>>();
    List<Vector2> uvList;

    public void AddLine(Vector3 p1, Vector3 p2, Vector2 uvs)
    {
        List<Vector3> line = new List<Vector3>();

        line.Add(p1);
        line.Add(p2);
        
        lineList.Add(line);
        uvList.Add(uvs);
    }

    public void ClearLine()
    {
        lineList = new List<List<Vector3>>();
        uvList = new List<Vector2>();
        drawGeometry.ClearMesh();
    }

    public void UpdateLines()
    {
        AddGeometryForAllLines();
        drawGeometry.DrawMesh();
    }

    private void AddGeometryForAllLines() //power transfer towers each has a list of every tower that conects to them
    {
        for (int i = 0; i < lineList.Count; i++)
        {
            Vector3 point1 = lineList[i][0];
            Vector3 point2 = lineList[i][1];

            Vector3 vector = point2 - point1;
            Vector3 vectorNorm = vector.normalized;

            Vector3 perp1 = Vector3.Cross(vector, point2.normalized).normalized; //this can bug out if these vectors happen to be parallel. No intrinsic meaning to point2 I just need a vector thats not parallel or 0s
            Vector3 perp2 = Vector3.Cross(vector, perp1).normalized;

            perp1 = perp1 * thickness;
            perp2 = perp2 * thickness;



            drawGeometry.AddQuad(point1 + perp1 + perp2, point1 + perp1 - perp2, point2 + perp1 + perp2, point2 + perp1 - perp2, uvList[i]);
            drawGeometry.AddQuad(point1 + perp1 + perp2, point1 - perp1 + perp2, point2 + perp1 + perp2, point2 - perp1 + perp2, uvList[i]);
            drawGeometry.AddQuad(point1 - perp1 - perp2, point1 + perp1 - perp2, point2 - perp1 - perp2, point2 + perp1 - perp2, uvList[i]);
            drawGeometry.AddQuad(point1 - perp1 - perp2, point1 - perp1 + perp2, point2 - perp1 - perp2, point2 - perp1 + perp2, uvList[i]);

            drawGeometry.AddQuad(point1 - perp1 - perp2, point1 - perp1 + perp2, point1 + perp1 - perp2, point1 + perp1 + perp2, uvList[i]); //end caps
            drawGeometry.AddQuad(point2 - perp1 - perp2, point2 - perp1 + perp2, point2 + perp1 - perp2, point2 + perp1 + perp2, uvList[i]);
        }  
    }
}
