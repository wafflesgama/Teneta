using Mirage;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;

public class GameManager : NetworkBehaviour
{
    public static GameManager instance;
    public NetworkServer server;
    public NetworkClient client;


    private int numPlayersInGame = 0;

    private VisualSync generator;
    private VisualSync receiver;


    public string[] keywords = new string[] { "macarena", "swim", "zombie", "chicken", "fish", "soldier", "clock", "plane", "scissors", "heart", "drive", "rancho", "kill", "ballerina", "maestro", "paint", "eat", "cowboy", "camel", "fight", "house", "star" };

    private void Awake()
    {
        instance = this;
    }
    void Start()
    {
        server.Connected.AddListener(Server_OnPlayerConnect);
        server.Disconnected.AddListener(OnPlayerDisconnect);
        server.Started.AddListener(FindSelf);
        client.Connected.AddListener(Client_Connected);
        //client.Connected.AddListener(Client_OnPlayerConnect);
    }


    private async void FindSelf()
    {

        await Task.Delay(250);
        if (generator != null) return;
        generator = GameObject.Find("Visual Canvas(Clone) (self)").GetComponent<VisualSync>();
        numPlayersInGame++;
    }


    private async void Server_OnPlayerConnect(INetworkPlayer player)
    {
        await Task.Delay(250);
        if (this.generator != null && !player.Identity.gameObject.name.Equals("Visual Canvas(Clone) (self)"))
        {
            receiver = player.Identity.gameObject.GetComponent<VisualSync>();
            numPlayersInGame++;
        }

        //if (numPlayersInGame >= 2)
        //    StartGame();
    }

    private async void Client_Connected(INetworkPlayer player)
    {
        if (IsServer) return;
        await Task.Delay(250);
        this.generator = GameObject.FindObjectsOfType<VisualSync>(true).Where(x => x.name == "Visual Canvas(Clone) (self)").FirstOrDefault();
        receiver = GameObject.Find("Visual Canvas(Clone)").GetComponent<VisualSync>();
        if (player != null && receiver != null)
            numPlayersInGame = 2;
    }

    private void Client_OnPlayerStarted()
    {
        //if (Identity.NetId != player.Identity.NetId)
        //{
        FindSelf();
        //}
    }

    private void OnPlayerDisconnect(INetworkPlayer player)
    {
        receiver = null;
        numPlayersInGame--;
    }
    [ContextMenu("Start Game")]
    public void StartGame()
    {
        if (!IsServer || numPlayersInGame < 2) return;

        OnStartGame();
    }

    [ClientRpc]
    private void OnStartGame()
    {
        generator.SetState(generate: true);
        receiver.SetState(generate: false);

        //if (!IsServer)
        receiver.StartReceiving();

        if (IsServer)
        {
            var wordToGuess = keywords[Random.Range(0, keywords.Length)];
            SetAwnser(wordToGuess);
            SetVis(wordToGuess);
        }
    }


    [ContextMenu("Switch Sides")]
    public void SwitchSides()
    {
        if (IsServer)
            OnSwitchSides();
    }

    [ClientRpc]
    public void OnSwitchSides()
    {
        //Debug.Log("Switching Sides");
        var test = !generator.gameObject.activeSelf;
        generator.gameObject.SetActive(!generator.gameObject.activeSelf);
        receiver.gameObject.SetActive(!receiver.gameObject.activeSelf);


        if (IsServer)
        {
            var wordToGuess = keywords[Random.Range(0, keywords.Length)];
            SetAwnser(wordToGuess);
            SetVis(wordToGuess);
        }

        //if (test)
        //{
        //    if (IsServer)
        //        receiver.StartReceiving();
        //    //else
        //    //    receiver.StopReceiving();
        //}
        //else
        //{
        //    if (IsServer)
        //        receiver.StartReceiving();
        //    //else
        //    //    receiver.StopReceiving();
        //}
    }

    [ClientRpc]
    private void SetAwnser(string aw)
    {
        receiver.SetAwnser(aw);
    }

    [ClientRpc]
    private void SetVis(string aw)
    {
        //Debug.Log("SetVis",gameObject);
        generator.SetVisWord(aw);
    }






}
