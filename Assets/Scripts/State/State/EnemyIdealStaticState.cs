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
    }   
    public void Update()
    {
       _enemy.LookSideToSide(_agent, _enemy.transform);
    }
    public void Exit()
    {
        throw new System.NotImplementedException();
    }

}
