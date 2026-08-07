import re

path = "F:/Unity projects/Test dungeon project/Assets/Resources/Scripts/PlayerControl/Controllers/WeaponIKController.cs"

with open(path, 'r', encoding='utf-8') as f:
    content = f.read()

# 1. Add idleReturnDuration field
if "idleReturnDuration" not in content:
    old_block = '''    [Header("IK Smoothing")]
    [Tooltip("Скорость нарастания/спадания веса IK")]
    [SerializeField] private float weightLerpSpeed = 7f;

    [Header("Runtime")]'''
    new_block = '''    [Header("IK Smoothing")]
    [Tooltip("Скорость нарастания/спадания веса IK")]
    [SerializeField] private float weightLerpSpeed = 7f;
    [Tooltip("Время плавного возвращения оружия в IDLE-позу после атаки")]
    [SerializeField] private float idleReturnDuration = 0.15f;

    [Header("Runtime")]'''
    content = content.replace(old_block, new_block)
    print("Added idleReturnDuration field")
else:
    print("idleReturnDuration already exists")

# 2. Replace SetAttackMode method using regex
pattern = r'(    /// <summary>\n    /// Переключает режим для двуручного оружия: idle \(обе руки на хвате\) или атака \(правая рука по анимации\).\n    /// </summary>\n    /// <param name="instantly">Если true, веса IK меняются мгновенно\. Рекомендуется true при входе в атаку\.</param>\n    public void SetAttackMode\(bool isAttacking, bool instantly = false\)\n    \{\n        if \(_currentWeaponData == null \|\| _currentWeaponData\.handling != WeaponHandling\.BothHands\)\n            return;\n\n        if \(weaponIdleParent == null\)\n            return;\n\n        _isAttacking = isAttacking;\n\n        // Меняем родителя оружия без скачка позиции/поворота в мировых координатах\n        Transform newParent = isAttacking \? weaponHolder_R : weaponIdleParent;\n        if \(_currentWeaponObject != null && newParent != null\)\n        \{\n            _currentWeaponObject\.transform\.SetParent\(newParent, worldPositionStays: true\);\n        \}\n\n        var \(rightWeight, leftWeight\) = GetTargetWeightsFor\(WeaponHandling\.BothHands\);\n        SetIKWeights\(rightWeight, leftWeight, instantly\);\n    \})'

new_method = '''    /// <summary>
    /// Переключает режим для двуручного оружия: idle (обе руки на хвате) или атака (правая рука по анимации).
    /// </summary>
    /// <param name="instantly">Если true, веса IK меняются мгновенно. Рекомендуется true при входе в атаку.</param>
    public void SetAttackMode(bool isAttacking, bool instantly = false)
    {
        if (_currentWeaponData == null || _currentWeaponData.handling != WeaponHandling.BothHands)
            return;

        if (weaponIdleParent == null)
            return;

        _isAttacking = isAttacking;

        if (_transitionCoroutine != null)
        {
            StopCoroutine(_transitionCoroutine);
            _transitionCoroutine = null;
        }

        Transform newParent = isAttacking ? weaponHolder_R : weaponIdleParent;
        if (_currentWeaponObject != null && newParent != null)
        {
            // Сохраняем мировую позу, чтобы не было скачка при смене родителя
            _currentWeaponObject.transform.SetParent(newParent, worldPositionStays: true);
        }

        var (rightWeight, leftWeight) = GetTargetWeightsFor(WeaponHandling.BothHands);
        SetIKWeights(rightWeight, leftWeight, instantly);

        // При выходе из атаки плавно возвращаем оружие в IDLE-позу
        if (!isAttacking && _currentWeaponObject != null)
        {
            _transitionCoroutine = StartCoroutine(TransitionToIdlePose());
        }
    }

    private System.Collections.IEnumerator TransitionToIdlePose()
    {
        if (_currentWeaponObject == null || _currentWeaponData == null)
            yield break;

        var t = _currentWeaponObject.transform;
        Vector3 startLocalPos = t.localPosition;
        Quaternion startLocalRot = t.localRotation;

        Vector3 targetLocalPos = _currentWeaponData.weaponHolderOffset;
        Quaternion targetLocalRot = Quaternion.Euler(_currentWeaponData.weaponHolderRotationOffsetEuler);

        float elapsed = 0f;
        while (elapsed < idleReturnDuration)
        {
            elapsed += Time.deltaTime;
            float normalized = Mathf.Clamp01(elapsed / idleReturnDuration);
            // Плавное замедление в конце
            float tValue = normalized * normalized * (3f - 2f * normalized);

            t.localPosition = Vector3.Lerp(startLocalPos, targetLocalPos, tValue);
            t.localRotation = Quaternion.Slerp(startLocalRot, targetLocalRot, tValue);

            yield return null;
        }

        t.localPosition = targetLocalPos;
        t.localRotation = targetLocalRot;
        _transitionCoroutine = null;
    }'''

if re.search(pattern, content):
    content = re.sub(pattern, new_method, content)
    print("Replaced SetAttackMode method")
else:
    print("WARNING: Could not find SetAttackMode method to replace")

with open(path, 'w', encoding='utf-8', newline='\r\n') as f:
    f.write(content)

print("Done")
