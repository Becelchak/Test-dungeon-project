using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ContextMenuView : BaseView<ContextMenuViewModel>
{
    [Header("UI")]
    [SerializeField] private GameObject contextMenuPanel;
    [SerializeField] private Transform buttonsContainer;
    [SerializeField] private GameObject buttonPrefab;

    private readonly List<GameObject> _buttons = new List<GameObject>();
    private CanvasGroup _canvasGroup;
    public ContextMenuViewModel viewModel => ViewModel;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
    }

    protected override void SetupBindings()
    {
        ViewModel.PropertyChanged += OnPropertyChanged;
        ViewModel.Initialize();
        contextMenuPanel.SetActive(false);
    }

    protected override void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ViewModel.IsVisible))
        {
            contextMenuPanel.SetActive(ViewModel.IsVisible);
            if (ViewModel.IsVisible)
            {
                RebuildButtons();
                _canvasGroup.blocksRaycasts = true;
            }
            else
            {
                _canvasGroup.blocksRaycasts = false;
            }
        }
    }

    private void RebuildButtons()
    {
        foreach (var btn in _buttons)
        {
            if (btn != null) Destroy(btn);
        }
        _buttons.Clear();

        if (ViewModel.Actions == null) return;

        for (int i = 0; i < ViewModel.Actions.Count; i++)
        {
            int index = i;
            var action = ViewModel.Actions[i];
            var btnObj = Instantiate(buttonPrefab, buttonsContainer);
            var btn = btnObj.GetComponent<Button>();
            var text = btnObj.GetComponentInChildren<TMPro.TextMeshProUGUI>();

            if (text != null)
            {
                text.text = action.ActionName;
            }

            if (btn != null)
            {
                btn.onClick.AddListener(() => ViewModel.ExecuteAction(index));
            }

            _buttons.Add(btnObj);
        }
    }

    public void Hide()
    {
        ViewModel?.Hide();
    }

    private void Update()
    {
        if (ViewModel != null && ViewModel.IsVisible)
        {
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
            {
                // РџСЂРѕРІРµСЂСЏРµРј, РєР»РёРєРЅСѓР» Р»Рё РїРѕР»СЊР·РѕРІР°С‚РµР»СЊ РІРЅРµ РєРѕРЅС‚РµРєСЃС‚РЅРѕРіРѕ РјРµРЅСЋ
                PointerEventData pointerData = new PointerEventData(EventSystem.current)
                {
                    position = Input.mousePosition
                };

                var results = new List<RaycastResult>();
                EventSystem.current.RaycastAll(pointerData, results);

                bool clickedOnMenu = false;
                foreach (var result in results)
                {
                    if (result.gameObject.transform.IsChildOf(transform))
                    {
                        clickedOnMenu = true;
                        break;
                    }
                }

                if (!clickedOnMenu)
                {
                    Hide();
                }
            }
        }
    }
}
