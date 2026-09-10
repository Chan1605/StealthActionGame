using System;
using UnityEngine;

public class ChaseCommand : ICommand
{
    private readonly EnemyPerception _perception;
    private readonly float _repathInterval;
    private EnemyStateMachine _fsm;
    private float _timer;

    public ChaseCommand(EnemyPerception perception, float repathInterval)
    {
        _perception = perception;
        _repathInterval = repathInterval;
    }

    public void Start(EnemyStateMachine fsm, Action onComplete)
    {
        _fsm = fsm;
        fsm.Movement.SetChaseSpeed(fsm.Data);
        _timer = 0f;
        MoveToTarget();
    }

    public void Tick()
    {
        _timer += Time.deltaTime;
        if (_timer < _repathInterval) return;
        _timer = 0f;
        MoveToTarget();
    }

    private void MoveToTarget()
    {
        if (_perception.Target == null) return;
        _fsm.Movement.MoveTo(_perception.Target.Position);
    }

    public void Cancel() { }
}