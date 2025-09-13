using UnityEngine;

public class LateTransform : MonoBehaviour
{
    [SerializeField] Transform target;
    void Update()
    {
        transform.position = target.position;
        transform.rotation = target.rotation;
    }

    void LateUpdate()
    {
        transform.position = target.position;
        transform.rotation = target.rotation;
    }
}
