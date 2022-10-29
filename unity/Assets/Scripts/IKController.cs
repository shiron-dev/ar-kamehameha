using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKController : MonoBehaviour
{
    private const float BODY_TILT_RATE = 50;
    private float modelSholderHeight = 1.35f / 4;
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
    private BodyBones<Transform> modelBodyBones;
    [SerializeField] private BodyBones<Vector3> boneCorrections;
    private Animator animator;

    [SerializeField] private GameObject ground;


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

        if (ikBodyBones.LeftShoulder.position != Vector3.zero && ikBodyBones.RightShoulder.position != Vector3.zero)
        {
            transform.position = new Vector3((ikBodyBones.LeftShoulder.position.x - (ikBodyBones.LeftShoulder.position.x - ikBodyBones.RightShoulder.position.x) / 2) - standardPosition,
                (ikBodyBones.LeftShoulder.position.y + ikBodyBones.RightShoulder.position.y) / 2 - modelSholderHeight, (ikBodyBones.LeftShoulder.position.z + ikBodyBones.RightShoulder.position.z)/2);
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
        animator.SetIKPosition(AvatarIKGoal.LeftFoot, new Vector3(transform.position.x + 0.5f, 0, ground.transform.position.z));

        animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);
        animator.SetIKPosition(AvatarIKGoal.RightFoot, new Vector3(transform.position.x - 0.5f, 0, ground.transform.position.z));
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
}