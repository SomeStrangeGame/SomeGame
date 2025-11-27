using UnityEngine;
using UnityEngine.AI;

public class TestCharacterMover : MonoBehaviour
{
    [SerializeField] private Transform _target;

    private Animator _anim;
    private NavMeshAgent _navAgent;

    private void OnEnable()
    {
        _anim = GetComponent<Animator>();
        _navAgent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        _navAgent.SetDestination(_target.position);
        var vel = _navAgent.velocity;
        _anim.SetFloat("Vert", Mathf.Clamp(transform.InverseTransformDirection(vel).z, -2f, 2f));
        _anim.SetFloat("Hor", Mathf.Clamp(transform.InverseTransformDirection(vel).x, -2f, 2f));
    }

}
