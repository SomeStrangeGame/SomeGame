using UnityEngine;
using UnityEngine.AI;

public class TestCharacterMover : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private Transform _lookAtTarget;

    private Animator _anim;
    private NavMeshAgent _navAgent;

    private float _rot;

    private void OnEnable()
    {
        _anim = GetComponent<Animator>();
        _navAgent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        _navAgent.SetDestination(_target.position);
        var vel = _navAgent.velocity;
        var clampValue = 2f;
        var vertValue = Mathf.Clamp(transform.InverseTransformDirection(vel).z, -clampValue, clampValue);
        var horValue = Mathf.Clamp(transform.InverseTransformDirection(vel).x, -clampValue, clampValue);
        _anim.SetFloat("Vert", vertValue);
        _anim.SetFloat("Hor", horValue);

        _anim.applyRootMotion = Mathf.Abs(vertValue) + Mathf.Abs(horValue) < 0.1f;

        var targetDotForward = GetDot(_anim.transform, _lookAtTarget, Vector3.forward);
        var targetDotRight = GetDot(_anim.transform, _lookAtTarget, Vector3.right);
        targetDotRight = targetDotRight > 0f ? 1f : -1f;

        _rot = Mathf.Lerp(_rot, targetDotRight, Time.deltaTime * 5f);

        var isRot = targetDotForward < 0f && _anim.applyRootMotion;
        _anim.SetBool("IsRot", isRot);
        _anim.SetFloat("Rot", _rot);
    }

    private void OnAnimatorIK(int layerIndex)
    {
        _anim.SetLookAtPosition(_lookAtTarget.position);
        _anim.SetLookAtWeight(1f, 0.5f, 0.7f, 0.9f, 0.5f);
    }

    private float GetDot(Transform origin, Transform target, Vector3 axis)
    {
        return Vector3.Dot(origin.TransformDirection(axis).normalized, (target.position - origin.position).normalized);
    }

}
