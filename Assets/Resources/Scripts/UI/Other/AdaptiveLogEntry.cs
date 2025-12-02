using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AdaptiveLogEntry : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Image portraitImage;
    [SerializeField] private RectTransform backgroundRect;
    [SerializeField] private Image backgroundImage;

    [Header("Size Settings")]
    [SerializeField] private float minWidth = 120f;
    [SerializeField] private float maxWidth = 450f;
    [SerializeField] private float minHeight = 40f;
    [SerializeField] private float padding = 15f;

    //[Header("Visual Settings")]
    //[SerializeField] private Color playerColor = new Color(0.1f, 0.3f, 0.6f, 0.8f);
    //[SerializeField] private Color npcColor = new Color(0.3f, 0.3f, 0.3f, 0.8f);

    // Кэш для оптимизации
    private RectTransform _rectTransform;
    private bool _isInitialized = false;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    public void Initialize(string text, bool isPlayer, Sprite portrait = null)
    {
        // Устанавливаем текст
        if (messageText != null)
        {
            messageText.text = text;
        }

        // Устанавливаем портрет
        if (portraitImage != null)
        {
            if (portrait != null)
            {
                portraitImage.sprite = portrait;
                portraitImage.gameObject.SetActive(true);
            }
            else
            {
                portraitImage.gameObject.SetActive(false);
            }
        }


        // Пересчитываем размеры
        CalculateOptimalSize();

        _isInitialized = true;
    }

    private void CalculateOptimalSize()
    {
        if (messageText == null || backgroundRect == null) return;

        // Принудительно обновляем текстовую сетку
        messageText.ForceMeshUpdate();

        // Получаем предпочтительные размеры текста
        Vector2 preferredSize = messageText.GetPreferredValues();

        // Ограничиваем ширину
        float width = Mathf.Clamp(
            preferredSize.x + padding,
            minWidth,
            maxWidth
        );

        // Если текст не помещается в ограниченную ширину,
        // получаем новую высоту для этой ширины
        if (preferredSize.x > width - padding)
        {
            preferredSize = messageText.GetPreferredValues(
                width - padding,
                0 // Неограниченная высота
            );
        }

        // Вычисляем итоговую высоту
        float height = Mathf.Max(
            minHeight,
            preferredSize.y + padding
        );

        // Применяем размеры
        backgroundRect.sizeDelta = new Vector2(width, height);

        // Опционально: настраиваем выравнивание
        if (_rectTransform != null)
        {
            ConfigureAlignment(preferredSize);
        }
    }

    private void ConfigureAlignment(Vector2 textSize)
    {
        // Можно добавить логику для разных типов выравнивания
        // Например, сообщения игрока справа, NPC слева
    }

    // Метод для обновления текста после инициализации
    public void UpdateText(string newText)
    {
        if (messageText != null)
        {
            messageText.text = newText;
            CalculateOptimalSize();
        }
    }

    // Автоматический пересчет при изменении текста через инспектор
    private void OnValidate()
    {
        if (_isInitialized && messageText != null)
        {
            CalculateOptimalSize();
        }
    }
}