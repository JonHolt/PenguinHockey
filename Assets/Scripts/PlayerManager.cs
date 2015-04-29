using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerManager : MonoBehaviour {
    int numPlayers = 1;
    public GameObject prefab;
    public GameObject rink;
    public float gameTime = 180;
    Dictionary<string, GameObject> players;
    GameObject ice;
    Color[] colors = {
        new Color(0,0,0),
        new Color(.71f,.02f,.02f),
        new Color(.13f,.39f,.12f),
        new Color(.09f,.11f,.39f),
        new Color(.81f,.54f,.05f),
        new Color(.71f,.02f,.02f)
    };
	// Use this for initialization
	void Start () {
        ice = (GameObject)Instantiate(rink, new Vector3(0, 0, 0), Quaternion.identity);
        numPlayers = PlayerPrefs.GetInt("numPlayers");
        Debug.Log(numPlayers);
        players = new Dictionary<string, GameObject>();
        for (int i = 0; i < numPlayers; ++i)
        {
            string id = "P" + (i + 1);
            players.Add(id,(GameObject)Instantiate(prefab, new Vector3(Random.Range(-3f, 3f), 0.4f, Random.Range(-3f, 3f)), Quaternion.identity));
            players[id].GetComponent<MeshRenderer>().material.color = colors[i % colors.Length];
            players[id].GetComponent<Control>().PlayerNum = id;
        }
	}
	
	// Update is called once per frame
	void Update () {
        for (int i = 0; i < numPlayers; ++i)
        {
            string id = "P" + (i + 1);
            if (players[id].transform.position.y < -10)
            {
                players[id].GetComponent<Rigidbody>().velocity *= .25f;
                players[id].transform.position = new Vector3(Random.Range(-3f, 3f), 0.4f, Random.Range(-3f, 3f));
                bounce bounceData = players[id].GetComponent<bounce>();
                string scorer = bounceData.lastHitBy;
                bounceData.lastHitBy = "";
                if(players.ContainsKey(scorer))
                    players[scorer].GetComponent<Control>().points++;
            }
        }
        GlobalWarming();
	}

    void GlobalWarming()
    {
        
    }
}
