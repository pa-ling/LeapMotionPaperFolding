using UnityEngine;

public class ExampleUseof_MeshCut : MonoBehaviour
{

    void Cut()
    {
        RaycastHit[] hits = Physics.RaycastAll(transform.position, transform.forward);
        foreach (RaycastHit hit in hits)
        {
            GameObject victim = hit.collider.gameObject;
            GameObject[] pieces = MeshCut.Cut(victim, transform.position, transform.right, victim.GetComponent<MeshRenderer>().material);
        }
    }

    public static void CutForward(Transform transform)
    {
        RaycastHit[] hits = Physics.RaycastAll(transform.position, transform.forward);
        foreach (RaycastHit hit in hits)
        {
            GameObject victim = hit.collider.gameObject;
            GameObject[] pieces = MeshCut.Cut(victim, transform.position, transform.right, victim.GetComponent<MeshRenderer>().material);
        }
    }
}
