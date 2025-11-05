using System.ComponentModel;
using UnityEngine;

public abstract class BaseView<T> : MonoBehaviour, IView where T : IViewModel
{
    protected T ViewModel { get; private set; }

    public virtual void Bind(IViewModel viewModel)
    {
        if (viewModel is T typedViewModel)
        {
            ViewModel = typedViewModel;
            ViewModel.PropertyChanged += OnPropertyChanged;
            SetupBindings();
        }
    }

    public virtual void Unbind()
    {
        if (ViewModel != null)
        {
            ViewModel.PropertyChanged -= OnPropertyChanged;
            ViewModel.Cleanup();
            ViewModel = default(T);
        }
    }

    protected abstract void SetupBindings();
    protected abstract void OnPropertyChanged(object sender, PropertyChangedEventArgs e);

    protected virtual void OnDestroy()
    {
        Unbind();
    }
}