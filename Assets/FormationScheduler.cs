using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class TimedFormation
{
    public float time; // seconds
    public FormationType formation;
}

public class FormationScheduler : MonoBehaviour
{
    public FormationManager formationManager;
    public List<TimedFormation> scheduledFormations;

    private int currentIndex = 0;
    private float timer = 0f;

    [SerializeField]
    Drones drones;

    void Start()
    {
        scheduledFormations.Sort((a, b) => a.time.CompareTo(b.time));
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (currentIndex < scheduledFormations.Count)
        {
            TimedFormation next = scheduledFormations[currentIndex];
            if (timer >= next.time)
            {
                formationManager.currentFormation = next.formation;

                // Assign smartly
                formationManager.GetAssignedTargetPositions(drones.positions);
                drones.moveToTarget = true; //obide formations in future can possibly switch behaviour

                currentIndex++;
            }
        }
    }

    public void ResetScheduler()
    {
        timer = 0f;
        currentIndex = 0;
    }
}





