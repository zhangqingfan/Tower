using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerCtrl : MonoBehaviour
{
    public int ID;
    public Color selfColor;
    public int selectEnemyID = -1;

    public Projector projector;
    public Material projectorMat;
    public float recentFireTime;

    private GameObject bulletStartPos;
    public TowerStats stats;

    private void Awake()
    {
        bulletStartPos = transform.Find("FirePos").gameObject;
        projector = transform.GetComponentInChildren<Projector>();
        
        var shader = Shader.Find("Unlit/ProjectorShader");
        projectorMat = new Material(shader);
    }

    // Start is called before the first frame update
    void Start()
    {
        stats = transform.GetComponentInChildren<TowerStats>();

        StartCoroutine(DetectEnemy());
        StartCoroutine(AimingEnemy());
        StartCoroutine(TryShooting());
    }

    public void SetTowerColor(Color color, Sprite selectSprite)
    {
        var meshRenderList = GetComponentsInChildren<MeshRenderer>();
        foreach (var m in meshRenderList)
        {
            var mpb = new MaterialPropertyBlock();
            m.GetPropertyBlock(mpb);
            mpb.SetColor("_Color", color);
            m.SetPropertyBlock(mpb);
        }

        projectorMat.SetTexture("_MainTex", selectSprite.texture);
        projectorMat.SetColor("_Color", color);
        projector.material = projectorMat;
    }

    public void UnProjectSelectWheel()
    {
        projector.enabled = false;

        var currentSelectEnemy = GameManager.instance.GetEnemy(selectEnemyID);
        if (currentSelectEnemy != null)
            currentSelectEnemy.GetComponent<EnemyCtrl>().projector.enabled = false;
    }

    public void ProjectSelectWheel()
    {
        projector.enabled = true;
        projector.material = projectorMat;

        var enemy = GameManager.instance.GetEnemy(selectEnemyID);
        if (enemy != null)
        {
            enemy.GetComponent<EnemyCtrl>().projector.enabled = true;
            enemy.GetComponent<EnemyCtrl>().projector.material = projectorMat;
        }
    }

    public IEnumerator DetectEnemy()
    {
        var delay = new WaitForSeconds(0.5f); 

        while (true)
        {
            yield return delay;

            var targetObj = GameManager.instance.GetEnemy(selectEnemyID);
            if (targetObj != null)
                continue;

            //todo...bug!!
            var results = Physics.OverlapSphere(bulletStartPos.transform.position, stats.GetValue(TowerStatsType.FireRange), LayerMask.GetMask("Enemy")); //LayerToName

            float minDistance = 999999;
            for (int i = 0; i < results.Length; i++)
            {
                var distance = Vector3.Distance(transform.position, results[i].gameObject.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    //Debug.Log(results[i].gameObject.GetComponent<EnemyCtrl>());
                    selectEnemyID = results[i].gameObject.GetComponent<EnemyCtrl>().enemyID; //TODO BUG!!!
                }
            }

            if(GameManager.instance.currentSelectTowerIndex == ID)
            {
                UnProjectSelectWheel();
                ProjectSelectWheel();
            }
        }
    }

    public void ChangeHP(int value)
    {
        var hp = stats.GetValue(TowerStatsType.Health);
        hp += value;
        hp = Mathf.Clamp(hp, 0, stats.GetValue(TowerStatsType.MaxHealth));
        stats.SetValue(TowerStatsType.Health, hp);

        var bottomUI = (BottomUI)UICtrl.instance.GetUI("BottomUI");
        bottomUI.SyncTowerStats(ID);

        if (hp == 0)
            this.gameObject.SetActive(false);
    }

    public IEnumerator AimingEnemy()
    {
        Quaternion rotateQuater = new Quaternion();
        
        while (true)
        {
            yield return null;

            var targetObj = GameManager.instance.GetEnemy(selectEnemyID);
            if (targetObj == null)
                continue;

            var dir = Vector3.Normalize(targetObj.transform.position - transform.position);
            rotateQuater = Quaternion.LookRotation(dir);
            
            var euler = rotateQuater.eulerAngles;
            euler.x = 0;
            rotateQuater.eulerAngles = euler;
            
            transform.rotation = Quaternion.RotateTowards(transform.rotation, rotateQuater, Time.deltaTime * stats.GetValue(TowerStatsType.RotationSpeed));
        }
    }

    public IEnumerator TryShooting()
    {
        var delay = new WaitForSeconds(0.2f);

        while (true)
        {
            yield return delay;

            var targetObj = GameManager.instance.GetEnemy(selectEnemyID);
            if (targetObj == null)
                continue;

            var dir = transform.forward;
            var hits = Physics.RaycastAll(bulletStartPos.transform.position, dir, stats.GetValue(TowerStatsType.FireRange), LayerMask.GetMask("Enemy"));

            for(int i = 0; i < hits.Length; i++)
            {
                if (hits[i].transform.gameObject != targetObj)
                    continue;

                if (Time.time - recentFireTime < stats.GetValue(TowerStatsType.CD))
                    break;

                recentFireTime = Time.time;

                var bullet = GameManager.instance.GetInstance("Prefab/Bullet", bulletStartPos.transform.position);
                bullet.GetComponent<MeshRenderer>().material.color = selfColor;
                bullet.transform.rotation = Quaternion.LookRotation(dir);
                bullet.GetComponent<Rigidbody>().velocity = bullet.transform.forward * 1.0f;
                StartCoroutine(GameManager.instance.RealseObj("Prefab/Bullet", bullet, 5.0f));
            }
        }
}

    //增加色彩 X
    //测试出生逻辑 X
    //测试距离 1.6f X
    //炮塔瞄准逻辑 X
    //增加炮弹逻辑 X
    //增加炮塔属性 X
    //增加炮弹接触逻辑 X
    //增加炮弹自毁逻辑 X
    //增加出怪点逻辑 X
    //增加怪物字典逻辑 X
    //增加navemesh X
    //增加投影选择栏 X
    //增加NPC瞄准投影 X
    //增加dodge类型的NPC X
    //增加闪避AI X
    //增加锁定NPC功能 X
    //修改摄像机和屏幕长宽 X
    //增加UI栏属性对应 X
    //增加怪物属性 X
    //增加速度怪物 X
    //增加繁殖AI 
    //增加繁殖NPC
    //增加属性升级面板 
    //增加胜负条件 X
    //增加怪物波次概念 X
    //增加塔死亡逻辑 X
    //增加怪物死亡逻辑
    //调整数值
    //增加全屏幕大招
}
