using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyCtrl : MonoBehaviour
{
    [HideInInspector]
    public int enemyID = -1;
    
    [HideInInspector]
    public float baseSpeed = 0.1f;
    
    [HideInInspector]
    public int targetTowerIndex = -1;
    
    [HideInInspector]
    public string enemyName;

    [HideInInspector]
    public int originalHp = 10;

    private int currentHp = 0;

    public Projector projector;
    public NavMeshAgent agent;

    public Stack<Vector3> destinationStack = new Stack<Vector3>();

    private void Awake()
    {
        projector = transform.GetComponentInChildren<Projector>();
        
        agent = GetComponent<NavMeshAgent>();
        //agent.speed = baseSpeed;
        agent.stoppingDistance = 0.01f;
        agent.angularSpeed = 0f;
        agent.acceleration = 600f;
                                   
        agent.SetDestination(transform.position);
    }

    private void Start()
    {
        if (enemyName == "Prefab/EnemySpeed")
            baseSpeed *= 5;
        
        agent.speed = baseSpeed;
        currentHp = originalHp;
    }

    public IEnumerator Move()
    {
        var delay = new WaitForSeconds(0.1f);

        while (true)
        {
            yield return delay;

            if (destinationStack.Count == 0)
                continue;

            if (agent.remainingDistance > agent.stoppingDistance)
                continue;

            if(agent.destination == destinationStack.Peek())
                destinationStack.Pop();

            if(destinationStack.Count > 0)
                agent.SetDestination(destinationStack.Peek());
        }
    }

    public IEnumerator DetectTower()
    {
        var delay = new WaitForSeconds(0.5f);

        while (true)
        {
            yield return delay;

            var tower = GameManager.instance.GetTower(targetTowerIndex);
            
            if (tower == null)  
            {
                targetTowerIndex = -1;
                tower = GameManager.instance.GetRandomTower();
            }

            agent.SetDestination(transform.position);

            if (tower != null)
            {
                targetTowerIndex = tower.ID;
                agent.SetDestination(tower.transform.position);
            }
        }
    }

    public void StartAllCoroutine()
    {
        StartCoroutine(DetectTower());
        //StartCoroutine(Move());
    }

    private void OnEnable()
    {
        StartAllCoroutine();
    }

    private void OnDisable()
    {
        GameManager.instance.RemoveEnemy(enemyID);
        GameManager.instance.RemoveTowersTargetingEnemy(enemyID);

        if (projector.enabled == true)
        {
            var tower = GameManager.instance.GetTower(GameManager.instance.currentSelectTowerIndex);
            if(tower != null)
            {
                tower.UnProjectSelectWheel();
                tower.ProjectSelectWheel();
            }
        }

        targetTowerIndex = -1;
        enemyID = -1;
        projector.enabled = false;
        currentHp = originalHp;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Tower"))
        {
            var towerCtrl = other.gameObject.GetComponent<TowerCtrl>();
            towerCtrl.ChangeHP(-5);
                        
            //GameManager.instance.RemoveEnemy(enemyID);
            StartCoroutine(GameManager.instance.RealseObj(enemyName, gameObject));
        }
    }

    public void OnHit(int damage)
    {
        currentHp += damage;
        currentHp = (currentHp >= 0 ? currentHp : 0);

        if (currentHp <= 0)
            StartCoroutine(GameManager.instance.RealseObj(enemyName, gameObject));
    }

    public void TryWrap(Vector3 pos)
    {
        var currentDestination = agent.destination;
        agent.Warp(pos);
        agent.destination = currentDestination;
    }

    public void SetEnemyColor(Color color)
    {
        var meshRenderList = GetComponentsInChildren<MeshRenderer>();
        foreach (var m in meshRenderList)
        {
            var mpb = new MaterialPropertyBlock();
            m.GetPropertyBlock(mpb);
            mpb.SetColor("_Color", color);
            m.SetPropertyBlock(mpb);
        }
    }
}
