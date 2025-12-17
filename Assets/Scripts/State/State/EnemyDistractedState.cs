using Assets.Scripts.State;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.AI;

public class EnemyDistractedState : IState
{
    private EnemyBehaviour _enemy;
    private StateMachine _machine;
    private NavMeshAgent _agent;
    private float _wanderTimer;
    private bool _walkingToLastKnown;
    private bool _wanderingAround;
    private bool _returningHome;


    private float _lookAwayTimer;
    private const float LookAwayDuration = 15f;
    private Vector3 _lookAwayDirection;
    private bool _isLookingAway;

    public EnemyDistractedState(EnemyBehaviour enemy, StateMachine machine, NavMeshAgent agent)
    {
        _enemy = enemy;
        _machine = machine;
        _agent = agent;
    }
    public void Enter()
    {
        UnityEngine.Debug.Log("Entering Distraction State");
       
    }

    public void Update()
    {

        //// 2. Walking to last known player position
        //if (_walkingToLastKnown)
        //{
        //    if (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance + 0.05f)
        //    {
        //        StartWandering();
        //    }
        //    return;
        //}
        if (_wanderingAround)
        {
            _wanderTimer += Time.deltaTime;

            // --------------------------------
            // LOOK AWAY LOGIC
            // --------------------------------
            if (_isLookingAway)
            {
                _lookAwayTimer += Time.deltaTime;

                if (_lookAwayTimer <= LookAwayDuration)
                {
                    RotateAwayFromPlayer();
                }
                else
                {
                    _isLookingAway = false; // stop forced rotation
                }
            }
            // --------------------------------

            if (_wanderTimer >= _enemy.MaxHuntTime)
            {
                StartReturningHome();
                return;
            }

            if (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance + 0.05f)
            {
                SetRandomWanderPoint();
            }
            return;
        }
    }
    public void Exit()
    {
        UnityEngine.Debug.Log("Exiting Alert");
        _agent.ResetPath();
        _agent.isStopped = true;

        _enemy.Animator.Play("Idle");
    }
    // ===========================================================
    // HELPER METHODS
    // ===========================================================
    private void RotateAwayFromPlayer()
    {
        if (_lookAwayDirection == Vector3.zero)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(_lookAwayDirection);
        _enemy.transform.rotation = Quaternion.Slerp(
            _enemy.transform.rotation,
            targetRotation,
            Time.deltaTime * 5f // rotation speed
        );
    }


    private void StartWandering()
    {
        _walkingToLastKnown = false;
        _wanderingAround = true;

        _wanderTimer = 0f;

        // -------- LOOK AWAY SETUP --------
        _lookAwayTimer = 0f;
        _isLookingAway = true;

        if (PlayerBehaviour.Instance != null)
        {
            Vector3 toPlayer = PlayerBehaviour.Instance.transform.position - _enemy.transform.position;
            toPlayer.y = 0f;

            // Opposite direction
            _lookAwayDirection = -toPlayer.normalized;
        }
        // --------------------------------

        SetRandomWanderPoint();
        _enemy.Animator.Play("Walking");
    }


    private void SetRandomWanderPoint()
    {
        Vector3 center = _enemy.transform.position;
        // Vector3 center = _enemy.LastKnownPlayerPosition; /---------------------- Enemys Comes to the player Detected spot

        // small random radius (1–5 meters)
        float radius = Random.Range(1f, 5f);
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

        Vector3 home = _enemy.transform.position;

        _agent.isStopped = false;
        _agent.speed = _enemy.AlertWalkSpeed;
        _agent.SetDestination(home);

        _enemy.Animator.Play("Slow Run");
    }
}