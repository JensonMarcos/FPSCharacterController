using Unity.Mathematics;
using UnityEngine;

public class LateRotate : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] float weight;

    public void Rotate()
    {
        transform.rotation = Quaternion.Lerp(transform.rotation, target.rotation, weight);
    }
}
