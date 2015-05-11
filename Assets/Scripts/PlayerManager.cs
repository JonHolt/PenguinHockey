using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerManager : MonoBehaviour {
    //TODO: abstract almost everything to RPC calls to keep everything in sync.
    NetworkView net;

    bool connected = false;
    bool isServer = false;
    bool isLocal = false;
    float connectionStartTime;
    int totalPlayers = 0;
    int numPlayers;
    public const int MAX_PLAYERS = 10;
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
        net = this.GetComponent<NetworkView>();
        ice = (GameObject)Instantiate(rink, new Vector3(0, 0, 0), Quaternion.identity);
        numPlayers = PlayerPrefs.GetInt("numPlayers");
        if(PlayerPrefs.GetString("multiplayerType") == "online")
        {
            if(PlayerPrefs.GetString("onlineRole") == "host")
            {
                //Launch the server 
                Network.incomingPassword = "testingPassword";
                bool useNat = !Network.HavePublicAddress();
                Network.InitializeServer(20, 41793, useNat);
                //and register with Master Server
                MasterServer.RegisterHost("LP_Penguin_Hockey", Network.player.ipAddress.ToString());
                isServer = true;
            }
            else
            {
                MasterServer.RequestHostList("LP_Penguin_Hockey"); //TODO: finish code for connecting.
                connectionStartTime = Time.time;
            }
        }
        else
        {
            isLocal = true;
        }

        players = new Dictionary<string, GameObject>();

        if(isServer || isLocal)
            for (int i = 0; i < numPlayers; ++i)
            {
                AddPlayer(i + 1);
            }
	}
	
	// Update is called once per frame
	void Update () {
        if(!isServer && !isLocal && !connected)
        {
            if (Connect())
                connected = true;
            return;
        }
        if(players.Count == 0)
        {
            for (int i = 0; i < numPlayers; ++i)
            {
                AddPlayer(i + 1);
            }
        }
        
        for (int i = 0; i < totalPlayers; ++i)
        {
            string id = "P" + (i + 1);
            if (players.ContainsKey(id) && players[id].transform.position.y < -10)
            {
                players[id].GetComponent<Rigidbody>().velocity *= .25f;
                players[id].transform.position = new Vector3(Random.Range(-3f, 3f), 0.4f, Random.Range(-3f, 3f));
                bounce bounceData = players[id].GetComponent<bounce>();
                string scorer = bounceData.lastHitBy;
                bounceData.lastHitBy = "";
                if (!isLocal)
                    net.RPC("AddPoints", RPCMode.AllBuffered, scorer);
                else
                    AddPoints(scorer);
            }
        }
        //GlobalWarming();
	}

    [RPC]
    void AddPoints(string id)
    {
        if (players.ContainsKey(id))
            players[id].GetComponent<Control>().points++;
    }

    void AddPlayer(int cNum)
    {
        string id = "P" + ++totalPlayers;
        if(isLocal)
            players.Add(id, (GameObject)Instantiate(prefab, new Vector3(Random.Range(-3f, 3f), 0.4f, Random.Range(-3f, 3f)), Quaternion.identity));
        else
            players.Add(id, (GameObject)Network.Instantiate(prefab, new Vector3(Random.Range(-3f, 3f), 0.4f, Random.Range(-3f, 3f)), Quaternion.identity, 1));
        players[id].GetComponent<MeshRenderer>().material.color = colors[totalPlayers % colors.Length];
        players[id].GetComponent<Control>().PlayerNum = id;
        players[id].GetComponent<Control>().ControlNum = "P" + cNum;
    }

    bool Connect()
    {
        if(Time.time - connectionStartTime > 30)
        {
            PlayerPrefs.SetString("message", "Could not find an available game, try hosting one.");
            Application.LoadLevel(0);
        }

        HostData[] data = MasterServer.PollHostList();
        foreach(HostData element in data)
        {
            if(element.connectedPlayers < element.playerLimit)
            {
                Network.Connect(element, "testingPassword");
                while (Network.connections.Length == 0)
                    Debug.Log(Network.isClient);
                return true;
            }
        }
        return false;
    }

    //void GlobalWarming()
    //{
        
    //}
}
