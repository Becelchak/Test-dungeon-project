/// <summary>
/// Центральный сервис сохранений. Хранит единый файл savegame.json
/// в Application.persistentDataPath, собирая секции от зарегистрированных
/// ISaveModule. Поддерживает dirty-флаг, автосохранение и атомарную запись.
/// </summary>
public interface ISaveService
{
    /// <summary>Путь к файлу сохранения (для отладки/отображения).</summary>
    string SaveFilePath { get; }

    /// <summary>
    /// Зарегистрировать доменный модуль. Если в файле есть его секция —
    /// RestoreStateJson будет вызван сразу или при старте SaveService
    /// (в зависимости от момента регистрации).
    /// </summary>
    void RegisterModule(ISaveModule module);

    /// <summary>
    /// Убрать модуль из сохранения (например, домен завершился или его сервис уничтожен).
    /// При следующем ForceSave его секция будет удалена из файла.
    /// </summary>
    void UnregisterModule(ISaveModule module);

    /// <summary>Пометить сохранение «грязным» — будет записано на ближайшем автосейве/выходе.</summary>
    void RequestSave();

    /// <summary>Немедленно записать все модули на диск.</summary>
    void ForceSave();

    /// <summary>Удалить файл сохранения целиком (новая игра / полный сброс).</summary>
    void DeleteSave();

    /// <summary>Удалить конкретную секцию (сиротскую или модульную) из файла.</summary>
    void DeleteSection(string key);

    /// <summary>Получить сырую JSON-секцию из загруженного файла (null — секции нет).</summary>
    bool TryGetSection(string key, out string json);
}
