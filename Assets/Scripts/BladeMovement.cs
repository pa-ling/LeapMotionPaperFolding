using System.Collections.Generic;
using UnityEngine;
using Leap.Unity;
using Leap.Unity.Interaction;

public class BladeMovement : MonoBehaviour {

    private enum HandObject { Laser, Marker};
    private enum HandVector { PaperHit };

    private const int PAPER_LAYER_MASK = ~(1 << 2);

    public GameObject laserPrefab;
    public GameObject markerPrefab;

    public HandModel leftHandModel;
    public InteractionHand leftInteractionHand;
    public HandModel rightHandModel;
    public InteractionHand rightInteractionHand;

    private List<GameObject>[] handObjects;
    private List<Vector3>[] handVectors;
    private GameObject verticalLaser; // the laser that is vertical to connectionLaser

    private void Start()
    {
        handObjects = new List<GameObject>[2]
        {
            new List<GameObject> { Instantiate(laserPrefab), Instantiate(markerPrefab) , null},
            new List<GameObject> { Instantiate(laserPrefab), Instantiate(markerPrefab) , null},
        };
        handObjects[0][0].name = "Left Hand Laser";
        handObjects[0][1].name = "Left Hand Marker";
        handObjects[1][0].name = "Right Hand Laser";
        handObjects[1][1].name = "Right Hand Marker";

        handVectors = new List<Vector3>[2]
        {
            new List<Vector3>{ Vector3.negativeInfinity },
            new List<Vector3>{ Vector3.negativeInfinity }
        };

        verticalLaser = Instantiate(laserPrefab);
        verticalLaser.name = "Vertical Laser";

        leftInteractionHand.OnStayPrimaryHoveringObject += OnLeftPrimaryHover;
        leftInteractionHand.OnEndPrimaryHoveringObject += OnLeftEndPrimaryHover;
        leftInteractionHand.OnGraspBegin += OnLeftGraspBegin;

        rightInteractionHand.OnStayPrimaryHoveringObject += OnRightPrimaryHover;
        rightInteractionHand.OnEndPrimaryHoveringObject += OnRightEndPrimaryHover;
        rightInteractionHand.OnGraspBegin += OnRightGraspBegin;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Cut();
        }

        Vector3 leftHit = handVectors[(int)Chirality.Left][(int)HandVector.PaperHit];
        Vector3 rightHit = handVectors[(int)Chirality.Right][(int)HandVector.PaperHit];

        if (Vector3.negativeInfinity.Equals(leftHit) || Vector3.negativeInfinity.Equals(rightHit))
        {
            verticalLaser.SetActive(false);
            return;
        }

        verticalLaser.SetActive(true);
        verticalLaser.transform.position = Vector3.Lerp(leftHit, rightHit, .5f);
        verticalLaser.transform.LookAt(rightHit);
        verticalLaser.transform.localScale = new Vector3(verticalLaser.transform.localScale.x, verticalLaser.transform.localScale.y, Vector3.Distance(leftHit, rightHit));
        verticalLaser.transform.Rotate(new Vector3(0, 90, 0));

        this.transform.position = verticalLaser.transform.position + verticalLaser.transform.up;
        this.transform.rotation = verticalLaser.transform.rotation;
        this.transform.Rotate(new Vector3(90, 0, 0));
    }

    private void OnPrimaryHover(HandModel hand, InteractionHand interHand, InteractionBehaviour obj)
    {
        int handedness = (int) hand.Handedness;
        Vector3 junction = Vector3.negativeInfinity;

        GameObject marker = handObjects[handedness][(int)HandObject.Marker];

        RaycastHit hit;
        Vector3 thumbTipPos = hand.fingers[0].GetTipPosition();
        Vector3 indexTipPos = hand.fingers[1].GetTipPosition();

        if (interHand.isGraspingObject)
        {
            Vector3 graspPoint = interHand.GetGraspPoint();
            graspPoint.y = obj.transform.position.y;
            marker.transform.position = graspPoint;
            marker.transform.rotation = obj.transform.rotation;
            junction = graspPoint;
            marker.SetActive(true);
        } 
        else if (Physics.Raycast(indexTipPos, thumbTipPos - indexTipPos, out hit, Vector3.Distance(indexTipPos, thumbTipPos), PAPER_LAYER_MASK))
        {
            marker.transform.position = hit.point;
            marker.transform.rotation = obj.transform.rotation;
            junction = hit.point;
            marker.SetActive(true);
        }
        else
        {
            marker.SetActive(false);
        }
        
        handVectors[handedness][(int)HandVector.PaperHit] = junction;
    }

    private void OnEndPrimaryHover(HandModel hand, InteractionBehaviour obj)
    {
        handVectors[(int)hand.Handedness][(int)HandVector.PaperHit] = Vector3.negativeInfinity;
    }

    private void OnGraspBegin(HandModel hand)
    {
        // when both hands grasp, the victim is cut
        if (leftInteractionHand.isGraspingObject && rightInteractionHand.isGraspingObject) //TODO: Make sure both hands are grasping the same object?
        {
            Cut();
        }
    }

    private void Cut()
    {
        Debug.Log("Cut");
        RaycastHit[] hits = Physics.RaycastAll(transform.position, transform.forward);
        foreach (RaycastHit hit in hits)
        {
            GameObject victim = hit.collider.gameObject;
            GameObject[] pieces = MeshCut.Cut(victim, transform.position, transform.right, victim.GetComponent<MeshRenderer>().material);
            pieces[0].transform.position += .0004f * transform.right;
            pieces[1].transform.position -= .0004f * transform.right;

            /*foreach (GameObject piece in pieces)
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
            }*/

            GameObject cutMarker = Instantiate(markerPrefab, hit.point, Quaternion.identity);
            cutMarker.SetActive(true);
            cutMarker.name = "Cut";
        }
    }

    #region leftEventHandlers

    private void OnLeftPrimaryHover(InteractionBehaviour obj)
    {
        OnPrimaryHover(leftHandModel, leftInteractionHand, obj);
    }

    private void OnLeftEndPrimaryHover(InteractionBehaviour obj)
    {
        OnEndPrimaryHover(leftHandModel, obj);
    }

    private void OnLeftGraspBegin()
    {
        Debug.Log("Left Grasp");
        OnGraspBegin(leftHandModel);
    }

    #endregion leftEventHandlers

    #region rightEventHandlers

    private void OnRightPrimaryHover(InteractionBehaviour obj)
    {
        OnPrimaryHover(rightHandModel, rightInteractionHand, obj);
    }

    private void OnRightEndPrimaryHover(InteractionBehaviour obj)
    {
        OnEndPrimaryHover(rightHandModel, obj);
    }

    private void OnRightGraspBegin()
    {
        Debug.Log("Right Grasp");
        OnGraspBegin(rightHandModel);
    }

    #endregion rightEventHandlers

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
