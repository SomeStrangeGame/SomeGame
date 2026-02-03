using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Game.Character.View
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(Collider))]
    public sealed class Character : MonoBehaviour
    {
        public struct Ctx
        {
            public float Speed;

            public Func<Vector3> GetTargetPosition;
            public Func<Vector3> GetLookAtTargetPosition;

            public Func<bool> GetAttackInput;
            public Func<bool> GetDodgeInput;

            public Action<int> OnDamage;
            public Action OnHit;
        }

        private static readonly HashSet<string> _animationClipEvents = new();

        [SerializeField] private HandPositionHandler _handPosition;
        [SerializeField] private GameObject _weaponView;

        [SerializeField] private AnimationClip _attack_0;
        [SerializeField] private AnimationClip _attack_1;
        [SerializeField] private AnimationClip _attack_2;

        private const float _inputSense = 15f;
        private const float _inputMaxValue = 1f;
        private const float _inputMinValue = 0.2f;
        private const float _stoppedRotationSpeed = 5f;
        private const float _animRotationSpeed = 5f;

        private int[] _attacksTriggersHashes;
        private readonly string[] _attacksTriggers = new string[]
        {
            "Attack_0",
            "Attack_1",
            "Attack_2",
        };

        private int[] _dodgingTriggersHashes;
        private readonly string[] _dodgingTriggers = new string[]
        {
            "Dodging_0",
        };

        private int[] _hittingTriggersHashes;
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

        private int _moveSpeedHash;
        private const string _moveSpeedParam = "MoveSpeed";
        private int _vertHash;
        private const string _vertParam = "Vert";
        private int _horHash;
        private const string _horParam = "Hor";
        private int _isRotHash;
        private const string _isRotParam = "IsRot";
        private int _rotHash;
        private const string _rotParam = "Rot";

        [ContextMenu("Die")]
        public void Die()
        {
            _mainCollider.enabled = false;
            _anim.enabled = false;
            _navAgent.enabled = false;
            foreach (var ragdollBone in _ragdollBones)
            {
                ragdollBone.isKinematic = false;
                ragdollBone.gameObject.SetActive(true);
            }
        }

        public void Setup(Ctx ctx)
        {
            _ctx = ctx;

            _mainCollider = GetComponent<Collider>();

            _attacksTriggersHashes = new int[_attacksTriggers.Length];
            for (var i = 0 ; i < _attacksTriggers.Length; i++)
                _attacksTriggersHashes[i] = Animator.StringToHash(_attacksTriggers[i]);

            _dodgingTriggersHashes = new int[_dodgingTriggers.Length];
            for (var i = 0 ; i < _dodgingTriggers.Length; i++)
                _dodgingTriggersHashes[i] = Animator.StringToHash(_dodgingTriggers[i]);

            _hittingTriggersHashes = new int[_hittingTriggers.Length];
            for (var i = 0 ; i < _hittingTriggers.Length; i++)
                _hittingTriggersHashes[i] = Animator.StringToHash(_hittingTriggers[i]);

            _moveSpeedHash = Animator.StringToHash(_moveSpeedParam);
            _vertHash = Animator.StringToHash(_vertParam);
            _horHash = Animator.StringToHash(_horParam);
            _isRotHash = Animator.StringToHash(_isRotParam);
            _rotHash = Animator.StringToHash(_rotParam);

            SetAnimEvent(_attack_0, 0.3f, "HitEvent");
            SetAnimEvent(_attack_1, 0.3f, "HitEvent");
            SetAnimEvent(_attack_2, 0.3f, "HitEvent");

            void SetAnimEvent(AnimationClip clip, float eventTime, string eventFunc)
            {
                if (_animationClipEvents.Contains(clip.name)) return;

                Debug.Log(clip.name);
                _animationClipEvents.Add(clip.name);
                clip.AddEvent(new AnimationEvent
                {
                    time = eventTime,
                    functionName = eventFunc,
                });
            }

            _anim = GetComponent<Animator>();
            _anim.SetFloat(_moveSpeedHash, _ctx.Speed);

            _navAgent = GetComponent<NavMeshAgent>();
            _navAgent.speed = _ctx.Speed;

            _ragdollBones = GetComponentsInChildren<Rigidbody>(true);
            foreach (var ragdollBone in _ragdollBones)
            {
                ragdollBone.isKinematic = true;
                ragdollBone.gameObject.SetActive(false);
            }
        }

        public bool IsAttacking() => IsTag(1, "Attack");
        public bool IsDodging() => IsTag(3, "Dodging");
        public bool IsHitting() => IsTag(2, "Hitting");
        private bool IsTag(int layer, string tag) => _anim.GetNextAnimatorStateInfo(layer).IsTag(tag) || _anim.GetCurrentAnimatorStateInfo(layer).IsTag(tag);

        //invoke via engine
        private void LateUpdate()
        {
            _weaponView.transform.SetPositionAndRotation(_handPosition.Pos, _handPosition.Rot);
        }

        //invoke via animator
        private void OnAnimatorIK(int layerIndex)
        {
            if (_mainCollider == null) return;
            if (_anim == null) return;
            if (_navAgent == null) return;

            _lookAtTargetPosition = _ctx.GetLookAtTargetPosition.Invoke();
            _anim.SetLookAtPosition(_lookAtTargetPosition);
            _anim.SetLookAtWeight(1f, 0.25f, 0.7f, 0.9f, 0.5f);

            _navAgent.SetDestination(_ctx.GetTargetPosition.Invoke());
            var vel = _navAgent.velocity;

            _input.y = Mathf.Lerp(_input.y, Mathf.Clamp(transform.InverseTransformDirection(vel).z, -_inputMaxValue, _inputMaxValue), Time.deltaTime * _inputSense);
            _input.x = Mathf.Lerp(_input.x, Mathf.Clamp(transform.InverseTransformDirection(vel).x, -_inputMaxValue, _inputMaxValue), Time.deltaTime * _inputSense);
            _anim.SetFloat(_vertHash, _input.y);
            _anim.SetFloat(_horHash, _input.x);

            var isAttack = IsAttacking();
            var isDodging = IsDodging();
            var isHitting = IsHitting();

            _anim.applyRootMotion = (Mathf.Abs(_input.y) + Mathf.Abs(_input.x) < _inputMinValue) || isAttack || isDodging || isHitting;
            _navAgent.isStopped = isAttack || isDodging || isHitting;

            if (_navAgent.isStopped)
            {
                var oldRotation = _anim.transform.rotation;
                _anim.transform.LookAt(_lookAtTargetPosition);
                _anim.transform.rotation = Quaternion.Lerp(oldRotation, _anim.transform.rotation, Time.deltaTime * _stoppedRotationSpeed);
            }

            var targetDotForward = GetDot(_anim.transform, _lookAtTargetPosition, Vector3.forward);
            var targetDotRight = GetDot(_anim.transform, _lookAtTargetPosition, Vector3.right);
            targetDotRight = targetDotRight > 0f ? 1f : -1f;

            _rot = Mathf.Lerp(_rot, targetDotRight, Time.deltaTime * _animRotationSpeed);

            var isRot = targetDotForward < 0f && _anim.applyRootMotion;
            _anim.SetBool(_isRotHash, isRot);
            _anim.SetFloat(_rotHash, _rot);

            if (_ctx.GetAttackInput.Invoke()) _anim.SetTrigger(_attacksTriggersHashes[_random.Next(0, _attacksTriggersHashes.Length)]);
            if (_ctx.GetDodgeInput.Invoke()) _anim.SetTrigger(_dodgingTriggersHashes[_random.Next(0, _dodgingTriggersHashes.Length)]);
        }

        private float GetDot(Transform origin, Vector3 targetPosition, Vector3 axis)
        {
            return Vector3.Dot(origin.TransformDirection(axis).normalized, (targetPosition - origin.position).normalized);
        }

        public void Damage(int damage)
        {
            Hit();
            _ctx.OnDamage.Invoke(damage);
        }

        private void Hit()
        {
            if (IsHitting()) return;

            _anim.SetTrigger(_hittingTriggersHashes[_random.Next(0, _hittingTriggersHashes.Length)]);
        }

        //invoke via animator
        private void HitEvent()
        {
            _ctx.OnHit.Invoke();
        }

        //invoke via animator
        private void SendEvent()
        {
            
        }
    }
}
