using UnityEngine;
using Leap.Unity;
using System.Collections.Generic;
using System.Collections;

public class BladeMovement : MonoBehaviour {

    private enum Hand { LEFT, RIGHT };
    private enum HandObject { LASER, MARKER };

    public GameObject laserPrefab;
    public GameObject markerPrefab;
    public HandModel leftHandModel;
    public PinchDetector leftDetector;
    public HandModel rightHandModel;
    public PinchDetector rightDetector;

    private List<GameObject>[] handObjects;
    private GameObject connectionLaser;
    private GameObject verticalLaser; // the laser that is vertical to connectionLaser

    private const int PAPER_LAYER_MASK = ~(1 << 2);

    private void Start()
    {
        List<GameObject> leftObjects;
        leftObjects = new List<GameObject>();
        leftObjects.Add(Instantiate(laserPrefab));
        leftObjects.Add(Instantiate(markerPrefab));

        List<GameObject> rightObjects;
        rightObjects = new List<GameObject>();
        rightObjects.Add(Instantiate(laserPrefab));
        rightObjects.Add(Instantiate(markerPrefab));

        handObjects = new List<GameObject>[2];
        handObjects[0] = leftObjects;
        handObjects[1] = rightObjects;

        connectionLaser = Instantiate(laserPrefab);
        connectionLaser.SetActive(false);
        verticalLaser = Instantiate(laserPrefab);
        verticalLaser.SetActive(false);
    }

    private void Update()
    {
        Vector3 leftHit = GetAndVisualizePaperHit(leftHandModel.fingers[1].GetTipPosition(), leftHandModel.fingers[0].GetTipPosition(), Hand.LEFT, leftDetector);
        Vector3 rightHit = GetAndVisualizePaperHit(rightHandModel.fingers[1].GetTipPosition(), rightHandModel.fingers[0].GetTipPosition(), Hand.RIGHT, rightDetector);

        if (Vector3.negativeInfinity.Equals(leftHit) || Vector3.negativeInfinity.Equals(rightHit))
        {
            connectionLaser.SetActive(false);
            verticalLaser.SetActive(false);
            return;
        }

        connectionLaser.SetActive(true);
        ShowLaser(connectionLaser, leftHit, rightHit);
        verticalLaser.SetActive(true);
        ShowLaser(verticalLaser, leftHit, rightHit);
        verticalLaser.transform.Rotate(new Vector3(0, 90, 0));

        transform.position = verticalLaser.transform.position + new Vector3(0, 1, 0);
        transform.rotation = verticalLaser.transform.rotation;
        transform.Rotate(new Vector3(90, 0, 0));

        // when both hands pinch, the victim is cut
        if ((leftDetector.DidStartPinch && rightDetector.IsPinching) || (leftDetector.IsPinching && rightDetector.DidStartPinch))
        {
            StartCoroutine("Blink", verticalLaser.GetComponent<MeshRenderer>());
            RaycastHit[] hits = Physics.RaycastAll(transform.position, transform.forward);
            foreach (RaycastHit hit in hits)
            {
                GameObject victim = hit.collider.gameObject;
                GameObject[] pieces = MeshCut.Cut(victim, transform.position, transform.right, victim.GetComponent<MeshRenderer>().material);
                pieces[0].transform.position += .0002f * transform.right;
                pieces[1].transform.position -= .0002f * transform.right;
            }
        }
    }

    private Vector3 GetAndVisualizePaperHit(Vector3 indexTipPos, Vector3 thumbTipPos, Hand hand, PinchDetector pinchDetector)
    {
        RaycastHit hit;
        float distance = Vector3.Distance(indexTipPos, thumbTipPos);

        if (Physics.Raycast(indexTipPos, thumbTipPos - indexTipPos, out hit, distance, PAPER_LAYER_MASK))
        {
            GameObject laser = handObjects[(int)hand][(int)HandObject.LASER];
            GameObject marker = handObjects[(int)hand][(int)HandObject.MARKER];

            if (pinchDetector.IsPinching)
            {
                laser.GetComponent<MeshRenderer>().material.color = Color.green;
                marker.GetComponent<MeshRenderer>().material.color = Color.green;
            } else
            {
                laser.GetComponent<MeshRenderer>().material.color = Color.blue;
                marker.GetComponent<MeshRenderer>().material.color = Color.blue;
            }
            laser.SetActive(true);
            marker.SetActive(true);
            marker.transform.position = hit.point;
            ShowLaser(laser, indexTipPos, thumbTipPos);
        }
        else
        {
            handObjects[(int)hand][(int)HandObject.MARKER].SetActive(false);
            handObjects[(int)hand][(int)HandObject.LASER].SetActive(false);
            return Vector3.negativeInfinity;
        }

        return hit.point;
    }

    private void ShowLaser(GameObject laser, Vector3 origin, Vector3 destination)
    {
        laser.transform.position = Vector3.Lerp(origin, destination, .5f); // Move laser to the middle between the controller and the position the raycast hit
        laser.transform.LookAt(destination); // Rotate laser facing the hit point
        laser.transform.localScale = new Vector3(laser.transform.localScale.x, laser.transform.localScale.y, Vector3.Distance(origin, destination)); // Scale laser so it fits exactly between the controller & the hit point
    }

    IEnumerator Blink(MeshRenderer renderer)
    {
        Color defaultColor = renderer.material.color;
        renderer.material.color = Color.green;
        yield return new WaitForSeconds(1);
        renderer.material.color = defaultColor;
    }

    void OnDrawGizmosSelected()
    {
        Vector3 gizmoStart = transform.position;
        Vector3 arrowEnd = transform.position + 0.5f * transform.forward;
        Vector3 arrowRight = arrowEnd - 0.25f * transform.forward + 0.25f * transform.up;
        Vector3 arrowLeft = arrowEnd - 0.25f * transform.forward - 0.25f * transform.up;

        Gizmos.DrawLine(gizmoStart, arrowEnd);
        Gizmos.DrawLine(arrowEnd, arrowLeft);
        Gizmos.DrawLine(arrowEnd, arrowRight);
    }
}
