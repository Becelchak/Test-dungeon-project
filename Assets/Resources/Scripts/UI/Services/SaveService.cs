using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Центральный сервис сохранений (один автослот).
/// Файл: Application.persistentDataPath/savegame.json
/// Структура: { "version": N, "sections": { "<moduleKey>": { ...json... } } }
/// Модули сами решают, чем сериализовать свою секцию.
/// Запись атомарная: сначала .tmp, затем замена файла.
/// Автосохранение — каждые 30 секунд при наличии несохранённых изменений,
/// а также при паузе/выходе из приложения.
/// </summary>
public class SaveService : BaseService, ISaveService
{
    private const int SaveVersion = 1;
    private const float AutosaveIntervalSeconds = 30f;
    private const string SaveFileName = "savegame.json";

    private readonly Dictionary<string, ISaveModule> _modules = new Dictionary<string, ISaveModule>();
    /// <summary>Секции выгруженных модулей (например, экспедиция при смене сцены) — сохраняются до явного удаления.</summary>
    private readonly Dictionary<string, string> _orphanedSections = new Dictionary<string, string>();
    private Dictionary<string, string> _loadedSections;
    private bool _fileLoaded;
    private bool _restoreApplied;
    private bool _dirty;
    private float _autosaveTimer;

    public string SaveFilePath => Path.Combine(Application.persistentDataPath, SaveFileName);

    protected override Type GetServiceType() => typeof(ISaveService);

    private void Awake()
    {
        base.Awake();
        EnsureFileLoaded();
    }

    private void Start()
    {
        // Все сервисы уже прошли Awake и могли зарегистрировать модули.
        ApplyAllRestores();
        _restoreApplied = true;
    }

    private void Update()
    {
        if (!_dirty) return;

        _autosaveTimer += Time.unscaledDeltaTime;
        if (_autosaveTimer >= AutosaveIntervalSeconds)
            ForceSave();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && _dirty) ForceSave();
    }

    private void OnApplicationQuit()
    {
        if (_dirty) ForceSave();
    }

    public void RegisterModule(ISaveModule module)
    {
        if (module == null || string.IsNullOrEmpty(module.Key)) return;

        _modules[module.Key] = module;
        _orphanedSections.Remove(module.Key);
        EnsureFileLoaded();

        // Модуль зарегистрировался после того, как Start уже применил
        // восстановления — восстанавливаем его секцию сразу.
        if (_restoreApplied && TryGetSection(module.Key, out var json))
            SafeRestore(module, json);
    }

    public void UnregisterModule(ISaveModule module)
    {
        if (module == null) return;

        if (_modules.TryGetValue(module.Key, out var existing) && ReferenceEquals(existing, module))
        {
            _modules.Remove(module.Key);

            // Замораживаем последнее состояние модуля: секция останется
            // в файле и будет восстановлена при повторной регистрации.
            try
            {
                var json = module.CaptureStateJson();
                if (!string.IsNullOrEmpty(json))
                    _orphanedSections[module.Key] = json;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveService] Не удалось снять слепок модуля '{module.Key}' при выгрузке: {e.Message}");
            }
        }
    }

    public void DeleteSection(string key)
    {
        if (string.IsNullOrEmpty(key)) return;
        _orphanedSections.Remove(key);
        _modules.Remove(key);
        _dirty = true;
    }

    public void RequestSave()
    {
        _dirty = true;
    }

    public void ForceSave()
    {
        try
        {
            var sections = new JObject();
            var writtenKeys = new HashSet<string>();
            foreach (var kv in _modules)
            {
                writtenKeys.Add(kv.Key);
                try
                {
                    var stateJson = kv.Value.CaptureStateJson();
                    if (!string.IsNullOrEmpty(stateJson))
                        sections[kv.Key] = JToken.Parse(stateJson);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[SaveService] Модуль '{kv.Key}' не смог сериализовать состояние: {e.Message}");
                }
            }

            // Сиротские секции (выгруженные модули) дописываются, если их нет среди живых
            foreach (var kv in _orphanedSections)
            {
                if (writtenKeys.Contains(kv.Key) || string.IsNullOrEmpty(kv.Value)) continue;
                try
                {
                    sections[kv.Key] = JToken.Parse(kv.Value);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[SaveService] Сиротская секция '{kv.Key}' повреждена: {e.Message}");
                }
            }

            var root = new JObject
            {
                ["version"] = SaveVersion,
                ["sections"] = sections
            };

            var dir = Path.GetDirectoryName(SaveFilePath);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            var tmpPath = SaveFilePath + ".tmp";
            File.WriteAllText(tmpPath, root.ToString(Formatting.Indented));

            if (File.Exists(SaveFilePath))
                File.Replace(tmpPath, SaveFilePath, null);
            else
                File.Move(tmpPath, SaveFilePath);

            // Обновляем кэш загруженных секций, чтобы TryGetSection был консистентен
            _loadedSections = null;
            _fileLoaded = false;
            EnsureFileLoaded();

            _dirty = false;
            _autosaveTimer = 0f;
            Debug.Log($"[SaveService] Игра сохранена: {SaveFilePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveService] Ошибка сохранения: {e.Message}");
        }
    }

    public void DeleteSave()
    {
        try
        {
            if (File.Exists(SaveFilePath))
                File.Delete(SaveFilePath);
            _loadedSections = null;
            _fileLoaded = false;
            _orphanedSections.Clear();
            EnsureFileLoaded();
            Debug.Log($"[SaveService] Сохранение удалено: {SaveFilePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveService] Ошибка удаления сохранения: {e.Message}");
        }
    }

    public bool TryGetSection(string key, out string json)
    {
        EnsureFileLoaded();
        if (_loadedSections != null && _loadedSections.TryGetValue(key, out json))
            return true;

        json = null;
        return false;
    }

    private void EnsureFileLoaded()
    {
        if (_fileLoaded) return;
        _fileLoaded = true;
        _loadedSections = new Dictionary<string, string>();

        try
        {
            if (!File.Exists(SaveFilePath)) return;

            var text = File.ReadAllText(SaveFilePath);
            var root = JObject.Parse(text);
            var sections = root["sections"] as JObject;
            if (sections == null) return;

            foreach (var prop in sections.Properties())
                _loadedSections[prop.Name] = prop.Value.ToString(Formatting.None);

            Debug.Log($"[SaveService] Загружено сохранение ({_loadedSections.Count} секций): {SaveFilePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveService] Ошибка чтения сохранения: {e.Message}");
        }
    }

    private void ApplyAllRestores()
    {
        EnsureFileLoaded();
        foreach (var kv in _modules)
        {
            if (_loadedSections.TryGetValue(kv.Key, out var json))
                SafeRestore(kv.Value, json);
        }
    }

    private void SafeRestore(ISaveModule module, string json)
    {
        try
        {
            module.RestoreStateJson(json);
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveService] Модуль '{module.Key}' не смог восстановить состояние: {e.Message}");
        }
    }
}
