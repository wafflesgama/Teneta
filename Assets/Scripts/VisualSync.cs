using Mirage;
using Mirage.Collections;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OpenCvSharp.Demo;

public class VisualSync : NetworkBehaviour
{
    public RawImage raw;
    public LiveSketchScript inputSource;


    Texture2D processText;

    SyncList<byte> texData = new SyncList<byte>();

    void Start()
    {
        if (IsLocalPlayer && !IsServer)
        {
            transform.parent.gameObject.SetActive(false);
            transform.parent.name += " (self)";
        }
    }

    void Update()
    {

        if (IsServer)
        {
            SyncTexture();
            raw.texture = inputSource.texture2D;
        }
        else
        {
            processText.LoadImage(texData.ToArray());
            raw.texture = processText;
        }
    }

    [ServerRpc]
    public void SyncTexture()
    {
        if (inputSource.texture2D != null)
            texData = new SyncList<byte>(inputSource.texture2D.GetRawTextureData());
    }
}
