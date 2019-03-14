using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowForward : MonoBehaviour {

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawRay(transform.position + transform.up * 0.005f, transform.forward * 0.05f);
        Gizmos.DrawSphere(transform.position + transform.forward * 0.05f + transform.up * 0.005f, 0.005f);
    }

}
