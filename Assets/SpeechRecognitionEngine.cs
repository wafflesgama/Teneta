using Mirage;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows.Speech;


public class SpeechRecognitionEngine : NetworkBehaviour
{
    public static PhraseRecognizer globalRecconizer;
    public string[] keywords = new string[] { "macarena", "swim", "zombie", "chicken", "fish", "soldier", "clock", "plane", "scissors", "heart", "drive", "rancho", "kill", "ballerina", "maestro", "paint", "eat", "cowboy", "camel", "fight", "house", "star" };
    public ConfidenceLevel confidence = ConfidenceLevel.Medium;
    VisualSync visualSync;


    protected PhraseRecognizer recognizer;


    private void Start()
    {
        if (recognizer == null && gameObject.activeSelf && globalRecconizer==null)
        {

            foreach (var mic in Microphone.devices)
            {
                Debug.Log(mic);
            }
            //Debug.Log(devi)
            Debug.LogWarning("Init Voice");
            Microphone.Start(Microphone.devices[0], true, 100, 44100);
            recognizer = new KeywordRecognizer(keywords, confidence);
            recognizer.OnPhraseRecognized += Recognizer_OnPhraseRecognized;
            globalRecconizer = recognizer;
            visualSync = GetComponent<VisualSync>();
            recognizer.Start();
        }
    }



    private void OnEnable()
    {
        if (recognizer != null)
        {
            Debug.LogWarning("Start Voice");
            if (!recognizer.IsRunning)
                recognizer.Start();
        }
        else
        {
            if(globalRecconizer != null)
            {
                globalRecconizer.Stop();
                globalRecconizer.Dispose();
            }
            recognizer = new KeywordRecognizer(keywords, confidence);
            recognizer.OnPhraseRecognized += Recognizer_OnPhraseRecognized;
            recognizer.Start();
            globalRecconizer = recognizer;
        }
    }
    private void OnDisable()
    {
        if (recognizer != null)
        {
            Debug.LogWarning("Stop Voice");
            if (recognizer.IsRunning)
                recognizer.Stop();

            recognizer.Dispose();
            recognizer = null;
        }
    }




    public void Recognizer_OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        //word = args.text;
        Debug.LogWarning($"Recognizer_OnPhraseRecognized {args.text}");
        visualSync.GuessedWord(args.text);

        //if (word == respostaCorreta)
        //{
        //    Debug.Log("Correct!");
        //}
        //else
        //{
        //    Debug.Log("Wrong guess...");
        //}
    }
    //public void NextRound()
    //{
    //    currentStringIndex++;
    //    respostaCorreta = respostas[currentStringIndex];

    //}

    private void OnApplicationQuit()
    {
        if (recognizer != null && recognizer.IsRunning)
        {
            recognizer.OnPhraseRecognized -= Recognizer_OnPhraseRecognized;
            recognizer.Stop();
        }
    }
}
