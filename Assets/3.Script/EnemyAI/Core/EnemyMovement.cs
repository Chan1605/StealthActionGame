using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyMovement : MonoBehaviour
{
    private NavMeshAgent _agent;
    private Animator _animator;

    private int _animIDSpeed;
    private int _animIDGrounded;

    public void Initialize(EnemyAIData data, Animator animator)
    {
        _agent = GetComponent<NavMeshAgent>();
        _animator = animator;
        _agent.stoppingDistance = data.stoppingDistance;
        SetPatrolSpeed(data);

        if (_animator != null)
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animator.SetBool(_animIDGrounded, true); // Jump/FreeFall 미사용, 계속 true 고정
        }
    }

    public void SetPatrolSpeed(EnemyAIData data) => _agent.speed = data.patrolSpeed;
    public void SetInvestigateSpeed(EnemyAIData data) => _agent.speed = data.investigateSpeed;
    public void SetChaseSpeed(EnemyAIData data) => _agent.speed = data.chaseSpeed;
    public void SetAutoRotation(bool enabled) => _agent.updateRotation = enabled;

    public void MoveTo(Vector3 destination)
    {
        // NavMesh 밖 좌표 보정
        if (NavMesh.SamplePosition(destination, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            _agent.SetDestination(hit.position);
        else
            Debug.LogWarning($"{name}: 목표 지점 근처에서 유효한 NavMesh를 찾지 못했습니다. ({destination})");
    }

    public void Warp(Vector3 position)
    {
        _agent.Warp(position);
    }

    public bool HasArrived()
    {
        if (_agent.pathPending) return false;
        return _agent.remainingDistance <= _agent.stoppingDistance;
    }

    public void Stop()
    {
        if (_agent.isOnNavMesh) _agent.ResetPath();
    }

    public void TickAnimator()
    {
        if (_animator == null) return;
        _animator.SetFloat(_animIDSpeed, _agent.velocity.magnitude);
    }
}
