using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LogEntryView : BaseView<LogEntryViewModel>
{
    [Header("UI References")]
    [SerializeField] private AdaptiveTextContainer adaptiveLogEntry;
    [SerializeField] private Image portraitImage;
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Image backgroundImage;

    protected override void SetupBindings()
    {
        UpdateView();

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

        if (ViewModel.SpeakerPortrait != null)
        {
            portraitImage.sprite = ViewModel.SpeakerPortrait;
            portraitImage.gameObject.SetActive(true);
        }
        else
        {
            portraitImage.gameObject.SetActive(false);
        }

        if (adaptiveLogEntry != null)
        {
            adaptiveLogEntry.Initialize(
                ViewModel.MessageText,
                ViewModel.IsPlayer,
                ViewModel.SpeakerPortrait
            );

            if (speakerNameText != null)
            {
                speakerNameText.text = ViewModel.SpeakerName;
            }
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(
            transform as RectTransform);
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