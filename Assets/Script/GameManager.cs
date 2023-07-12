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

    private void Awake()
    {
        instance = this;
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

        //TODO...BUG!!!!!
        for (int i = 0; i < 1; i++)
        {
            var go = SpawnEnemy();

            var currentID = enemyID++;
            go.GetComponent<EnemyCtrl>().enemyID = currentID;
            go.GetComponent<EnemyCtrl>().StartAllCoroutine();
            enemyDict[currentID] = go;
        }
    }

    public void RemoveEnemy(int id)
    {
        enemyDict.Remove(id);
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

    bool IsAllTowerDestory()
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

    GameObject SpawnEnemy()
    {
        var index = Random.Range(0, enemySpawnPos.Length);
        var type = Random.Range((int)EnemyType.Speed, (int)EnemyType.Child);

        string prefabPath;
        switch (type)
        {
            case (int)EnemyType.Normal:
                prefabPath = "Prefab/EnemyNormal";
                break;

            case (int)EnemyType.Speed:
                prefabPath = "Prefab/EnemyNormal";
                break;

            case (int)EnemyType.Dodge:
                prefabPath = "Prefab/EnemyDodge";
                break;

            case (int)EnemyType.Spawn:
                prefabPath = "Prefab/EnemyDodge";
                break;

            default:
                prefabPath = "Prefab/EnemyNormal";
                break;
        }
        
        var go = GetInstance(prefabPath, enemySpawnPos[index].position);
        go.GetComponent<EnemyCtrl>().SetEnemyColor(enemyColor[(int)type]);
        go.GetComponent<EnemyCtrl>().enemyName = prefabPath;

        return go;
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
