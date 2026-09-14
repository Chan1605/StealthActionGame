using UnityEngine;

public class DetectedState : IEnemyState
{
    private float _loseTimer;
    private float _attackTimer;
    private ChaseCommand _chase;

    public void Enter(EnemyStateMachine fsm)
    {
        fsm.Owner.Indicator.ShowDetected();
        fsm.Movement.SetStoppingDistance(fsm.Data.attackRange);

        _loseTimer = 0f;
        _attackTimer = 0f;
        _chase = new ChaseCommand(fsm.Perception, fsm.Data.chaseRepathInterval);
        _chase.Start(fsm, null);
    }

    public void Tick(EnemyStateMachine fsm)
    {
        _chase.Tick();
        TickAttack(fsm);

        if (fsm.Perception.MaxScore >= fsm.Data.strongSuspicionThreshold)
        {
            _loseTimer = 0f;
            return;
        }

        _loseTimer += Time.deltaTime;
        if (_loseTimer < fsm.Data.detectedLoseTime) return;

        if (fsm.Perception.MaxScore >= fsm.Data.weakSuspicionThreshold)
            fsm.ChangeState(new WeakSuspicionState());
        else
            fsm.ChangeState(new NormalState());
    }

    private void TickAttack(EnemyStateMachine fsm)
    {
        if (fsm.Perception.Target == null) return;

        float distance = Vector3.Distance(fsm.Owner.transform.position, fsm.Perception.Target.Position);
        if (distance > fsm.Data.attackRange)
        {
            _attackTimer = 0f;
            return;
        }

        _attackTimer += Time.deltaTime;
        if (_attackTimer < fsm.Data.attackInterval) return;

        _attackTimer = 0f;
        fsm.DamageTarget?.TakeDamage(fsm.Data.attackDamage);
    }

    public void Exit(EnemyStateMachine fsm)
    {
        _chase.Cancel();
        fsm.Movement.SetStoppingDistance(fsm.Data.stoppingDistance);
    }
}