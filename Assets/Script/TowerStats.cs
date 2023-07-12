using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TowerStatsType
{
    CD,
    Damage,
    RotationSpeed,
    Health,
    FireRange,
    SplatterDamage,
    TargetsNum,
    TypeMax
}


public class TowerStats : MonoBehaviour
{
    public float[] stats = new float[(int)TowerStatsType.TypeMax] { 1f, 1f, 20f, 5.0f, 1.6f, 1f, 1f};
    public float abc = 111;

    public void SetValue(TowerStatsType type, float value)
    {
        if (type >= TowerStatsType.TypeMax)
            return;

        stats[(int)type] = value;
    }

    public float GetValue(TowerStatsType type)
    {
        if (type >= TowerStatsType.TypeMax)
            return -1;

        return stats[(int)type];
    }

}
