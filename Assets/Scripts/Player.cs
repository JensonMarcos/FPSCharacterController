using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] PlayerCharacter playerCharacter;
    [SerializeField] PlayerCamera playerCamera;
    [SerializeField] PlayerAnimation playerAnimation;

    void Start()
    {
        playerCamera.Initialize(playerCharacter.camTarget);
        playerAnimation.Initialize();
    }

    void Update()
    {
        //playerCharacter.HandleInputs();
        playerCamera.UpdateRotation();
        HandleCharacterInput();

        var stance = playerCharacter.state.Stance == Stance.Stand ? 1f : 0f;
        var vel = playerCharacter.Motor.Velocity;

        playerAnimation.SetAnimator(stance, 0f, Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"), 0);


#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out var hit))
            {
                Teleport(hit.point);
            }
        }

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            //playerAnimation.TriggerAnimator((Random.Range(0f, 1f) > 0.5f) ? "RPunch" : "LPunch");
            playerAnimation.Punch();
        }
#endif
    }

    void LateUpdate()
    {
        playerAnimation.UpdateRigs();
        playerCamera.UpdatePosition(playerCharacter.camTarget);
    }

    private void HandleCharacterInput()
    {
        PlayerCharacterInputs characterInputs = new PlayerCharacterInputs();

        // Build the CharacterInputs struct
        characterInputs.ForwardAxis = Input.GetAxisRaw("Vertical");
        characterInputs.RightAxis = Input.GetAxisRaw("Horizontal");
        characterInputs.CameraRotation = playerCamera.transform.rotation;
        characterInputs.Jump = Input.GetKeyDown(KeyCode.Space);
        characterInputs.Crouch = Input.GetKey(KeyCode.LeftControl);

        // Apply inputs to character
        playerCharacter.SetInputs(ref characterInputs);
    }

    public void Teleport(Vector3 position)
    {
        playerCharacter.SetPosition(position);
    }


}
