using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LogEntryView : BaseView<LogEntryViewModel>  // Теперь используем ViewModel
{
    [Header("UI References")]
    [SerializeField] private Image portraitImage;
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Image backgroundImage;

    [Header("Visual Settings")]
    [SerializeField] private Color playerBackgroundColor = new Color(0.2f, 0.3f, 0.5f, 0.3f);
    [SerializeField] private Color npcBackgroundColor = new Color(0.3f, 0.3f, 0.3f, 0.3f);

    protected override void SetupBindings()
    {
        UpdateView();

        // Подписываемся на изменения свойств ViewModel
        ViewModel.PropertyChanged += OnPropertyChanged;
    }

    protected override void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        UpdateView();
    }

    private void UpdateView()
    {
        if (ViewModel == null) return;

        portraitImage.sprite = ViewModel.SpeakerPortrait;
        speakerNameText.text = ViewModel.SpeakerName;
        messageText.text = ViewModel.MessageText;

        // Разный цвет фона для игрока и NPC
        backgroundImage.color = ViewModel.IsPlayer ? playerBackgroundColor : npcBackgroundColor;
    }

    public override void Unbind()
    {
        if (ViewModel != null)
        {
            ViewModel.PropertyChanged -= OnPropertyChanged;
        }
        base.Unbind();
    }
}