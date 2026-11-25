using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;
using NUnit.Framework;
using System.Collections.Generic;

public class SkillNodeView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Settings")]
    public string nodeId;

    [Header("UI Components")]
    [Tooltip("Иконка навыка. Может назначаться само, если изображение на объекте")]
    public Image iconImage;
    public Outline outlineImage;
    [Tooltip("Кнопка взаимодействия с навыком. Может назначаться сама, если кнопка на объекте")]
    public Button unlockButton;
    [Tooltip("Линия рендера связей между навыками")]
    public UILineRenderer lineRenderer;

    [Header("Colors")]
    [Tooltip("Цвет для доступных навыков (бледный оранжевый)")]
    public Color availableColor = new Color(1f, 0.8f, 0.4f, 0.7f);
    [Tooltip("Цвет обводки для доступных навыков (насыщенный оранжевый)")]
    public Color availableOutlineColor = new Color(1f, 0.6f, 0.2f, 1f);
    [Tooltip("Цвет для разблокированных навыков (белый)")]
    public Color unlockedColor = Color.white;
    [Tooltip("Цвет для заблокированных навыков (прозрачный и темный)")]
    public Color lockedColor = new Color(0.3f, 0.3f, 0.3f, 0.3f);

    private SkillNodeViewModel _viewModel;

    /// <summary>Курсор вошёл на узел. Аргумент — текущая ViewModel узла.</summary>
    public event Action<SkillNodeViewModel> Hovered;
    /// <summary>Курсор покинул узел.</summary>
    public event Action Unhovered;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_viewModel != null) Hovered?.Invoke(_viewModel);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Unhovered?.Invoke();
    }

    public void Bind(SkillNodeViewModel viewModel)
    {
        _viewModel = viewModel;

        if (iconImage == null)
            iconImage = gameObject.GetComponent<Image>();
        else
            iconImage.sprite = viewModel.Icon;

        UpdateStatus();
        // Подписка на изменения свойств
        _viewModel.PropertyChanged += OnPropertyChanged;

        if (unlockButton != null)
            unlockButton.onClick.AddListener(OnUnlockClick);
        else
        {
            unlockButton = gameObject.GetComponent<Button>();
            unlockButton.onClick.AddListener(OnUnlockClick);
        }
    }

    public void Unbind()
    {
        if (unlockButton != null)
            unlockButton.onClick.RemoveListener(OnUnlockClick);

        if (_viewModel != null)
        {
            _viewModel.PropertyChanged -= OnPropertyChanged;
        }

        _viewModel = null;
    }

    private void OnPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SkillNodeViewModel.IsUnlocked) ||
            e.PropertyName == nameof(SkillNodeViewModel.CanUnlock) ||
            e.PropertyName == nameof(SkillNodeViewModel.IsLocked))
        {
            UpdateStatus();
        }
    }

    public List<SkillNodeData> GetRequiredNodes()
    {
        return _viewModel.SkillNodes;
    }

    private void UpdateStatus()
    {
        if (_viewModel == null) return;

        // Состояние 1: Разблокирован (белый)
        if (_viewModel.IsUnlocked)
        {
            if (iconImage != null)
                iconImage.color = unlockedColor;

            if (outlineImage != null)
                outlineImage.effectColor = unlockedColor;
        }
        // Состояние 2: Доступен для разблокировки (бледный оранжевый + насыщенная обводка)
        else if (_viewModel.CanUnlock)
        {
            if (iconImage != null)
                iconImage.color = availableColor;

            if (outlineImage != null)
                outlineImage.effectColor = availableOutlineColor;
        }
        // Состояние 3: Заблокирован окончательно (прозрачный и темный)
        else if (_viewModel.IsLocked)
        {
            if (iconImage != null)
                iconImage.color = lockedColor;

            if (outlineImage != null)
                outlineImage.effectColor = lockedColor;
        }

        if (unlockButton != null)
        {
            unlockButton.interactable = _viewModel.CanUnlock;
        }
    }

    private void OnUnlockClick()
    {
        if (_viewModel != null && _viewModel.UnlockCommand != null)
        {
            if (_viewModel.UnlockCommand.CanExecute(null))
            {
                _viewModel.UnlockCommand.Execute(null);
            }
        }
    }

    private void OnDestroy()
    {
        Unbind();
    }
}
