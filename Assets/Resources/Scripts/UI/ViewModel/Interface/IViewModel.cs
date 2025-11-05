using System.ComponentModel;

public interface IViewModel
{
    event PropertyChangedEventHandler PropertyChanged;
    void Initialize();
    void Cleanup();
}

public interface IView
{
    void Bind(IViewModel viewModel);
    void Unbind();
}

public interface IWindowService
{
    void ShowWindow<T>() where T : IViewModel;
    void CloseWindow<T>() where T : IViewModel;
    bool IsWindowOpen<T>() where T : IViewModel;
}