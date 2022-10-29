using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKController : MonoBehaviour
{
    private const float BODY_TILT_RATE = 50;
    [SerializeField]
    private float modelSholderHeight = 1.35f / 4;
    [SerializeField]
    private float standardPosition = 0;

    [System.Serializable]
    public struct BodyBones<Type>
    {
        public Type LeftHand;
        public Type RightHand;
        public Type LeftElbow;
        public Type RightElbow;
        public Type LeftShoulder;
        public Type RightShoulder;
        public Type Spine;
        public Type LeftFoot;
        public Type RightFoot;
        public Type LeftUpperLeg;
        public Type RightUpperLeg;
    }
    [SerializeField] private BodyBones<Transform> ikBodyBones;
    private BodyBones<Vector3> ikLastBones;
    private BodyBones<Transform> modelBodyBones;
    [SerializeField] private BodyBones<Vector3> boneCorrections;
    private Animator animator;

    [SerializeField] private GameObject ground;

    [SerializeField] private bool reSize = true;
    private const float UNITY_CHAN_SHOULDER_SIZE = 0.4f;


    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();

        modelBodyBones.LeftHand = animator.GetBoneTransform(HumanBodyBones.LeftHand);
        modelBodyBones.RightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);
        modelBodyBones.LeftElbow = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
        modelBodyBones.RightElbow = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
        modelBodyBones.LeftShoulder = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
        modelBodyBones.RightShoulder = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
        modelBodyBones.Spine = animator.GetBoneTransform(HumanBodyBones.Spine);
        modelBodyBones.LeftFoot = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
        modelBodyBones.RightFoot = animator.GetBoneTransform(HumanBodyBones.RightFoot);
        modelBodyBones.LeftUpperLeg = animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
        modelBodyBones.RightUpperLeg = animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);

    }

    void Update()
    {
        if (reSize && ikBodyBones.LeftShoulder.position != Vector3.zero && ikBodyBones.RightShoulder.position != Vector3.zero)
        {
            transform.localScale = Vector3.one * Mathf.Abs(ikBodyBones.LeftShoulder.position.x - ikBodyBones.RightShoulder.position.x) / UNITY_CHAN_SHOULDER_SIZE;
        }

        if (ikBodyBones.LeftShoulder.position != Vector3.zero && ikBodyBones.RightShoulder.position != Vector3.zero)
        {
            transform.position = new Vector3((ikBodyBones.LeftShoulder.position.x - (ikBodyBones.LeftShoulder.position.x - ikBodyBones.RightShoulder.position.x) / 2) - standardPosition,
                (ikBodyBones.LeftShoulder.position.y + ikBodyBones.RightShoulder.position.y) / 2 - modelSholderHeight * transform.localScale.x, 0);
            //(ikBodyBones.LeftShoulder.position.z + ikBodyBones.RightShoulder.position.z)/2);
        }
    }

    void LateUpdate()
    {
        BoneLookAt(modelBodyBones.LeftShoulder, ikBodyBones.LeftElbow, boneCorrections.LeftShoulder);
        BoneLookAt(modelBodyBones.LeftElbow, ikBodyBones.LeftHand, boneCorrections.LeftElbow);

        BoneLookAt(modelBodyBones.RightShoulder, ikBodyBones.RightElbow, boneCorrections.RightShoulder);
        BoneLookAt(modelBodyBones.RightElbow, ikBodyBones.RightHand, boneCorrections.RightElbow);

        if (ikBodyBones.LeftShoulder.position != Vector3.zero && ikBodyBones.RightShoulder.position != Vector3.zero)
        {
            modelBodyBones.Spine.rotation = Quaternion.Euler(new Vector3(
            Mathf.Atan((ikBodyBones.RightShoulder.position.y - ikBodyBones.LeftShoulder.position.y) / (ikBodyBones.RightShoulder.position.x - ikBodyBones.LeftShoulder.position.x)) * BODY_TILT_RATE,
            0, 0)
            + boneCorrections.Spine);
        }

        /*
        BoneLookAt(modelBodyBones.LeftFoot, ikBodyBones.LeftFoot, boneCorrections.LeftFoot);
        BoneLookAt(modelBodyBones.LeftUpperLeg, ikBodyBones.LeftUpperLeg, boneCorrections.LeftUpperLeg);

        BoneLookAt(modelBodyBones.RightFoot, ikBodyBones.RightFoot, boneCorrections.RightFoot);
        BoneLookAt(modelBodyBones.RightUpperLeg, ikBodyBones.RightUpperLeg, boneCorrections.RightUpperLeg);
        */

    }

    void OnAnimatorIK()
    {
        Debug.Log("IK");
        animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);
        animator.SetIKPosition(AvatarIKGoal.LeftFoot, new Vector3(transform.position.x + 0.5f, ground.transform.position.y, ground.transform.position.z));

        animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);
        animator.SetIKPosition(AvatarIKGoal.RightFoot, new Vector3(transform.position.x - 0.5f, ground.transform.position.y, ground.transform.position.z));
    }

    private void BoneLookAt(Transform objectTF, Transform targetTF)
    {
        BoneLookAt(objectTF, targetTF, new Vector3(0, 0, 0));
    }
    private void BoneLookAt(Transform objectTF, Transform targetTF, Vector3 correction)
    {
        Vector3 dir = targetTF.position - objectTF.position;
        Quaternion lookAtRotation = Quaternion.LookRotation(dir, Vector3.up);
        objectTF.rotation = lookAtRotation * Quaternion.Euler(correction);
    }

    public void StartCalibration()
    {
        Invoke(nameof(OnCalibration), 5.0f);
    }
    private void OnCalibration()
    {
        modelSholderHeight = (ikBodyBones.LeftShoulder.position.y + ikBodyBones.RightShoulder.position.y) / 2;
        standardPosition = ikBodyBones.LeftShoulder.position.x - (ikBodyBones.LeftShoulder.position.x - ikBodyBones.RightShoulder.position.x) / 2;
    }

    private Vector3 AverageV3(Vector3 v1,Vector3 v2)
    {
        return new((v1.x + v2.x) / 2, (v1.y + v2.y) / 2, (v1.z + v2.z) / 2);
    }

    private void SetLastBones()
    {
        ikLastBones.LeftHand = ikBodyBones.LeftElbow.position;
        ikLastBones.RightHand = ikBodyBones.LeftElbow.position;
        ikLastBones.LeftElbow = ikBodyBones.LeftElbow.position;
        ikLastBones.RightElbow = ikBodyBones.LeftElbow.position;
        ikLastBones.LeftElbow = ikBodyBones.LeftElbow.position;
        ikLastBones.LeftElbow = ikBodyBones.LeftElbow.position;
        ikLastBones.LeftElbow = ikBodyBones.LeftElbow.position;
        ikLastBones.LeftElbow = ikBodyBones.LeftElbow.position;
        ikLastBones.LeftElbow = ikBodyBones.LeftElbow.position;
        ikLastBones.LeftElbow = ikBodyBones.LeftElbow.position;
    }
}