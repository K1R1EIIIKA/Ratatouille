using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("PanTarget"))
        {
            Debug.Log(1);
            if (other.TryGetComponent(out Rigidbody rb))
                rb.velocity = Vector3.zero;
            
            if (other.TryGetComponent(out EggMovingWithCamera egg))
                AudioManager.Instance.Play("Death");
            
            CheckPointLogic.Instance.SpawnOnCheckPoint(other.gameObject);
        }
    }
}
