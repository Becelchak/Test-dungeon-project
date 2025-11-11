using System;

public interface IAIService
{
    event Action<string> OnAIResponseReceived;
    event Action<bool> OnConnectionStatusChanged;
    event Action<string> OnConnectionError;

    void SendMessage(string message);
    bool IsConnected { get; }
    void RetryConnection();
    void ClearConversation();
}
