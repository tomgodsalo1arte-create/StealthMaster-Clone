using System.Collections.Generic;
using UnityEngine;

public class MiniBossController : MonoBehaviour
{
    [SerializeField] private MiniBossData data;
    [SerializeField] private EnemyBehaviour enemy;
    private List<GameObject> waypointList;
    private int currentHealth;
    private int currentPhase;

    private void Awake()
    {
      //  currentHealth = data.maxHealth;
        currentPhase = 0;
    }

    public void TakeHit()
    {
        currentHealth--;

        if (currentHealth <= 0)
        {
            enemy.Died();
            return;
        }

       AdvancePhase();
    }

    private void AdvancePhase()
    {
        currentPhase++;

        if (currentPhase >= waypointList.Count)
        {
            enemy.Died();
            return;
        }

        Vector3 nextPos = waypointList[currentPhase].transform.position;

      //  enemy.MoveToForcedPosition(nextPos);
    }
}
