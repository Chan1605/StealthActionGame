using System.Collections;
using UnityEngine;

public class AlarmTriggerAction : InteractionAction
{
    [Header("경보 소리")]
    [SerializeField] private float alarmSoundIntensity = 90f;
    [SerializeField] private float alarmSoundRadius = 25f;
    [SerializeField] private float alarmDuration = 6f;
    [SerializeField] private float alarmPulseInterval = 0.5f;

    [Header("연출 (선택)")]
    [SerializeField] private GameObject alarmVisualEffect;
    [SerializeField] private AudioSource alarmAudioSource;

    private bool _isAlarming;

    protected override bool CanExecute(Transform user)
    {
        return !_isAlarming;
    }

    protected override void OnExecute(Transform user)
    {
        StartCoroutine(Alarm_co());
    }

    private IEnumerator Alarm_co()
    {
        _isAlarming = true;

        //alarmVisualEffect?.SetActive(true);
        //alarmAudioSource?.Play();

        float elapsed = 0f;
        while (elapsed < alarmDuration)
        {
            EmitAlarmSound();
            yield return new WaitForSeconds(alarmPulseInterval);
            elapsed += alarmPulseInterval;
        }

        //alarmVisualEffect?.SetActive(false);
        //alarmAudioSource?.Stop();

        _isAlarming = false;
    }

    private void EmitAlarmSound()
    {
        EnemyPerception[] enemies = FindObjectsByType<EnemyPerception>(FindObjectsSortMode.None);
        foreach (EnemyPerception enemy in enemies)
        {
            enemy.RegisterSound(transform.position, alarmSoundIntensity, true, alarmSoundRadius);
        }
    }
}