using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AdaptiveTextContainer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private RectTransform messageRectTransform;
    [SerializeField] private Image portraitImage;
    [SerializeField] private RectTransform backgroundRect;
    //[SerializeField] private Image backgroundImage;

    [Header("Size Settings")]
    [SerializeField] private float minWidth = 120f;
    [SerializeField] private float maxWidth = 450f;
    [SerializeField] private float minHeight = 40f;
    [SerializeField] private Vector2 padding = new Vector2(15f, 10f);

    private RectTransform textContainerRectTransform;
    private bool _isInitialized = false;

    private void Awake()
    {
        textContainerRectTransform = GetComponent<RectTransform>();
    }

    //TO DO: Разделить задачи по рассчету оптимальных размеров (ширина/высота/все сразу)
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
        CalculateOptimalSizeForHeight();

        _isInitialized = true;
    }

    //TO DO: Разделить задачи по рассчету оптимальных размеров (ширина/высота/все сразу)
    public void Initialize(string text)
    {
        if (messageText != null)
        {
            messageText.text = text;
        }

        CalculateOptimalSize();

        _isInitialized = true;
    }

    private void CalculateOptimalSizeForHeight()
    {
        if (messageText == null) return;

        messageText.ForceMeshUpdate(forceTextReparsing:true);

        Vector2 preferredSize = messageText.GetPreferredValues();

        float targetWidth = Mathf.Clamp(
            preferredSize.x,
            minWidth,
            maxWidth
        );

        float textWidth = targetWidth - padding.x * 2;
        float textHeight;

        if (preferredSize.x > textWidth)
        {
            textHeight = messageText.GetPreferredValues(textWidth, messageText.renderedHeight).y;
        }
        else
        {
            textHeight = preferredSize.y;
        }

        float targetHeight = Mathf.Max(
            minHeight,
            textHeight + padding.y * 2
        );

        messageRectTransform.sizeDelta = new Vector2(messageRectTransform.sizeDelta.x, targetHeight);
        textContainerRectTransform.sizeDelta = new Vector2(textContainerRectTransform.sizeDelta.x, targetHeight + Math.Abs(messageRectTransform.anchoredPosition.y));

        ValidateTextFits();
    }

    private void CalculateOptimalSizeForWidth()
    {
        if (messageText == null) return;

        messageText.ForceMeshUpdate(forceTextReparsing: true);

        Vector2 preferredSize = messageText.GetPreferredValues();

        float targetWidth = Mathf.Clamp(
            preferredSize.x,
            minWidth,
            maxWidth
        );

        float textWidth = targetWidth - padding.x * 2;
        float textHeight;

        if (preferredSize.x > textWidth)
        {
            textHeight = messageText.GetPreferredValues(textWidth, messageText.renderedHeight).y;
        }
        else
        {
            textHeight = preferredSize.y;
        }

        messageRectTransform.sizeDelta = new Vector2(targetWidth, messageRectTransform.sizeDelta.y);
        textContainerRectTransform.sizeDelta = new Vector2(targetWidth + Math.Abs(messageRectTransform.anchoredPosition.x), messageRectTransform.sizeDelta.x);

        ValidateTextFits();
    }

    private void CalculateOptimalSize()
    {
        if (messageText == null) return;

        // Принудительно обновляем текстовую сетку
        messageText.ForceMeshUpdate(forceTextReparsing: true);

        // Получаем предпочтительные размеры текста
        Vector2 preferredSize = messageText.GetPreferredValues();

        // 3. Рассчитываем ширину с учетом ограничений
        float targetWidth = Mathf.Clamp(
            preferredSize.x, // Текст + горизонтальные отступы
            minWidth,
            maxWidth
        );

        // 4. Если текст шире доступного пространства, пересчитываем высоту
        float textWidth = targetWidth - padding.x * 2;
        float textHeight;

        if (preferredSize.x > textWidth)
        {
            // Текст не помещается по ширине - получаем высоту с учетом переноса
            textHeight = messageText.GetPreferredValues(textWidth, messageText.renderedHeight).y;
        }
        else
        {
            // Текст помещается - используем исходную высоту
            textHeight = preferredSize.y;
        }

        // 5. Рассчитываем общую высоту
        float targetHeight = Mathf.Max(
            minHeight,
            textHeight + padding.y * 2
        );

        messageRectTransform.sizeDelta = new Vector2(targetWidth, targetHeight);
        textContainerRectTransform.sizeDelta = new Vector2(targetWidth, targetHeight);

        ValidateTextFits();
    }

    private void ValidateTextFits()
    {
        messageText.ForceMeshUpdate();
        TMP_TextInfo textInfo = messageText.textInfo;

        if (textInfo != null && textInfo.characterCount > 0)
        {
            TMP_CharacterInfo lastChar = textInfo.characterInfo[textInfo.characterCount - 1];

            if (!lastChar.isVisible)
            {
                Debug.LogWarning("Текст не помещается, возможно нужно увеличить высоту или уменьшить шрифт");

                if(backgroundRect == null) return;

                // Автоматически увеличиваем высоту, если текст не помещается
                float currentHeight = backgroundRect.sizeDelta.y;
                backgroundRect.sizeDelta = new Vector2(
                    backgroundRect.sizeDelta.x,
                    currentHeight * 1.2f // Увеличиваем на 20%
                );
                backgroundRect.transform.position = new Vector3(backgroundRect.position.x,
                    currentHeight * 1.2f);
            }
        }
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

    public Vector2 GetCurrentSize()
    {
        return backgroundRect != null ? backgroundRect.sizeDelta : Vector2.zero;
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