using UnityEngine;

public class EnemyStateMachine
{
    public EnemyAI Owner { get; }
    public EnemyMovement Movement { get; }
    public EnemyPerception Perception { get; }
    public EnemyAIData Data { get; }
    public WaypointGroup Waypoints { get; }

    public int CurrentWaypointIndex { get; set; }

    private IEnemyState _current;

    public EnemyStateMachine(EnemyAI owner, EnemyMovement movement, EnemyPerception perception, EnemyAIData data, WaypointGroup waypoints)
    {
        Owner = owner;
        Movement = movement;
        Perception = perception;
        Data = data;
        Waypoints = waypoints;
    }

    public void ChangeState(IEnemyState next)
    {
        _current?.Exit(this);
        _current = next;
        _current.Enter(this);
    }

    public void Tick()
    {
        Perception.Tick(Time.deltaTime);
        _current?.Tick(this);
        Movement.TickAnimator();
    }

#if UNITY_EDITOR
    public string CurrentStateName => _current?.GetType().Name ?? "None";
#endif
}
