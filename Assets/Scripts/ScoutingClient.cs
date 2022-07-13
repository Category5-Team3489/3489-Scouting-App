using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NetCoreServer;
using System.Threading;
using System.Text;
using System.Net.Sockets;
using TcpClient = NetCoreServer.TcpClient;
using ScoutingShared3489;
using Newtonsoft.Json;

public class ScoutingClient : TcpClient
{
    private bool isStoppped = false;

    public ScoutingClient(string address, int port) : base(address, port)
    {

    }

    public void DisconnectAndStop()
    {
        isStoppped = true;
        DisconnectAsync();
    }

    protected override void OnConnected()
    {
        Debug.Log($"{Id}: connected");

        SendMessage(new PingSBMessage()
        {
            Text = "Hello, Server 0!"
        });
    }

    protected override void OnDisconnected()
    {
        Debug.Log($"{Id}: disconnected");

        Thread.Sleep(1000);

        if (!isStoppped)
        {
            ConnectAsync();
        }
    }

    protected override void OnError(SocketError error)
    {
        Debug.Log($"{Id}: error: {error}");
    }

    protected override void OnReceived(byte[] buffer, long offset, long size)
    {
        string messageJson = Encoding.UTF8.GetString(buffer, (int)offset, (int)size);
        Message message = JsonConvert.DeserializeObject<Message>(messageJson);
        if (message is not null)
        {
            if (message.Version == "0")
            {
                GameManager.I.EnqueueReceivedMessage(message);
            }
            else
            {
                Debug.Log($"v{message.Version}: invalid: {messageJson}");
            }
        }
        else
        {
            Debug.Log($"message null: {messageJson}");
        }
    }

    public void SendMessage<T>(T message)
    {
        string data = JsonConvert.SerializeObject(new Message
        {
            Version = "0",
            Type = typeof(T).Name,
            Json = JsonConvert.SerializeObject(message)
        });

        SendAsync(data);
    }
}
