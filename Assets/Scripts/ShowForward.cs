using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowForward : MonoBehaviour {

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        DrawGizmos();
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        DrawGizmos();
    }

    private void DrawGizmos()
    {
        if (transform.parent == null)
        {
            Gizmos.color = Color.magenta;
        }

        Gizmos.DrawCube(transform.position, new Vector3(0.01f, 0.01f, 0.01f));
        Gizmos.DrawRay(transform.position, transform.forward * 0.05f);
        Gizmos.DrawSphere(transform.position + transform.forward * 0.05f, 0.005f);

        //Gizmos.DrawRay(transform.position, transform.forward * 0.05f + transform.up * 0.05f);
    }
}
