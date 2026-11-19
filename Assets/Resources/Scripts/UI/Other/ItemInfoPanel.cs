using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Панель информации о предмете: название (цвет редкости), описание
/// и две колонки технических характеристик. Сама позиционируется
/// рядом с курсором с клампингом по границам экрана.
/// При показе может переноситься в overlayParent (например, Gameplay Layer),
/// чтобы отображаться поверх окон, и возвращаться обратно при скрытии.
/// Автоматически пересчитывает высоту панели, позиции строк
/// и размер рамки заголовка под объём контента.
/// </summary>
public class ItemInfoPanel : MonoBehaviour
{
    [Header("Текстовые поля")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text statsLeftText;
    [SerializeField] private TMP_Text statsRightText;

    [Header("Рамка заголовка")]
    [Tooltip("Рамка вокруг названия предмета. Если не назначена — ищется автоматически как родитель titleText. Если titleText — прямой ребёнок панели, рамки нет.")]
    [SerializeField] private RectTransform titleFrame;
    [Tooltip("Внутренние отступы рамки заголовка (X — горизонтальные, Y — вертикальные)")]
    [SerializeField] private Vector2 titleFramePadding = new Vector2(24f, 8f);

    [Header("Позиционирование")]
    [Tooltip("Смещение верхнего-левого угла панели относительно курсора")]
    [SerializeField] private Vector2 cursorOffset = new Vector2(20f, -20f);
    [Tooltip("Отступ панели от краёв экрана")]
    [SerializeField] private float screenPadding = 8f;

    [Header("Иерархия")]
    [Tooltip("Родитель, в который панель переносится при показе (например, Gameplay Layer), чтобы отображаться поверх окон. Если пуст — без переноса.")]
    [SerializeField] private Transform overlayParent;

    [Header("Адаптация")]
    [Tooltip("Внутренние отступы панели от краёв контента")]
    [SerializeField] private float contentPadding = 16f;
    [Tooltip("Расстояние между строками контента (заголовок / описание / статы)")]
    [SerializeField] private float rowSpacing = 8f;
    [Tooltip("Включать перенос строк у текстов, чтобы они не вылезали за панель")]
    [SerializeField] private bool enableWordWrap = true;
    [Tooltip("Минимальная высота панели при адаптации")]
    [SerializeField] private float minPanelHeight = 60f;

    private RectTransform _rt;
    private Vector2 _anchorScreenPos;
    private bool _visible;
    private Transform _originalParent;
    private int _originalSiblingIndex;

    private void Awake()
    {
        _rt = GetComponent<RectTransform>();
        _originalParent = transform.parent;
        _originalSiblingIndex = transform.GetSiblingIndex();
        overlayParent = _originalParent.parent;

        if (titleFrame == null && titleText != null)
        {
            var parentRt = titleText.rectTransform.parent as RectTransform;
            if (parentRt != null && parentRt != _rt)
                titleFrame = parentRt;
        }
    }

    /// <summary>Показать панель с данными предмета рядом с точкой screenPos (Input.mousePosition).</summary>
    public void Show(ItemData item, Vector2 screenPos)
    {
        if (item == null)
        {
            Hide();
            return;
        }

        MoveToOverlay();
        FillContent(item);
        _anchorScreenPos = screenPos;

        if (!gameObject.activeSelf) gameObject.SetActive(true);
        _visible = true;

        FitToContent();
        UpdatePosition();
    }

    public void Hide()
    {
        _visible = false;
        RestoreParent();
        gameObject.SetActive(false);
    }

    /// <summary>Переносит панель в overlayParent и выводит её на передний план.</summary>
    private void MoveToOverlay()
    {
        if (overlayParent == null || transform.parent == overlayParent) return;

        transform.SetParent(overlayParent, false);
        transform.SetAsLastSibling();
    }

    /// <summary>Возвращает панель в исходного родителя и исходный порядок.</summary>
    private void RestoreParent()
    {
        if (_originalParent == null || transform.parent == _originalParent) return;

        transform.SetParent(_originalParent, false);
        transform.SetSiblingIndex(_originalSiblingIndex);
    }

    private void LateUpdate()
    {
        // Пересчёт позиции на случай изменения размера панели после layout
        if (_visible) UpdatePosition();
    }

    private void FillContent(ItemData item)
    {
        if (titleText != null) titleText.text = ItemInfoFormatter.FormatTitle(item);
        if (descriptionText != null) descriptionText.text = item.description ?? string.Empty;

        var lines = ItemInfoFormatter.BuildStatLines(item);
        int mid = (lines.Count + 1) / 2;
        var left = lines.GetRange(0, mid);
        var right = lines.GetRange(mid, lines.Count - mid);

        if (statsLeftText != null) statsLeftText.text = ItemInfoFormatter.BuildColumnText(left);
        if (statsRightText != null) statsRightText.text = ItemInfoFormatter.BuildColumnText(right);

        if (enableWordWrap)
        {
            if (descriptionText != null) descriptionText.enableWordWrapping = true;
            if (statsLeftText != null) statsLeftText.enableWordWrapping = true;
            if (statsRightText != null) statsRightText.enableWordWrapping = true;
        }
    }

    /// <summary>
    /// Пересчитывает размеры и позиции под контент:
    /// измеряет предпочтительную высоту каждого текста,
    /// выстраивает строки сверху вниз от верха панели
    /// и задаёт итоговую высоту панели.
    /// Строка заголовка — это рамка titleFrame (или сам titleText,
    /// если рамки нет): рамка подгоняется под текст.
    /// Работает и на увеличение, и на уменьшение контента.
    /// </summary>
    private void FitToContent()
    {
        Canvas.ForceUpdateCanvases();
        float descH = MeasureTextHeight(descriptionText);
        float statsH = Mathf.Max(MeasureTextHeight(statsLeftText), MeasureTextHeight(statsRightText));

        float y = contentPadding;
        y = PlaceTitleRow(y);
        if (descH > 0.5f)
        {
            y += rowSpacing;
            y = PlaceTextRow(descriptionText, y, descH);
        }
        if (statsH > 0.5f)
        {
            y += rowSpacing;
            PlaceTextRow(statsLeftText, y, statsH);
            PlaceTextRow(statsRightText, y, statsH);
            y += statsH;
        }

        // Итоговая высота: контент + нижний отступ
        float totalH = y + contentPadding;
        float finalH = Mathf.Max(minPanelHeight, totalH);
        _rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, finalH);

        LayoutRebuilder.ForceRebuildLayoutImmediate(_rt);
    }

    /// <summary>
    /// Размещает строку заголовка: если есть рамка — подгоняет её
    /// под размер текста и ставит как строку; иначе ставит сам текст.
    /// Возвращает нижнюю границу строки.
    /// </summary>
    private float PlaceTitleRow(float topDistance)
    {
        if (titleText == null) return topDistance;

        if (titleFrame == null)
        {
            float h = MeasureTextHeight(titleText);
            return PlaceTextRow(titleText, topDistance, h);
        }

        // Размер текста внутри рамки
        float textH = MeasureTextHeight(titleText);
        float textW = MeasureTextWidth(titleText);

        // Рамка = текст + внутренние отступы, ограничена шириной панели
        float maxFrameW = _rt.rect.width - contentPadding * 2f;
        float frameW = Mathf.Min(textW + titleFramePadding.x * 2f, maxFrameW);
        float frameH = textH + titleFramePadding.y * 2f;

        PlaceRow(titleFrame, topDistance, frameH, frameW);

        var trt = titleText.rectTransform;
        trt.anchorMin = new Vector2(0.5f, 0.5f);
        trt.anchorMax = new Vector2(0.5f, 0.5f);
        trt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, frameW - titleFramePadding.x * 2f);
        trt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, textH);
        trt.anchoredPosition = Vector2.zero;

        return topDistance + frameH;
    }

    /// <summary>
    /// Ставит текст в строку на расстоянии topDistance от верха панели
    /// (якорь top-center) и возвращает нижнюю границу строки.
    /// </summary>
    private float PlaceTextRow(TMP_Text tmp, float topDistance, float height)
    {
        if (tmp == null) return topDistance;
        return PlaceRow(tmp.rectTransform, topDistance, height);
    }

    /// <summary>
    /// Ставит RectTransform в строку на расстоянии topDistance от верха панели
    /// (якорь top-center, опционально новая ширина) и возвращает нижнюю границу.
    /// </summary>
    private float PlaceRow(RectTransform rt, float topDistance, float height, float? width = null)
    {
        float x = rt.anchoredPosition.x;

        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        if (width.HasValue)
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width.Value);
        rt.anchoredPosition = new Vector2(x, -(topDistance + height * 0.5f));

        return topDistance + height;
    }

    /// <summary>Предпочтительная высота текста при его текущей ширине.</summary>
    private float MeasureTextHeight(TMP_Text tmp)
    {
        if (tmp == null || string.IsNullOrEmpty(tmp.text)) return 0f;

        var rt = tmp.rectTransform;
        float w = rt.rect.width;
        if (w <= 0.5f) return rt.rect.height;

        var pref = tmp.GetPreferredValues(tmp.text, w, 0f);
        return pref.y > 0.5f ? pref.y : rt.rect.height;
    }

    /// <summary>Предпочтительная ширина текста без учёта ограничений.</summary>
    private float MeasureTextWidth(TMP_Text tmp)
    {
        if (tmp == null || string.IsNullOrEmpty(tmp.text)) return 0f;

        var pref = tmp.GetPreferredValues(tmp.text, 8192f, 0f);
        return pref.x > 0.5f ? pref.x : tmp.rectTransform.rect.width;
    }

    /// <summary>
    /// Ставит панель рядом с _anchorScreenPos и клампит её в пределах экрана.
    /// Пробует прямое смещение, затем отражённые варианты, иначе клампит.
    /// </summary>
    private void UpdatePosition()
    {
        if (_rt == null) return;

        Vector2 size = _rt.rect.size;
        Vector2 pivot = _rt.pivot;

        // Кандидаты верхнего-левого угла панели: прямой и отражённые по осям
        var candidates = new Vector2[]
        {
            _anchorScreenPos + cursorOffset,
            new Vector2(_anchorScreenPos.x - cursorOffset.x - size.x, _anchorScreenPos.y + cursorOffset.y),
            new Vector2(_anchorScreenPos.x + cursorOffset.x, _anchorScreenPos.y - cursorOffset.y + size.y),
            new Vector2(_anchorScreenPos.x - cursorOffset.x - size.x, _anchorScreenPos.y - cursorOffset.y + size.y),
        };

        foreach (var topLeft in candidates)
        {
            Vector2 pivotPos = TopLeftToPivot(topLeft, size, pivot);
            if (FitsScreen(pivotPos, size, pivot))
            {
                ApplyPosition(pivotPos);
                return;
            }
        }

        ApplyPosition(ClampToScreen(TopLeftToPivot(candidates[0], size, pivot), size, pivot));
    }

    private Vector2 TopLeftToPivot(Vector2 topLeft, Vector2 size, Vector2 pivot)
    {
        return new Vector2(
            topLeft.x + pivot.x * size.x,
            topLeft.y - (1f - pivot.y) * size.y);
    }

    private bool FitsScreen(Vector2 pivotPos, Vector2 size, Vector2 pivot)
    {
        float minX = screenPadding + pivot.x * size.x;
        float maxX = Screen.width - screenPadding - (1f - pivot.x) * size.x;
        float minY = screenPadding + pivot.y * size.y;
        float maxY = Screen.height - screenPadding - (1f - pivot.y) * size.y;

        return pivotPos.x >= minX && pivotPos.x <= maxX
            && pivotPos.y >= minY && pivotPos.y <= maxY;
    }

    private Vector2 ClampToScreen(Vector2 pivotPos, Vector2 size, Vector2 pivot)
    {
        float minX = screenPadding + pivot.x * size.x;
        float maxX = Screen.width - screenPadding - (1f - pivot.x) * size.x;
        float minY = screenPadding + pivot.y * size.y;
        float maxY = Screen.height - screenPadding - (1f - pivot.y) * size.y;

        return new Vector2(
            maxX < minX ? pivotPos.x : Mathf.Clamp(pivotPos.x, minX, maxX),
            maxY < minY ? pivotPos.y : Mathf.Clamp(pivotPos.y, minY, maxY));
    }

    private void ApplyPosition(Vector2 pivotPos)
    {
        var pos = _rt.position;
        _rt.position = new Vector3(pivotPos.x, pivotPos.y, pos.z);
    }
}
