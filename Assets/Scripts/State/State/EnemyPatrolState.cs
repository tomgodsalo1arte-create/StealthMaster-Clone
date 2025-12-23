using Assets.Scripts.State;
using UnityEngine;
using UnityEngine.AI;

public class EnemyPatrolState : IState
{
    private readonly EnemyBehaviour _enemy;
    private readonly StateMachine _machine;
    private readonly NavMeshAgent _agent;

    // local state for patrolling
    private int _currentWaypointIndex = -1;
    private float _waitTimer;
    private const float WaitAtPointTime = 1f; // how long to idle at each waypoint

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
        Debug.Log("Update PatrolState");
        // If we're still calculating a path, do nothing
        if (_agent.pathPending)
            return;

        // Has the enemy reached the waypoint?
        if (_agent.remainingDistance <= _agent.stoppingDistance + 0.05f)
        {
            _agent.isStopped = true;

            // Idle on this point for a bit
            _waitTimer += Time.deltaTime;
            if (_waitTimer >= WaitAtPointTime)
            {
                // _enemy.Animator.Play("Idle");
                _enemy.Animator.Play("Walking");
                _waitTimer = 0f;
               LookAround();
                // Move to another random waypoint (different to current)
             
                  //SetNextWaypointDestination();
            }
        }
        else
        {
            _agent.isStopped = false;
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

        Vector3 targetPos = _enemy.GetWaypointPosition(_currentWaypointIndex);
        _agent.isStopped = false;
        _agent.SetDestination(targetPos);

        // Make the enemy face the waypoint (optional, NavMeshAgent will also rotate)
        _enemy.transform.LookAt(new Vector3(targetPos.x, _enemy.transform.position.y, targetPos.z));
    }
}
