using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] PlayerCharacter playerCharacter;
    [SerializeField] PlayerCamera playerCamera;
    [SerializeField] PlayerBodyAnimation playerBody;
    [SerializeField] PlayerHandsAnimation playerHands;

    //temp, editor
    [Space]
    [Header("Temp")]
    [SerializeField] bool melee;
    [SerializeField] GameObject gun;
    [SerializeField] Transform gunLhand, gunRHand;

    void Start()
    {
        playerCamera.Initialize(playerCharacter.camTarget);
        playerBody.Initialize();
        playerHands.Initialize();
    }

    void Update()
    {
        //playerCharacter.HandleInputs();
        playerCamera.UpdateRotation();
        HandleCharacterInput();

        var stance = playerCharacter.state.Stance is Stance.Stand or Stance.Sprint ? 1f : 0f;
        var moving = playerCharacter.state.Stance is Stance.Sprint ? 1f : 0f;
        var vel = playerCharacter.Motor.Velocity;

        playerBody.SetAnimator(stance, moving, Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"), melee ? 0 : 1);


#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out var hit))
            {
                Teleport(hit.point);
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            melee = !melee;
            playerHands.ResetHands();
            gun.SetActive(!melee);
        } 

        if (melee && Input.GetKeyDown(KeyCode.Mouse0))
        {
            //playerBody.TriggerAnimator((Random.Range(0f, 1f) > 0.5f) ? "RPunch" : "LPunch");
            playerHands.Punch();
        }

        if (!melee && Input.GetKeyDown(KeyCode.Mouse0))
        {
            playerCamera.AddRotation(-2, 1.4f, 0.1f, 1);
        }
#endif
    }

    void LateUpdate()
    {
        playerBody.UpdateRigs();
        playerHands.UpdateRigs(melee ? null : gunRHand, melee ? null : gunLhand);
        playerCamera.UpdatePosition(playerCharacter.camTarget);
        playerHands.UpdateTransform(playerCharacter.camTarget);
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
        characterInputs.Sprint = Input.GetKey(KeyCode.LeftShift);

        // Apply inputs to character
        playerCharacter.SetInputs(ref characterInputs);
    }

    public void Teleport(Vector3 position)
    {
        playerCharacter.SetPosition(position);
    }


}
