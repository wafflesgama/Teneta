using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using OpenCvSharp.Demo;
using System;
using System.Threading.Tasks;

public class VisualSync : MonoBehaviour
{
    public LiveSketchScript generationSript;
    public VisualReceiver receiverSript;
    public GameObject waitingUI;

    public int encodingQuality = 10;
    public int messageSize = 10;

    byte[] sendingData;
    byte[] receivedData;
    Texture2D receivedTexture;


    //NetworkClient client;
    //NetworkServer server;

    int receivedMessageSize;

    int currentIndex = 0;
    DateTime lastRenderTime;
    DateTime lastSentTime;
    bool hasReceivedInitFrame;

    bool waitingForNewFrame;

    private bool isGenerating;

    public string wordToGuess;

    public Text visWord;


    public Animator transitionAnim;
    public Text transitionText;

    //[NetworkMessage]
    public struct SyncMessage
    {
        public int index;
        public int totalSize;
        public ArraySegment<byte> data;
    }

    private void Awake()
    {
        var manager = GameObject.Find("Network Manager");
        //client = manager.GetComponent<NetworkClient>();
        //server = manager.GetComponent<NetworkServer>();
    }

    void Start()
    {
        ////if (IsLocalPlayer)
        ////    transform.name += " (self)";

        receivedTexture = new Texture2D(2, 2);

        //client.MessageHandler.RegisterHandler<SyncMessage>(OnSyncTexture);

        ////if (Identity.NetId > 1 && ((!IsLocalPlayer && IsServer) || (IsLocalPlayer && !IsServer)))
        ////{
        ////    gameObject.SetActive(false);
        ////    return;
        ////}


        //SetState(IsServer && IsLocalPlayer);



        //server.PeerConfig = new Mirage.SocketLayer.Config();
        //server.PeerConfig.MaxPacketSize = 8000;
        //server.PeerConfig.MaxReliablePacketsInSendBufferPerConnection = 8000;

    }

    //private void OnEnable()
    //{
    //    client.MessageHandler.RegisterHandler<SyncMessage>(OnSyncTexture);
    //}

    //private void OnDisable()
    //{
    //    client.MessageHandler.UnregisterHandler<SyncMessage>();
    //}

    public async  void SetAwnser(string aw)
    {
        transitionText.text = "You are guessing";
        transitionAnim.SetBool("fade", true);
        await Task.Delay(700);
        wordToGuess = aw;
        visWord.transform.parent.gameObject.SetActive(false);
        transitionAnim.SetBool("fade", false);
    }

    public async void SetVisWord(string word)
    {
        transitionText.text = "You are mimicking";
        transitionAnim.SetBool("fade", true);
        visWord.text = word;
        await Task.Delay(700);
        visWord.transform.parent.gameObject.SetActive(true);
        transitionAnim.SetBool("fade", false);
    }
    public void GuessedWord(string word)
    {
        Debug.LogWarning($"Guessed word {word}");
        //if (word.Equals(wordToGuess))
        //    GameManager.instance.OnSwitchSides();

    }

    public void StopReceiving()
    {
        ////if (IsServer)
        ////    client.MessageHandler.UnregisterHandler<SyncMessage>();
        ////else
        ////    server.MessageHandler.UnregisterHandler<SyncMessage>();
    }

    public void StartReceiving()
    {
        //if (IsServer)
        //    server.MessageHandler.RegisterHandler<SyncMessage>(OnSyncTexture);
        //else
        //    client.MessageHandler.RegisterHandler<SyncMessage>(OnSyncTexture);
    }


    void FixedUpdate()
    {
        if (isGenerating)
        {
            SyncTexture();
        }
    }

    public void SetState(bool generate)
    {
        waitingUI.SetActive(false);

        isGenerating = generate;

        if (generate && receiverSript != null)
            Destroy(receiverSript.gameObject);
        else if (!generate && generationSript != null)
            Destroy(generationSript.gameObject);
    }

    private void OnSyncTexture( SyncMessage syncMessage)
    {
        //Debug.Log($"OnSyncTexture received index {syncMessage.index}");
        //Debug.Log($"OnSyncTexture received from owner ID {player.Identity.NetId}, to ID {Identity.NetId} obj {gameObject.name}");

        //if (Identity.NetId == player.Identity.NetId || !gameObject.activeSelf) return;
        if (!gameObject.activeSelf) return;


        if (syncMessage.index == 0)
        {
            //Debug.Log($"OnSyncTexture init data");
            if (lastRenderTime != null)
            {
                var timeDif = DateTime.Now.Subtract(lastRenderTime).TotalMilliseconds;
                //Debug.Log($"Received timeDif {timeDif}");
            }
            lastRenderTime = DateTime.Now;
            receivedData = syncMessage.data.ToArray();
            receivedMessageSize = syncMessage.totalSize;
            hasReceivedInitFrame = true;
            waitingForNewFrame = false;

        }
        else if (syncMessage.index < currentIndex)
        {
            //Debug.Log($"OnSyncTexture resetting data");
            waitingForNewFrame = true;
        }
        else if (hasReceivedInitFrame && !waitingForNewFrame)
        {
            //Debug.Log($"OnSyncTexture adding to data");
            receivedData = receivedData.Concat(syncMessage.data).ToArray();
        }


        if (hasReceivedInitFrame && !waitingForNewFrame && syncMessage.index == receivedMessageSize - 1)
        {
            //if (receivedMessageSize > messageSize)
            //    Debug.Log($"OnSyncTexture finalizing bigger data");
            //else
            //    Debug.Log($"OnSyncTexture finalizing normal data");

            try
            {
                receivedData = CLZF.Decompress(receivedData);
                receivedTexture.LoadImage(receivedData);
                //test.texture = receivedTexture;
                receiverSript.UpdateVisual(ref receivedTexture);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Exception occurred {ex.Message}");
            }
        }
    }





    public void SyncTexture()
    {
        if (generationSript.generatedTex == null) return;

        //Debug.Log("Setting up new frame data");
        sendingData = CLZF.Compress(generationSript.generatedTex.EncodeToJPG(encodingQuality));
        //Debug.Log($"sendingData total legnth{sendingData.Length}");
        int sum = 0;

        var interval = sendingData.Length / messageSize;
        //Debug.Log($"interval {interval}");
        var leftovers = sendingData.Length - (interval * messageSize);
        var extraSize = (int)Math.Ceiling((float)leftovers / interval);
        var actualSize = messageSize + extraSize;
        //Debug.Log($"actualSize {actualSize}");

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
            //server.SendToAll(msg);
            //if (IsServer)
            //    server.SendToAll<SyncMessage>(msg, Channel.Unreliable);
            //else
            //    client.Send<SyncMessage>(msg, Channel.Unreliable);
        }

        //Debug.Log($"sendingData sum legnth{sum}");
        if (lastSentTime != null)
        {
            var timeDif = DateTime.Now.Subtract(lastSentTime).TotalMilliseconds;
            //Debug.Log($"Sent timeDif {timeDif}");
        }
        lastSentTime = DateTime.Now;

    }

}