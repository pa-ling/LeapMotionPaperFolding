using System.Collections.Generic;
using UnityEngine;
using Leap.Unity;
using Leap.Unity.Interaction;

public class BladeMovement : MonoBehaviour {

    private const int PAPER_LAYER_MASK = ~(1 << 2);

    public GameObject laserPrefab;
    public GameObject markerPrefab;

    public HandModel leftHandModel;
    public InteractionHand leftInteractionHand;
    public HandModel rightHandModel;
    public InteractionHand rightInteractionHand;

    private GameObject verticalLaser;

    private void Start()
    { 
        verticalLaser = Instantiate(laserPrefab);
        verticalLaser.name = "Vertical Laser";

        leftInteractionHand.OnGraspBegin += OnGraspBegin;
        rightInteractionHand.OnGraspBegin += OnGraspBegin;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Cut();
        }

        if (!(leftHandModel.gameObject.activeSelf && rightHandModel.gameObject.activeSelf))
        {
            return;
        }
   
        Vector3 leftThumbTipPos = leftHandModel.fingers[0].GetTipPosition();
        Vector3 leftIndexTipPos = leftHandModel.fingers[1].GetTipPosition();
        Vector3 rightThumbTipPos = rightHandModel.fingers[0].GetTipPosition();
        Vector3 rightIndexTipPos = rightHandModel.fingers[1].GetTipPosition();
        Vector3 midThumbTipPos = Vector3.Lerp(leftThumbTipPos, rightThumbTipPos, .5f);
        Vector3 midIndexTipPos = Vector3.Lerp(leftIndexTipPos, rightIndexTipPos, .5f);

        Vector3 frontDirection = RotateAroundAxis(Vector3.Normalize(leftThumbTipPos - rightThumbTipPos), 90, new Vector3(0, 1, 0));
        Vector3 frontThumbMidPos = midThumbTipPos + frontDirection * 0.001f;
        Vector3 frontIndexMidPos = midIndexTipPos + frontDirection * 0.001f;

        #region debug
        DebugRay(leftThumbTipPos, rightThumbTipPos, Color.green);
        DebugRay(leftIndexTipPos, rightIndexTipPos, Color.green);
        DebugRay(midThumbTipPos, midIndexTipPos, Color.yellow);
        DebugRay(frontThumbMidPos, frontIndexMidPos, Color.red);
        #endregion debug

        RaycastHit midHit, frontHit;
        bool hitMid = Physics.Raycast(midIndexTipPos, midThumbTipPos - midIndexTipPos, out midHit, Vector3.Distance(midIndexTipPos, midThumbTipPos), PAPER_LAYER_MASK);
        bool hitFront = Physics.Raycast(frontIndexMidPos, frontThumbMidPos - frontIndexMidPos, out frontHit, Vector3.Distance(frontIndexMidPos, frontThumbMidPos), PAPER_LAYER_MASK);

        if (hitMid && hitFront)
        {
            verticalLaser.transform.position = midHit.point;
            verticalLaser.transform.LookAt(frontHit.point);
            verticalLaser.transform.rotation *= midHit.collider.gameObject.transform.rotation;
            verticalLaser.transform.localScale = new Vector3(verticalLaser.transform.localScale.x, verticalLaser.transform.localScale.y, Vector3.Distance(leftIndexTipPos, rightIndexTipPos));
            verticalLaser.SetActive(true);

            this.transform.position = verticalLaser.transform.position + verticalLaser.transform.up;
            this.transform.rotation = verticalLaser.transform.rotation;
            this.transform.Rotate(new Vector3(90, 0, 0));
        } else
        {
            verticalLaser.SetActive(false);
        }

    }

    public static Vector3 RotateAroundAxis(Vector3 v, float a, Vector3 axis, bool bUseRadians = false)
    {
        if (bUseRadians) a *= Mathf.Rad2Deg;
        var q = Quaternion.AngleAxis(a, axis);
        return q * v;
    }

    private void DebugRay(Vector3 origin, Vector3 destination, Color color)
    {
        Debug.DrawRay(origin, Vector3.Normalize(destination - origin) * Vector3.Distance(origin, destination), color, 0, true);
    }

    private void OnGraspBegin()
    {
        // when both hands grasp, the victim is cut
        if (leftInteractionHand.isGraspingObject && rightInteractionHand.isGraspingObject) //TODO: Make sure both hands are grasping the same object?
        {
            Cut();
        }
    }

    private void Cut()
    {
        RaycastHit[] hits = Physics.RaycastAll(transform.position, transform.forward);
        foreach (RaycastHit hit in hits)
        {
            GameObject victim = hit.collider.gameObject;
            GameObject[] pieces = MeshCut.Cut(victim, transform.position, transform.right, victim.GetComponent<MeshRenderer>().material);
            pieces[0].transform.position += 0.0004f * transform.right;
            pieces[1].transform.position -= 0.0004f * transform.right;

            foreach (GameObject piece in pieces)
            {
                piece.AddComponent<MeshCollider>();
                piece.GetComponent<MeshCollider>().sharedMesh = piece.GetComponent<MeshFilter>().mesh;
                piece.GetComponent<MeshCollider>().convex = true;
                piece.AddComponent<Rigidbody>();
                piece.AddComponent<InteractionBehaviour>();
                InteractionBehaviour ib = piece.GetComponent<InteractionBehaviour>();
                ib.ignoreContact = true;
                ib.moveObjectWhenGrasped = false;
                ib.allowMultiGrasp = true;
                piece.AddComponent<InteractionGlow>();
            }

            /*GameObject cutMarker = Instantiate(markerPrefab, hit.point, Quaternion.identity);
            cutMarker.SetActive(true);
            cutMarker.name = "Cut";*/
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        Vector3 gizmoStart = transform.position;
        Vector3 arrowEnd = transform.position + 0.25f * transform.forward;
        Vector3 arrowRight = arrowEnd - 0.1f * transform.forward + 0.1f * transform.up;
        Vector3 arrowLeft = arrowEnd - 0.1f * transform.forward - 0.1f * transform.up;

        Gizmos.DrawLine(gizmoStart, arrowEnd);
        Gizmos.DrawLine(arrowEnd, arrowLeft);
        Gizmos.DrawLine(arrowEnd, arrowRight);
    }
}
