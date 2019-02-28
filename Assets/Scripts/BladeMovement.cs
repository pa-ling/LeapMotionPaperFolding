using UnityEngine;
using Leap.Unity;

public class BladeMovement : MonoBehaviour {

    public HandModel leftHandModel;
    public HandModel rightHandModel;
    public GameObject laserPrefab; // The laser prefab
    public GameObject sphere;

    private GameObject leftLaser;
    private GameObject rightLaser;
    private GameObject connectionLaser;
    private GameObject sphere1;
    private GameObject sphere2;
    private GameObject sphere3;

    public void HandEnabled(int hand)
    {
        if (hand == 0)
        {
            leftLaser.SetActive(true);
        }
        else
        {
            rightLaser.SetActive(true);
        }
    }

    public void HandDisabled(int hand)
    {
        if (hand == 0)
        {
            leftLaser.SetActive(false);
        }
        else
        {
            rightLaser.SetActive(false);
        }
    }

    private void Start()
    {
        leftLaser = Instantiate(laserPrefab);
        leftLaser.SetActive(true);

        rightLaser = Instantiate(laserPrefab);
        rightLaser.SetActive(true);

        sphere1 = Instantiate(sphere);
        sphere2 = Instantiate(sphere);
        sphere3 = Instantiate(sphere);
    }

    private void Update()
    {       
        FingerModel leftIndex = leftHandModel.fingers[1];
        FingerModel leftThumb = leftHandModel.fingers[0];

        sphere1.transform.position = leftIndex.GetTipPosition();
        sphere2.transform.position = leftThumb.GetTipPosition();

        RaycastHit hit;
        int layerMask = 1 << 2;
        layerMask = ~layerMask;
        if (Physics.Raycast(leftIndex.GetTipPosition(), leftThumb.GetTipPosition() - leftIndex.GetTipPosition(), out hit, Mathf.Infinity, layerMask))
        {
            sphere3.transform.position = hit.point;
            ShowLaser(leftLaser, leftIndex.GetTipPosition(), hit.point);
            Debug.Log(hit.collider.gameObject.name);
        }
    }

    private void ShowLaser(GameObject laser, Vector3 origin, Vector3 destination)
    {
        laser.transform.position = Vector3.Lerp(origin, destination, .5f); // Move laser to the middle between the controller and the position the raycast hit
        laser.transform.LookAt(destination); // Rotate laser facing the hit point
        laser.transform.localScale = new Vector3(laser.transform.localScale.x, laser.transform.localScale.y, Vector3.Distance(origin, destination)); // Scale laser so it fits exactly between the controller & the hit point
    }
}
