using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class KillWendigo : MonoBehaviour
{
    public string bulletTag = string.Empty;
    
    void OnCollisionEnter (Collision col) {
        if (col.gameObject.tag == bulletTag ) {
            Destroy(gameObject);
        }
    }
}
