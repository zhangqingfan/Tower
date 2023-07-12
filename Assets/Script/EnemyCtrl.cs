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
    private NavMeshAgent agent;


    private void Awake()
    {
        projector = transform.GetComponentInChildren<Projector>();
        
        agent = GetComponent<NavMeshAgent>();
        agent.speed = 0.1f;
        agent.stoppingDistance = 0.01f;
        agent.angularSpeed = 0f;
        agent.acceleration = 600f;
    }

    public IEnumerator DetectTower()
    {
        var delay = new WaitForSeconds(0.5f);

        while (true)
        {
            yield return delay;

            if(GameManager.instance.IsTowerDestory(targetTowerIndex) == true)
            {
                var tower = GameManager.instance.GetRandomTower();
                if (tower == null)
                {
                    agent.SetDestination(transform.position);
                    break;
                }

                targetTowerIndex = tower.ID;
                agent.SetDestination(tower.transform.position);
            }
        }
    }

    public void StartAllCoroutine()
    {
        StartCoroutine(DetectTower());
    }

    private void OnDisable()
    {
        targetTowerIndex = -1;
        enemyID = -1;
        projector.enabled = false;
        GameManager.instance.RemoveEnemy(enemyID);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Tower"))
        {
            Debug.Log("tower!");
            Debug.Log(gameObject);
            StartCoroutine(GameManager.instance.RealseObj(enemyName, gameObject, 0f));
        }
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
