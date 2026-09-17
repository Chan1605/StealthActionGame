using UnityEngine;

public static class WitnessCheck
{
    public static void NotifyNearbyEnemies(Vector3 eventPosition, float witnessRadius, float witnessAngle, LayerMask obstacleMask)
    {
        EnemyPerception[] enemies = Object.FindObjectsByType<EnemyPerception>(FindObjectsSortMode.None);
        foreach (EnemyPerception enemy in enemies)
        {
            enemy.TryWitness(eventPosition, witnessRadius, witnessAngle, obstacleMask);
        }
    }
}