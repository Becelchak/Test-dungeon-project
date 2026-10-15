using System;

public interface IArchetypeService
{
    ArchetypeData Current { get; }

    void SetArchetype(ArchetypeData data);

    event Action<ArchetypeData> OnArchetypeChanged;
}
