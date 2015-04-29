using UnityEngine;
using System.Collections;

public class AI : MonoBehaviour {

    bool isChasing = false;
    public Vector3 center = new Vector3(0f, 0.4f, 0f);
	// Update is called once per frame
	void Update () {
        if (!isChasing)
        {
            Vector3 direction = center - transform.position + new Vector3(Random.Range(-5f,5f),0,Random.Range(-5f,5f));
            GetComponent<Rigidbody>().AddForce(direction);
            transform.LookAt(center);
        }
	}

    void OnTriggerEnter(Collider other)
    {
        isChasing = true;
    }
    void OnTriggerStay(Collider other)
    {
        if (isChasing)
        {
            Vector3 direction = (other.transform.position - transform.position);
            direction.y = 0;
            GetComponent<Rigidbody>().AddForce(direction);
            transform.LookAt(other.transform.position);
        }
    }
    void OnTriggerExit(Collider other)
    {
        isChasing = false;
    }

    void OnCollisionEnter(Collision collision)
    {
        isChasing = false;
        GetComponent<Rigidbody>().AddForce(collision.contacts[0].normal * -1.5f ,ForceMode.Impulse);
    }
}
