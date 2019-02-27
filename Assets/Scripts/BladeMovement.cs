using UnityEngine;
using Leap.Unity;

public class BladeMovement : MonoBehaviour {

    public HandModel leftHandModel;
    public HandModel rightHandModel;
    public GameObject laserPrefab; // The laser prefab

    private GameObject leftLaser;
    private GameObject rightLaser;

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
    }

    private void Update()
    {       
        FingerModel leftIndex = leftHandModel.fingers[1];
        FingerModel leftThumb = leftHandModel.fingers[0];
        ShowLaser(leftLaser, leftIndex.GetTipPosition(), leftThumb.GetTipPosition());

        FingerModel rightIndex = rightHandModel.fingers[1];
        FingerModel rightThumb = rightHandModel.fingers[0];
        ShowLaser(rightLaser, rightIndex.GetTipPosition(), rightThumb.GetTipPosition());
    }

    private void ShowLaser(GameObject laser, Vector3 origin, Vector3 destination)
    {
        laser.transform.position = Vector3.Lerp(origin, destination, .5f); // Move laser to the middle between the controller and the position the raycast hit
        laser.transform.LookAt(destination); // Rotate laser facing the hit point
        laser.transform.localScale = new Vector3(laser.transform.localScale.x, laser.transform.localScale.y, Vector3.Distance(origin, destination)); // Scale laser so it fits exactly between the controller & the hit point
    }
}
