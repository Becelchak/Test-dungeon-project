using System;
using Unity.Cinemachine;
using UnityEngine;

public class CameraService : BaseService, ICameraService
{
    [SerializeField] private GameObject cinemachineObject;
    private CinemachineCamera _followCam;
    private CinemachineInputAxisController _inputAxis;
    //private CinemachineFreeLook _orbitCam;
    public PlayerCameraMode CameraMode { get; set; }

    public float OrbitSpeed { get; set; }

    protected override void Awake()
    {
        base.Awake();
        if (cinemachineObject == null) cinemachineObject = GameObject.Find("CinemachineCamera");
        _followCam = cinemachineObject.GetComponent<CinemachineCamera>();
        _inputAxis = cinemachineObject.GetComponent<CinemachineInputAxisController>();
        SetFollowMode();
    }

    public void SetFollowMode()
    {
        CameraMode = PlayerCameraMode.Follow;
    }

    public void OrbitInputAxisOn()
    {
        if (_inputAxis != null) _inputAxis.enabled = true;
    }

    public void OrbitInputAxisOff()
    {
        if (_inputAxis != null) _inputAxis.enabled = false;
    }


    public void SetOrbitMode()
    {
        CameraMode = PlayerCameraMode.Orbit;
    }

    public void ChangeRotateOrbitInput()
    {
        _inputAxis.enabled = !_inputAxis.enabled;
    }

    public void SetOrbitSpeed(float speed)
    {

    }

    public void SetTarget(Transform target)
    {

    }

    public void SetZoom(float value)
    {

    }

    protected override Type GetServiceType() => typeof(ICameraService);

}

public enum PlayerCameraMode
{
    Follow = 0,
    Dialog = 1,
    Orbit = 2,
}
