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
    private Dictionary<int, GameObject> enemyDict = new Dictionary<int, GameObject>();

    private Dictionary<string, List<GameObject>> gameObjPool = new Dictionary<string, List<GameObject>>();

    public int currentSelectTowerIndex = -1;
    public static GameManager instance { get; private set; }

    public EnemyWaveData waveData;

    private int currentWave = 0;
    Coroutine countDownCoroutine;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        var selectWheel = Resources.Load<Sprite>("Material/SelectWheel");
        //Debug.Log(selectWheel);

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

        var currentSelectTower = GetTower(currentSelectTowerIndex);
        if(currentSelectTower != null)
        {
            var enemy = SelectEnemy();
            if(enemy != null)
            {
                currentSelectTower.UnProjectSelectWheel();
                currentSelectTower.selectEnemyID = enemy.GetComponent<EnemyCtrl>().enemyID;
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

            if(enemyDict.Count == 0 && currentWave >= waveData.waveInfo.Count)
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

        while(true)
        {
            yield return delay;

            if (enemyDict.Count != 0)
                continue;

            if (currentWave >= waveData.waveInfo.Count)
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
        if(enemyDict.ContainsKey(id) == true)
            enemyDict.Remove(id);
    }

    public void ClearAllEnemy()
    {
        var enemyCtrlList = new List<GameObject>(enemyDict.Values);
        for(int i = 0; i < enemyCtrlList.Count; i++)
        {
            var enemyCtrl = enemyCtrlList[i].GetComponent<EnemyCtrl>();
            StartCoroutine(RealseObj(enemyCtrl.enemyName, enemyCtrlList[i], 0f));
        }
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
        var currentSelectTower = GetTower(currentSelectTowerIndex);
        if (currentSelectTower != null)
            currentSelectTower.UnProjectSelectWheel();

        var tower = GetTower(index);
        if (tower != null)
        {
            currentSelectTowerIndex = index;
            tower.ProjectSelectWheel();
        }

        var bottomUI = (BottomUI)UICtrl.instance.GetUI("BottomUI");
        bottomUI.SyncTowerStats(index);
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
            var prefabPath = "Prefab/" + info.enemyNameList[i].gameObject.name;
            var index = Random.Range(0, enemySpawnPos.Length);
            var type = Random.Range((int)EnemyType.Speed, (int)EnemyType.Child);

            var go = GetInstance(prefabPath, enemySpawnPos[index].position);
            go.GetComponent<EnemyCtrl>().SetEnemyColor(enemyColor[(int)type]);
            go.GetComponent<EnemyCtrl>().enemyName = prefabPath;

            var currentID = enemyID++;
            go.GetComponent<EnemyCtrl>().enemyID = currentID;
            go.GetComponent<EnemyCtrl>().StartAllCoroutine();
            enemyDict[currentID] = go;
        }
        
        //TODO...BUG!!
        currentWave = currentWave + 1;
        return true;
    }

    public GameObject GetInstance(string path, Vector3 pos)
    {
        if (gameObjPool.ContainsKey(path) == false)
            gameObjPool.Add(path, new List<GameObject>());

        if (gameObjPool[path].Count == 0)
        {
            var a = Resources.Load<GameObject>(path);
            var b = GameObject.Instantiate(a);
            gameObjPool[path].Add(b);
        }

        var obj = gameObjPool[path].First();
        obj.transform.position = pos;
        obj.SetActive(true);
        gameObjPool[path].RemoveAt(0);

        return obj;
    }

    public IEnumerator RealseObj(string path, GameObject obj, float time = 0f)
    {
        if (gameObjPool.ContainsKey(path) == false)
            yield break;

        yield return new WaitForSeconds(time);

        obj.SetActive(false);
        gameObjPool[path].Add(obj);
    }
}
