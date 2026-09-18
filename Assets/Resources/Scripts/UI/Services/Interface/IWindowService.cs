using System;
using UnityEngine;

public interface IWindowService
{
    //void ShowWindow<T>() where T : IViewModel;
    //void CloseWindow<T>() where T : IViewModel;
    //bool IsWindowOpen<T>() where T : IViewModel;
    //void ShowAIDialogue(string npcId);
    //void ShowClassicalDialogue(string dialogueId);
    //void ShowInventoryGroup();

    GameObject ShowWindow<TViewModel>(UILayer layer, Action<TViewModel> onBeforeBind = null) where TViewModel : class, IViewModel;
    void CloseWindow<TViewModel>() where TViewModel : class, IViewModel;
    bool IsWindowOpen<TViewModel>() where TViewModel : class, IViewModel;
}
