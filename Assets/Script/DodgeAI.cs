using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DodgeAI : MonoBehaviour
{
    public int dodgeTime = 15;
    float detectRange = 0.3f;

    private EnemyCtrl enemyCtrl;
    //private NavMeshAgent agent;
    void Start()
    {
        enemyCtrl = GetComponent<EnemyCtrl>();
        //agent = GetComponent<NavMeshAgent>();
        StartCoroutine(DodgeBullet());
    }

    public IEnumerator DodgeBullet()
    {
        var delay = new WaitForSeconds(0.1f);

        while (true)
        {
            yield return delay;

            if (dodgeTime <= 0)
                continue;

            var results = Physics.OverlapSphere(transform.position, detectRange, LayerMask.GetMask("Bullet"));
            if (results.Length == 0)
                continue;

            for (int i = 0; i < 150; i++) 
            {
                var randomSpherePos = Random.insideUnitSphere * 1.0f;
                var randomPos = transform.position;
                //randomSpherePos.x = (randomSpherePos.x < detectRange ? detectRange * 1.5f : randomSpherePos.x);
                randomPos.x = randomSpherePos.x;
                //randomPos.z = 0;// transform.position.z + randomSpherePos.z;

                NavMeshHit hit;
                if (NavMesh.SamplePosition(randomPos, out hit, 0.01f, 1 << NavMesh.GetAreaFromName("Walkable")) == true)
                {
                    randomPos = hit.position;
                    enemyCtrl.TryWrap(randomPos);
                    dodgeTime = dodgeTime - 1;
                    
                    break;
                }
            }
        }
    }

    private void OnDisable()
    {
        dodgeTime = 15;
    }

    private void OnEnable()
    {
        StartCoroutine(DodgeBullet());
    }
}
