using Assets.Scripts.State;
using UnityEngine;
using UnityEngine.AI;

public class EnemyIdealStaticState : IState
{
    private EnemyBehaviour _enemy;
    private StateMachine _machine;
    private NavMeshAgent _agent;

    public EnemyIdealStaticState(EnemyBehaviour enemy, StateMachine machine, NavMeshAgent agent)
    {
        _enemy = enemy;
        _machine = machine;
        _agent = agent;
    }

    public void Enter()
    {
        Debug.Log("Entering IdealState");

        _enemy._waitingAtWaypoint = true;
        _enemy._scanTimer = 0f;
    }   
    public void Update()
    {
        //Debug.Log("Updating IdealState");
        if (_enemy._waitingAtWaypoint)
        {
            _enemy._scanTimer += Time.deltaTime;

            if (_enemy._scanTimer >= _enemy.scanInterval)
            {
                _enemy._scanTimer = 0f;
              //_enemy.ResetLook();
                _enemy.LookSideToSide();
            }
        }



        //  _enemy.LookSideToSide(_agent, _enemy.transform);
    }
    public void Exit()
    {
      //  throw new System.NotImplementedException();
    }

}
