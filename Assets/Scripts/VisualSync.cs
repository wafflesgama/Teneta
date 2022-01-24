using Mirage;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using OpenCvSharp.Demo;
using System;

public class VisualSync : NetworkBehaviour
{
    public RawImage raw;
    public LiveSketchScript inputSource;

    Texture2D processText;

    byte[] receivedData;
    private Texture2D receivedTexture;


    private NetworkClient client;
    private NetworkServer server;

    private int messageSize = 4;

    private int currentIndex=0;

    [NetworkMessage]
    public struct SyncMessage
    {
        //public int index;
        public ArraySegment<byte> data;
    }

    void Start()
    {
        if (IsLocalPlayer)
            transform.parent.name += " (self)";

        if (IsClient && !IsLocalPlayer)
            receivedTexture = new Texture2D(2, 2);

        var manager = GameObject.Find("Network Manager");

        client = manager.GetComponent<NetworkClient>();
        server = manager.GetComponent<NetworkServer>();

        if (Identity.NetId > 1 && ((!IsLocalPlayer && IsServer) || (IsLocalPlayer && !IsServer)))
        {
            transform.parent.gameObject.SetActive(false);
            return;
        }
        
        client.MessageHandler.RegisterHandler<SyncMessage>(OnSyncTexture);


        server.PeerConfig = new Mirage.SocketLayer.Config();
        server.PeerConfig.MaxPacketSize = 8000;
        server.PeerConfig.MaxReliablePacketsInSendBufferPerConnection = 8000;

    }

    void Update()
    {
        if (IsServer && IsLocalPlayer)
        {
            SyncTexture();

            Debug.Log("Server is applying mat");
            processText = inputSource.texture2D;
            raw.texture = processText;
        }
    }

    private void OnSyncTexture(INetworkPlayer player, SyncMessage syncMessage)
    {
        if (IsServer && IsLocalPlayer) return;

        Debug.Log($"OnSyncTexture received");

        receivedData = syncMessage.data.ToArray();

        receivedData = CLZF.Decompress(receivedData);

        receivedTexture.LoadImage(receivedData);
        raw.texture = receivedTexture;
    }

    public void SyncTexture()
    {
        if (inputSource.finalMat == null) return;

        Debug.Log("Sending texture data");
        var pngData = inputSource.texture2D.GetRawTextureData();
        var jpgData = inputSource.texture2D.EncodeToJPG(1);

        var compressedjpgData = CLZF.Compress(jpgData);
        var compressedpngData = CLZF.Compress(pngData);

        Debug.Log($"pngData {pngData.Length}, jpgData {jpgData.Length}, cJpg {compressedjpgData.Length}, cPng {compressedpngData.Length}");
        SyncMessage msg = new SyncMessage()
        {
            data = new ArraySegment<byte>(compressedjpgData)
        };
        server.SendToAll(msg);
        //server.SendToAll(msg,Channel.Unreliable);
    }


    //private void OnSyncTexture(INetworkPlayer player, SyncMessage syncMessage)
    //{
    //    if (IsServer && IsLocalPlayer) return;

    //    Debug.Log($"OnSyncTexture received");

    //    receivedData = syncMessage.data.ToArray();

    //    receivedData = CLZF.Decompress(receivedData);

    //    receivedTexture.LoadImage(receivedData);
    //    raw.texture = receivedTexture;
    //}

    //public void SyncTexture()
    //{
    //    if (inputSource.finalMat == null) return;

    //    Debug.Log("Sending texture data");
    //    var pngData = inputSource.texture2D.GetRawTextureData();
    //    var jpgData = inputSource.texture2D.EncodeToJPG(1);

    //    var compressedjpgData = CLZF.Compress(jpgData);
    //    var compressedpngData = CLZF.Compress(pngData);

    //    Debug.Log($"pngData {pngData.Length}, jpgData {jpgData.Length}, cJpg {compressedjpgData.Length}, cPng {compressedpngData.Length}");
    //    SyncMessage msg = new SyncMessage()
    //    {
    //        data = new ArraySegment<byte>(compressedjpgData)
    //    };
    //    server.SendToAll(msg, Channel.Unreliable);
    //}
}