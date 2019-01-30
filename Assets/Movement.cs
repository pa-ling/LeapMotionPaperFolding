using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour {

    public float speed = 100;

    private const float MAX_VERTICAL_ROTATION = 90;
    private float verticalRotation = -10;

	// Use this for initialization
	void Start () {
        transform.RotateAround(Vector3.zero, Vector3.left, -10);
    }
	
	// Update is called once per frame
	void Update () {
        float horizontal = -(Input.GetAxis("Horizontal") * speed * Time.deltaTime);
        float vertical = -(Input.GetAxis("Vertical") * speed * Time.deltaTime);

        transform.RotateAround(Vector3.zero, Vector3.up, horizontal);

        if (Input.GetButton("Jump"))
        {
            //TODO: Fix
            transform.position += transform.forward * vertical;
            return;
        }

        if (MAX_VERTICAL_ROTATION < Mathf.Abs(verticalRotation + vertical))
        {
            return;
        }

        transform.RotateAround(Vector3.zero, -transform.right, vertical);
        verticalRotation += vertical;
    }
}
