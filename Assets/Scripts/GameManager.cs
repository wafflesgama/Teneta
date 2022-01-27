using Mirage;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;

public class GameManager : NetworkBehaviour
{
    public NetworkServer server;
    public NetworkClient client;


    private int numPlayersInGame = 0;

    private VisualSync player;
    private VisualSync opponent;


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
        if (player != null) return;
        player = GameObject.Find("Visual Canvas(Clone) (self)").GetComponent<VisualSync>();
        numPlayersInGame++;
    }


    private async void Server_OnPlayerConnect(INetworkPlayer player)
    {
        await Task.Delay(250);
        if (this.player != null && !player.Identity.gameObject.name.Equals("Visual Canvas(Clone) (self)"))
        {
            opponent = player.Identity.gameObject.GetComponent<VisualSync>();
            numPlayersInGame++;
        }

        //if (numPlayersInGame >= 2)
        //    StartGame();
    }

    private async void Client_Connected(INetworkPlayer player)
    {
        if (IsServer) return;
        await Task.Delay(250);
        this.player = GameObject.FindObjectsOfType<VisualSync>(true).Where(x => x.name == "Visual Canvas(Clone) (self)").FirstOrDefault();
        opponent = GameObject.Find("Visual Canvas(Clone)").GetComponent<VisualSync>();
        if (player != null && opponent != null)
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
        opponent = null;
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
        player.SetState(generate: true);
        opponent.SetState(generate: false);
    }


    [ContextMenu("Switch Sides")]
    public void SwitchSides()
    {
        if (IsServer)
            OnSwitchSides();
    }

    [ClientRpc]
    private void OnSwitchSides()
    {
        Debug.Log("Switching Sides");
        player.gameObject.SetActive(!player.gameObject.activeSelf);
        opponent.gameObject.SetActive(!opponent.gameObject.activeSelf);
    }






}
