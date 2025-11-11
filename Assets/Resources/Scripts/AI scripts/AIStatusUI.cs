using UnityEngine;

public class AIStatusUI : MonoBehaviour
{
    public AIClient aiClient;
    public GameObject connectionPanel;
    public TMPro.TextMeshProUGUI statusText;

    void Update()
    {
        if (connectionPanel != null)
        {
            connectionPanel.SetActive(!aiClient.IsConnected);

            if (statusText != null)
            {
                statusText.text = aiClient.IsConnected ?
                    "Нейросеть подключена" :
                    "Нейросеть неактивна\nЗапустите LM Studio";
            }
        }
    }

    public void OnRetryConnection()
    {
        aiClient.RetryConnection();
    }

    public void OnOpenInstructions()
    {
        string instructionsPath = System.IO.Path.Combine(
            Application.streamingAssetsPath,
            "Инструкция по установке AI.txt"
        );

        if (System.IO.File.Exists(instructionsPath))
        {
            System.Diagnostics.Process.Start(instructionsPath);
        }
    }
}