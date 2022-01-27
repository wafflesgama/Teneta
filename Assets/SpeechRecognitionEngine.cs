using Mirage;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows.Speech;


public class SpeechRecognitionEngine : NetworkBehaviour
{
    public string[] keywords = new string[] { "macarena", "swim", "zombie", "chicken", "fish", "soldier", "clock", "plane", "scissors", "heart", "drive", "rancho", "kill", "ballerina", "maestro", "paint", "eat", "cowboy", "camel", "fight", "house", "star" };
    public ConfidenceLevel confidence = ConfidenceLevel.Medium;
    public bool correctAnswer = false;
    public string respostaCorreta;
    public int currentStringIndex;
    public string respostaDada;
    VisualSync visualSync;


    protected PhraseRecognizer recognizer;
    public string word;


    private void Start()
    {
        if (recognizer == null)
        {

            foreach (var mic in Microphone.devices)
            {
                Debug.Log(mic);
            }
            //Debug.Log(devi)
            Debug.LogWarning("Init Voice");
            Microphone.Start(Microphone.devices[0], true, 100, 4100);
            recognizer = new KeywordRecognizer(keywords, confidence);
            recognizer.OnPhraseRecognized += Recognizer_OnPhraseRecognized;
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
    }
    private void OnDisable()
    {
        if (recognizer != null)
        {
            Debug.LogWarning("Stop Voice");
            if (recognizer.IsRunning)
                recognizer.Stop();
        }
    }



    public void SetWord(string word)
    {
        respostaCorreta = word;
    }

    public void Recognizer_OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        word = args.text;
        Debug.LogWarning($"Recognizer_OnPhraseRecognized {args.text}");
        visualSync.GuessedWord(word);

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
