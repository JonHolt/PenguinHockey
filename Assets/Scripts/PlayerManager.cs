using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerManager : MonoBehaviour {
    //TODO: abstract almost everything to RPC calls to keep everything in sync.
    GameObject ice;
    PlayerManagerBehavior action;

    public GameObject rink;
    public GameObject prefab;

    // Use this for initialization
    void Start()
    {
        //initialize things that will be the same regardless of role
        NetworkView net = this.GetComponent<NetworkView>();
        ice = (GameObject)Instantiate(rink, new Vector3(0, 0, 0), Quaternion.identity);
        int localPlayers = PlayerPrefs.GetInt("numPlayers");
        if (PlayerPrefs.GetString("multiplayerType") == "online")
        {
            if (PlayerPrefs.GetString("onlineRole") == "host")
            {
                //create serverBehavior
                gameObject.AddComponent<ServerBehavior>();
                ServerBehavior serv = gameObject.GetComponent<ServerBehavior>();
                serv.InitServerBehavior(net, prefab, localPlayers);
                action = serv;
            }
            else
            {
                //create clientBehavior
                gameObject.AddComponent<ClientBehavior>();
                ClientBehavior cli = gameObject.GetComponent<ClientBehavior>();
                cli.InitClientBehavior(net, prefab, localPlayers);
                action = cli;
            }
        }
        else
        {
            //create localBehavior
            gameObject.AddComponent<LocalBehavior>();
            LocalBehavior loc = gameObject.GetComponent<LocalBehavior>();
            loc.InitLocalBehavior(prefab, localPlayers);
            action = loc;
        }
    }

    void Update()
    {
        action.Update();
    }

    [RPC]
    void RequestPlayerIDs(int num, NetworkMessageInfo info)
    {
        ((ServerBehavior)action).RequestPlayerIDs(num, info);
    }
    [RPC]
    void RecieveIDs(string[] ids, NetworkPlayer recipiant)
    {
        ((ClientBehavior)action).RecieveIDs(ids, recipiant);
    }
}

public abstract class PlayerManagerBehavior : MonoBehaviour
{
    protected int localPlayers;
    protected Dictionary<string, GameObject> players;
    protected GameObject prefab;
    protected Color[] colors = {
        new Color(0,0,0),
        new Color(.71f,.02f,.02f),
        new Color(.13f,.39f,.12f),
        new Color(.09f,.11f,.39f),
        new Color(.81f,.54f,.05f),
        new Color(.71f,.02f,.02f) //Add more when time permits
    };

    public virtual void Update()
    {
        foreach (string id in players.Keys)
        {            
            if (players.ContainsKey(id) && players[id].transform.position.y < -10)
            {
                players[id].GetComponent<Rigidbody>().velocity *= .25f;
                players[id].transform.position = new Vector3(Random.Range(-3f, 3f), 0.4f, Random.Range(-3f, 3f));
                //Add a point to scorer when point system is working
                //bounce bounceData = players[id].GetComponent<bounce>();
                //string scorer = bounceData.lastHitBy;
                //bounceData.lastHitBy = "";
            }
        }
    }
}

public class ServerBehavior : PlayerManagerBehavior
{
    NetworkView net;
    int totalPlayers;
    public void InitServerBehavior(NetworkView n, GameObject pref, int locals)
    {
        net = n;
        localPlayers = locals;
        prefab = pref;
        totalPlayers = 0;
        players = new Dictionary<string, GameObject>();

        //Launch the server 
        //Network.incomingPassword = "testingPassword";
        bool useNat = !Network.HavePublicAddress();
        Network.InitializeServer(20, 41793, useNat);
        //and register with Master Server
        MasterServer.RegisterHost("LP_Penguin_Hockey", Network.player.ipAddress.ToString());
    }
    public override void Update()
    {
        base.Update();
        //more later?
    }

    string genID()
    {
        return "P" + totalPlayers++; 
    }

    //RPC function for sending a group of ids out to a specific client.
    public void RequestPlayerIDs(int num, NetworkMessageInfo info)
    {
        string[] ids = new string[num];
        for (int i = 0; i < num; ++i)
            ids[i] = genID();
        net.RPC("RecieveIDs", RPCMode.Others, ids, info.sender);
    }

    void OnServerInitialized()
    {
        for(int i = 1; i <= localPlayers; ++i)
        {
            string id = genID();
            string controlID = "P" + i;
            players.Add(id, (GameObject)Network.Instantiate(prefab, new Vector3(Random.Range(-3f, 3f), 0.4f, Random.Range(-3f, 3f)), Quaternion.identity, 1));
            players[id].GetComponent<MeshRenderer>().material.color = colors[totalPlayers % colors.Length];
            players[id].GetComponent<Control>().PlayerNum = id;
            players[id].GetComponent<Control>().ControlNum = controlID;
        }
    }
}

public class ClientBehavior : PlayerManagerBehavior
{
    NetworkView net;
    int connected; //0 = no Attempt, 1 = Attempting, 2 = Connected
    float connectionStartTime;

    public void InitClientBehavior(NetworkView n, GameObject pref, int locals) 
    {
        net = n;
        localPlayers = locals;
        prefab = pref;
        players = new Dictionary<string, GameObject>();
        connected = 0;

        //find a server to connect to
        MasterServer.RequestHostList("LP_Penguin_Hockey");
        connectionStartTime = Time.time;
        Debug.Log("Client Init");
    }
    public override void Update()
    {
        if(connected == 0 && Connect())
        {
            connected = 1; //attempting;
        }
        else if(connected == 2)//connected
        {
            base.Update();
        }
    }

    bool Connect()
    {
        if (Time.time - connectionStartTime > 30)
        {
            PlayerPrefs.SetString("message", "Could not find an available game, try hosting one.");
            Application.LoadLevel(0);
        }

        HostData[] data = MasterServer.PollHostList();

        foreach (HostData element in data)
        {
            if (element.connectedPlayers < element.playerLimit)
            {
                Network.Connect("192.168.1.144", 41793);
                Debug.Log("Server Found");
                return true;
            }
        }
        return false;
    }

    void OnConnectedToServer()
    {
        net.RPC("RequestPlayerIDs", RPCMode.Server, localPlayers);
        Debug.Log("Connected, requesting IDs");
    }

    void OnFailedToConnect(NetworkConnectionError error)
    {
        Debug.Log("Could not connect to server: " + error);
        Application.LoadLevel(0);
    }

    void OnDisconnectedFromServer(NetworkDisconnection info)
    {
        foreach(GameObject player in players.Values)
        {
            Destroy(player);
        }
        //if (info == NetworkDisconnection.LostConnection)
        //    Debug.Log("Lost connection to the server");
        //else
        //    Debug.Log("Successfully diconnected from the server");
    }

    //RPC for recieving a group of ids from server iff the destination matches this client
    public void RecieveIDs(string[] ids, NetworkPlayer recipiant)
    {
        Debug.Log("Ids Recieved, making players");
        if(recipiant == Network.player)
        {
            for (int i = 1; i <= ids.Length; ++i)
            {
                string id = ids[i - 1];
                string controlID = "P" + i;
                players.Add(id, (GameObject)Network.Instantiate(prefab, new Vector3(Random.Range(-3f, 3f), 0.4f, Random.Range(-3f, 3f)), Quaternion.identity, 1));
                players[id].GetComponent<MeshRenderer>().material.color = colors[i % colors.Length];
                players[id].GetComponent<Control>().PlayerNum = id;
                players[id].GetComponent<Control>().ControlNum = controlID;

                Debug.Log("Player " + id + " created");
            }
            connected = 2;
        }
    }
}

public class LocalBehavior : PlayerManagerBehavior
{
    public void InitLocalBehavior(GameObject pref, int locals)
    {
        localPlayers = locals;
        prefab = pref;
        players = new Dictionary<string, GameObject>();

        for (int i = 1; i <= localPlayers; ++i)
        {
            string id = "P" + i;
            players.Add(id, (GameObject)Instantiate(prefab, new Vector3(Random.Range(-3f, 3f), 0.4f, Random.Range(-3f, 3f)), Quaternion.identity));
            players[id].GetComponent<MeshRenderer>().material.color = colors[i % colors.Length];
            players[id].GetComponent<Control>().PlayerNum = id;
            players[id].GetComponent<Control>().ControlNum = id;
        }
    }

    public override void Update()
    {
        base.Update();
    }
}