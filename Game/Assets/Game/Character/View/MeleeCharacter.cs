using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

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

        //invoke via engine
        private void LateUpdate()
        {
            _weaponView.transform.SetPositionAndRotation(_handPosition.Pos, _handPosition.Rot);
        }
    }
}

