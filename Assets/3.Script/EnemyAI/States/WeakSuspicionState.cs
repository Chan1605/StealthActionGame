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

        bool triggeredBySound = fsm.Perception.IsSoundDominant;
        Vector3 focus = fsm.Perception.GetSuspectedPosition();
        float duration = triggeredBySound
            ? fsm.Data.soundGlanceDuration
            : Random.Range(fsm.Data.patrolIdleMinDuration, fsm.Data.patrolIdleMaxDuration);

        _lookCommand = new LookAroundCommand(fsm.Owner.transform, fsm.Movement, duration, fsm.Data.lookAroundAngle, focus);
        _lookCommand.Start(fsm, () => _lookCommand = null);
    }

    public void Tick(EnemyStateMachine fsm)
    {
        if (fsm.Perception.MaxScore >= fsm.Data.strongSuspicionThreshold)
        {
            fsm.ChangeState(new StrongSuspicionState());
            return;
        }

        if (_lookCommand != null)
        {
            _lookCommand.Tick();
            return; // 둘러보는 동안은 점수 하락에 의한 강등을 보류 (최소 반응 시간 보장)
        }

        if (fsm.Perception.MaxScore < fsm.Data.weakSuspicionThreshold)
        {
            fsm.ChangeState(new NormalState());
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