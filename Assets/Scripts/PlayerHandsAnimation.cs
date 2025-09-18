using UnityEngine;
using System.Collections;
using DitzelGames.FastIK;

[System.Serializable]
public struct HandRig
{
    public Transform hand;
    public FastIKFabric IK;
    public Vector3 startPos;
    public Quaternion startRot;
}

public class PlayerHandsAnimation : MonoBehaviour
{
    [Header("Rig")]
    public HandRig RHand, LHand;
    [Range(0, 1)]
    public float HandIKWeight;
    [SerializeField] Transform HandParent;


    [Header("Punch")]
    [SerializeField] Transform punchTarget;
    [SerializeField] float punchSpeed;
    [SerializeField] float punchHoldTime;
    bool whichhand;
    [SerializeField] PlayerBodyAnimation body;
    [SerializeField] float tiltAmount;

    public void Initialize()
    {
        RHand.startPos = RHand.hand.localPosition;
        RHand.startRot = RHand.hand.localRotation;
        LHand.startPos = LHand.hand.localPosition;
        LHand.startRot = LHand.hand.localRotation;
    }

    public void UpdateRigs()
    {
        RHand.IK.Weight = LHand.IK.Weight = HandIKWeight;
    }

    public void UpdateTransform(Transform target)
    {
        transform.position = target.position;
        transform.rotation = target.rotation;
    }

    public void Punch()
    {
        StopCoroutine("PunchAnimation");
        
        LHand.hand.transform.localPosition = LHand.startPos;
        LHand.hand.transform.localRotation = LHand.startRot;
        RHand.hand.transform.localPosition = RHand.startPos;
        RHand.hand.transform.localRotation = RHand.startRot;

        body.UpperBodyTilt = 0f;

        whichhand = !whichhand;

        StartCoroutine(PunchAnimation(whichhand ? RHand : LHand, whichhand ? -1 : 1));
    }

    public IEnumerator PunchAnimation(HandRig rig, int tiltMult)
    {
        Vector3 punchPos;
        Quaternion punchRot;

        var t = 0f;
        var x = 0f;
        while (x < 1)
        {
            x += punchSpeed * Time.deltaTime;
            //t = 1 - Mathf.Cos((x * Mathf.PI) / 2); //ease in lerping function
            t = 2.70158f * x * x * x - 1.70158f * x * x;

            punchPos = HandParent.InverseTransformPoint(punchTarget.position);
            punchRot = Quaternion.Inverse(HandParent.rotation) * punchTarget.rotation;

            rig.hand.transform.localPosition = Vector3.LerpUnclamped(rig.startPos, punchPos, t);
            rig.hand.transform.localRotation = Quaternion.Lerp(rig.startRot, punchRot, x);

            body.UpperBodyTilt = Mathf.Lerp(0, tiltAmount * tiltMult, t);
            yield return null;
        }

        yield return new WaitForSeconds(punchHoldTime);

        // punchPos = HandParent.InverseTransformPoint(rig.IK.transform.position);
        // punchRot = Quaternion.Inverse(HandParent.rotation) * rig.IK.transform.rotation;

        t = 1f;
        x = 1f;
        while (x > 0)
        {
            x -= punchSpeed * Time.deltaTime;
            t = -(Mathf.Cos(Mathf.PI * x) - 1) / 2; //ease in lerping function

            punchPos = HandParent.InverseTransformPoint(punchTarget.position);
            punchRot = Quaternion.Inverse(HandParent.rotation) * punchTarget.rotation;

            rig.hand.transform.localPosition = Vector3.LerpUnclamped(rig.startPos, punchPos, t);
            rig.hand.transform.localRotation = Quaternion.Lerp(rig.startRot, punchRot, x);

            body.UpperBodyTilt = Mathf.Lerp(0, tiltAmount * tiltMult, t);
            yield return null;
        }

        rig.hand.transform.localPosition = rig.startPos;
        rig.hand.transform.localRotation = rig.startRot;

        body.UpperBodyTilt = 0f;

    }
}
