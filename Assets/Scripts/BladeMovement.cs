using UnityEngine;
using Leap.Unity;
using System.Collections.Generic;

public class BladeMovement : MonoBehaviour {

    private enum HandObject { LASER, MARKER };

    public GameObject laserPrefab;
    public GameObject markerPrefab;
    public HandModel leftHandModel;
    public HandModel rightHandModel;

    private List<GameObject>[] handObjects;
    private GameObject connectionLaser;

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
    }

    private void Update()
    {
        Vector3 leftHit = GetAndVisualizePaperHit(leftHandModel.fingers[1].GetTipPosition(), leftHandModel.fingers[0].GetTipPosition(), 0);
        Vector3 rightHit = GetAndVisualizePaperHit(rightHandModel.fingers[1].GetTipPosition(), rightHandModel.fingers[0].GetTipPosition(), 1);

        //TODO: Make connection Laser
        //TODO: Move blade accordingly
    }

    private Vector3 GetAndVisualizePaperHit(Vector3 indexTipPos, Vector3 thumbTipPos, int hand)
    {
        RaycastHit hit;
        float distance = Vector3.Distance(indexTipPos, thumbTipPos);

        if (Physics.Raycast(indexTipPos, thumbTipPos - indexTipPos, out hit, distance, PAPER_LAYER_MASK))
        {
            handObjects[hand][(int)HandObject.LASER].SetActive(true);
            handObjects[hand][(int)HandObject.MARKER].SetActive(true);
            handObjects[hand][(int)HandObject.MARKER].transform.position = hit.point;
            ShowLaser(handObjects[hand][(int)HandObject.LASER], indexTipPos, thumbTipPos);
        }
        else
        {
            handObjects[hand][(int)HandObject.MARKER].SetActive(false);
            handObjects[hand][(int)HandObject.LASER].SetActive(false);
        }

        return hit.point;
    }

    private void ShowLaser(GameObject laser, Vector3 origin, Vector3 destination)
    {
        laser.transform.position = Vector3.Lerp(origin, destination, .5f); // Move laser to the middle between the controller and the position the raycast hit
        laser.transform.LookAt(destination); // Rotate laser facing the hit point
        laser.transform.localScale = new Vector3(laser.transform.localScale.x, laser.transform.localScale.y, Vector3.Distance(origin, destination)); // Scale laser so it fits exactly between the controller & the hit point
    }
}
