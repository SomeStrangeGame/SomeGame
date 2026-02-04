using System.Collections.Generic;
using UnityEngine;

namespace Game.Character.View
{
    public class LongBowCharacter : Character
    {
        private static readonly HashSet<string> _animationClipEvents = new();
        [SerializeField] private AnimationClip _recoilAnim;

        private const string _attackParam = "IsAttack";

        public override void Setup(Ctx ctx)
        {
            SetAnimEvent(_recoilAnim, 0.01f, "RecoilEvent");

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
            return _ctx.GetLookAtTargetPosition.Invoke(true);
        }

        protected override void OnAnimatorIK(int layerIndex)
        {
            base.OnAnimatorIK(layerIndex);
            Anim.SetBool(AnimHash(_attackParam), _ctx.GetAttackInput.Invoke());
        }

        protected override void OnHitEvent()
        {
            Debug.Log("Recoil");
        }
    }
}

