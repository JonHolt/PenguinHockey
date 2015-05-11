using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ErrorMessage : MonoBehaviour {

	// Use this for initialization
	void Start () {
        string errstring = PlayerPrefs.GetString("message");
	    if(errstring != null || errstring != "")
        {
            GetComponent<Text>().text = errstring;
            PlayerPrefs.DeleteAll();
        }
	}

	// Update is called once per frame
	void Update () {
	
	}
}
