using Assets.Scripts.State;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

public class EnemyPatrolState : IState
{
    private readonly EnemyBehaviour _enemy;
    private readonly StateMachine _machine;
    private readonly NavMeshAgent _agent;

    // local state for patrolling
    private int _currentWaypointIndex = -1;
    private float _waitTimer;
    private const float WaitAtPointTime = 1f; // how long to idle at each waypoint
    private Vector3 targetPos;
    public LTDescr LookAroundLTD { get; private set; }

    public EnemyPatrolState(EnemyBehaviour enemy, StateMachine machine, NavMeshAgent agent)
    {
        _enemy = enemy;
        _machine = machine;
        _agent = agent;
    }

    public void Enter()
    {
     Debug.Log("Enter PatrolState");
        // If there are no waypoints, just idle
        if (_enemy.WaypointCount == 0)
        {
            _agent.isStopped = true;
            _enemy.Animator.Play("Idle");
            return;
        }

        _agent.isStopped = false;

        // Optional: set a slow walk speed for patrol
        _agent.speed = _enemy.PatrolSpeed;  // expose this in EnemyBehaviour, or hardcode

        // Play your “walking” type animations
      //  _enemy.Animator.Play("Slow Run");   // you were using this already for walking
        _enemy.Animator.Play("Walking");   // you were using this already for walking

        _waitTimer = 0f;
        _enemy.SetNextWaypointDestination(_agent);
    }

    public void Update()
    {
        if (_enemy.IsDead) return;
        if (_enemy.WaypointCount == 0) return;
        if (_agent.pathPending) return;

        //  ARRIVAL --only once
        if (!_enemy._waitingAtWaypoint &&
            _agent.remainingDistance <= _agent.stoppingDistance + 0.05f)
        {
           // Debug.Log("[Patrol] Arrived at waypoint");

            _enemy._waitingAtWaypoint = true;
            _waitTimer = 0f;

            _agent.isStopped = true;      // STOP movement
            _agent.velocity = Vector3.zero;
            _enemy.Animator.Play("Idle");

            // TURN IN PLACE (example: turn around)
            Vector3 turnDir = -_enemy.transform.forward;
            _enemy.StartCoroutine(_enemy.TurnTowards(turnDir, 180f));

            return;
        }

        // WAINTING
        if (_enemy._waitingAtWaypoint)
        {
            _waitTimer += Time.deltaTime;

            if (_waitTimer >= WaitAtPointTime)
            {
             //   Debug.Log("[Patrol] Wait finished ? moving");

                _enemy._waitingAtWaypoint = false;
                _waitTimer = 0f;

                _agent.isStopped = false;
                _agent.speed = _enemy.PatrolSpeed;

                //SetNextWaypointDestination();
                _enemy.SetNextWaypointDestination(_agent);
            }

            return;
        }
    }



    public void Exit()
    {
        Debug.Log("Exit PatrolState"); // Stop moving along patrol path
        if (_agent != null)
        {
            _agent.ResetPath();
            _agent.isStopped = true;
        }

        // Back to idle when leaving 
        _enemy.Animator.Play("Idle");
    }

   /* private void SetNextWaypointDestination()
    {
        Debug.Log("starting SetNextWaypointDestination & _enemy.WaypointCount== "+ _enemy.WaypointCount);
        if (_enemy.WaypointCount == 0) return;

        int nextIndex = _currentWaypointIndex;

        // Choose a new index that’s not the same as the current one
        if (_enemy.WaypointCount == 1)
        {
            nextIndex = 0;
        }
        else
        {
            while (nextIndex == _currentWaypointIndex)
            {
                nextIndex = Random.Range(0, _enemy.WaypointCount);
            }
        }

        _currentWaypointIndex = nextIndex;

        targetPos = _enemy.GetWaypointPosition(_currentWaypointIndex);
        _agent.isStopped = false;

        
        _agent.SetDestination(targetPos );

        _enemy.Animator.Play("Walking");

    }*/
}
