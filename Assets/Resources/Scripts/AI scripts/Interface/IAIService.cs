using System;

public interface IAIService
{
    event Action<string> OnAIResponseReceived;
    event Action<bool> OnConnectionStatusChanged;
    
    void SendMessage(string message);
    bool isConnected { get; set; }
    void RetryConnection();
}
