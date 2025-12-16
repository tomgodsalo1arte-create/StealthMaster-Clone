using Assets.Scripts.State;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.AI;

public class EnemyDistractedState : IState
{
    private EnemyBehaviour _enemy;
    private StateMachine _machine;
    private NavMeshAgent _agent;

    public EnemyDistractedState(EnemyBehaviour enemy, StateMachine machine, NavMeshAgent agent)
    {
        _enemy = enemy;
        _machine = machine;
        _agent = agent;
    }
    public void Enter()
    {
        UnityEngine.Debug.Log("Entering Distraction State");
        if (_enemy.IsDead) return;
        // need to set  Distraction
        //throw new System.NotImplementedException();
    }

    public void Exit()
    {
        //
      //  throw new System.NotImplementedException();
    }

    public void Update()
    {
        throw new System.NotImplementedException();
    }
}