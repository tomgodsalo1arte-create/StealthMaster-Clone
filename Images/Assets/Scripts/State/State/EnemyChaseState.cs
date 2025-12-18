using Assets.Scripts.State;
using UnityEngine;
using UnityEngine.AI;

public class EnemyChaseState : IState
{
    private StateMachine stateMachine;
    private readonly EnemyBehaviour _enemy;
    private readonly StateMachine _machine;
    private readonly NavMeshAgent _agent;

    private float _standTimer;
    private float _wanderTimer;

    private bool _walkingToLastKnown;
    private bool _wanderingAround;
    private bool _returningHome;

    public EnemyChaseState(EnemyBehaviour enemyBehaviour, StateMachine stateMachine, NavMeshAgent agent)
    {
        this._enemy = enemyBehaviour;
        this.stateMachine = stateMachine;
        _agent = agent;
    }

    public void Enter()
    {
        if (_enemy.IsDead) return;
        Debug.Log("Enter ChaseState");
        _standTimer = 0f;
        _wanderTimer = 0f;

        _walkingToLastKnown = false;
        _wanderingAround = false;
        _returningHome = false;

        // Look toward the last known position
        Vector3 lastPos = _enemy.LastKnownPlayerPosition;
        Vector3 flat = new Vector3(lastPos.x, _enemy.transform.position.y, lastPos.z);
        _enemy.transform.LookAt(flat);

        // Stand still
        _agent.isStopped = true;
        _agent.ResetPath();

        // Idle anim
        _enemy.Animator.Play("Idle");
    }

    public void Update()
    {
        if (_enemy.IsDead) return;

        // 1. Stand still first
        if (!_walkingToLastKnown && !_wanderingAround && !_returningHome)
        {
            _standTimer += Time.deltaTime;
            if (_standTimer >= _enemy.AlertStandTime)
            {
                StartWalkingToLastKnown();
            }
            return;
        }

        // 2. Walking to last known player position
        if (_walkingToLastKnown)
        {
            if (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance + 0.05f)
            {
                StartWandering();
            }
            return;
        }

        // 3. Wandering around the last known location for 10 seconds
        if (_wanderingAround)
        {
            _wanderTimer += Time.deltaTime;

            if (_wanderTimer >= _enemy.MaxHuntTime)
            {
                // Done wandering ? return home
                StartReturningHome();
                return;
            }

            // If reached the small wander point, pick a new random one
            if (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance + 0.05f)
            {
                SetRandomWanderPoint();
            }
            return;
        }

        // 4. Going back home
        if (_returningHome)
        {
            if (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance + 0.05f)
            {
                _enemy.SwitchToPatrolFromAlert();
            }
        }
    }

    public void Exit()
    {
        Debug.Log("Ext ChaseState");
        _agent.ResetPath();
        _agent.isStopped = true;

        _enemy.Animator.Play("Idle");
    }

    // ===========================================================
    // HELPER METHODS
    // ===========================================================

    private void StartWalkingToLastKnown()
    {
        _walkingToLastKnown = true;

        _agent.isStopped = false;
        _agent.speed = _enemy.AlertWalkSpeed;

        _agent.SetDestination(_enemy.LastKnownPlayerPosition);

        _enemy.Animator.Play("Slow Run");
    }

    private void StartWandering()
    {
        _walkingToLastKnown = false;
        _wanderingAround = true;

        _wanderTimer = 0f;

        SetRandomWanderPoint();

        _enemy.Animator.Play("Walking");
    }

    private void SetRandomWanderPoint()
    {
        Vector3 center = _enemy.LastKnownPlayerPosition;

        // small random radius (1–3 meters)
        float radius = Random.Range(1f, 3f);
        float angle = Random.Range(0f, 360f);

        Vector3 offset = new Vector3(
            Mathf.Cos(angle * Mathf.Deg2Rad),
            0,
            Mathf.Sin(angle * Mathf.Deg2Rad)
        ) * radius;

        Vector3 wanderPoint = center + offset;

        _agent.isStopped = false;
        _agent.SetDestination(wanderPoint);
    }

    private void StartReturningHome()
    {
        _wanderingAround = false;
        _returningHome = true;

        Vector3 home = _enemy.OriginalPosition;

        _agent.isStopped = false;
        _agent.speed = _enemy.AlertWalkSpeed;
        _agent.SetDestination(home);

        _enemy.Animator.Play("Slow Run");
    }

}
