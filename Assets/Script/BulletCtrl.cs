using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletCtrl : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            //other.gameObject.GetComponent<EnemyCtrl>().OnHit(-1);
            //TODO...BUG 
            gameObject.SetActive(false);
        }
    }
}
