using Assets.Scripts.State;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAlertState : IState
{
    private EnemyBehaviour _enemy;
    private StateMachine _machine;
    private Animator animtor;
    private bool _walkingToLastKnown;
    private bool _wanderingAround;
    private bool _returningHome;
    private float _wanderTimer;
    private readonly NavMeshAgent _agent;
    public EnemyAlertState(EnemyBehaviour enemy, StateMachine machine, NavMeshAgent agent)
    {
        _enemy = enemy;
        _machine = machine;
        _agent = agent;
    }

    public void Enter()
    {
        Debug.Log("Enter AlertState");
        if (_enemy.IsDead) return;
      
        _wanderTimer = 0f;

        _walkingToLastKnown = true;
        _wanderingAround = false;
        _returningHome = false;
       // _enemy.IsInAlert = true;

        // Look toward the last known position
        Vector3 lastPos = _enemy.transform.position;
       // Vector3 lastPos = _enemy.LastKnownPlayerPosition; 

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
      //  Debug.Log("Exiting_If reached the small wander point, pick a new random one------------------------------- Alert");
        // 4. Going back home
        if (_returningHome)
        {
            if (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance + 0.05f)
            {
                //Debug.Log("_returningHome------------------------------- Alert");
                _enemy.SwitchToPatrolFromAlert();
            }
        }
    }
    public void Exit()
    {
        Debug.Log("Exiting Alert");
        _agent.ResetPath();
        _agent.isStopped = true;

        _enemy.Animator.Play("Idle");
    }
    // ===========================================================
    // HELPER METHODS
    // ===========================================================


    private void StartWandering()
    {
        _enemy.Animator.Play("Walking");
        _walkingToLastKnown = false;
        _wanderingAround = true;

        _wanderTimer = 0f;

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
