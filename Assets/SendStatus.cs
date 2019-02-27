using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SendStatus : MonoBehaviour {

    public BladeMovement bladeMovement;
    public int handId;

    private void OnEnable()
    {
        bladeMovement.HandEnabled(handId);
    }

    private void OnDisable()
    {
        bladeMovement.HandDisabled(handId);
    }
}
