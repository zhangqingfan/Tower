using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BottomUI : UICtrl
{
    private void Awake()
    {
        base.Awake();
        uiDict["BottomUI"] = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        ChangeText("Panel/Text", "Cool Down: ");
        ChangeText("Panel/Text (1)", "Damage: ");
        ChangeText("Panel/Text (2)", "RotationSpeed: ");
        ChangeText("Panel/Text (3)", "Health: ");
        ChangeText("Panel/Text (4)", "FireRange: ");
        ChangeText("Panel/Text (5)", "Splatter: ");
        ChangeText("Panel/Text (6)", "TargetsNum: ");
        ChangeText("Panel/Text (7)", "TowerID: ");
    }

    public void SyncTowerStats(int towerIndex)
    {
        var tower = GameManager.instance.GetTower(towerIndex);
        if (tower == null)
            return;

        ChangeText("Panel/Text", "Cool Down: " + tower.stats.GetValue(TowerStatsType.CD));
        ChangeText("Panel/Text (1)", "Damage: " + tower.stats.GetValue(TowerStatsType.Damage));
        ChangeText("Panel/Text (2)", "RotationSpeed: " + tower.stats.GetValue(TowerStatsType.RotationSpeed));
        ChangeText("Panel/Text (3)", "Health: " + tower.stats.GetValue(TowerStatsType.Health));
        ChangeText("Panel/Text (4)", "FireRange: " + tower.stats.GetValue(TowerStatsType.FireRange));
        ChangeText("Panel/Text (5)", "Splatter: " + tower.stats.GetValue(TowerStatsType.SplatterDamage));
        ChangeText("Panel/Text (6)", "TargetsNum: " + tower.stats.GetValue(TowerStatsType.TargetsNum));
        ChangeText("Panel/Text (7)", "TowerID: " + tower.ID);
    }
}
