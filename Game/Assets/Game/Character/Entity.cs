using System;
using Disposable;
using UnityEngine;

namespace Game.Character
{
    public sealed class Entity : BaseDisposable
    {
        public struct Ctx
        {
            public GameObject CharacterView;
            public int Health;

            public Func<Entity, Vector3> GetTargetPosition;
            public Func<Entity, bool, Vector3> GetLookAtTargetPosition;
            public Func<Entity, bool, bool> GetAttackInput;
            public Func<Entity, bool> GetDodgeInput;
        }

        private View.Character _character;
        private int _health;

        private readonly Ctx _ctx;

        public Animator Anim => _character.Anim;
        public Transform ChestTransform => _character.ChestTransform;

        public bool IsAttacking => _character.IsAttacking();
        public bool IsDodging => _character.IsDodging();
        public bool IsHitting => _character.IsHitting();

        public Entity(Ctx ctx)
        {
            _ctx = ctx;
        }

        public void Init()
        {
            _health = _ctx.Health;

            _character = _ctx.CharacterView.GetComponent<View.Character>();
            _character.Setup(new View.Character.Ctx
            {
                GetTargetPosition = () => _ctx.GetTargetPosition.Invoke(this),
                GetLookAtTargetPosition = isDistance => _ctx.GetLookAtTargetPosition.Invoke(this, isDistance),

                GetAttackInput = isDistance => _ctx.GetAttackInput.Invoke(this, isDistance),
                GetDodgeInput = () => _ctx.GetDodgeInput.Invoke(this),

                OnDamage = damage =>
                {
                    _health -= damage;

                    if (_health > 0) return;
                    _character.Die();
                }
            });
        }
    }
}

