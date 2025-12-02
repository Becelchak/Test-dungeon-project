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