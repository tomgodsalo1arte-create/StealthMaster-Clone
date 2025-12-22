using Assets.Scripts.State;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAttackState : IState
{
    private StateMachine _stateMachine;
    readonly NavMeshAgent agent;
    readonly Vector3 startPoint;
    readonly float wanderRadius;
    private readonly EnemyBehaviour _enemy;
    public EnemyAttackState( EnemyBehaviour enemy, StateMachine stateMachine, NavMeshAgent agent)
    {
         _stateMachine = stateMachine;
         _enemy  = enemy;
        this.agent = agent;
       
    }
    public void Enter()
    {
        Debug.Log("Enter AttackState");
        if (_enemy.IsDead) return;

        // Face the player
        if (PlayerBehaviour.Instance != null)
        {
            Vector3 playerPos =_enemy.LastKnownPlayerPosition;
            Vector3 flat = new Vector3(playerPos.x, _enemy.transform.position.y, playerPos.z);
            _enemy.transform.LookAt(flat);

            // Store last known position for AlertState / Hunt
            //_enemy.LastKnownPlayerPosition = playerPos;
        }

        // Do the attack 
        _enemy.Attack();

        // Make sure we're in alert mode (red cone, global alert once)
        if (!_enemy.IsInAlert)
        {
            if (GameController.Instance != null)
                GameController.Instance.EnterAlertState();

            _enemy.IsInAlert = true;
            _enemy.FieldOfView.SetAlert(true);
        }
    }

    public void Update()
    {
        if (_enemy.IsDead) return;

        // Optional: keep facing player while in attack state
        if (PlayerBehaviour.Instance != null)
        {
            Vector3 playerPos = PlayerBehaviour.Instance.transform.position;
            Vector3 flat = new Vector3(playerPos.x, _enemy.transform.position.y, playerPos.z);
            _enemy.transform.LookAt(flat);
        }
        _enemy.Attack();
       
    }

    public void Exit()
    {
        // exit f
    }
}
