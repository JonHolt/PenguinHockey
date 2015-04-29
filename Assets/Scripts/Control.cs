using UnityEngine;
using System.Collections;

public class Control : MonoBehaviour {
    public float speed = 5;
    public float maxRotation = 100;
    public float rotAccl = 30f;
    public string PlayerNum;
    public int points = 0;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        Rigidbody body = GetComponent<Rigidbody>();
        float horiz = Input.GetAxis("Horizontal_" + PlayerNum);
        float vert = Input.GetAxis("Vertical_" + PlayerNum);

        body.AddForce(horiz * speed, 0.0f, vert * speed);

        float adj = Input.GetAxis("RightH_" + PlayerNum);

        if (Mathf.Abs(adj) > .4)
        {
            if (Mathf.Abs(body.angularVelocity.y) < maxRotation)
                body.AddTorque(0.0f, adj * rotAccl, 0.0f);
        }
        else
            body.angularVelocity = new Vector3(0, 0, 0);

        //commented out because it feels wrong
        //float opp = Input.GetAxis("RightV_" + PlayerNum);
        //if (Mathf.Abs(adj) > 0.5f || Mathf.Abs(opp) > 0.5f)
        //{
        //    //transform.LookAt(new Vector3(transform.position.x + opp, transform.position.y, transform.position.z + adj));
        //    float tan = Mathf.Atan2(opp, adj);
        //    float goalAngle = tan * 180 / Mathf.PI;
        //    if (goalAngle < 0)
        //        goalAngle += 360f;

        //    float currentAngle = transform.eulerAngles.y;
        //    float diff = goalAngle - currentAngle;
        //    diff = mod(diff + 180, 360) - 180;

        //    if ((diff < -5) && body.angularVelocity.y > -maxRotation)
        //    {
        //        body.AddTorque(0f, -0.2f, 0f);
        //    }
        //    else if ((diff > 5) && body.angularVelocity.y < maxRotation)
        //    {
        //        body.AddTorque(0f, 0.2f, 0f);
        //    }
        //    else
        //    {
        //        body.angularVelocity = new Vector3(0, 0, 0);
        //    }
        //}
        //else
        //{
        //    body.angularVelocity = new Vector3(0, 0, 0);
        //}
	}
    //float mod(float a, float n)
    //{
    //    //mod = (a, n) -> a - floor(a/n) * n
    //    float q = Mathf.Floor(a / n) * n;
    //    return a - q;
    //}
}
