using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EquipmentService : BaseService, IEquipmentService
{
    [Header("Слоты экипировки")]
    [Tooltip("Список слотов. Если пуст, будет создана стандартная раскладка.")]
    [SerializeField] private List<EquipmentSlot> _slots = new List<EquipmentSlot>();
    [Tooltip("Индекс активного оружейного слота (0..2)")]
    [SerializeField] private int _activeWeaponSlotIndex = 0;
    private int _previousWeaponSlotIndex = 0;

    public IReadOnlyList<EquipmentSlot> Slots => _slots;
    public int ActiveWeaponSlotIndex => _activeWeaponSlotIndex;

    public WeaponData CurrentWeapon
    {
        get
        {
            var activeSlot = GetSlot(EquipmentSlotType.Weapon1 + _activeWeaponSlotIndex);
            if (activeSlot != null && activeSlot.IsOccupied)
                return activeSlot.Item as WeaponData;
            Debug.Log("Слот пуст!");
            // Если активный слот пуст, берём первое занятое оружейное слот
            //var weaponSlot = _slots
            //    .FirstOrDefault(s => s.allowedItemType == ItemType.Weapon && s.IsOccupied);
            return GetSlot(EquipmentSlotType.Weapon1 + _previousWeaponSlotIndex).Item as WeaponData;
        }
    }

    public WeaponData CurrentShield
    {
        get
        {
            foreach (var slot in _slots)
            {
                if (slot.slotType < EquipmentSlotType.Weapon1 || slot.slotType > EquipmentSlotType.Weapon3)
                    continue;

                if (slot.Item is WeaponData weapon && weapon.handling == WeaponHandling.OffHand)
                    return weapon;
            }
            return null;
        }
    }

    public event Action<EquipmentSlotType, ItemData> OnEquipmentChanged;
    public event Action<WeaponData> OnWeaponChanged;
    public event Action<WeaponData> OnShieldChanged;

    protected override Type GetServiceType() => typeof(IEquipmentService);

    private void Awake()
    {
        base.Awake();
        InitializeSlots();

        // По умолчанию экипируем руки
        var defaultWeapon = Resources.Load<WeaponData>("Data/ScriptableObjects/Weapons/Unarmed");
        if (defaultWeapon != null)
            Equip(defaultWeapon, EquipmentSlotType.Weapon1);
        // Заглушка для теста смены оружия
        var anotherWeapon = Resources.Load<WeaponData>("Data/ScriptableObjects/Weapons/Claymor");
        if (anotherWeapon != null)
            Equip(anotherWeapon, EquipmentSlotType.Weapon2);
        anotherWeapon = Resources.Load<WeaponData>("Data/ScriptableObjects/Weapons/Boardsword");
        if (anotherWeapon != null)
            Equip(anotherWeapon, EquipmentSlotType.Weapon3);
    }

    private void Start()
    {
        var input = ServiceLocator.Instance.GetService<IInputService>();
        if (input != null)
            input.OnSwitchWeaponSlot += HandleSwitchWeaponSlot;
    }

    private void OnDestroy()
    {
        if (ServiceLocator.Instance != null)
        {
            var input = ServiceLocator.Instance.GetService<IInputService>();
            if (input != null)
                input.OnSwitchWeaponSlot -= HandleSwitchWeaponSlot;
        }
        base.OnDestroy();
    }

    private void HandleSwitchWeaponSlot(int slotIndex)
    {
        SetActiveWeaponSlot(slotIndex);
    }

    private void InitializeSlots()
    {
        if (_slots != null && _slots.Count > 0)
            return;

        _slots = new List<EquipmentSlot>
        {
            new EquipmentSlot(EquipmentSlotType.Weapon1, ItemType.Weapon),
            new EquipmentSlot(EquipmentSlotType.Weapon2, ItemType.Weapon),
            new EquipmentSlot(EquipmentSlotType.Weapon3, ItemType.Weapon),
            new EquipmentSlot(EquipmentSlotType.Trinket1, ItemType.Trinket, true),
            new EquipmentSlot(EquipmentSlotType.Trinket2, ItemType.Trinket, true),
            new EquipmentSlot(EquipmentSlotType.Trinket3, ItemType.Trinket, false),
            new EquipmentSlot(EquipmentSlotType.Head, ItemType.Armor),
            new EquipmentSlot(EquipmentSlotType.Body, ItemType.Armor),
            new EquipmentSlot(EquipmentSlotType.Legs, ItemType.Armor),
        };
    }

    public bool Equip(ItemData item, EquipmentSlotType slotType)
    {
        if (item == null)
            return false;

        var slot = _slots.FirstOrDefault(s => s.slotType == slotType);
        if (slot == null)
        {
            Debug.LogWarning($"[EquipmentService] Слот {slotType} не найден.");
            return false;
        }

        if (item is WeaponData weaponData && weaponData.handling == WeaponHandling.OffHand)
        {
            if (slotType < EquipmentSlotType.Weapon1 || slotType > EquipmentSlotType.Weapon3)
            {
                Debug.LogWarning($"[EquipmentService] Щит {item.displayName} можно экипировать только в оружейный слот.");
                return false;
            }
        }

        var previousWeapon = CurrentWeapon;
        var previousShield = CurrentShield;

        if (!slot.Equip(item))
        {
            Debug.LogWarning($"[EquipmentService] Невозможно экипировать {item.displayName} в слот {slotType}.");
            return false;
        }

        Debug.Log($"[EquipmentService] Экипировано {item.displayName} в слот {slotType}.");

        OnEquipmentChanged?.Invoke(slotType, item);

        var newWeapon = CurrentWeapon;
        if (newWeapon != previousWeapon)
            OnWeaponChanged?.Invoke(newWeapon);

        var newShield = CurrentShield;
        if (newShield != previousShield)
            OnShieldChanged?.Invoke(newShield);

        return true;
    }

    public bool Unequip(EquipmentSlotType slotType)
    {
        var slot = _slots.FirstOrDefault(s => s.slotType == slotType);
        if (slot == null || !slot.IsOccupied)
            return false;

        var previousWeapon = CurrentWeapon;
        var previousShield = CurrentShield;
        var removedItem = slot.Unequip();

        Debug.Log($"[EquipmentService] Снято {removedItem.displayName} из слота {slotType}.");

        OnEquipmentChanged?.Invoke(slotType, null);

        var newWeapon = CurrentWeapon;
        if (newWeapon != previousWeapon)
            OnWeaponChanged?.Invoke(newWeapon);

        var newShield = CurrentShield;
        if (newShield != previousShield)
            OnShieldChanged?.Invoke(newShield);

        return true;
    }

    public void SetActiveWeaponSlot(int index)
    {
        if (index < 0 || index > 2)
        {
            Debug.LogWarning($"[EquipmentService] Неверный индекс оружейного слота: {index}. Допустимые значения: 0..2.");
            return;
        }

        var selectedItem = GetItemInSlot(EquipmentSlotType.Weapon1 + index);
        if (selectedItem is WeaponData selectedWeapon && selectedWeapon.handling == WeaponHandling.OffHand)
        {
            Debug.Log("[EquipmentService] Слот со щитом нельзя сделать активным оружием.");
            return;
        }

        if (_activeWeaponSlotIndex == index)
            return;

        _previousWeaponSlotIndex = _activeWeaponSlotIndex;
        _activeWeaponSlotIndex = index;
        OnEquipmentChanged?.Invoke(EquipmentSlotType.Weapon1 + index, selectedItem);
        OnWeaponChanged?.Invoke(CurrentWeapon);
        OnShieldChanged?.Invoke(CurrentShield);
    }

    public ItemData GetItemInSlot(EquipmentSlotType slotType)
    {
        return GetSlot(slotType)?.Item;
    }

    /// <summary>
    /// Сериализует текущую экипировку в JSON.
    /// </summary>
    public string SaveToJson()
    {
        var saveData = new EquipmentSaveData
        {
            activeWeaponSlotIndex = _activeWeaponSlotIndex,
            previousWeaponSlotIndex = _previousWeaponSlotIndex,
            slots = _slots.Select(s => new SlotSaveData
            {
                slotType = s.slotType,
                itemId = s.Item != null ? s.Item.itemId : string.Empty,
                isUnlocked = s.isUnlocked
            }).ToList()
        };

        return JsonUtility.ToJson(saveData, true);
    }

    /// <summary>
    /// Восстанавливает экипировку из JSON. Предметы ищутся по itemId среди всех ItemData в Resources.
    /// </summary>
    public void LoadFromJson(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            Debug.LogWarning("[EquipmentService] Пустая строка JSON, загрузка экипировки отменена.");
            return;
        }

        var saveData = JsonUtility.FromJson<EquipmentSaveData>(json);
        if (saveData == null)
        {
            Debug.LogWarning("[EquipmentService] Не удалось распарсить JSON экипировки.");
            return;
        }

        var allItems = Resources.LoadAll<ItemData>("");
        var itemById = allItems.ToDictionary(i => i.itemId, i => i);

        var previousWeapon = CurrentWeapon;
        var previousShield = CurrentShield;

        foreach (var slotSave in saveData.slots)
        {
            var slot = GetSlot(slotSave.slotType);
            if (slot == null)
                continue;

            slot.isUnlocked = slotSave.isUnlocked;
            slot.Unequip();

            if (!string.IsNullOrEmpty(slotSave.itemId) && itemById.TryGetValue(slotSave.itemId, out var item))
            {
                slot.Equip(item);
            }
        }

        _activeWeaponSlotIndex = Mathf.Clamp(saveData.activeWeaponSlotIndex, 0, 2);
        _previousWeaponSlotIndex = Mathf.Clamp(saveData.previousWeaponSlotIndex, 0, 2);

        foreach (var slot in _slots)
            OnEquipmentChanged?.Invoke(slot.slotType, slot.Item);

        var newWeapon = CurrentWeapon;
        if (newWeapon != previousWeapon)
            OnWeaponChanged?.Invoke(newWeapon);

        var newShield = CurrentShield;
        if (newShield != previousShield)
            OnShieldChanged?.Invoke(newShield);
    }

    private EquipmentSlot GetSlot(EquipmentSlotType slotType)
    {
        return _slots.FirstOrDefault(s => s.slotType == slotType);
    }
}
