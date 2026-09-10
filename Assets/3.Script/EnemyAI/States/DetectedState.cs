using UnityEngine;

public class DetectedState : IEnemyState
{
    private float _loseTimer;
    private ChaseCommand _chase;

    public void Enter(EnemyStateMachine fsm)
    {
        fsm.Owner.Indicator.ShowDetected();
        _loseTimer = 0f;
        _chase = new ChaseCommand(fsm.Perception, fsm.Data.chaseRepathInterval);
        _chase.Start(fsm, null);
    }

    public void Tick(EnemyStateMachine fsm)
    {
        _chase.Tick();

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

    public void Exit(EnemyStateMachine fsm)
    {
        _chase.Cancel();
    }
}