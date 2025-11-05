using System;
using UnityEngine;

public class ResourceService : BaseService, IResourceService
{
    protected override Type GetServiceType() => typeof(IResourceService);

    public T LoadJson<T>(string path) where T : class
    {
        try
        {
            if (path.EndsWith(".json"))
                path = path.Substring(0, path.Length - 5);

            var jsonFile = Resources.Load<TextAsset>(path);
            if (jsonFile != null)
            {
                return JsonUtility.FromJson<T>(jsonFile.text);
            }
            else
            {
                Debug.LogWarning($"JSON file not found at path: {path}");
                return null;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading JSON from {path}: {e.Message}");
            return null;
        }
    }

    public void SaveJson<T>(string path, T data) where T : class
    {
        Debug.LogWarning("SaveJson not implemented - using Resources is read-only. Progress in working");
        // Для сохранения нужно использовать Application.persistentDataPath
    }
}