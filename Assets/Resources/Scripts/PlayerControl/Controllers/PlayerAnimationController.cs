using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerMovementService movement;
    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float runSpeed = 8f;
    private PlayerProfileService playerProfile;
    private CharacterRotator characterRotator;

    private bool isRunning;

    private void Start()
    {
        playerProfile = (PlayerProfileService) ServiceLocator.Instance.GetService<IPlayerProfileService>();
        walkSpeed = playerProfile.CurrentProfile.speedMove;
        runSpeed = playerProfile.CurrentProfile.speedRun;
        characterRotator = GetComponent<CharacterRotator>();
    }

    private void FixedUpdate()
    {
        isRunning = movement._currentSpeed > walkSpeed;
        Vector2 localInput = movement.GetLocalMovementInput(characterRotator.rotationModel);
        float maxSpeed = isRunning ? runSpeed : walkSpeed;
        float forward = Mathf.Clamp(localInput.y / maxSpeed, -1f, 1f);
        float right = Mathf.Clamp(localInput.x / maxSpeed, -1f, 1f);

        animator.SetFloat("ForwardVelocity", forward);
        animator.SetFloat("RightVelocity", right);
        animator.SetFloat("Speed", Mathf.Clamp01(movement._currentSpeed / playerProfile.CurrentProfile.maxSpeed));
        //Debug.Log($"localInput: {localInput}, modelRot: {characterRotator.rotationModel.eulerAngles}, moveDir: {movement.MoveDirection}");
    }
}