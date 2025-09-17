using UnityEngine;

public class PlayerHands : MonoBehaviour
{
    public void UpdateTransform(Transform target)
    {
        transform.position = target.position;
        transform.rotation = target.rotation;
    }
}
