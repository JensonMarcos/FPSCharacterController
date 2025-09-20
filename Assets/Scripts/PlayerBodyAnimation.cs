using System.Collections;
using DitzelGames.FastIK;
using UnityEngine;

[System.Serializable]
public struct LookRig
{
    public Transform bone;
    public float weight;
    public bool x, y, z;
}

public class PlayerBodyAnimation : MonoBehaviour
{
    public Animator bodyAnimator;

    [Header("Legs")]
    public float Stance;
    public float Moving;
    public float Horizontal;
    public float Vertical;
    public float IdleState;

    [SerializeField] float moveResponse;
    [SerializeField] float crouchResponse;


    [Header("Rig")]
    [SerializeField] Transform cam;
    [SerializeField] LookRig[] aimRig;

    [SerializeField] Transform upperBodyBone;
    public float UpperBodyTilt;


    public void Initialize()
    {
        //
    }

    void Update()
    {
        bodyAnimator.SetFloat("Stance", Mathf.Lerp(bodyAnimator.GetFloat("Stance"), Stance, crouchResponse * Time.deltaTime));
        bodyAnimator.SetFloat("Moving", Mathf.Lerp(bodyAnimator.GetFloat("Moving"), Moving, moveResponse * Time.deltaTime));
        bodyAnimator.SetFloat("Horizontal", Mathf.Lerp(bodyAnimator.GetFloat("Horizontal"), Horizontal, moveResponse * Time.deltaTime));
        bodyAnimator.SetFloat("Vertical", Mathf.Lerp(bodyAnimator.GetFloat("Vertical"), Vertical, moveResponse * Time.deltaTime));
        bodyAnimator.SetFloat("IdleState", Mathf.Lerp(bodyAnimator.GetFloat("IdleState"), IdleState, 10 * Time.deltaTime));
    }

    public void UpdateRigs()
    {   
        upperBodyBone.transform.rotation *= Quaternion.Euler(0, UpperBodyTilt, 0);

        foreach (var rig in aimRig)
        {
            var newRot = Quaternion.Lerp(rig.bone.transform.rotation, cam.rotation, rig.weight).eulerAngles;
            var oldRot = rig.bone.transform.eulerAngles;

            rig.bone.transform.rotation = Quaternion.Euler(rig.x ? newRot.x : oldRot.x, rig.y ? newRot.y : oldRot.y, rig.z ? newRot.z : oldRot.z);
        }

        //RHand.IK.Weight = LHand.IK.Weight = HandIKWeight;

        
    }

    public void SetAnimator(float _stance, float _moving, float x, float y, float idle)
    {
        Stance = _stance;
        Moving = _moving;
        Horizontal = x;
        Vertical = y;
        IdleState = idle;
    }

    public void TriggerAnimator(string name)
    {
        bodyAnimator.SetTrigger(name);
    }
}
