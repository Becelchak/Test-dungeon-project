using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public class ResourceService : BaseService, IResourceService
{
    [SerializeField] private ItemDatabase itemDatabaseAsset;
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
    }

    public Dictionary<string, ItemData> GetItemDataBase()
    {
        if (itemDatabaseAsset == null)
        {
            Debug.LogError("[ResourceService] Не назначена ItemDatabase в инспекторе сервиса!");
            return new Dictionary<string, ItemData>();
        }

        return itemDatabaseAsset.Database;
    }
}