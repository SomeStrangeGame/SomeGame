using System;
using Cysharp.Threading.Tasks;
using Game.Disposable;
using UnityEngine;

namespace Game.Character
{
    public sealed class Entity : BaseDisposable
    {
        public struct Ctx
        {
            public GameObject CharacterView;
            public float Speed;

            public Func<Vector3> GetTargetPosition;
            public Func<Vector3> GetLookAtTargetPosition;
            public Func<bool> GetAttackInput;
            public Func<bool> GetDodgeInput;
        }

        private View.Character _character;

        private readonly Ctx _ctx;

        public Entity(Ctx ctx)
        {
            _ctx = ctx;
        }

        public async UniTask Init()
        {
            _character = _ctx.CharacterView.GetComponent<View.Character>();
            _character.Setup(new View.Character.Ctx
            {
                Speed = _ctx.Speed,

                GetTargetPosition = _ctx.GetTargetPosition,
                GetLookAtTargetPosition = _ctx.GetLookAtTargetPosition,

                GetAttackInput = _ctx.GetAttackInput,
                GetDodgeInput = _ctx.GetDodgeInput,
            });
        }
    }
}

