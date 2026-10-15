using UnityEngine;

public struct InteractionOption
{
    public string Id;              // "pickup", "examine", "use"
    public string Label;           // "Подобрать"
    public KeyCode Hotkey;         // или InputAction reference
}
