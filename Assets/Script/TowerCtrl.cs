using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerCtrl : MonoBehaviour
{
    [HideInInspector]
    //public int selectEnemyID = -1;
    private List<int> selectEnemyList = new List<int>();

    public int ID;
    public Color selfColor;

    public Projector projector;
    public Material projectorMat;
    public List<Material> enemyMat = new List<Material>();
    public float recentFireTime;

    private GameObject bulletStartPos;
    public TowerStats stats;

    private void Awake()
    {
        bulletStartPos = transform.Find("FirePos").gameObject;
        projector = transform.GetComponentInChildren<Projector>();

        var shader = Shader.Find("Unlit/ProjectorShader");
        projectorMat = new Material(shader);

        stats = transform.GetComponentInChildren<TowerStats>();
    }

    // Start is called before the first frame update
    void Start()
    {
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
        projectorMat.DisableKeyword("_SelectNumber");
        projector.material = projectorMat;

        var shader = Shader.Find("Unlit/ProjectorShader");

        for (int i = 0; i < GameManager.instance.selectNumList.Count; i++)
        {
            var sprite = GameManager.instance.selectNumList[i];
            var mat = new Material(shader);
            mat.SetTexture("_MainTex", selectSprite.texture);
            mat.SetTexture("_SelectTex", sprite.texture);
            mat.SetColor("_Color", color);
            mat.EnableKeyword("_SelectNumber");
            enemyMat.Add(mat);
        }
    }

    public void TryRemoveEnemy(int enemyID)
    {
        for (int i = selectEnemyList.Count - 1; i >= 0; i--)
        {
            if (selectEnemyList[i] == enemyID)
                selectEnemyList.RemoveAt(i);
        }
    }

    public void AddSelectEnemy(int enemyID)
    {
        var enemy = GameManager.instance.GetEnemy(enemyID);
        if (enemy == null)
            return;

        TryRemoveEnemy(enemyID);

        while(selectEnemyList.Count >= stats.GetValue(TowerStatsType.TargetsNum))
            selectEnemyList.RemoveAt(0);

        selectEnemyList.Insert(0, enemyID);
    }
    
    public void UnProjectSelectWheel()
    {
        projector.enabled = false;

        foreach(var elem in selectEnemyList)
        {
            var currentSelectEnemy = GameManager.instance.GetEnemy(elem);
            if (currentSelectEnemy != null)
                currentSelectEnemy.GetComponent<EnemyCtrl>().projector.enabled = false;
        }
    }

    public void ProjectSelectWheel()
    {
        projector.enabled = true;
        projector.material = projectorMat;

        for (int i = 0; i < selectEnemyList.Count; i++)
        {
            var enemy = GameManager.instance.GetEnemy(selectEnemyList[i]);

            if (enemy != null)
            {
                enemy.GetComponent<EnemyCtrl>().projector.enabled = true;
                enemy.GetComponent<EnemyCtrl>().projector.material = projectorMat;

                if(stats.GetValue(TowerStatsType.TargetsNum) > 1)
                    enemy.GetComponent<EnemyCtrl>().projector.material = enemyMat[i];
            }
        }
    }

    public IEnumerator DetectEnemy()
    {
        var delay = new WaitForSeconds(0.2f); 

        while (true)
        {
            yield return delay;

            if (selectEnemyList.Count != 0)
                continue;
            
            var results = Physics.OverlapSphere(transform.position, stats.GetValue(TowerStatsType.FireRange), LayerMask.GetMask("Enemy")); //LayerToName
           
            EnemyCtrl enemyCtrl = null;
            float minDistance = 999999;
            
            for (int i = 0; i < results.Length; i++)
            {
                var distance = Vector3.Distance(transform.position, results[i].gameObject.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;

                    enemyCtrl = results[i].gameObject.GetComponent<EnemyCtrl>();
                    if(enemyCtrl == null)
                    {
                        Debug.Log(results[i].gameObject);
                        Debug.Log("null!!");
                        continue;
                    }
                }
            }

            if (enemyCtrl == null)
                continue;

            selectEnemyList.Insert(0, enemyCtrl.enemyID);

            if (GameManager.instance.currentSelectTowerIndex == ID)
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
        {
            gameObject.SetActive(false);

            if (GameManager.instance.currentSelectTowerIndex == ID)
            {
                UnProjectSelectWheel();

                //TODO...BUG!!
                bottomUI.elemDict["PanelUpgrade"].SetActive(false);
                bottomUI.elemDict["PanelStats"].SetActive(false);

                bottomUI.CheckToogle("TabNode/Upgrade", false);
                bottomUI.CheckToogle("TabNode/Stat", false);
            }
        }
    }

    public IEnumerator AimingEnemy()
    {
        Quaternion rotateQuater = new Quaternion();
        
        while (true)
        {
            yield return null;

            if (selectEnemyList.Count == 0)
                continue;

            var targetObj = GameManager.instance.GetEnemy(selectEnemyList[0]);
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

            if (selectEnemyList.Count == 0)
                continue;

            var targetObj = GameManager.instance.GetEnemy(selectEnemyList[0]);
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
                bullet.GetComponent<BulletCtrl>().isSplash = stats.GetUpgradeValue(TowerStatsType.Splash) > 0 ? true : false;
                bullet.transform.rotation = Quaternion.LookRotation(dir);
                bullet.GetComponent<Rigidbody>().velocity = bullet.transform.forward * 1.0f;
                GameManager.instance.StartCoroutine(GameManager.instance.RealseObj("Prefab/Bullet", bullet, 5.0f));
            }
        }
    }
}
