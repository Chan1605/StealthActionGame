using System;
using UnityEngine;

public class AttackCommand : ICommand
{
    private enum Phase { Windup, Recovery, Done }

    private Phase _phase;
    private float _timer;
    private EnemyStateMachine _fsm;
    private Action _onComplete;

    public void Start(EnemyStateMachine fsm, Action onComplete)
    {
        _fsm = fsm;
        _onComplete = onComplete;
        _phase = Phase.Windup;
        _timer = 0f;

        fsm.Movement.Stop();
        fsm.SuppressMovementAnim = true;
        fsm.Movement.PlayAttackAnimation();
    }

    public void Tick()
    {
        _timer += Time.deltaTime;

        switch (_phase)
        {
            case Phase.Windup:
                if (_timer >= _fsm.Data.attackWindupTime)
                {
                    _fsm.DamageTarget?.TakeDamage(_fsm.Data.attackDamage);
                    _phase = Phase.Recovery;
                    _timer = 0f;
                }
                break;

            case Phase.Recovery:
                if (_timer >= _fsm.Data.attackRecoveryTime)
                {
                    _phase = Phase.Done;
                    _onComplete?.Invoke();
                }
                break;
        }
    }

    public void Cancel()
    {
        _fsm.SuppressMovementAnim = false;
        _phase = Phase.Done;
    }
}