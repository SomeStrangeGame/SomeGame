using System;
using Cysharp.Threading.Tasks;
using Game.Disposable;
using UnityEngine;
using UnityEngine.AI;

namespace Game.Character
{
    public sealed class Entity : BaseDisposable
    {
        public struct Ctx
        {
            public GameObject CharacterView;
            public int Health;
            public float Speed;
            public float AttackDistance;

            public Func<Entity, Vector3> GetTargetPosition;
            public Func<Entity, Vector3> GetLookAtTargetPosition;
            public Func<Entity, bool> GetAttackInput;
            public Func<Entity, bool> GetDodgeInput;
        }

        private View.Character _character;
        private Transform _chestTransform;
        private int _health;

        private readonly Ctx _ctx;

        public Animator Anim => _character.Anim;
        public NavMeshAgent NavAgent => _character.NavAgent;
        public Transform ChestTransform => _chestTransform;
        public Transform CharacterTransform => _character.transform;

        public bool IsAttacking => _character.IsAttacking();
        public bool IsDodging => _character.IsDodging();
        public bool IsHitting => _character.IsHitting();

        public Entity(Ctx ctx)
        {
            _ctx = ctx;
        }

        public void Init()
        {
            _character = _ctx.CharacterView.GetComponent<View.Character>();
            _character.Setup(new View.Character.Ctx
            {
                Speed = _ctx.Speed,

                GetTargetPosition = () => _ctx.GetTargetPosition.Invoke(this),
                GetLookAtTargetPosition = () => _ctx.GetLookAtTargetPosition.Invoke(this),

                GetAttackInput = () => _ctx.GetAttackInput.Invoke(this),
                GetDodgeInput = () => _ctx.GetDodgeInput.Invoke(this),

                OnDamage = Damage,
                OnHit = Hit,
            });
            _chestTransform = _character.Anim.GetBoneTransform(HumanBodyBones.Chest);
            _health = _ctx.Health;
        }

        private void Damage(int damage)
        {
            if (_character.IsDodging()) return;

            _health -= damage;

            if (_health > 0) return;
            
            _character.Die();
        }

        private void Hit()
        {
            if (IsHitting) return;
            if (IsDodging) return;

            var headTrans = Anim.GetBoneTransform(HumanBodyBones.Head);
            var ray = new Ray(headTrans.position, headTrans.forward);
            if (!Physics.Raycast(ray, out var hit, _ctx.AttackDistance, Physics.AllLayers, QueryTriggerInteraction.Ignore)) return;

            var character = hit.collider.GetComponentInParent<View.Character>();
            if (character == null) return;
            
            character.Damage(1);
        }
    }
}

