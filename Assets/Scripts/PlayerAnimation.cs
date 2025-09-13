using System.Collections;
using DitzelGames.FastIK;
using UnityEngine;

[System.Serializable]
public struct LookRig
{
    public Transform bone;
    public float weight;
}

[System.Serializable]
public struct HandRig
{
    public Transform hand;
    public FastIKFabric IK;
    public Vector3 startPos;
    public Quaternion startRot;
}

public class PlayerAnimation : MonoBehaviour
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

    public HandRig RHand, LHand;
    [Range(0, 1)]
    public float HandIKWeight;
    [SerializeField] Transform HandParent;
    

    [Header("Punch")]
    [SerializeField] Transform punchTarget, punchTransform;
    [SerializeField] float punchSpeed;
    [SerializeField] float punchHoldTime;
    bool whichhand;

    public void Initialize()
    {
        RHand.startPos = RHand.hand.localPosition;
        RHand.startRot = RHand.hand.localRotation;
        LHand.startPos = LHand.hand.localPosition;
        LHand.startRot = LHand.hand.localRotation;
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
        foreach (var rig in aimRig)
        {
            rig.bone.transform.rotation = Quaternion.Lerp(rig.bone.transform.rotation, cam.rotation, rig.weight);
        }

        RHand.IK.Weight = LHand.IK.Weight = HandIKWeight;
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

    public void Punch()
    {
        StopCoroutine("PunchAnimation");
        
        LHand.hand.transform.localPosition = LHand.startPos;
        LHand.hand.transform.localRotation = LHand.startRot;
        RHand.hand.transform.localPosition = RHand.startPos;
        RHand.hand.transform.localRotation = RHand.startRot;

        whichhand = !whichhand;

        StartCoroutine(PunchAnimation(whichhand ? RHand : LHand));
    }

    public IEnumerator PunchAnimation(HandRig rig)
    {
        var t = 0f;
        var x = 0f;
        while (x < 1)
        {
            x += punchSpeed * Time.deltaTime;
            //t = 1 - Mathf.Cos((x * Mathf.PI) / 2); //ease in lerping function
            t = 2.70158f * x * x * x - 1.70158f * x * x;

            punchTransform.position = punchTarget.position;
            punchTransform.rotation = punchTarget.rotation;

            rig.hand.transform.localPosition = Vector3.LerpUnclamped(rig.startPos, punchTransform.localPosition, t);
            rig.hand.transform.localRotation = Quaternion.Lerp(rig.startRot, punchTransform.localRotation, x);
            yield return null;
        }

        yield return new WaitForSeconds(punchHoldTime);

        var punchPos = HandParent.InverseTransformPoint(rig.IK.transform.position);
        var punchRot = Quaternion.Inverse(HandParent.rotation) * rig.IK.transform.rotation;

        t = 1f;
        x = 1f;
        while (x > 0)
        {
            x -= punchSpeed * Time.deltaTime;
            t = -(Mathf.Cos(Mathf.PI * x) - 1) / 2; //ease in lerping function

            rig.hand.transform.localPosition = Vector3.LerpUnclamped(rig.startPos, punchPos, t);
            rig.hand.transform.localRotation = Quaternion.Lerp(rig.startRot, punchRot, x);
            yield return null;
        }

        rig.hand.transform.localPosition = rig.startPos;
        rig.hand.transform.localRotation = rig.startRot;

    }

     
}
