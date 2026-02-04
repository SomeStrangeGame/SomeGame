using System.Collections.Generic;
using UnityEngine;

namespace Game.Character.View
{
    public sealed class MeleeCharacter : Character
    {
        private static readonly HashSet<string> _animationClipEvents = new();
        [SerializeField] private AnimationClip _attack_0;
        [SerializeField] private AnimationClip _attack_1;
        [SerializeField] private AnimationClip _attack_2;

        [SerializeField] private HandPositionHandler _handPosition;
        [SerializeField] private GameObject _weaponView;

        private readonly string[] _attacksParams = new string[] {"Attack_0", "Attack_1", "Attack_2"};
        private readonly string[] _dodgingParams = new string[] {"Dodging_0"};
        private readonly string[] _hittingParams = new string[] {"Hit"};

        public override void Setup(Ctx ctx)
        {
            SetAnimEvent(_attack_0, 0.3f, "HitEvent");
            SetAnimEvent(_attack_1, 0.3f, "HitEvent");
            SetAnimEvent(_attack_2, 0.3f, "HitEvent");

            void SetAnimEvent(AnimationClip clip, float eventTime, string eventFunc)
            {
                if (_animationClipEvents.Contains(clip.name)) return;

                _animationClipEvents.Add(clip.name);
                clip.AddEvent(new AnimationEvent
                {
                    time = eventTime,
                    functionName = eventFunc,
                });
            }

            base.Setup(ctx);
        }

        protected override Vector3 GetLookAtTargetPosition()
        {
            return _ctx.GetLookAtTargetPosition.Invoke(false);
        }

        protected override void OnAnimatorIK(int layerIndex)
        {
            base.OnAnimatorIK(layerIndex);

            if (_ctx.GetAttackInput.Invoke()) Anim.SetTrigger(AnimHash(_attacksParams[_random.Next(0, _attacksParams.Length)]));
            if (_ctx.GetDodgeInput.Invoke()) Anim.SetTrigger(AnimHash(_dodgingParams[_random.Next(0, _dodgingParams.Length)]));
        }

        //invoke via engine
        private void LateUpdate()
        {
            _weaponView.transform.SetPositionAndRotation(_handPosition.Pos, _handPosition.Rot);
        }

        protected override void Damage(int damage)
        {
            Anim.SetTrigger(AnimHash(_hittingParams[_random.Next(0, _hittingParams.Length)]));
            base.Damage(damage);
        }
    }
}

