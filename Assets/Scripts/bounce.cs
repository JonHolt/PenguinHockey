using UnityEngine;
using System.Collections;

public class bounce : MonoBehaviour {
    public string lastHitBy;

    void OnCollisionEnter(Collision collision)
    {
        Rigidbody otherBody = collision.gameObject.GetComponent<Rigidbody>();
        Control id = collision.gameObject.GetComponent<Control>();
        if (id != null)
            lastHitBy = id.PlayerNum;
        Rigidbody thisBody = GetComponent<Rigidbody>();
        float impactForce = thisBody.velocity.magnitude / 5;
        if(otherBody != null)
            otherBody.AddForce(collision.contacts[0].normal * -impactForce, ForceMode.Impulse);
    }
}
