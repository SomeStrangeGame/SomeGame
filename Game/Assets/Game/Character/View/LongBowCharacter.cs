using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Character.View
{
    public class LongBowCharacter : Character
    {
        private static readonly HashSet<string> _animationClipEvents = new();
        [SerializeField] private AnimationClip _recoilAnim;
        [SerializeField] private GameObject _arrowPrefab;
        [SerializeField] private HandPositionHandler _handPosition;
        [SerializeField] private GameObject _arrowView;
        [SerializeField] private float _arrowSpeed;
        [SerializeField] private float _arrowGravity;
        [SerializeField] private Transform _arrowPoint;

        private List<GameObject> _arrows = new();

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

        //invoke via engine
        private void LateUpdate()
        {
            _arrowView.transform.SetPositionAndRotation(_handPosition.Pos, _handPosition.Rot);

            foreach (var arrow in _arrows)
            {
                if (!arrow.activeSelf) continue;

                var newPosition = arrow.transform.position + (arrow.transform.forward * _arrowSpeed + Vector3.down * _arrowGravity) * Time.deltaTime;
                arrow.transform.LookAt(newPosition);
                //physics here...
                arrow.transform.position = newPosition;
            }
        }

        protected override void OnAnimatorIK(int layerIndex)
        {
            base.OnAnimatorIK(layerIndex);
            Anim.SetBool(AnimHash(_attackParam), _ctx.GetAttackInput.Invoke());
        }

        protected override void OnHitEvent()
        {
            var disabledArrow = _arrows.FirstOrDefault(a => !a.activeSelf);
            if (disabledArrow == null)
            {
                disabledArrow = Instantiate(_arrowPrefab, _arrowPoint.position, _arrowPoint.rotation);
                //disabledArrow.transform.LookAt(disabledArrow.transform.position + Anim.GetBoneTransform(HumanBodyBones.Head).forward);
                //var arrowAngle = disabledArrow.transform.eulerAngles;
                //arrowAngle.x = 0;
                //disabledArrow.transform.eulerAngles = arrowAngle;
                _arrows.Add(disabledArrow);
            }
            //disabledArrow.transform.LookAt(GetLookAtTargetPosition());
            disabledArrow.SetActive(true);
        }

        private void OnDisable()
        {
            foreach(var arrow in _arrows)
                GameObject.Destroy(arrow);
            _arrows.Clear();
        }
    }
}

