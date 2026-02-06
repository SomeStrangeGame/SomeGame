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
        [SerializeField] private GameObject _aimPoint;

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
                var ray = new Ray(arrow.transform.position, arrow.transform.forward);
                if (Physics.Raycast(ray, out var hit, Vector3.Distance(arrow.transform.position, newPosition)))
                {
                    arrow.SetActive(false);

                    var character = hit.collider.GetComponentInParent<Character>();
                    if (character == null) return;
                    if (character.IsHitting()) return;
                    if (character.IsDodging()) return;
                    character.Damage(1);
                }
                else
                {
                    arrow.transform.position = newPosition;
                }
            }
        }

        protected override void OnAnimatorIK(int layerIndex)
        {
            base.OnAnimatorIK(layerIndex);
            Anim.SetBool(AnimHash(_attackParam), _ctx.GetAttackInput.Invoke(true));
            _aimPoint.SetActive(IsTag(1, "Aiming"));
        }

        protected override void OnHitEvent()
        {
            var disabledArrow = _arrows.FirstOrDefault(a => !a.activeSelf);
            if (disabledArrow == null)
            {
                disabledArrow = Instantiate(_arrowPrefab);
                _arrows.Add(disabledArrow);
            }
            disabledArrow.transform.SetPositionAndRotation(_arrowPoint.position, _arrowPoint.rotation);
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

