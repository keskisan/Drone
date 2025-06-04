using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FormationManager : MonoBehaviour
{
    public FormationType currentFormation;

    [System.Serializable]
    public class Formation
    {
        public FormationType type;
        public GameObject meshObject;
    }

    public Formation[] formations;

    List<int> verticesUsedIndexPosition = new List<int>();
    //int[] verticesUsedIndexPositionArray = new int[500];

    public Vector2[] worldUvs;

    public Vector3[] GetTargetPositions() //every frame
    {
        GameObject target = null;

        foreach (var f in formations)
        {
            if (f.type == currentFormation)
            {
                target = f.meshObject;
                break;
            }
        }

        if (target == null)
        {
            Debug.LogError("Target is null");
            return null;
        }

        Mesh mesh;
        MeshFilter meshfilter;
        bool useSkinnedTransform;
        if (target.TryGetComponent<MeshFilter>(out meshfilter))
        {
            mesh = meshfilter.sharedMesh;
            useSkinnedTransform = false;
        }
        else
        {
            SkinnedMeshRenderer skin = target.GetComponent<SkinnedMeshRenderer>();
            mesh = new Mesh();
            skin.BakeMesh(mesh);
            useSkinnedTransform = true;
        }
 




        //Vector3[] localVerts = mesh.vertices;

        //Vector3[] worldVerts = new Vector3[localVerts.Length];

        //for (int i = 0; i < localVerts.Length && i < 500; i++)
        //{
        //    worldVerts[i] = target.transform.TransformPoint(localVerts[i]);
        //}

        Vector3[] localVerts = mesh.vertices;
        Vector2[] meshuvs = mesh.uv;

        Vector3[] worldVerts = new Vector3[localVerts.Length];
        worldUvs = new Vector2[localVerts.Length];

        for (int i = 0; i < localVerts.Length && i < 500; i++)
        {
            if (verticesUsedIndexPosition.Count >= 500)
            {
                if (useSkinnedTransform)
                {
                    worldVerts[i] = target.transform.TransformDirection(localVerts[verticesUsedIndexPosition[i]]) + target.transform.position;
                }
                else
                {
                    worldVerts[i] = target.transform.TransformPoint(localVerts[verticesUsedIndexPosition[i]]);
                }
                worldUvs[i] = meshuvs[verticesUsedIndexPosition[i]];
            }
            else
            {
                return null;
            }
        }

        return worldVerts;
    }

    public void GetAssignedTargetPositions(Vector3[] currentDronePositions)
    {
        GameObject target = formations.FirstOrDefault(f => f.type == currentFormation)?.meshObject;

        if (target == null)
        {
            Debug.LogError("Formation target missing");

        }

        Mesh mesh;
        MeshFilter meshfilter;
        bool useSkinnedTransform;
        if (target.TryGetComponent<MeshFilter>(out meshfilter))
        {
            mesh = meshfilter.sharedMesh;
            useSkinnedTransform = false;
        }
        else
        {
            SkinnedMeshRenderer skin = target.GetComponent<SkinnedMeshRenderer>();
            mesh = new Mesh();
            skin.BakeMesh(mesh);
            useSkinnedTransform = true;
        }

        Vector3[] allVerts = mesh.vertices;
        int count = Mathf.Min(500, allVerts.Length);

        System.Random rand = new System.Random();
        HashSet<int> usedIndices = new HashSet<int>();
        List<Vector3> availableTargets = new List<Vector3>();
        verticesUsedIndexPosition = new List<int>();

        while (availableTargets.Count < count)
        {
            int index = rand.Next(allVerts.Length);
            if (!usedIndices.Contains(index))
            {
                usedIndices.Add(index);
                Vector3 worldPos;
                if (useSkinnedTransform)
                {
                    worldPos = target.transform.TransformDirection(allVerts[index]) + target.transform.position;
                }
                else
                {
                    worldPos = target.transform.TransformPoint(allVerts[index]);
                }
                
                availableTargets.Add(worldPos);
                verticesUsedIndexPosition.Add(index);
            }
        }

        /*Vector3[] assignedTargets = new Vector3[500];


        for (int i = 0; i < 500; i++)
        {
            float worstMinDist = -1f;
            int worstDrone = -1;

            // Find drone furthest from any unassigned target
            for (int j = 0; j < currentDronePositions.Length; j++)
            {
                if (assignedTargets[j] != Vector3.zero) continue;

                float closestDist = float.MaxValue;
                for (int k = 0; k < availableTargets.Count; k++)
                {
                    if (usedIndices.Contains(k)) continue;
                    float dist = Vector3.Distance(currentDronePositions[j], availableTargets[k]);
                    if (dist < closestDist)
                        closestDist = dist;
                }

                if (closestDist > worstMinDist)
                {
                    worstMinDist = closestDist;
                    worstDrone = j;
                }
            }

            // Assign closest available vertex to this drone
            int closestIdx = -1;
            float minDist = float.MaxValue;
            for (int k = 0; k < availableTargets.Count; k++)
            {
                if (usedIndices.Contains(k)) continue;
                float dist = Vector3.Distance(currentDronePositions[worstDrone], availableTargets[k]);
                if (dist < minDist)
                {
                    minDist = dist;
                    closestIdx = k;
                }
            }

            if (closestIdx != -1)
            {
                assignedTargets[worstDrone] = availableTargets[closestIdx];
                verticesUsedIndexPositionArray[worstDrone] = closestIdx;
                usedIndices.Add(closestIdx);
            }
        }

        for (int i = 0; i < 500; i++)
        {
            Debug.Log(verticesUsedIndexPositionArray[i]);
        }*/
    }
}
