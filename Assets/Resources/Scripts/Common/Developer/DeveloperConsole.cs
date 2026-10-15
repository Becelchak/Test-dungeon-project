using System;
using System.Collections.Generic;
using UnityEngine;

public class DeveloperConsole : MonoBehaviour
{
    [Tooltip("Клавиша открытия/закрытия консоли")]
    [SerializeField] private KeyCode _toggleKey = KeyCode.BackQuote;

    [Tooltip("Максимальное количество строк в истории")]
    [SerializeField] private int _maxLogLines = 50;

    private bool _isVisible;
    private string _inputText = "";
    private Vector2 _scrollPosition;
    private List<string> _logLines = new List<string>();

    private IPlayerProfileService _profileService;
    private IPlayerCombatService _combatService;
    private IInputService _inputService;
    private IProgressionService _progressionService;

    private void Start()
    {
        _profileService = ServiceLocator.Instance.GetService<IPlayerProfileService>();
        _combatService = ServiceLocator.Instance.GetService<IPlayerCombatService>();
        _inputService = ServiceLocator.Instance.GetService<IInputService>();
        _progressionService = ServiceLocator.Instance.GetService<IProgressionService>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(_toggleKey))
        {
            ToggleVisibility();
        }
        if (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return))
            SubmitCommand();
    }

    private void ToggleVisibility()
    {
        _isVisible = !_isVisible;

        if (_isVisible)
        {
            _inputService?.DisableAllInput();
            Log("Консоль разработчика открыта. Введи 'help' для списка команд.");
        }
        else
        {
            _inputService?.EnableAllInput();
        }
    }

    private void OnGUI()
    {
        if (!_isVisible) return;

        float width = Screen.width - 40f;
        float height = Screen.height * 0.35f;
        Rect windowRect = new Rect(20f, 20f, width, height);

        GUILayout.BeginArea(windowRect, GUI.skin.window);
        GUILayout.Label("Developer Console", GUI.skin.customStyles.Length > 0 ? GUI.skin.label : null);

        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(height - 70f));
        foreach (var line in _logLines)
        {
            GUILayout.Label(line);
        }
        GUILayout.EndScrollView();

        GUILayout.BeginHorizontal();
        GUI.SetNextControlName("DevConsoleInput");
        _inputText = GUILayout.TextField(_inputText, GUILayout.ExpandWidth(true));

        if (GUILayout.Button("Run", GUILayout.Width(60f)) ||
            (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return))
        {
            ExecuteCommand(_inputText);
            _inputText = "";
            Event.current.Use();
        }
        GUILayout.EndHorizontal();

        GUILayout.EndArea();

        GUI.FocusControl("DevConsoleInput");
    }

    private void SubmitCommand()
    {
        ExecuteCommand(_inputText);
        _inputText = "";
    }

    private void ExecuteCommand(string rawCommand)
    {
        string command = rawCommand.Trim();
        if (string.IsNullOrWhiteSpace(command))
            return;

        Log($"> {command}");

        string[] parts = command.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        string cmd = parts[0].ToLowerInvariant();

        try
        {
            switch (cmd)
            {
                case "help":
                    Log("Доступные команды:\n" +
                        "  heal / maxhealth — восстановить здоровье и стамину до максимума\n" +
                        "  sethealth <n> — установить здоровье\n" +
                        "  setstamina <n> — установить стамину\n" +
                        "  resetstats — сбросить здоровье и стамину до максимума\n" +
                        "  kill — убить игрока\n" +
                        "  revive — воскресить игрока и восстановить статы\n" +
                        "  godmode — переключить неуязвимость\n" +
                        "  clear — очистить консоль\n" +
                        "  clearinventory - очистить инвентарь\n" +
                        "  addxp <n> — начислить n очков опыта\n" +
                        "  setlevel <n> — выставить определенный уровень\n" +
                        "  progression — показать текущие параметры прогрессии\n" +
                        "  addtrait <id> — добавить особенность по ID\n" +
                        "  removetrait <id> — удалить особенность по ID\n" +
                        "  traits — показать активные особенности\n" +
                        "  listtraits — показать все доступные особенности\n");
                    break;

                case "heal":
                case "maxhealth":
                case "resetstats":
                    ReviveAndHeal();
                    break;

                case "sethealth":
                    if (parts.Length < 2 || !int.TryParse(parts[1], out int healthValue))
                    {
                        Log("Ошибка: укажи число. Пример: sethealth 100");
                        break;
                    }
                    SetHealth(healthValue);
                    break;

                case "setstamina":
                    if (parts.Length < 2 || !int.TryParse(parts[1], out int staminaValue))
                    {
                        Log("Ошибка: укажи число. Пример: setstamina 100");
                        break;
                    }
                    SetStamina(staminaValue);
                    break;

                case "kill":
                    KillPlayer();
                    break;

                case "revive":
                    RevivePlayer();
                    break;

                case "godmode":
                    ToggleGodMode();
                    break;

                case "clear":
                    _logLines.Clear();
                    break;

                case "clearinventory":
                    ClearPlayerInventory();
                    break;

                case "addxp":
                    if (parts.Length < 2 || !int.TryParse(parts[1], out int xpValue))
                    {
                        Log("Ошибка: укажи число. Пример: addxp 100");
                        break;
                    }
                    AddXP(xpValue);
                    break;

                case "setlevel":
                    if (parts.Length < 2 || !int.TryParse(parts[1], out int levelValue))
                    {
                        Log("Ошибка: укажи число. Пример: setLevel 5");
                        break;
                    }
                    SetLevel(levelValue);
                    break;

                case "progression":
                    Log($"Текущий уровень {_progressionService.CurrentLevel}, накоплено {_progressionService.CurrentXP} опыта. До следующего уровня требуется {_progressionService.XPToNextLevel} опыта. Не потрачено {_progressionService.SkillPoints} очков навыка");
                    break;

                case "addtrait":
                    if (parts.Length < 2)
                    {
                        Log("Ошибка: укажи ID особенности. Пример: addtrait lettered");
                        break;
                    }
                    AddTrait(parts[1]);
                    break;

                case "removetrait":
                    if (parts.Length < 2)
                    {
                        Log("Ошибка: укажи ID особенности. Пример: removetrait lettered");
                        break;
                    }
                    RemoveTrait(parts[1]);
                    break;

                case "traits":
                    ShowActiveTraits();
                    break;

                case "listtraits":
                    ListAllTraits();
                    break;

                default:
                    Log($"Неизвестная команда: {cmd}. Введи 'help' для списка.");
                    break;
            }
        }
        catch (Exception e)
        {
            Log($"Ошибка выполнения команды: {e.Message}");
        }
    }

    private void AddXP(int value)
    {
        _progressionService.AddXP(value, XPSource.Console);
    }

    private void SetLevel(int level)
    {
        _progressionService.ResetToLevel(level);
    }

    private void AddTrait(string traitId)
    {
        _progressionService.AddTrait(traitId);
    }

    private void RemoveTrait(string traitId)
    {
        _progressionService.RemoveTrait(traitId);
    }

    private void ShowActiveTraits()
    {
        var activeTraits = _progressionService.GetActiveTraits();
        if (activeTraits.Count == 0)
        {
            Log("Активных особенностей нет");
            return;
        }

        Log($"Активные особенности ({activeTraits.Count}):");
        foreach (var trait in activeTraits)
        {
            Log($"  - {trait.displayName} ({trait.traitId})");
        }
    }

    private void ListAllTraits()
    {
        Log("Функция listtraits требует доступа к TraitDatabase напрямую");
        Log("Используйте addtrait <id> для добавления конкретной особенности");
    }

    private void ClearPlayerInventory()
    {
        var profile = _profileService?.CurrentProfile;
        if (profile == null)
        {
            Log("Ошибка: профиль игрока не найден.");
            return;
        }
        _profileService.ClearPlayerInventory();
        Debug.LogWarning("Инвентарь очищен!");
    }

    private void ReviveAndHeal()
    {
        var profile = _profileService?.CurrentProfile;
        if (profile == null)
        {
            Log("Ошибка: профиль игрока не найден.");
            return;
        }

        int healthDelta = profile.maxHealth - profile.health;
        int staminaDelta = profile.maxStamina - profile.stamina;
        _profileService.ModifyHealth(healthDelta);
        _profileService.ModifyStamina(staminaDelta);
        Log($"Восстановлено: HP={profile.maxHealth}, Stamina={profile.maxStamina}");
    }

    private void SetHealth(int value)
    {
        var profile = _profileService?.CurrentProfile;
        if (profile == null)
        {
            Log("Ошибка: профиль игрока не найден.");
            return;
        }

        int delta = Mathf.Clamp(value, 0, profile.maxHealth) - profile.health;
        _profileService.ModifyHealth(delta);
        Log($"Здоровье установлено: {profile.health}/{profile.maxHealth}");
    }

    private void SetStamina(int value)
    {
        var profile = _profileService?.CurrentProfile;
        if (profile == null)
        {
            Log("Ошибка: профиль игрока не найден.");
            return;
        }

        _profileService.SetMaxStamina(value);
        int delta = Mathf.Clamp(value, 0, profile.maxStamina) - profile.stamina;
        _profileService.ModifyStamina(delta);
        Log($"Стамина установлена: {profile.stamina}/{profile.maxStamina}");
    }

    private void KillPlayer()
    {
        var profile = _profileService?.CurrentProfile;
        if (profile == null)
        {
            Log("Ошибка: профиль игрока не найден.");
            return;
        }

        bool previousGodMode = _combatService?.IsGodMode ?? false;
        if (_combatService != null)
            _combatService.IsGodMode = false;

        _profileService.ModifyHealth(-profile.health);
        _combatService?.ApplyDamage(new DamageInfo()
        {
            BaseDamage = 1,
            DamageMultiplier = 1,
            DamageType = DamageType.True,

        });

        if (_combatService != null)
            _combatService.IsGodMode = previousGodMode;

        Log("Игрок убит.");
    }

    private void RevivePlayer()
    {
        var stateMachine = UnityEngine.Object.FindObjectOfType<PlayerStateMachine>();
        if (stateMachine != null)
            stateMachine.RevivePlayer();
        else
            _combatService?.Revive();

        Log("Игрок воскрешён.");
    }

    private void ToggleGodMode()
    {
        if (_combatService == null)
        {
            Log("Ошибка: боевой сервис не найден.");
            return;
        }

        bool newState = !_combatService.IsGodMode;
        _combatService.IsGodMode = newState;
        Log($"GodMode: {(newState ? "ON" : "OFF")}");
    }

    private void Log(string message)
    {
        string line = $"[{Time.time:F2}] {message}";
        _logLines.Add(line);
        if (_logLines.Count > _maxLogLines)
            _logLines.RemoveAt(0);

        _scrollPosition = new Vector2(0, float.MaxValue);
        Debug.Log($"[DevConsole] {message}");
    }
}
