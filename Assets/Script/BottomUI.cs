using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BottomUI : UICtrl
{
    private void Awake()
    {
        base.Awake();
        uiDict["BottomUI"] = this;

        ChangeText("PanelStats/Text", "Cool Down: ");
        ChangeText("PanelStats/Text (1)", "Damage: ");
        ChangeText("PanelStats/Text (2)", "Rotation: ");
        ChangeText("PanelStats/Text (3)", "Health: ");
        ChangeText("PanelStats/Text (4)", "FireRange: ");
        ChangeText("PanelStats/Text (5)", "Splatter: ");
        ChangeText("PanelStats/Text (6)", "TargetsNum: ");
        ChangeText("PanelStats/Text (7)", "TowerID: ");

        ChangeText("PanelUpgrade/Text", "0");
        ChangeText("PanelUpgrade/Text (1)", "0");
        ChangeText("PanelUpgrade/Text (2)", "0");
        ChangeText("PanelUpgrade/Text (3)", "0");
        ChangeText("PanelUpgrade/Text (4)", "0");

        AddButtonListener("PanelUpgrade/Button", () => UpgradeStats(TowerStatsType.CD));
        AddButtonListener("PanelUpgrade/Button (1)", () => UpgradeStats(TowerStatsType.RotationSpeed));
        AddButtonListener("PanelUpgrade/Button (2)", () => UpgradeStats(TowerStatsType.MaxHealth));
        AddButtonListener("PanelUpgrade/Button (3)", () => UpgradeStats(TowerStatsType.FireRange));
        AddButtonListener("PanelUpgrade/Button (4)", () => UpgradeStats(TowerStatsType.TargetsNum));

        elemDict["PanelUpgrade"].SetActive(false);

    }

    public void SyncTowerStats(int towerIndex)
    {
        var tower = GameManager.instance.GetTower(towerIndex);
        if (tower == null)
            return;

        ChangeText("PanelStats/Text", "Cool Down: " + tower.stats.GetValue(TowerStatsType.CD));
        ChangeText("PanelStats/Text (1)", "Damage: " + tower.stats.GetValue(TowerStatsType.Damage));
        ChangeText("PanelStats/Text (2)", "RotationSpeed: " + tower.stats.GetValue(TowerStatsType.RotationSpeed));
        ChangeText("PanelStats/Text (3)", "Health: " + tower.stats.GetValue(TowerStatsType.Health));
        ChangeText("PanelStats/Text (4)", "FireRange: " + tower.stats.GetValue(TowerStatsType.FireRange));
        ChangeText("PanelStats/Text (5)", "Splatter: " + tower.stats.GetValue(TowerStatsType.SplatterDamage));
        ChangeText("PanelStats/Text (6)", "TargetsNum: " + tower.stats.GetValue(TowerStatsType.TargetsNum));
        ChangeText("PanelStats/Text (7)", "TowerID: " + tower.ID);
    }

    public void SyncTowerUpgrade(int towerIndex)
    {
        var tower = GameManager.instance.GetTower(towerIndex);
        if (tower == null)
            return;

        ChangeText("PanelUpgrade/Text", tower.stats.GetUpgradeValue(TowerStatsType.CD).ToString());
        ChangeText("PanelUpgrade/Text (1)", tower.stats.GetUpgradeValue(TowerStatsType.RotationSpeed).ToString());
        ChangeText("PanelUpgrade/Text (2)", tower.stats.GetUpgradeValue(TowerStatsType.MaxHealth).ToString());
        ChangeText("PanelUpgrade/Text (3)", tower.stats.GetUpgradeValue(TowerStatsType.FireRange).ToString());
        ChangeText("PanelUpgrade/Text (4)", tower.stats.GetUpgradeValue(TowerStatsType.TargetsNum).ToString());
    }

    void UpgradeStats(TowerStatsType type)
    {
        var towerIndex = GameManager.instance.currentSelectTowerIndex;
        var towerCtrl = GameManager.instance.GetTower(towerIndex);
        if (towerCtrl == null)
            return;

        towerCtrl.stats.UpgradeStats(type);
        SyncTowerStats(towerIndex);
        SyncTowerUpgrade(towerIndex);
    }
}
