/// <summary>
/// Доменный модуль сохранения. Каждый сервис-владелец данных
/// (профиль, экипировка, экспедиция, память NPC и т.д.) реализует этот
/// интерфейс и регистрируется в ISaveService.
/// Сериализация секции — на усмотрение модуля (JsonUtility/Newtonsoft/...
/// ), SaveService хранит результат как JSON и вставляет в корневой файл.
/// </summary>
public interface ISaveModule
{
    /// <summary>Ключ секции в файле сохранения ("profile", "equipment", ...).</summary>
    string Key { get; }

    /// <summary>Сериализовать текущее состояние модуля в JSON. null/пусто — секция не пишется.</summary>
    string CaptureStateJson();

    /// <summary>
    /// Восстановить состояние из JSON-секции.
    /// Вызывается один раз при загрузке, если секция существует в файле.
    /// </summary>
    void RestoreStateJson(string json);
}
