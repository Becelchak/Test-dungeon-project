public interface IResourceService
{
    T LoadJson<T>(string path) where T : class;
    void SaveJson<T>(string path, T data) where T : class;
}