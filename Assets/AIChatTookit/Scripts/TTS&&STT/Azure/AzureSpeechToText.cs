﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.Networking;
[RequireComponent(typeof(AzureSettings))]
public class AzureSpeechToText : STT
{

    /// <summary>
    /// Azure配置项
    /// </summary>
    [SerializeField] private AzureSettings m_AzureSettings;
    public string mode = "conversation";


    private void Awake()
    {
        // Initialize the audio source
        m_AzureSettings = this.GetComponent<AzureSettings>();
        GetUrl();
    }
    /// <summary>
    /// 拼url
    /// </summary>
    private void GetUrl()
    {
        if (m_AzureSettings == null)
            return;

        m_SpeechRecognizeURL = "https://" +
            m_AzureSettings.serviceRegion +
            ".stt.speech.microsoft.com/speech/recognition/conversation/cognitiveservices/v1?language=" +
            m_AzureSettings.language;
    }
    /// <summary>
    /// 语音识别
    /// </summary>
    /// <param name="_clip"></param>
    /// <param name="_callback"></param>
    public override void SpeechToText(AudioClip _clip, Action<string> _callback)
    {
        byte[] _audioData= WavUtility.FromAudioClip(_clip);
        StartCoroutine(SendAudioData(_audioData, _callback));
    }

    /// <summary>
    /// 语音识别
    /// </summary>
    /// <param name="_audioData"></param>
    /// <param name="_callback"></param>
    public override void SpeechToText(byte[] _audioData, Action<string> _callback)
    {
        StartCoroutine(SendAudioData(_audioData, _callback));
    }


    /// <summary>
    /// 识别语音
    /// </summary>
    /// <param name="audioData"></param>
    /// <param name="_callback"></param>
    /// <returns></returns>
    private IEnumerator SendAudioData(byte[] audioData, Action<string> _callback)
    {
        stopwatch.Restart();
        // Create the request object
        UnityWebRequest request = UnityWebRequest.Post(m_SpeechRecognizeURL, "application/octet-stream");
        request.SetRequestHeader("Ocp-Apim-Subscription-Key", m_AzureSettings.subscriptionKey);
        request.SetRequestHeader("Content-Type", "audio/wav; codec=audio/pcm; samplerate=44100");

        // Attach the audio data to the request
        request.uploadHandler = new UploadHandlerRaw(audioData);
        request.uploadHandler.contentType = "application/octet-stream";

        // Send the request and wait for the response
        yield return request.SendWebRequest();

        // Check for errors
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Speech recognition request failed: " + request.error);
            yield break;
        }

        // Parse the response JSON and extract the recognition result
        string json = request.downloadHandler.text;
        SpeechRecognitionResult result = JsonUtility.FromJson<SpeechRecognitionResult>(json);
        string recognizedText = result.DisplayText;

        // Display the recognized text in the console
        Debug.Log("Recognized text: " + recognizedText);
        _callback(recognizedText);

        stopwatch.Stop();
        Debug.Log("Azure语音识别耗时：" + stopwatch.Elapsed.TotalSeconds);
    }
}

[System.Serializable]
public class SpeechRecognitionResult
{
    public string RecognitionStatus;
    public string DisplayText;
}
