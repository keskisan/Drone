using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class ExportToOBJ : MonoBehaviour
{
    public static void ExportSelectedToOBJ(GameObject gameObject)
    {
        string path ="TestingNew.obj";
        if (string.IsNullOrEmpty(path)) return;

        MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            Debug.LogError("Selected object does not have a MeshFilter component.");
            return;
        }

        Mesh mesh = meshFilter.sharedMesh;
        using (StreamWriter sw = new StreamWriter(path))
        {
            sw.Write(MeshToString(mesh));
        }

        Debug.Log("Exported to " + path);
    }

    static string MeshToString(Mesh mesh)
    {
        StringWriter sw = new StringWriter();
        sw.WriteLine("g " + mesh.name);

        foreach (Vector3 v in mesh.vertices)
        {
            sw.WriteLine(string.Format("v {0} {1} {2}", v.x, v.y, v.z));
        }
        sw.WriteLine();

        foreach (Vector3 v in mesh.normals)
        {
            sw.WriteLine(string.Format("vn {0} {1} {2}", v.x, v.y, v.z));
        }
        sw.WriteLine();

        foreach (Vector3 v in mesh.uv)
        {
            sw.WriteLine(string.Format("vt {0} {1}", v.x, v.y));
        }

        for (int i = 0; i < mesh.subMeshCount; i++)
        {
            sw.WriteLine();
            int[] triangles = mesh.GetTriangles(i);
            for (int j = 0; j < triangles.Length; j += 3)
            {
                sw.WriteLine(string.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}",
                    triangles[j] + 1, triangles[j + 1] + 1, triangles[j + 2] + 1));
            }
        }

        return sw.ToString();
    }

}
