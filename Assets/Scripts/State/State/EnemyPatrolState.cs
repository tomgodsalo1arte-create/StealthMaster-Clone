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
        SetNextWaypointDestination();
    }

    public void Update()
    {
        if (_enemy.IsDead) return;
        if (_enemy.WaypointCount == 0) return;
        if (_agent.pathPending) return;

        // 1. Arrival detection (ONCE)
        if (!_enemy._waitingAtWaypoint &&
            _agent.remainingDistance <= _agent.stoppingDistance + 0.05f)
        {
            _enemy._waitingAtWaypoint = true;
            _waitTimer = 0f;
            _agent.speed = 0.01f;
           // _enemy.Animator.Play("Idle");

            SetNextWaypointDestination();

            // TURN AROUND / FACE FORWARD TO the Destination 
            _enemy.StartCoroutine(  _enemy.TurnTowards(targetPos, 180f)  );

        }

        // 2. Waiting + scanning
        if (_enemy._waitingAtWaypoint)
        {
            _waitTimer += Time.deltaTime;
          // _enemy.LookSideToSide(_agent, _enemy.transform);

            if (_waitTimer >= WaitAtPointTime)
            {
                _enemy._waitingAtWaypoint = false;
                _waitTimer = 0f;

               // ResetLookNavMesh();
                _agent.speed = _enemy.PatrolSpeed;
               
            }

            return;
        }
    }

    private void LookAround()  // this is not done--------------------need to change full to code based look agound----------------------------------------------------
    {
        float angle = Random.Range(0, 2) == 0 ? -45 : 45;
        LookAroundLTD = LeanTween.rotateAround(_enemy.gameObject, Vector3.up, angle, 1f).setOnComplete(() =>
        {
            LeanTween.delayedCall(0.5f, () =>
            {
                LeanTween.rotateAround(_enemy.gameObject, Vector3.up, -angle * 2, 1f).setOnComplete(() =>
                {
                    if (!_enemy.IsDead)
                    {
                        SetNextWaypointDestination();  //StartCoroutine(EnemyAI());
                    }
                });
            });
        });
    }///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
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

    private void SetNextWaypointDestination()
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

        //Vector3 dir = (targetPos - _agent.transform.position).normalized;
        //Vector3 lookPoint = _agent.transform.position + dir * 0.5f;
      //  _enemy.TurnAround(targetPos, 1f);
        //_agent.SetDestination(lookPoint);
        _agent.SetDestination(targetPos );

        _enemy.Animator.Play("Walking");

        // Make the enemy face the waypoint (optional, NavMeshAgent will also rotate)
        //   _enemy.transform.LookAt(new Vector3(targetPos.x, _enemy.transform.position.y, targetPos.z));
    }
}
