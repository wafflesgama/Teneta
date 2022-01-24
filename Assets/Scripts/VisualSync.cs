using Mirage;
using Mirage.Collections;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OpenCvSharp;
using OpenCvSharp.Demo;
using System;

public class VisualSync : NetworkBehaviour
{
    public NetworkClient client;
    public NetworkServer server;

    public RawImage raw;
    public LiveSketchScript inputSource;

    Texture2D processText;

    byte[] receivedData;
    Texture2D receivedTexture;
    //Mat receivedMat;
    bool hasReceivedData;

    public struct SyncMessage
    {
        public ArraySegment<byte> data;
    }

    private void Awake()
    {

    }
    void Start()
    {
        //processText=new Texture2D(16,16);

        if (IsLocalPlayer)
            transform.parent.name += " (self)";

        var manager = GameObject.Find("Network Manager");
        client = manager.GetComponent<NetworkClient>();
        server = manager.GetComponent<NetworkServer>();

        if (Identity.NetId > 1 && ((!IsLocalPlayer && IsServer) || (IsLocalPlayer && !IsServer)))
        {
            transform.parent.gameObject.SetActive(false);
            return;
        }


        client.MessageHandler.RegisterHandler<SyncMessage>(OnSyncTexture);
    }

    void Update()
    {

        if (IsServer && IsLocalPlayer)
            SyncTexture();
        {

            Debug.Log("Server is applying mat");
            processText = inputSource.texture2D;
            raw.texture = processText;
            //raw.texture = inputSource.texture2D;
        }


        if (hasReceivedData)
        {
            Debug.Log("Client is applying mat");
            //processText = OpenCvSharp.Unity.MatToTexture(receivedMat);
            Texture2D tex = new Texture2D(2, 2);
            ImageConversion.LoadImage(tex, receivedData);
            raw.texture = tex;

            //    if (receivedData != null && receivedData.Length >0)
            //    {
            //        //Debug.Log("Texdata leng "+ texData.Count);
            //        //Debug.Log("Texdata array leng "+ texData.ToArray().Length);
            //        processText.LoadImage(receivedData);
            //        raw.texture = processText;
            //    }
            //}
        }
    }

    void OnSyncTexture(INetworkPlayer player, SyncMessage syncMessage)
    {
        if (IsServer && IsLocalPlayer) return;

        Debug.Log($"OnSyncTexture received");
        hasReceivedData = true;
        var decompressedData = CLZF.Decompress(syncMessage.data.ToArray());
        receivedData = decompressedData;

    }


    public void SyncTexture()
    {
        if (inputSource.finalMat == null) return;

        Debug.Log("Sending texture data");
        var tempData = inputSource.texture2D.EncodeToJPG(5);
        var compressedData = CLZF.Compress(tempData);
        SyncMessage msg = new SyncMessage()
        {
            data = new ArraySegment<byte>(compressedData)
        };

        server.SendToAll(msg);
    }
}
