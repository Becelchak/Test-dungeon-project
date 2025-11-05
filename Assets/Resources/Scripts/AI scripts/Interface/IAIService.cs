using System;
using UnityEngine;

public class IAIService
{
    public Action<string> OnAIResponseReceived { get; internal set; }
    public Action<bool> OnConnectionStatusChanged { get; internal set; }

    public void SendMessage(string message)
    {

    }
}
