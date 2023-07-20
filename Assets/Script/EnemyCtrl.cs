using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyCtrl : MonoBehaviour
{
    public int enemyID = -1;
    public float speed = 0f;
    public int targetTowerIndex = -1;
    public string enemyName;

    public Projector projector;
    public NavMeshAgent agent;

    public Stack<Vector3> destinationStack = new Stack<Vector3>();

    private void Awake()
    {
        projector = transform.GetComponentInChildren<Projector>();
        
        agent = GetComponent<NavMeshAgent>();
        agent.speed = 0.1f;
        agent.stoppingDistance = 0.01f;
        agent.angularSpeed = 0f;
        agent.acceleration = 600f;
                                   
        agent.SetDestination(transform.position);
    }

    private void Start()
    {
        if (enemyName == "Prefab/EnemySpeed")
        {
            agent.speed *= 5;
            Debug.Log("123123");
        }
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
                var destPos = transform.position;

                tower = GameManager.instance.GetRandomTower();
                if(tower != null)
                {
                    targetTowerIndex = tower.ID;
                    destPos = tower.transform.position;
                }

                agent.SetDestination(destPos);
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
        //Debug.Log("1231232");
        targetTowerIndex = -1;
        enemyID = -1;
        projector.enabled = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Tower"))
        {
            //Debug.Log("tower!");
            var towerCtrl = other.gameObject.GetComponent<TowerCtrl>();
            towerCtrl.ChangeHP(-5);
                        
            GameManager.instance.RemoveEnemy(enemyID);
            StartCoroutine(GameManager.instance.RealseObj(enemyName, gameObject, 0f));
        }
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
