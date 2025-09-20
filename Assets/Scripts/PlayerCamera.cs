using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public float sensitivity = 1f;
    public Vector3 realRotation;
    //public Camera cam;

    [Header("Camera Animations")]
    public Vector3 targetRot;
    [SerializeField] Vector3 offsetRot;

    public float snap, returnSpeed;


    public void Initialize(Transform target)
    {
        transform.position = target.position;
        transform.rotation = target.rotation;
        realRotation = transform.eulerAngles;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void UpdateRotation()
    {
        float xMovement = Input.GetAxisRaw("Mouse X") * sensitivity;// * ADSsensitivity;
        float yMovement = -Input.GetAxisRaw("Mouse Y") * sensitivity;// * ADSsensitivity;

        // Calculate rotation from input
        realRotation = new Vector3(Mathf.Clamp(realRotation.x + yMovement, -90f, 90f), realRotation.y + xMovement, 0);

        //cam offset
        targetRot = Vector3.Lerp(targetRot, Vector3.zero, returnSpeed * Time.deltaTime);
        offsetRot = Vector3.Slerp(offsetRot, targetRot, snap * Time.fixedDeltaTime);

        //Apply rotation to body
        transform.eulerAngles = realRotation + offsetRot;
    }

    public void UpdatePosition(Transform target)
    {
        transform.position = target.position;
    }

    public void AddRotation(float x, float y, float z, float mult)
    {
        targetRot += new Vector3(x, y, z) * mult;
    }
}
