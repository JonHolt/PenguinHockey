using UnityEngine;
using System.Collections;

public class Menu : MonoBehaviour {

    public int numAI = 4;
    GameObject[] AIs;
    public GameObject AI;
    Color[] colors = {
        new Color(.71f,.02f,.02f),
        new Color(.13f,.39f,.12f),
        new Color(.09f,.11f,.39f),
        new Color(.81f,.54f,.05f),
        new Color(0,0,0),
        new Color(.71f,.02f,.02f)
    };
	// Use this for initialization
	void Start () {
        AIs = new GameObject[numAI];
        for (int i = 0; i < numAI; ++i)
        {
            AIs[i] = (GameObject)Instantiate(AI, new Vector3(Random.Range(-4.5f, 4.5f), 0.4f, Random.Range(-4.5f, 4.5f)), Quaternion.identity);
            AIs[i].GetComponent<MeshRenderer>().material.color = colors[i%colors.Length];
        }
	}
	// Update is called once per frame
	void Update () {
        for (int i = 0; i < numAI; ++i)
        {
            if (AIs[i].transform.position.y < -5)
            {
                AIs[i].transform.position = new Vector3(Random.Range(-4.5f, 4.5f), 0.4f, Random.Range(-4.5f, 4.5f));
            }
        }
	}
    
    public void SetOnline()
    {
        PlayerPrefs.SetString("net", "online");
    }
    
    public void SetLocal()
    {
        PlayerPrefs.SetString("net", "offline");
    }

    public void StartGame(int numPlayers)
    {
        PlayerPrefs.SetInt("numPlayers", numPlayers);
        Application.LoadLevel("Iceberg");
    }
}
