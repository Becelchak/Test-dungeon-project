using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MessageView : MonoBehaviour
{
    [SerializeField] private Image avatarImage;
    [SerializeField] private TextMeshProUGUI senderNameText;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private TextMeshProUGUI timestampText;

    public void Bind(MessageData message, PlayerProfile playerProfile, AICharacterData aiCharacter)
    {
        messageText.text = message.text;
        timestampText.text = message.timestamp.ToString("HH:mm");

        if (message.isAI)
        {
            senderNameText.text = aiCharacter.characterName;
            avatarImage.sprite = aiCharacter.avatar;
            // Стиль для сообщений AI
        }
        else
        {
            senderNameText.text = playerProfile.playerName;
            avatarImage.sprite = playerProfile.avatar;
            // Стиль для сообщений игрока
        }
    }
}