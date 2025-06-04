using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public enum FormationType
{
    Sphere,
    Helix,
    Torus,
    Cylinder,
    Monkey,
    Koi
}



[DefaultExecutionOrder(2)]
public class Drones : MonoBehaviour
{
    public Vector3[] positions;
    Vector3[] velocities, previousPositions;
    bool[] danger;
    int[] dangerFrames;

    [SerializeField]
    DrawLine drawLine, drawLineDanger;

    [SerializeField]
    float timestep = 0.1f, areaHeight = 80f, areaRadius = 150f, droneHeight = 0.1f,
        minDistanceBetweenDrones = 3f, criticalDistanceBetweenDrones = 2f, maxSeperatingSpeed = 1.5f, moveTowardsTargetSpeed = 1f, Drag = 0.5f, maxDroneVelocity = 4f,
        avoidSidesBuffer = 2f, avoidDronesBuffer = 1f, jitterStrength = 0.5f, easeInDistance = 4f;

    [SerializeField]
    int jitterThreshold = 10; // Number of frames in danger before jittering

    [SerializeField]
    FormationManager formationManager;

    Vector3[] targetPositions;
    Vector2[] targetUvs;

    public bool moveToTarget = false;

    private void AssignRandomPositions(Vector3[] positionArray)
    {
        for (int i = 0; i < positionArray.Length; i++)
        {
            bool valid = false;
            int count = 0;
            while (!valid) 
            {
                count++;
                if (count >= 100)
                {
                    Debug.Log("no valid position found in while loop");
                    return;
                }
                float height = Random.Range(0f, areaHeight);
                float radius = Random.Range(0f, areaRadius);
                Vector2 position2D = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized * radius;
                Vector3 position3D = new Vector3(position2D.x, height, position2D.y);

                bool doUse = true;

                for (int j = 0; j < positionArray.Length; j++)
                {
                    if (Vector3.Distance(positionArray[j], position3D) < minDistanceBetweenDrones + avoidDronesBuffer)
                    {
                        doUse = false;
                        continue; //not valid    
                    }
                } 
                if (doUse)
                {
                    positionArray[i] = position3D;
                    valid = true;
                }
            }
        }

        SetTargetPositionToDroneCurrentPosition(positionArray);
    }

    private void SetTargetPositionToDroneCurrentPosition(Vector3[] positionArray) //if no formation just do nothing
    {
        for (int i = 0; i < positionArray.Length; i++)
        {

            targetPositions[i] = positionArray[i];
            targetUvs[i] = new Vector2(0.5f, 0.5f);
        }
    }

    private void DrawDrones(Vector3[] positionArray)
    {
        Vector3 height = Vector3.up * droneHeight;
        for (int i = 0; i < positionArray.Length; i++)
        {
            if (danger[i])
            {
                drawLineDanger.AddLine(positionArray[i], positionArray[i] + height, targetUvs[i]);
            }
            else
            {
                drawLine.AddLine(positionArray[i], positionArray[i] + height, targetUvs[i]);
            }
        }
    }

    private void MinDistance(Vector3[] positionArray, Vector3[] velocityArray, float minDist)
    {
        float minDistWBuffer = avoidDronesBuffer + minDist;
        for (int i = 0; i < positionArray.Length; i++)
        {
            for (int j = i + 1; j < positionArray.Length; j++)
            {
                float distance = Vector3.Distance(positionArray[i], positionArray[j]);
                if (distance < minDistWBuffer)
                {
                    //float strength = 1f - Mathf.InverseLerp(minDist, minDistWBuffer, distance);
                    float strength = Mathf.Pow(1f - Mathf.InverseLerp(minDist, minDistWBuffer, distance), 2f); // quadratic
                    Vector3 dir = (positionArray[i] - positionArray[j]).normalized;

                    Vector3 repulsion = dir * (maxSeperatingSpeed * strength);

                    velocityArray[i] += repulsion;
                    velocityArray[j] -= repulsion;
                }

                if (distance < criticalDistanceBetweenDrones)
                {
                    Debug.Log("Drone too close to another : " + i + " " + j);
                    danger[i] = true;
                    danger[j] = true;
                }
            }
        }
    }

    private void AvoidSides(Vector3[] positionArray, Vector3[] velocityArray)
    {
        for (int i = 0; i < positionArray.Length; i++)
        {
            if (positionArray[i].y < avoidSidesBuffer)
            {
                velocityArray[i] += Vector3.up * maxSeperatingSpeed;
                if (positionArray[i].y < 0f)
                {
                    Debug.Log("Drone went below ground : " + i);
                    danger[i] = true;
                }
            }

            if (positionArray[i].y > areaHeight - avoidSidesBuffer)
            {
                velocityArray[i] += Vector3.down * maxSeperatingSpeed;
                if (positionArray[i].y > areaHeight)
                {
                    Debug.Log("Drone went above height : " + i);
                    danger[i] = true;
                }
            }

            float distanceFromCenter = Mathf.Sqrt(positionArray[i].x * positionArray[i].x + positionArray[i].z * positionArray[i].z);
            float distanceMax = areaRadius - avoidSidesBuffer;
            if (distanceFromCenter > distanceMax)
            {
                Vector3 pos = positionArray[i];
                Vector3 directionToCenter = (-new Vector3(pos.x, 0, pos.z)).normalized;
                velocityArray[i] += directionToCenter * maxSeperatingSpeed;
            }
            if (distanceFromCenter > areaRadius)
            {
                Debug.Log("Drone went outside area : " + i);
                danger[i] = true;
            }
        }
    }

    private void UpdatePositions(Vector3[] positionArray, Vector3[] velocityArray)
    {
        for (int i = 0; i < positionArray.Length; i++)
        {
            if (velocityArray[i].magnitude > maxDroneVelocity) // can modify later if want descent max 3, horizontal 6, climb mixed 4 //towards audience max 4 audience is on one side somewhere
            {
                velocityArray[i] = velocityArray[i].normalized * maxDroneVelocity;
            }

            velocityArray[i] -= velocityArray[i] * Drag * timestep;

            positionArray[i] += velocityArray[i] * timestep;
        }
    }

    private void UpdateDangerStates()
    {
        for (int i = 0; i < danger.Length; i++)
        {
            bool isStillInDanger = false;

            if (positions[i].y < 0f || positions[i].y > areaHeight)
                isStillInDanger = true;

            float distFromCenter = Mathf.Sqrt(positions[i].x * positions[i].x + positions[i].z * positions[i].z);
            if (distFromCenter > areaRadius)
                isStillInDanger = true;

            for (int j = 0; j < positions.Length; j++)
            {
                if (i == j) continue;
                float distance = Vector3.Distance(positions[i], positions[j]);
                if (distance < minDistanceBetweenDrones)
                {
                    isStillInDanger = true;
                    break;
                }
            }

            if (isStillInDanger)
            {
                danger[i] = true;
                dangerFrames[i]++;
            }
            else
            {
                danger[i] = false;
                dangerFrames[i] = 0;
            }
        }
    }

    private void ApplyJitterIfStuck()
    {
        for (int i = 0; i < danger.Length; i++)
        {
            if (danger[i] && dangerFrames[i] >= jitterThreshold)
            {
                Vector3 jitter = Random.insideUnitSphere * jitterStrength;
                velocities[i] += jitter;
                Debug.Log("Jitter applied to drone: " + i);
            }
        }
    }

    private void MoveTowardTargets(Vector3[] positions, Vector3[] velocities, Vector3[] targets)
    {
        for (int i = 0; i < positions.Length; i++)
        {
            Vector3 desiredDir = (targets[i] - positions[i]);
            float dist = desiredDir.magnitude;

            if (dist < 0.1f) continue;

            Vector3 proposedMove;
            if (dist < easeInDistance)
            {
                float strength = Mathf.InverseLerp(0, easeInDistance, dist);
                proposedMove = desiredDir.normalized * maxSeperatingSpeed * timestep * strength;
            }
            else
            {
                proposedMove = desiredDir.normalized * maxSeperatingSpeed * timestep;
            }
            
            Vector3 newPos = positions[i] + proposedMove;

            bool safe = true;
            for (int j = 0; j < positions.Length; j++)
            {
                if (i == j) continue;
                if (Vector3.Distance(newPos, positions[j]) < minDistanceBetweenDrones)
                {
                    safe = false;
                    break;
                }
            }

            if (safe)
            {
                velocities[i] += desiredDir.normalized * maxSeperatingSpeed;
            }
        }
    }


    private void SetTarget()
    {
        Vector3[] formationTargets = formationManager.GetTargetPositions();
        if (formationTargets != null)
        {
            for (int i = 0; i < positions.Length; i++)
            {
                if (i < formationTargets.Length)
                {
                    targetPositions[i] = formationTargets[i];
                    targetUvs[i] = formationManager.worldUvs[i];
                }
            }
        }
    }

    private void Start()
    {
        int count = 500;
        positions = new Vector3[count];
        previousPositions = new Vector3[count];
        velocities = new Vector3[count];
        danger = new bool[count];
        dangerFrames = new int[count];
        targetPositions = new Vector3[count];
        targetUvs = new Vector2[count];
        

        AssignRandomPositions(positions);
        positions.CopyTo(previousPositions, 0);
    }

    private void Update()
    {
        drawLine.ClearLine();
        drawLineDanger.ClearLine();

        SetTarget();
        if (moveToTarget)
        {
            MoveTowardTargets(positions, velocities, targetPositions);
        }

        MinDistance(positions, velocities, minDistanceBetweenDrones);
        AvoidSides(positions, velocities);
        ApplyJitterIfStuck();
        UpdatePositions(positions, velocities);
        UpdateDangerStates();
        DrawDrones(positions);

        drawLine.UpdateLines();
        drawLineDanger.UpdateLines();
    }
}