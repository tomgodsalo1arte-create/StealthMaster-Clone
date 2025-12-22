using Assets.Scripts.State;
using UnityEngine;
using UnityEngine.AI;

public class EnemyDeadState : IState
{
    private readonly EnemyBehaviour _enemy;
    private readonly NavMeshAgent _agent;
    private readonly Animator _animator;

    public EnemyDeadState(EnemyBehaviour enemy, StateMachine machine, NavMeshAgent agent)
    {
        _enemy = enemy;
        _agent = agent;
        _animator = enemy.Animator;
    }

    public void Enter()
    {
        Debug.Log("Enemy entered DeadState");

        // Stop AI movement
        if (_agent != null)
        {
            _agent.isStopped = true;
            _agent.ResetPath();
            _agent.enabled = false;
        }

        // Play death animation
        if (_animator != null)
        {
            _animator.Play("Death");
        }

        // Tag as dead (used by FOV logic)
        _enemy.transform.tag = "Dead";

        // Disable vision cone
        if (_enemy.FieldOfView != null)
        {
            _enemy.FieldOfView.gameObject.SetActive(false);
        }

        // Exit global alert if needed
        if (_enemy.IsInAlert && GameController.Instance != null)
        {
            GameController.Instance.ExitAlertState();
            _enemy.IsInAlert = false;
        }

        // Play death audio
        if (_enemy.AudioSource != null && _enemy.AudioSource.clip != null)
        {
            _enemy.AudioSource.PlayOneShot(_enemy.AudioSource.clip);
        }

        // Optional: disable collider so corpse doesn’t interfere
        Collider col = _enemy.GetComponent<Collider>();
        if (col != null)
            col.enabled = false;
    }

    public void Update()
    {
        
    }

    public void Exit()
    {
        Debug.Log("Enemy Exit DeadState");
    }
}
