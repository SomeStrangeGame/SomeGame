using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Game.Character.View
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(Collider))]
    public abstract class Character : MonoBehaviour
    {
        public struct Ctx
        {
            public Func<Vector3> GetTargetPosition;
            public Func<Vector3> GetLookAtTargetPosition;

            public Func<bool> GetAttackInput;
            public Func<bool> GetDodgeInput;

            public Action<int> OnDamage;
        }

        private const float _speed = 2.5f;
        private const float _inputMaxValue = 1f;
        private const float _inputMinValue = 0.2f;
        private const float _stoppedRotationSpeed = 5f;

        private readonly Dictionary<string, int> _animHashes = new();
        private readonly string[] _attacksParams = new string[] {"Attack_0", "Attack_1", "Attack_2"};
        private readonly string[] _dodgingParams = new string[] {"Dodging_0"};
        private readonly string[] _hittingParams = new string[] {"Hit"};
        private const string _moveSpeedParam = "MoveSpeed";
        private const string _vertParam = "Vert";
        private const string _horParam = "Hor";
        private const string _isRotParam = "IsRot";
        private const string _rotParam = "Rot";

        [SerializeField] private float _attackDistance = 2f;

        private Animator _anim;
        public Animator Anim
        {
            get
            {
                if (_anim == null) _anim = GetComponent<Animator>();
                return _anim;
            }
        }

        private Transform _chestTransform;
        public Transform ChestTransform
        {
            get
            {
                if (_chestTransform == null) _chestTransform = Anim.GetBoneTransform(HumanBodyBones.Chest);
                return _chestTransform;
            }
        }

        private NavMeshAgent _navAgent;
        public NavMeshAgent NavAgent
        {
            get
            {
                if (_navAgent == null) _navAgent = GetComponent<NavMeshAgent>();
                return _navAgent;
            }
        }

        private Collider _mainCollider;
        public Collider MainCollider
        {
            get
            {
                if (_mainCollider == null) _mainCollider = GetComponent<Collider>();
                return _mainCollider;
            }
        }

        private readonly System.Random _random = new(DateTime.UtcNow.Second);

        private Ctx _ctx;

        private int AnimHash(string paramName)
        {
            if (!_animHashes.TryGetValue(paramName, out _))
                _animHashes.Add(paramName, Animator.StringToHash(paramName));
            return _animHashes[paramName];
        }

        private void SetRagDoll(bool state)
        {
            var ragdollBones = GetComponentsInChildren<Rigidbody>(true);
            foreach (var ragdollBone in ragdollBones)
            {
                ragdollBone.isKinematic = !state;
                ragdollBone.gameObject.SetActive(state);
            }
        }

        public void Die()
        {
            MainCollider.enabled = false;
            Anim.enabled = false;
            NavAgent.enabled = false;
            SetRagDoll(true);
        }

        public virtual void Setup(Ctx ctx)
        {
            _ctx = ctx;
            Anim.SetFloat(AnimHash(_moveSpeedParam), _speed);
            NavAgent.speed = _speed;
            SetRagDoll(false);
        }

        public bool IsAttacking() => IsTag(1, "Attack");
        public bool IsDodging() => IsTag(3, "Dodging");
        public bool IsHitting() => IsTag(2, "Hitting");
        private bool IsTag(int layer, string tag) => Anim.GetNextAnimatorStateInfo(layer).IsTag(tag) || Anim.GetCurrentAnimatorStateInfo(layer).IsTag(tag);

        //invoke via animator
        private void OnAnimatorIK(int layerIndex)
        {
            if (MainCollider == null) return;
            if (Anim == null) return;
            if (NavAgent == null) return;

            var lookAtTargetPosition = _ctx.GetLookAtTargetPosition.Invoke();
            Anim.SetLookAtPosition(lookAtTargetPosition);
            Anim.SetLookAtWeight(1f, 0.25f, 0.7f, 0.9f, 0.5f);

            NavAgent.SetDestination(_ctx.GetTargetPosition.Invoke());
            var vel = NavAgent.velocity;

            var inputY = Mathf.Clamp(transform.InverseTransformDirection(vel).z, -_inputMaxValue, _inputMaxValue);
            var inputX = Mathf.Clamp(transform.InverseTransformDirection(vel).x, -_inputMaxValue, _inputMaxValue);

            Anim.SetFloat(AnimHash(_vertParam), inputY);
            Anim.SetFloat(AnimHash(_horParam), inputX);

            Anim.applyRootMotion = (Mathf.Abs(inputY) + Mathf.Abs(inputX) < _inputMinValue) || IsAttacking() || IsDodging() || IsHitting();
            NavAgent.isStopped = IsAttacking() || IsDodging() || IsHitting();

            if (NavAgent.isStopped)
            {
                var oldRotation = Anim.transform.rotation;
                Anim.transform.LookAt(lookAtTargetPosition);
                Anim.transform.rotation = Quaternion.Lerp(oldRotation, Anim.transform.rotation, Time.deltaTime * _stoppedRotationSpeed);
            }

            var targetDotForward = GetDot(Anim.transform, lookAtTargetPosition, Vector3.forward);
            var rot = GetDot(Anim.transform, lookAtTargetPosition, Vector3.right);
            rot = rot > 0f ? 1f : -1f;

            var isRot = targetDotForward < 0f && Anim.applyRootMotion;
            Anim.SetBool(AnimHash(_isRotParam), isRot);
            Anim.SetFloat(AnimHash(_rotParam), rot);

            if (_ctx.GetAttackInput.Invoke()) Anim.SetTrigger(AnimHash(_attacksParams[_random.Next(0, _attacksParams.Length)]));
            if (_ctx.GetDodgeInput.Invoke()) Anim.SetTrigger(AnimHash(_dodgingParams[_random.Next(0, _dodgingParams.Length)]));
        }

        private float GetDot(Transform origin, Vector3 targetPosition, Vector3 axis)
        {
            return Vector3.Dot(origin.TransformDirection(axis).normalized, (targetPosition - origin.position).normalized);
        }

        private void Damage(int damage)
        {
            Anim.SetTrigger(AnimHash(_hittingParams[_random.Next(0, _hittingParams.Length)]));
            _ctx.OnDamage.Invoke(damage);
        }

        //attack invoke via animator
        private void HitEvent() 
        {
            if (IsHitting()) return;
            if (IsDodging()) return;

            var headTrans = Anim.GetBoneTransform(HumanBodyBones.Head);
            var ray = new Ray(headTrans.position, headTrans.forward);
            if (!Physics.Raycast(ray, out var hit, _attackDistance, Physics.AllLayers, QueryTriggerInteraction.Ignore)) return;

            var character = hit.collider.GetComponentInParent<Character>();
            if (character == null) return;
            if (character.IsHitting()) return;
            if (character.IsDodging()) return;
            character.Damage(1);
        }
        private void SendEvent() { }
    }
}
