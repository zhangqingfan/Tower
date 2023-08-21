using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletCtrl : MonoBehaviour
{
    public bool isSplash;

    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            if(isSplash == false)
            {
                other.gameObject.GetComponent<EnemyCtrl>().OnHit(-1);
            }
            
            if(isSplash == true)
            {
                var splashEffect = GameManager.instance.GetInstance("Prefab/Fire_B", transform.position);
                GameManager.instance.StartCoroutine(GameManager.instance.RealseObj("Prefab/Fire_B", splashEffect, 0.3f));

                var results = Physics.OverlapSphere(transform.position, 0.25f, LayerMask.GetMask("Enemy"));
                for(int i = 0; i < results.Length; i++)
                {
                    var enemyCtrl = results[i].gameObject.GetComponent<EnemyCtrl>();
                    enemyCtrl.OnHit(-5);
                }
            }

            gameObject.SetActive(false);
        }
    }
}
