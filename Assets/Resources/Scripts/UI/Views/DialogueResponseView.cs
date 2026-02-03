using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueResponseView : BaseView<DialogueResponseViewModel>
{
    [Header("UI References")]
    [SerializeField] private AdaptiveTextContainer adaptiveLogEntry;
    [SerializeField] private TextMeshProUGUI messageText;
    protected override void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        UpdateView();

        ViewModel.PropertyChanged += OnPropertyChanged;
    }

    protected override void SetupBindings()
    {
        UpdateView();
    }

    private void UpdateView()
    {
        if (adaptiveLogEntry != null)
        {
            adaptiveLogEntry.Initialize(ViewModel.Text);
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
