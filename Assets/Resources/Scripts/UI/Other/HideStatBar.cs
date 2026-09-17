using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class HudStatBar : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private HudStatBarConfig config;

    [Header("UI References")]
    [SerializeField] private Image mainBar;
    [SerializeField] private Image delayedBar;
    [SerializeField] private TextMeshProUGUI valueText;

    private CanvasGroup _canvasGroup;
    private CanvasGroup CanvasGroup
    {
        get
        {
            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
                if (_canvasGroup == null) _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
            return _canvasGroup;
        }
    }
    private float _idleTimer;
    private float _currentAlpha;
    private float _delayedFill;
    private float _targetFill;
    private bool _initialized;
    private bool _permanentlyVisible;

    private void Awake()
    {
        _currentAlpha = 1f;
        CanvasGroup.alpha = 1f;
    }

    /// <summary>
    /// Первичная инициализация. НЕ будит бар (он остаётся скрытым, если autoHide = true).
    /// </summary>
    public void Initialize(int current, int max)
    {
        ApplyValue(current, max, instant: true);
        _initialized = true;

        if (config != null && config.autoHide && !ShouldStayVisible())
        {
            SetAlpha(0f);
            _idleTimer = config.idleTimeout; // уже "проспал"
        }
        else
        {
            SetAlpha(1f);
        }
    }

    /// <summary>
    /// Обновление значения. Пробуждает бар, если он скрыт.
    /// </summary>
    public void SetValue(int current, int max)
    {
        ApplyValue(current, max, instant: false);
        WakeUp();
    }

    /// <summary>
    /// Принудительно показать бар и удерживать его (например, в бою).
    /// </summary>
    public void ShowPermanently()
    {
        _permanentlyVisible = true;
        SetAlpha(1f);
    }

    /// <summary>
    /// Снять блокировку постоянного отображения.
    /// </summary>
    public void ReleasePermanent()
    {
        _permanentlyVisible = false;
        WakeUp();
    }

    // ------------------------------------------------------------
    // Внутренняя логика
    // ------------------------------------------------------------

    private void ApplyValue(int current, int max, bool instant)
    {
        float newFill = max > 0 ? Mathf.Clamp01((float)current / max) : 0f;

        if (mainBar != null)
            mainBar.fillAmount = newFill;

        if (valueText != null)
            valueText.text = $"{current}/{max}";

        // Если лечение или первичная инициализация — отстающая полоса прыгает сразу
        if (!_initialized || instant || newFill > _targetFill)
        {
            _delayedFill = newFill;
            if (delayedBar != null)
                delayedBar.fillAmount = newFill;
        }

        _targetFill = newFill;
    }

    private void WakeUp()
    {
        _idleTimer = 0f;
        SetAlpha(1f);
    }

    private bool ShouldStayVisible()
    {
        if (_permanentlyVisible) return true;
        if (config == null) return true;
        if (!config.autoHide) return true;

        // Критический порог: не скрывать, если значение ниже порога
        if (config.criticalThreshold > 0f && _targetFill <= config.criticalThreshold)
            return true;

        return false;
    }

    private void Update()
    {
        float dt = Time.deltaTime;

        AnimateDelayedBar(dt);
        UpdateVisibility(dt);
    }

    private void AnimateDelayedBar(float dt)
    {
        if (delayedBar == null || config == null) return;
        if (Mathf.Approximately(_delayedFill, _targetFill)) return;

        _delayedFill = Mathf.MoveTowards(_delayedFill, _targetFill, config.delayedBarSpeed * dt);
        delayedBar.fillAmount = _delayedFill;
    }

    private void UpdateVisibility(float dt)
    {
        if (ShouldStayVisible())
        {
            SetAlpha(1f);
            _idleTimer = 0f;
            return;
        }

        _idleTimer += dt;

        float targetAlpha = _idleTimer >= config.idleTimeout ? 0f : 1f;
        if (Mathf.Approximately(_currentAlpha, targetAlpha)) return;

        float step = config.fadeDuration > 0f ? dt / config.fadeDuration : 1f;
        float newAlpha = Mathf.MoveTowards(_currentAlpha, targetAlpha, step);
        SetAlpha(newAlpha);
    }

    private void SetAlpha(float alpha)
    {
        _currentAlpha = alpha;
        float curved = config != null ? config.fadeCurve.Evaluate(alpha) : alpha;
        CanvasGroup.alpha = curved;
        CanvasGroup.blocksRaycasts = alpha > 0.01f;
        CanvasGroup.interactable = alpha > 0.01f;
    }
}