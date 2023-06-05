using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BikeCollider : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == LayerMask.NameToLayer("Traffic"))
            Debug.Log(other.name);
    }
}
