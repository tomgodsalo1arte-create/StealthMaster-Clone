using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [HideInInspector] public float speed = default;
    public Transform targetEnemy;

    void Update()
    {
        transform.Rotate(0, speed, 0);
        transform.position = Vector3.MoveTowards(transform.position, targetEnemy.position + Vector3.up, Time.deltaTime * speed);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform == targetEnemy)
        {
            EnemyBehaviour enemy = targetEnemy.GetComponent<EnemyBehaviour>();

            // --- Apply Knockback ---
            Rigidbody enemyRb = enemy.GetComponent<Rigidbody>();
            if (enemyRb != null)
            {
                ////  Remove constraints of position and rotation,  so knockback 
                //enemyRb.constraints = RigidbodyConstraints.None;
                //Vector3 knockbackDir = (targetEnemy.position - transform.position).normalized;
                //float knockbackForce = 50f; // adjust to your liking

                //enemyRb.AddForce(knockbackDir * knockbackForce, ForceMode.Impulse);
                //StartCoroutine(KnockbackCooldow(enemyRb));
            }

            // Kill the enemy
            enemy.Died();

            Destroy(gameObject);
        }
    }
    private IEnumerator KnockbackCooldow(Rigidbody rb)
    {

        yield return new WaitForSeconds(.1f);

        rb.mass = 10f;
        yield return new WaitForSeconds(4f);
        // optional: re-limit them so they don't slide everywhere
        rb.constraints = RigidbodyConstraints.FreezePositionX |
                         RigidbodyConstraints.FreezePositionZ |
                         RigidbodyConstraints.FreezeRotationX |
                         RigidbodyConstraints.FreezeRotationY |
                         RigidbodyConstraints.FreezeRotationZ;
    }
}
