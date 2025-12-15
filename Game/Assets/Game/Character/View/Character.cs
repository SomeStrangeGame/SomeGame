using System;
using UnityEngine;
using UnityEngine.AI;

namespace Game.Character.View
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(Collider))]
    internal sealed class Character : MonoBehaviour
    {
        internal struct Ctx
        {
            internal float Speed;

            internal Func<Vector3> GetTargetPosition;
            internal Func<Vector3> GetLookAtTargetPosition;

            internal Func<bool> GetAttackInput;
            internal Func<bool> GetDodgeInput;

            internal Action<int> OnDamage;
            internal Action OnHit;

            internal Func<Transform, Vector3, Vector3, float> GetDot;
        }

        private const float _inputSense = 15f;
        private const float _inputMaxValue = 1f;
        private const float _inputMinValue = 0.2f;
        private const float _stoppedRotationSpeed = 5f;
        private const float _animRotationSpeed = 5f;

        private readonly string[] _attacksTriggers = new string[]
        {
            //"Attack_0",
            //"Attack_1",
            "Attack_2",
        };

        private readonly string[] _dodgingTriggers = new string[]
        {
            "Dodging_0",
        };

        private readonly string[] _hittingTriggers = new string[]
        {
            "Hit",
        };

        private Animator _anim;
        private NavMeshAgent _navAgent;

        private float _rot;

        private Vector2 _input;
        private Vector3 _lookAtTargetPosition;

        private readonly System.Random _random = new(DateTime.UtcNow.Second);

        private Ctx _ctx;

        private Collider _mainCollider;
        private Rigidbody[] _ragdollBones;

        public Animator Anim => _anim;
        public NavMeshAgent NavAgent => _navAgent;

        [ContextMenu("Die")]
        internal void Die()
        {
            _mainCollider.enabled = false;
            _anim.enabled = false;
            _navAgent.enabled = false;
            foreach (var ragdollBone in _ragdollBones)
            {
                ragdollBone.gameObject.SetActive(true);
            }
        }

        internal void Setup(Ctx ctx)
        {
            _ctx = ctx;

            _mainCollider = GetComponent<Collider>();

            _anim = GetComponent<Animator>();
            _anim.SetFloat("MoveSpeed", _ctx.Speed);

            _navAgent = GetComponent<NavMeshAgent>();
            _navAgent.speed = _ctx.Speed;

            _ragdollBones = GetComponentsInChildren<Rigidbody>(true);
            foreach (var ragdollBone in _ragdollBones)
            {
                ragdollBone.gameObject.SetActive(false);
            }
        }

        private bool IsSetupDone()
        {
            if (_mainCollider == null) return false;
            if (_anim == null) return false;
            if (_navAgent == null) return false;

            return true;
        }

        //invoke via engine
        private void Update()
        {
            if (!IsSetupDone()) return;
            if (!_mainCollider.enabled) return;
            if (!_anim.enabled) return;
            if (!_navAgent.enabled) return;

            _navAgent.SetDestination(_ctx.GetTargetPosition.Invoke());
            var vel = _navAgent.velocity;

            _input.y = Mathf.Lerp(_input.y, Mathf.Clamp(transform.InverseTransformDirection(vel).z, -_inputMaxValue, _inputMaxValue), Time.deltaTime * _inputSense);
            _input.x = Mathf.Lerp(_input.x, Mathf.Clamp(transform.InverseTransformDirection(vel).x, -_inputMaxValue, _inputMaxValue), Time.deltaTime * _inputSense);
            _anim.SetFloat("Vert", _input.y);
            _anim.SetFloat("Hor", _input.x);

            var isAttack = IsAttacking();
            var isDodging = IsDodging();
            var isHitting = IsHitting();

            _anim.applyRootMotion = (Mathf.Abs(_input.y) + Mathf.Abs(_input.x) < _inputMinValue) || isAttack || isDodging || isHitting;
            _navAgent.isStopped = isAttack || isDodging || isHitting;

            if (_navAgent.isStopped)
            {
                var oldRotation = _anim.transform.rotation;
                _anim.transform.LookAt(_ctx.GetLookAtTargetPosition.Invoke());
                _anim.transform.rotation = Quaternion.Lerp(oldRotation, _anim.transform.rotation, Time.deltaTime * _stoppedRotationSpeed);
            }

            var targetDotForward = GetDot(_anim.transform, _ctx.GetLookAtTargetPosition.Invoke(), Vector3.forward);
            var targetDotRight = GetDot(_anim.transform, _ctx.GetLookAtTargetPosition.Invoke(), Vector3.right);
            targetDotRight = targetDotRight > 0f ? 1f : -1f;

            _rot = Mathf.Lerp(_rot, targetDotRight, Time.deltaTime * _animRotationSpeed);

            var isRot = targetDotForward < 0f && _anim.applyRootMotion;
            _anim.SetBool("IsRot", isRot);
            _anim.SetFloat("Rot", _rot);

            if (_ctx.GetAttackInput.Invoke()) _anim.SetTrigger(_attacksTriggers[_random.Next(0, _attacksTriggers.Length)]);
            if (_ctx.GetDodgeInput.Invoke()) _anim.SetTrigger(_dodgingTriggers[_random.Next(0, _dodgingTriggers.Length)]);
        }

        internal bool IsAttacking()
        {
            var isAttackTag = "Attack";
            return _anim.GetNextAnimatorStateInfo(1).IsTag(isAttackTag) || _anim.GetCurrentAnimatorStateInfo(1).IsTag(isAttackTag);
        }

        internal bool IsDodging()
        {
            var dodgingTag = "Dodging";
            return _anim.GetNextAnimatorStateInfo(2).IsTag(dodgingTag) || _anim.GetCurrentAnimatorStateInfo(2).IsTag(dodgingTag);
        }

        internal bool IsHitting()
        {
            var hittingTag = "Hitting";
            return _anim.GetNextAnimatorStateInfo(3).IsTag(hittingTag) || _anim.GetCurrentAnimatorStateInfo(3).IsTag(hittingTag);
        }

        //invoke via animator
        private void OnAnimatorIK(int layerIndex)
        {
            if (!IsSetupDone()) return;

            _lookAtTargetPosition = Vector3.Lerp(_lookAtTargetPosition, _ctx.GetLookAtTargetPosition.Invoke(), Time.deltaTime * 2f);
            _anim.SetLookAtPosition(_lookAtTargetPosition);
            _anim.SetLookAtWeight(1f, 0.25f, 0.7f, 0.9f, 0.5f);
        }

        private float GetDot(Transform origin, Vector3 targetPosition, Vector3 axis)
        {
            return Vector3.Dot(origin.TransformDirection(axis).normalized, (targetPosition - origin.position).normalized);
        }

        internal void Damage(int damage)
        {
            Hit();
            _ctx.OnDamage.Invoke(damage);
        }

        private void Hit()
        {
            if (IsHitting()) return;

            _anim.SetTrigger(_hittingTriggers[_random.Next(0, _hittingTriggers.Length)]);
        }

        //invoke via animator
        private void HitEvent()
        {
            _ctx.OnHit.Invoke();
        }
    }
}
