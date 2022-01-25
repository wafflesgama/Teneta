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

    byte[] sendingData;
    byte[] receivedData;
    private Texture2D receivedTexture;


    private NetworkClient client;
    private NetworkServer server;

    public int encodingQuality = 10;
    public int messageSize = 10;
    private int receivedMessageSize;

    private int currentIndex = 0;
    DateTime lastRenderTime;
    DateTime lastSentTime;
    bool hasReceivedInitFrame;

    bool waitingForNewFrame;

    [NetworkMessage]
    public struct SyncMessage
    {
        public int index;
        public int totalSize;
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

    void FixedUpdate()
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
        //Debug.Log($"OnSyncTexture received index {syncMessage.index}");

        if (Identity.NetId == player.Identity.NetId) return;

        if (syncMessage.index == 0)
        {
            //Debug.Log($"OnSyncTexture init data");
            if (lastRenderTime != null)
            {
                var timeDif = DateTime.Now.Subtract(lastRenderTime).TotalMilliseconds;
                Debug.Log($"Received timeDif {timeDif}");
            }
            lastRenderTime = DateTime.Now;
            receivedData = syncMessage.data.ToArray();
            receivedMessageSize = syncMessage.totalSize;
            hasReceivedInitFrame = true;
            waitingForNewFrame = false;

        }
        else if (syncMessage.index < currentIndex)
        {
            Debug.Log($"OnSyncTexture resetting data");
            waitingForNewFrame = true;
        }
        else if (hasReceivedInitFrame && !waitingForNewFrame)
        {
            //Debug.Log($"OnSyncTexture adding to data");
            receivedData = receivedData.Concat(syncMessage.data).ToArray();
        }


        if (hasReceivedInitFrame && !waitingForNewFrame && syncMessage.index == receivedMessageSize - 1)
        {
            if (receivedMessageSize > messageSize)
                Debug.Log($"OnSyncTexture finalizing bigger data");
            else
                Debug.Log($"OnSyncTexture finalizing normal data");

            try
            {
                receivedData = CLZF.Decompress(receivedData);
                receivedTexture.LoadImage(receivedData);
                raw.texture = receivedTexture;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Exception occurred {ex.Message}");
            }
        }
    }





    public void SyncTexture()
    {
        if (inputSource.texture2D == null) return;

        //Debug.Log("Setting up new frame data");
        sendingData = CLZF.Compress(inputSource.texture2D.EncodeToJPG(encodingQuality));
        //Debug.Log($"sendingData total legnth{sendingData.Length}");
        int sum = 0;

        var interval = sendingData.Length / messageSize;
        //Debug.Log($"interval {interval}");
        var leftovers = sendingData.Length - (interval * messageSize);
        var extraSize = (int)Math.Ceiling((float)leftovers / interval);
        var actualSize = messageSize + extraSize;
        Debug.Log($"actualSize {actualSize}");

        for (currentIndex = 0; currentIndex < actualSize; currentIndex++)
        {
            //if (currentIndex==0)
            //{
            //}
            var segm = new ArraySegment<byte>(sendingData.Skip((currentIndex) * interval).Take(interval).ToArray());
            sum += segm.Count;
            //Debug.Log($"sending legnth{segm.Count}");
            SyncMessage msg = new SyncMessage()
            {
                index = currentIndex,
                totalSize = actualSize,
                data = segm
            };
            server.SendToAll(msg);
            //server.SendToAll(msg, Channel.Unreliable);
        }

        Debug.Log($"sendingData sum legnth{sum}");
        if (lastSentTime != null)
        {
            var timeDif = DateTime.Now.Subtract(lastSentTime).TotalMilliseconds;
            Debug.Log($"Sent timeDif {timeDif}");
        }
        lastSentTime = DateTime.Now;

    }

}