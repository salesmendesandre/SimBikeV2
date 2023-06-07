using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BikeCollider : MonoBehaviour
{
    public UnityEvent OnCollision = new();
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Traffic"))
        {
            if (other.name != "FrontTrigger")
            {
                OnCollision.Invoke();
            }

        }
    }
}
