using UnityEngine;

public class WeakSuspicionState : IEnemyState
{
    private LookAroundCommand _lookCommand;
    private bool _resumedMovement;

    public void Enter(EnemyStateMachine fsm)
    {
        fsm.Owner.Indicator.ShowWeak();
        fsm.Movement.Stop();
        _resumedMovement = false;

        float duration = Random.Range(fsm.Data.patrolIdleMinDuration, fsm.Data.patrolIdleMaxDuration);
        _lookCommand = new LookAroundCommand(fsm.Owner.transform, fsm.Movement, duration, fsm.Data.lookAroundAngle, fsm.Perception.GetSuspectedPosition());
        _lookCommand.Start(fsm, () => _lookCommand = null);
    }

    public void Tick(EnemyStateMachine fsm)
    {
        if (fsm.Perception.MaxScore >= fsm.Data.strongSuspicionThreshold)
        {
            fsm.ChangeState(new StrongSuspicionState());
            return;
        }

        if (fsm.Perception.MaxScore < fsm.Data.weakSuspicionThreshold)
        {
            fsm.ChangeState(new NormalState());
            return;
        }

        if (_lookCommand != null)
        {
            _lookCommand.Tick();
            return;
        }

        if (!_resumedMovement)
        {
            _resumedMovement = true;
            if (fsm.Waypoints.Count > 0)
                fsm.Movement.MoveTo(fsm.Waypoints.GetPosition(fsm.CurrentWaypointIndex));
        }
    }

    public void Exit(EnemyStateMachine fsm)
    {
        _lookCommand?.Cancel();
        _lookCommand = null;
    }
}