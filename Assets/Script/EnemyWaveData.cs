using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WaveInfo
{
    public List<GameObject> enemyNameList = new List<GameObject>();
}

[CreateAssetMenu(fileName = "EnemyWaveData", menuName = "ScriptableObjects/EnemyWaveData", order = 1)]
public class EnemyWaveData : ScriptableObject
{
    [SerializeField]
    public List<WaveInfo> waveInfo = new List<WaveInfo>();
}
