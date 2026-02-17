using UnityEngine;

public interface ICameraService
{
    float OrbitSpeed { get; set; }
    PlayerCameraMode CameraMode { get; set; }
    void SetFollowMode();
    void SetOrbitMode();
    void SetTarget(Transform target);
    void SetOrbitSpeed(float speed);
    void SetZoom(float value);
    void ChangeRotateOrbitInput();
    void OrbitInputAxisOn();
    void OrbitInputAxisOff();
}
