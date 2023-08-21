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
    MaxHealth,
    Splash,
    TypeMax
}


public class TowerStats : MonoBehaviour
{
    private float[] stats = new float[(int)TowerStatsType.TypeMax] { 1f, 1f, 20f, 5.0f, 1.6f, 1f, 1f, 5.0f, 0f };
    private int[] statsUpGrade = new int[(int)TowerStatsType.TypeMax] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };

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

    public int GetUpgradeValue(TowerStatsType type)
    {
        if (type >= TowerStatsType.TypeMax)
            return -1;

        return statsUpGrade[(int)type];
    }

    public void UpgradeStats(TowerStatsType type)
    {
        int value = statsUpGrade[(int)type];

        switch (type)
        {
        case TowerStatsType.CD:
            if(value < 3)
            {
                    statsUpGrade[(int)type] += 1;
                    stats[(int)type] -= 0.2f ;
                }
            break;

         case TowerStatsType.RotationSpeed:
            if (value < 3)
            {
                    statsUpGrade[(int)type] += 1;
                    stats[(int)type] += 30f;
                }
            break;

        case TowerStatsType.MaxHealth:
                if (value < 1)
                {
                    statsUpGrade[(int)type] += 1;
                    stats[(int)type] += 1f;
                }
                break;

         case TowerStatsType.FireRange:
                if (value < 3)
                {
                    statsUpGrade[(int)type] += 1;
                    stats[(int)type] += 0.4f;
                }
                break;

         case TowerStatsType.TargetsNum:
                if (value < GameManager.instance.selectNumList.Count - 1)
                {
                    statsUpGrade[(int)type] += 1;
                    stats[(int)type] += 1;
                }
                break;

        case TowerStatsType.Splash:
                if (value < 1)
                {
                    statsUpGrade[(int)type] += 1;
                }
                break;
        }
    }
}
