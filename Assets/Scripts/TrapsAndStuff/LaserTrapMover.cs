using System.Collections;
using UnityEngine;

public class LaserTrapMover : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;
    public float moveSpeed = 2f;
    public float waitTime = 1f;

    private bool goingToB = true;

    void Start()
    {
        StartCoroutine(MoveLaser());
    }

    IEnumerator MoveLaser()
    {
        while (true)
        {
            Transform target = goingToB ? pointB : pointA;

            while (Vector3.Distance(transform.position, target.position) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    target.position,
                    moveSpeed * Time.deltaTime
                );
                yield return null;
            }

            yield return new WaitForSeconds(waitTime);
            goingToB = !goingToB;
        }
    }
}
