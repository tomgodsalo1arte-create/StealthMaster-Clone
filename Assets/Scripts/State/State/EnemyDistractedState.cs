using Assets.Scripts.State;
using UnityEngine;
using UnityEngine.AI;

public class EnemyDistractedState : IState
{
    private EnemyBehaviour _enemy;
    private StateMachine _machine;
    private NavMeshAgent _agent;

    private float powerupTimer;
    //private const float PowerUpDuration = 15f;

    private bool _walkingToLastKnown;
    private bool _wanderingAround;
    private bool _returningHome;
  
    private Vector3 _lookAwayDirection;


    public EnemyDistractedState(EnemyBehaviour enemy, StateMachine machine, NavMeshAgent agent)
    {
        _enemy = enemy;
        _machine = machine;
        _agent = agent;
    }
    public void Enter()
    {
        UnityEngine.Debug.Log("Entering Distraction State");
       
       StartWandering();
    }

    public void Update()
    {
      //-----------------WANTERING STARTS HERE-----------------------------------------------------------------
        if (_wanderingAround)
        {
            powerupTimer += Time.deltaTime;

            // FORCE look away from player (always)
            ForceLookAway();
            GameController.Instance.ForceAllCamToBeDesabled();

            // Wander ends after 15 seconds
            if (powerupTimer >= GameController.Instance.DistractedDuration)
            {
                UnityEngine.Debug.Log("powerupTimer >= GameController.Instance.DistractedDuration Passed on thime===>>" + powerupTimer);
                StartReturningHome();
                
                return;
            }

            // Pick a new random point when reached
            if (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance + 0.05f)
            {
                SetRandomWanderPoint();
            }

            return;
        }
        if (_returningHome)
        {
            // Wait until agent reaches destination
            if (!_agent.pathPending &&
                _agent.remainingDistance <= _agent.stoppingDistance + 0.05f)
            {
                //_machine.TransitionTo(_enemy.PatrolState);
                _enemy.SwitchToPatrolFromAlert();
            }
        }
    }

    public void Exit()
    {
        UnityEngine.Debug.Log("Exiting Alert");
        _agent.ResetPath();
        _agent.isStopped = true;
        // Re-enable normal rotation for other states
        _agent.updateRotation = true;

        _enemy.Animator.Play("Idle");
        //---------------------------------------------
        foreach(var cam in GameController.Instance.camsInLevel)
        {
            if(GameController.Instance.camsInLevel.Count != 0)
            {
                cam.enabled = true;
                Debug.Log("  cam.enabled = true;");
                cam.Spotlight.SpotlightToNormal();
            }
        }
    }
    // ============================================================
    // HELPER METHODS
    // ==========================================================
    private void ForceLookAway()
    {
        if (PlayerBehaviour.Instance == null)
            return;

        Vector3 toPlayer = PlayerBehaviour.Instance.transform.position - _enemy.transform.position;
        toPlayer.y = 0f;

        if (toPlayer.sqrMagnitude < 0.01f)
            return;

        Vector3 lookAwayDir = -toPlayer.normalized;

        Quaternion targetRotation = Quaternion.LookRotation(lookAwayDir);

        _enemy.transform.rotation = Quaternion.Slerp(
            _enemy.transform.rotation,
            targetRotation,
            Time.deltaTime * 8f
        );
    }

    private void StartWandering()
    {
        _wanderingAround = true;
        _returningHome = false;

        powerupTimer = 0f;

        // Disable NavMeshAgent rotation COMPLETELY
        _agent.updateRotation = false;

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