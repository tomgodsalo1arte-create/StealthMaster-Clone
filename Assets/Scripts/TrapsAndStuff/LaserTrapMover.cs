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
            Vector3 start = goingToB ? pointA.position : pointB.position;
            Vector3 end = goingToB ? pointB.position : pointA.position;

            float t = 0f;

            while (t < 1f)
            {
                t += Time.deltaTime * moveSpeed / Vector3.Distance(start, end);
                transform.position = Vector3.Lerp(start, end, t);
                yield return null;
            }

            transform.position = end; // snap exactly
            yield return new WaitForSeconds(waitTime);

            goingToB = !goingToB;
        }
    }
}
