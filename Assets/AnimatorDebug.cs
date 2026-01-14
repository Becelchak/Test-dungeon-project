using UnityEngine;

public class AnimatorDebug : MonoBehaviour
{
    private void Awake() { Debug.Log($"{name} AnimatorDebug.Awake", this); }
    private void Start() { Debug.Log($"{name} AnimatorDebug.Start", this); }
    private void OnEnable() { Debug.Log($"{name} AnimatorDebug.OnEnable", this); }
}