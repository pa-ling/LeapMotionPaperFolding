﻿using System.Collections;
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
    private List<Transform> rotatingObjects;
    private Vector3[] lastGraspPoints;

    private void Start()
    { 
        verticalLaser = Instantiate(laserPrefab);
        verticalLaser.name = "Vertical Laser";

        rotatingObjects = new List<Transform>();
        lastGraspPoints = new Vector3[2];
        lastGraspPoints[0] = Vector3.negativeInfinity;
        lastGraspPoints[1] = Vector3.negativeInfinity;

        leftInteractionHand.OnGraspBegin += OnGraspBegin;
        rightInteractionHand.OnGraspBegin += OnGraspBegin;

        leftInteractionHand.OnStayPrimaryHoveringObject += OnLeftPrimaryHover;
        rightInteractionHand.OnStayPrimaryHoveringObject += OnRightPrimaryHover;

        StartCoroutine("MakeCuts");
    }

    private IEnumerator MakeCuts()
    {
        for (int i = 0; i < 1; i++)
        {
            Cut();
            this.transform.Rotate(new Vector3(0, 0, 1), 45);
            this.transform.position += Vector3.right * 0.1f;
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void FixedUpdate()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Cut();
        }

        if (!(leftHandModel.gameObject.activeSelf && rightHandModel.gameObject.activeSelf))
        {
            return;
        }

        HandleMovement();
    }

    private void HandleMovement()
    {
        Vector3 leftThumbTipPos = leftHandModel.fingers[0].GetTipPosition();
        Vector3 leftIndexTipPos = leftHandModel.fingers[1].GetTipPosition();
        Vector3 rightThumbTipPos = rightHandModel.fingers[0].GetTipPosition();
        Vector3 rightIndexTipPos = rightHandModel.fingers[1].GetTipPosition();
        Vector3 midThumbTipPos = Vector3.Lerp(leftThumbTipPos, rightThumbTipPos, .5f);
        Vector3 midIndexTipPos = Vector3.Lerp(leftIndexTipPos, rightIndexTipPos, .5f);

        Vector3 frontDirection = Util.RotateAroundAxis(Vector3.Normalize(leftThumbTipPos - rightThumbTipPos), 90, new Vector3(0, 1, 0));
        Vector3 frontThumbMidPos = midThumbTipPos + frontDirection * 0.001f;
        Vector3 frontIndexMidPos = midIndexTipPos + frontDirection * 0.001f;

        #region debug
        Util.DebugRay(leftThumbTipPos, rightThumbTipPos, Color.green);
        Util.DebugRay(leftIndexTipPos, rightIndexTipPos, Color.green);
        Util.DebugRay(midThumbTipPos, midIndexTipPos, Color.yellow);
        Util.DebugRay(frontThumbMidPos, frontIndexMidPos, Color.red);
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
        }
        else
        {
            verticalLaser.SetActive(false);
        }
    }

    // Cutting

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
        Debug.Log("Cut");
        RaycastHit[] hits = Physics.BoxCastAll(
            GetComponent<Collider>().bounds.center,
            this.transform.localScale, this.transform.forward,
            this.transform.rotation,
            300,
            PAPER_LAYER_MASK
        );
        foreach (RaycastHit hit in hits)
        {
            GameObject victim = hit.collider.gameObject;
            RemoveRotatingObject(victim.transform);
            List<Transform> children = new List<Transform>();
            foreach (Transform child in victim.transform)
            {
                children.Add(child);
            }
            victim.transform.DetachChildren();

            GameObject[] pieces = MeshCut.Cut(victim, this.transform.position, this.transform.right, victim.GetComponent<MeshRenderer>().material);
            pieces[0].transform.position += 0.00015f * transform.right;
            pieces[1].transform.position -= 0.00015f * transform.right;

            foreach (GameObject piece in pieces)
            {
                AddNecessaryComponents(piece);

                // Create rotator marker for each piece
                GameObject cutMarker = Instantiate(markerPrefab, hit.point, Quaternion.identity);
                cutMarker.SetActive(true);
                cutMarker.name = piece.GetInstanceID().ToString() + "/" + cutMarker.GetInstanceID().ToString();
                cutMarker.tag = "Cut";
                cutMarker.transform.forward = this.transform.right;
                cutMarker.transform.SetParent(piece.transform);

                // Duplicate each rotator of parent to the children
                foreach (Transform child in children)
                {
                    GameObject dup = Instantiate(markerPrefab, child.transform.position, child.transform.rotation);
                    dup.SetActive(true);
                    dup.name = piece.GetInstanceID().ToString() + "/" + dup.GetInstanceID().ToString();
                    dup.tag = "Cut";
                    dup.transform.SetParent(piece.transform);
                }
            }

            foreach (Transform child in children)
            {
                Destroy(child.gameObject);
            }
        }
    }

    private void AddNecessaryComponents(GameObject piece)
    {
        piece.name = piece.GetInstanceID().ToString();
        piece.tag = "Paper";
        piece.AddComponent<MeshCollider>();
        piece.GetComponent<MeshCollider>().sharedMesh = piece.GetComponent<MeshFilter>().mesh;
        piece.GetComponent<MeshCollider>().convex = true;
        piece.AddComponent<Rigidbody>();
        Rigidbody rb = piece.GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ |
            RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ;
        piece.AddComponent<InteractionBehaviour>();
        InteractionBehaviour ib = piece.GetComponent<InteractionBehaviour>();
        ib.ignoreContact = true;
        ib.moveObjectWhenGrasped = false;
        ib.allowMultiGrasp = true;
        piece.AddComponent<InteractionGlow>();
    }

    // Rotating

    private void OnLeftPrimaryHover(InteractionBehaviour obj)
    {
        OnPrimaryHover(leftInteractionHand, obj);
    }

    private void OnRightPrimaryHover(InteractionBehaviour obj)
    {
        OnPrimaryHover(rightInteractionHand, obj);
    }

    private void OnPrimaryHover(InteractionHand hand, InteractionBehaviour obj)
    {
        if (obj.Equals(hand.graspedObject) && obj.transform.childCount != 0 && !rotatingObjects.Contains(obj.transform))
        {
            AddRotatingObject(obj.transform);
        }

        HandleRotatingObjects(hand);
    }

    private void HandleRotatingObjects(InteractionHand hand)
    {
        for (int i = 0; i < rotatingObjects.Count; i++)
        {
            Transform rotObj = rotatingObjects[i];
            InteractionBehaviour rotObjBehaviour = rotObj.GetComponent<InteractionBehaviour>();
            Transform rotator = rotObj.parent;
            if (rotObjBehaviour.isGrasped)
            {
                if (rotObjBehaviour.graspingHands.Contains(hand))
                {
                    Vector3 graspPoint = rotObjBehaviour.GetGraspPoint(hand);
                    Vector3 lastGraspPoint = lastGraspPoints[Util.BoolToInt(hand.leapHand.IsLeft)];

                    if (!Vector3.negativeInfinity.Equals(lastGraspPoint))
                    {
                        Util.DebugPoint(graspPoint, Color.blue);

                        Vector3 nearestRotatePos = Util.NearestPointOnLine(rotator.position, rotator.right, graspPoint);
                        Util.DebugPoint(nearestRotatePos, Color.red);

                        Debug.DrawRay(graspPoint, (nearestRotatePos - graspPoint) * 10, Color.green);
                        Debug.DrawRay(lastGraspPoint, (nearestRotatePos - lastGraspPoint) * 10, Color.green);

                        Vector3 normal = Util.RotateAroundAxis(Vector3.Normalize(nearestRotatePos - graspPoint), 90, rotObj.up);

                        Debug.DrawRay(lastGraspPoint, normal, Color.magenta);

                        float rotateAngle = Vector3.SignedAngle(nearestRotatePos - lastGraspPoint, nearestRotatePos - graspPoint, normal);
                        rotator.Rotate(rotator.right, rotateAngle, Space.World);
                    }

                    lastGraspPoints[Util.BoolToInt(hand.leapHand.IsLeft)] = graspPoint;
                }
            }
            else
            {
                RemoveRotatingObject(rotObj);
                i--;
            }
        }
    }

    private void AddRotatingObject(Transform obj)
    {
        Transform rotator = obj.GetChild(0);
        rotator.SetParent(null);
        obj.SetParent(rotator);
        rotatingObjects.Add(obj);
    }

    private void RemoveRotatingObject(Transform obj)
    {
        if (!rotatingObjects.Contains(obj))
        {
            return;
        }

        Transform rotator = obj.parent;
        rotatingObjects.Remove(obj);
        obj.SetParent(GameObject.Find("Paper").transform);
        rotator.SetParent(obj);

        foreach(InteractionHand hand in obj.GetComponent<InteractionBehaviour>().graspingHands)
        {
            lastGraspPoints[Util.BoolToInt(hand.leapHand.IsLeft)] = Vector3.negativeInfinity;
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
