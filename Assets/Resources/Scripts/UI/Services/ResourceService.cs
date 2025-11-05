// Services/ResourceService.cs
using System.IO;
using UnityEngine;

public class ResourceService : IResourceService
{
    public T LoadJson<T>(string path) where T : class
    {
        var fullPath = Path.Combine(Application.streamingAssetsPath, path);
        if (File.Exists(fullPath))
        {
            var json = File.ReadAllText(fullPath);
            return JsonUtility.FromJson<T>(json);
        }
        return null;
    }

    public void SaveJson<T>(string path, T data) where T : class
    {
        var fullPath = Path.Combine(Application.streamingAssetsPath, path);
        var json = JsonUtility.ToJson(data, true);
        File.WriteAllText(fullPath, json);
    }
}