using UnityEngine;


// 적 AI(EnemyPerception)와 Player 사이의 경계 인터페이스.
// 적 AI 개발 단계에서는 DummyDetectable로 테스트하고,
// 병합 단계에서 실제 Player 클래스가 이 인터페이스만 구현하면
// EnemyPerception 쪽 코드는 수정할 필요x

public interface IDetectable
{
    Vector3 Position { get; }
    float SoundIntensity { get; }
    bool IsStealthed { get; }   // 은신 상태 여부 (은신 시 감지 가중치 감소용)
    bool IsCrouching { get; }   // 앉기 여부 (시야/청각 점수 증가량 감소용)
}
