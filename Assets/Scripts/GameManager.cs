using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using ScoutingShared3489;
using Newtonsoft.Json;

public class GameManager : MonoBehaviour
{
    #region Singleton
    private static GameManager instance;
    public static GameManager I { get { return instance; } }
    private void AwakeSingleton()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
    }
    #endregion Singleton

    private readonly Dictionary<string, Action<Message>> messageHandlers = new();
    private readonly ConcurrentQueue<Message> receivedMessageQueue = new();

    private ScoutingClient client;

    private void Awake()
    {
        AwakeSingleton();

        AddMessageHandlers();

        client = new ScoutingClient("127.0.0.1", 8080);
        client.ConnectAsync();
    }

    private void Start()
    {

    }

    private void OnApplicationQuit()
    {
        client.DisconnectAndStop();
    }

    public void EnqueueReceivedMessage(Message message)
    {
        receivedMessageQueue.Enqueue(message);
    }

    private void Update()
    {
        int receivedMessageQueueCount = receivedMessageQueue.Count;
        for (int i = 0; i < receivedMessageQueueCount; i++)
        {
            if (receivedMessageQueue.TryDequeue(out var receivedMessage))
            {
                if (messageHandlers.TryGetValue(receivedMessage.Type, out var messageHandler))
                {
                    messageHandler(receivedMessage);
                }
                else
                {
                    Debug.Log($"Unknown message: type: {receivedMessage.Type} with message: {receivedMessage.Json}");
                }
            }
            else
            {
                break;
            }
        }
    }

    private void AddMessageHandler<T>(Action<T> messageHandler)
    {
        messageHandlers.Add(typeof(T).Name, (message) =>
        {
            messageHandler(JsonConvert.DeserializeObject<T>(message.Json));
        });
    }

    private void AddMessageHandlers()
    {
        AddMessageHandler((PingCBMessage message) =>
        {
            Debug.Log($"{message.Text}");
        });
    }
}