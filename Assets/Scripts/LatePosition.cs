using UnityEngine;

public class LatePosition : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] float speed;

    void LateUpdate()
    {
        transform.position = Vector3.Lerp(transform.position, target.position, speed * Time.deltaTime);
    }
}
