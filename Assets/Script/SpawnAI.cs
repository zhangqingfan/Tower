using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnAI : MonoBehaviour
{
    private EnemyCtrl enemyCtrl;
    private Animation animation;
    public int originalSpawnTime = 1;

    private int currentSpawnTime = -1;

    void Start()
    {
        enemyCtrl = GetComponent<EnemyCtrl>();
        animation = GetComponent<Animation>();
        currentSpawnTime = originalSpawnTime;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Bullet"))
        {
            if (currentSpawnTime == 0)
                return;

            if (animation != null && animation.isPlaying == false)
                animation.Play("HitEffect");
        }
    }

    //animation event!!
    void SpawnChild()
    {
        for(int i = 0; i < 2; i++)
        {
            //for (int j = 0; j < 150; i++)
            {
                var randomPos = transform.position;
                
                if(i == 0)
                    randomPos.x -= 0.13f;
                if(i == 1)
                    randomPos.x += 0.13f;

                UnityEngine.AI.NavMeshHit hit;
                if (UnityEngine.AI.NavMesh.SamplePosition(randomPos, out hit, 0.1f, 1 << UnityEngine.AI.NavMesh.GetAreaFromName("Walkable")) == true)
                {
                    randomPos = hit.position;
                    GameManager.instance.CreateEnemy("Prefab/EnemyNormal", randomPos);
                }
            }
        }

        currentSpawnTime -= 1;
    }

    private void OnDisable()
    {
        currentSpawnTime = originalSpawnTime;
    }
}
