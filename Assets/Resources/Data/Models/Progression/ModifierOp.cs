public enum ModifierOp
{
    Add,
    Multiply,
    Override,
    Percent
}

[System.Serializable]
public struct StatModifier
{
    public string SourceId;
    public ModifierTarget Target;
    public ModifierOp Op;
    public float Value;
}
