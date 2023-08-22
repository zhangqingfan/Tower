using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

enum EnemyType
{
    Speed,
    Normal,
    Dodge,
    Spawn,
    Child,
    TypeMax
};

public class GameManager : MonoBehaviour
{
    public Transform[] towerSpawnPos = new Transform[4];
    public Color[] towerColor = new Color[4];

    public Transform[] enemySpawnPos = new Transform[3];
    public Color[] enemyColor = new Color[(int)EnemyType.TypeMax];

    public List<TowerCtrl> towerCtrlList = new List<TowerCtrl>();

    private static int enemyID = 0;
    public Dictionary<int, GameObject> enemyDict = new Dictionary<int, GameObject>();

    private Dictionary<string, List<GameObject>> gameObjPool = new Dictionary<string, List<GameObject>>();
    public List<Sprite> selectNumList = new List<Sprite>();

    public int currentSelectTowerIndex = -1;
    public static GameManager instance { get; private set; }

    public EnemyWaveData waveData;

    private int currentWave = 0;
    public int gold = 0;

    private void Awake()
    {
        instance = this;
        gold = 0;

        selectNumList.Add(Resources.Load<Sprite>("Material/Select1"));
        selectNumList.Add(Resources.Load<Sprite>("Material/Select2"));
    }

    void Start()
    {
        var selectWheel = Resources.Load<Sprite>("Material/SelectWheel");

        for (int i = 0; i < towerSpawnPos.Length; i++)
        {
            var go = GetInstance("Prefab/Tower", towerSpawnPos[i].position);
            go.GetComponent<TowerCtrl>().selfColor = towerColor[i];
            go.GetComponent<TowerCtrl>().SetTowerColor(towerColor[i], selectWheel);
            go.GetComponent<TowerCtrl>().ID = i;

            towerCtrlList.Add(go.GetComponent<TowerCtrl>());
        }

        StartCoroutine(TryEndGame());
        StartCoroutine(TryCountDown());

        SelectTower(0);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) == true)
            SelectTower(0);

        if (Input.GetKeyDown(KeyCode.Alpha2) == true)
            SelectTower(1);

        if (Input.GetKeyDown(KeyCode.Alpha3) == true)
            SelectTower(2);

        if (Input.GetKeyDown(KeyCode.Alpha4) == true)
            SelectTower(3);

        if (Input.GetKeyDown(KeyCode.Q) == true)
            ChageTowerPosition(0);

        if (Input.GetKeyDown(KeyCode.W) == true)
            ChageTowerPosition(1);

        if (Input.GetKeyDown(KeyCode.E) == true)
            ChageTowerPosition(2);

        if (Input.GetKeyDown(KeyCode.R) == true)
            ChageTowerPosition(3);

        var currentSelectTower = GetTower(currentSelectTowerIndex);
        if (currentSelectTower != null)
        {
            var enemy = SelectEnemy();
            if (enemy != null)
            {
                currentSelectTower.UnProjectSelectWheel();
                currentSelectTower.AddSelectEnemy(enemy.GetComponent<EnemyCtrl>().enemyID);
                currentSelectTower.ProjectSelectWheel();
            }
        }
    }

    public IEnumerator TryEndGame()
    {
        var delay = new WaitForSeconds(1f);

        while (true)
        {
            yield return delay;

            if (IsAllTowerDestory() == true)
            {
                yield return delay;
                yield return delay;

                var upUI = (UpUI)UICtrl.instance.GetUI("UpUI");
                upUI.ShowEnd(false);
                yield break;
            }

            if (enemyDict.Count == 0 && currentWave >= waveData.waveInfo.Count)
            {
                yield return delay;
                yield return delay;

                var upUI = (UpUI)UICtrl.instance.GetUI("UpUI");
                upUI.ShowEnd(true);
                yield break;
            }
        }
    }

    public IEnumerator TryCountDown()
    {
        var delay = new WaitForSeconds(1f);

        while (true)
        {
            yield return delay;

            if (enemyDict.Count != 0)
                continue;

            if (currentWave >= waveData.waveInfo.Count)
                yield break;

            if(IsAllTowerDestory() == true)
                yield break;

            yield return delay;
            yield return delay;

            var upUI = (UpUI)UICtrl.instance.GetUI("UpUI");

            for (int countDownTime = 5; countDownTime >= 0; countDownTime--)
            {
                upUI.ShowCountDown(countDownTime, true);
                yield return new WaitForSeconds(1f);
            }

            upUI.ShowCountDown(-1, false);
            GenerateEnemyWave();
        }
    }

    public void RemoveEnemy(int id)
    {
        if (enemyDict.ContainsKey(id) == true)
            enemyDict.Remove(id);
    }

    public void ClearAllEnemy()
    {
        var enemyCtrlList = new List<GameObject>(enemyDict.Values);
        for (int i = 0; i < enemyCtrlList.Count; i++)
        {
            var enemyCtrl = enemyCtrlList[i].GetComponent<EnemyCtrl>();
            StartCoroutine(RealseObj(enemyCtrl.enemyName, enemyCtrlList[i]));
        }
        enemyDict.Clear();
    }

    public GameObject GetEnemy(int id)
    {
        if (enemyDict.ContainsKey(id) == false)
            return null;
        return enemyDict[id];
    }

    public TowerCtrl GetTower(int index)
    {
        if (index < 0 || index >= towerSpawnPos.Length)
            return null;

        if (towerCtrlList[index].gameObject.activeSelf == false)
            return null;

        return towerCtrlList[index];
    }

    void SelectTower(int index)
    {
        var tower = GetTower(index);
        if (tower == null)
            return;

        var currentSelectTower = GetTower(currentSelectTowerIndex);
        if (currentSelectTower != null)
            currentSelectTower.UnProjectSelectWheel();

        currentSelectTowerIndex = index;
        tower.ProjectSelectWheel();
        
        var bottomUI = (BottomUI)UICtrl.instance.GetUI("BottomUI");
        bottomUI.SyncTowerStats(index);
        bottomUI.SyncTowerUpgrade(index);

        var toggleUpgrade = bottomUI.GetToogleState("TabNode/Upgrade");
        var toggleStat = bottomUI.GetToogleState("TabNode/Stat");

        if(toggleStat == false && toggleUpgrade == false)
            bottomUI.CheckToogle("TabNode/Upgrade", true);
    }

    void ChageTowerPosition(int index)
    {
        if (currentSelectTowerIndex < 0 || currentSelectTowerIndex >= towerCtrlList.Count)
            return;

        var temp = towerCtrlList[currentSelectTowerIndex];
        towerCtrlList[currentSelectTowerIndex] = towerCtrlList[index];
        towerCtrlList[index] = temp;

        towerCtrlList[index].ID = index;
        towerCtrlList[index].transform.position = towerSpawnPos[index].position;

        towerCtrlList[currentSelectTowerIndex].ID = currentSelectTowerIndex;
        towerCtrlList[currentSelectTowerIndex].transform.position = towerSpawnPos[currentSelectTowerIndex].position;

        SelectTower(index);
    }

    public void RemoveTowersTargetingEnemy(int enemyID)
    {
        for(int i = 0; i < towerCtrlList.Count; i++)
        {
            var tower = GetTower(i);
            if (tower != null)
                tower.TryRemoveEnemy(enemyID);
        }
    }

    GameObject SelectEnemy()
    {
        if (Input.GetMouseButtonDown(0) == false)
            return null;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 999, LayerMask.GetMask("Enemy")) == false)
            return null;

        return hit.collider.transform.gameObject;
    }

    public bool IsAllTowerDestory()
    {
        foreach (var t in towerCtrlList)
        {
            if (t.gameObject.activeSelf == true)
                return false;
        }
        return true;
    }

    public TowerCtrl GetRandomTower()
    {
        for (int i = 0; i < 200; i++)
        {
            var index = Random.Range(0, towerCtrlList.Count);

            if (towerCtrlList[index].gameObject.activeSelf == true)
                return towerCtrlList[index];
        }

        return null;
    }

    public bool IsTowerDestory(int index)
    {
        if (index < 0 || index >= towerCtrlList.Count)
            return true;

        return !towerCtrlList[index].gameObject.activeSelf;
    }

    bool GenerateEnemyWave()
    {
        if (currentWave >= waveData.waveInfo.Count)
            return false;

        var info = waveData.waveInfo[currentWave];
        for (int i = 0; i < info.enemyNameList.Count; i++)
        {
            var index = Random.Range(0, enemySpawnPos.Length);
            var pos = enemySpawnPos[index].position; //TODO...BUG
            //Debug.Log(pos);
            var prefabPath = "Prefab/" + info.enemyNameList[i].gameObject.name;

            //Debug.Log(index);
            CreateEnemy(prefabPath, pos);
        }

        //TODO...BUG!!
        currentWave = currentWave + 1;
        return true;
    }

    public void CreateEnemy(string prefabPath, Vector3 position)
    {
        var type = Random.Range((int)EnemyType.Speed, (int)EnemyType.Child);

        var go = GetInstance(prefabPath, position);
        go.GetComponent<EnemyCtrl>().SetEnemyColor(enemyColor[(int)type]);
        go.GetComponent<EnemyCtrl>().enemyName = prefabPath;

        var currentID = enemyID++;
        go.GetComponent<EnemyCtrl>().enemyID = currentID;
        go.GetComponent<EnemyCtrl>().StartAllCoroutine();
        enemyDict[currentID] = go;

        //Debug.Log(go.transform.position);
    }

    public GameObject GetInstance(string path, Vector3 pos)
    {
        if (gameObjPool.ContainsKey(path) == false)
            gameObjPool.Add(path, new List<GameObject>());

        if (gameObjPool[path].Count == 0)
        {
            var a = Resources.Load<GameObject>(path);
            var b = GameObject.Instantiate(a, pos, Quaternion.identity);
            gameObjPool[path].Add(b);
        }

        var obj = gameObjPool[path].First();
        obj.transform.position = pos;
        obj.SetActive(true);
        gameObjPool[path].RemoveAt(0);

        return obj;
    }

    public IEnumerator RealseObj(string path, GameObject obj, float time = -1f)
    {
        if (gameObjPool.ContainsKey(path) == false)
            yield break;

        if (time > 0)
            yield return new WaitForSeconds(time);

        obj.SetActive(false);
        gameObjPool[path].Add(obj);
    }

    public void RepairAllTowers()
    {
        for(int i = 0; i < towerCtrlList.Count; i++)
        {
            if(towerCtrlList[i].gameObject.activeSelf == true)
            {
                var maxHp = towerCtrlList[i].stats.GetValue(TowerStatsType.MaxHealth);
                towerCtrlList[i].stats.SetValue(TowerStatsType.Health, maxHp);

                var bottomUI = (BottomUI)UICtrl.uiDict["BottomUI"];
                bottomUI.SyncTowerStats(i);

                var effect = GetInstance("Prefab/Star_A", towerCtrlList[i].transform.position);
                StartCoroutine(RealseObj("Prefab/Star_A", effect, 0.5f));
            }
        }
    }

    public void ChangeAllEnemySpeed(float speedRatio)
    {
        foreach(var pair in enemyDict)
        {
            var enemyCtrl = pair.Value.GetComponent<EnemyCtrl>();
            enemyCtrl.agent.speed = enemyCtrl.baseSpeed * speedRatio;
        }
    }
}
