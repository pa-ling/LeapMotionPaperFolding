
using UnityEngine;
using System.Collections.Generic;
using Leap;
using Leap.Unity;

public class VisualizeIndexThumbConnection : MonoBehaviour
{
    public GameObject laserPrefab; // The laser prefab

    private GameObject laser; // A reference to the spawned laser
    private HandModel handModel;

    void Start()
    {
        laser = Instantiate(laserPrefab);
        laser.SetActive(true);

        handModel = GetComponent<HandModel>();
    }

    void Update()
    {
        FingerModel index = handModel.fingers[1];
        FingerModel thumb = handModel.fingers[0];
        ShowLaser(index.GetTipPosition(), thumb.GetTipPosition());
    }

    private void OnDisable()
    {
        if (laser)
        {
            laser.SetActive(false);
        }
    }

    private void OnEnable()
    {
        if (laser)
        {
            laser.SetActive(true);
        }
    }

    private void ShowLaser(Vector3 origin, Vector3 destination)
    {
        laser.transform.position = Vector3.Lerp(origin, destination, .5f); // Move laser to the middle between the controller and the position the raycast hit
        laser.transform.LookAt(destination); // Rotate laser facing the hit point
        laser.transform.localScale = new Vector3(laser.transform.localScale.x, laser.transform.localScale.y, Vector3.Distance(origin, destination)); // Scale laser so it fits exactly between the controller & the hit point
    }

}
