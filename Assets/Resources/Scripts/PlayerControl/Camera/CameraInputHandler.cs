using UnityEngine;
using UnityEngine.InputSystem;

public class CameraInputHandler : MonoBehaviour
{
    [SerializeField] private float sensitivity;
    private InputActionAsset _inputActions;
    private ICameraService _cameraService;
    private InputAction _orbitAction;
    private InputAction _zoomAction;

    private void Start()
    {
        _inputActions = InputSystem.actions;
        _cameraService = ServiceLocator.Instance.GetService<ICameraService>();

        _orbitAction = _inputActions.FindAction("Look"); 
        _zoomAction = _inputActions.FindAction("Zoom"); ;

        _orbitAction.performed += ctx => StartOrbit();
        _orbitAction.canceled += ctx => StopOrbit();

        _zoomAction.performed += ctx => HandleZoomInput(ctx.ReadValue<Vector2>().y);
    }

    private void StartOrbit()
    {
        _cameraService.OrbitInputAxisOn();
    }

    private void StopOrbit()
    {
        _cameraService.OrbitInputAxisOff();
    }

    private void HandleZoomInput(float scale)
    {

    }
}
