using UnityEngine;

public class ExampleUseof_MeshCut : MonoBehaviour
{

    public Material capMaterial;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit[] hits = Physics.RaycastAll(transform.position, transform.forward);
            foreach (RaycastHit hit in hits)
            {
                GameObject victim = hit.collider.gameObject;
                GameObject[] pieces = MeshCut.Cut(victim, transform.position, transform.right, capMaterial);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;

        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 5.0f);
        Gizmos.DrawLine(transform.position + transform.up * 0.5f, transform.position + transform.up * 0.5f + transform.forward * 5.0f);
        Gizmos.DrawLine(transform.position + -transform.up * 0.5f, transform.position + -transform.up * 0.5f + transform.forward * 5.0f);

        Gizmos.DrawLine(transform.position, transform.position + transform.up * 0.5f);
        Gizmos.DrawLine(transform.position, transform.position + -transform.up * 0.5f);
    }

}
