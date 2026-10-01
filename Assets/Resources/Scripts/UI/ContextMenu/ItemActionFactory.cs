using System.Collections.Generic;
using UnityEngine;

public static class ItemActionFactory
{
    private static readonly List<IItemAction> _availableActions = new List<IItemAction>
    {
        new EquipItemAction(),
        new ConsumeItemAction(),
        new DropItemAction()
    };

    public static List<IItemAction> GetActionsForItem(ItemData item)
    {
        var actions = new List<IItemAction>();

        foreach (var action in _availableActions)
        {
            if (action.CanExecute(item))
            {
                actions.Add(action);
            }
        }

        return actions;
    }
}
