using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;



[DefaultExecutionOrder(0)]
public class DrawGeometry : MonoBehaviour
{
    Mesh mesh;


    List<Vector3> newVertices;
    List<int> newTriangles;
    List<Vector2> newUVs;

    private void Start()
    {
        mesh = gameObject.GetComponent<MeshFilter>().mesh;

    }

    public void ClearMesh()
    {
        newVertices = new List<Vector3>();
        newTriangles = new List<int>();
        newUVs = new List<Vector2>();
    }


    public void AddTriangle(Vector3 p1, Vector3 p2, Vector3 p3, Vector2 uv)
    {
        newVertices.Add(p1);
        newVertices.Add(p2);
        newVertices.Add(p3);

        newTriangles.Add(newVertices.Count - 2);
        newTriangles.Add(newVertices.Count - 3);
        newTriangles.Add(newVertices.Count - 1);

        newUVs.Add(uv);
        newUVs.Add(uv);
        newUVs.Add(uv);
    }

    public void AddQuad(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, Vector2 uv)
    {
        newVertices.Add(p1);
        newVertices.Add(p2);
        newVertices.Add(p3);
        newVertices.Add(p4);

        newTriangles.Add(newVertices.Count - 4);
        newTriangles.Add(newVertices.Count - 3);
        newTriangles.Add(newVertices.Count - 2);

        newTriangles.Add(newVertices.Count - 2);
        newTriangles.Add(newVertices.Count - 3);
        newTriangles.Add(newVertices.Count - 1);

        newUVs.Add(uv);
        newUVs.Add(uv);
        newUVs.Add(uv);
        newUVs.Add(uv);
    }

    public void DrawMesh()
    {
        if (newVertices.Count == 0) return;   

        mesh.Clear();

        mesh.vertices = newVertices.ToArray();
        mesh.triangles = newTriangles.ToArray();
        mesh.uv = newUVs.ToArray();

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        //ExportToOBJ.ExportSelectedToOBJ(gameObject);
    }
}
