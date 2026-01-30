using System.Collections.Generic;
using System.Linq;
using Game.Disposable;
using UnityEngine;

namespace Game.Character
{
    public class Behaviour : BaseDisposable
    {
        public struct Ctx
        {
            public Entity PlayerCharacterEntity;
            public List<Entity> EnemyCharacterEntites;
        }

        private const float _defaultLookAtOffset = 5f;

        private const float _enemyMoveDistance = 1.9f;

        private const float _enemyAttackDistance = 2.2f;

        private const float _enemyDodgeDotTrigger = 0.1f;
        private const float _enemyDodgeDistance = 2.1f;

        private System.Random _random;

        private Ctx _ctx;

        public Behaviour(Ctx ctx)
        {
            var randomSeed = Mathf.RoundToInt(Time.time);
            _random = new System.Random(randomSeed);
            _ctx = ctx;
        }

        public Vector3 GetTargetPosition(Entity characterEntity, bool isPlayer)
        {
            var targetPos = characterEntity.CharacterTransform.position;

            if (isPlayer)
            {
                targetPos += Vector3.right * SimpleInput.GetAxis("Horizontal");
                targetPos += Vector3.forward * SimpleInput.GetAxis("Vertical");
            }
            else
            {
                var heading = _ctx.PlayerCharacterEntity.CharacterTransform.position - characterEntity.CharacterTransform.position;
                var distance = heading.magnitude;
                var direction = heading / distance;
                targetPos = _ctx.PlayerCharacterEntity.CharacterTransform.position - direction * _enemyMoveDistance;
            }
            return targetPos;
        }

        public Vector3 GetLookAtTargetPosition(Entity characterEntity, bool isPlayer)
        {
            var lookAtPoint = characterEntity.CharacterTransform.position + characterEntity.CharacterTransform.forward * _defaultLookAtOffset;

            if (isPlayer)
            {
                var minDistance = float.MaxValue;
                foreach (var enemyEntity in _ctx.EnemyCharacterEntites)
                {
                    if (!enemyEntity.Anim.enabled) continue;

                    var distance = Vector3.SqrMagnitude(enemyEntity.CharacterTransform.position - characterEntity.CharacterTransform.position);
                    if (distance > minDistance) continue;

                    minDistance = distance;
                    lookAtPoint = enemyEntity.ChestTransform.position;
                }
            }
            else
            {
                lookAtPoint = _ctx.PlayerCharacterEntity.ChestTransform.position;
            }

            return lookAtPoint;
        }

        public bool GetAttackInput(Entity characterEntity, bool isPlayer)
        {
            if (!characterEntity.Anim.enabled) return false;
            if (characterEntity.IsAttacking) return false;
            if (characterEntity.IsHitting) return false;
            if (characterEntity.IsDodging) return false;

            if (isPlayer)
            {
                return SimpleInput.GetKeyUp(KeyCode.Space);
            }
            else
            {
                return false;
                if (!_ctx.PlayerCharacterEntity.Anim.enabled) return false;
                if (_ctx.PlayerCharacterEntity.IsHitting) return false;

                var distance = Vector3.SqrMagnitude(_ctx.PlayerCharacterEntity.Anim.rootPosition - characterEntity.Anim.rootPosition);
                if (distance > _enemyAttackDistance * _enemyAttackDistance) return false;
                if (_ctx.EnemyCharacterEntites.Any(e => e.Anim.enabled && e.IsAttacking)) return false;
                if (_random.Next(0, 100) > 10) return false;

                return true;
            }
        }

        public bool GetDodgeInput(Entity characterEntity, bool isPlayer)
        {
            if (!characterEntity.Anim.enabled) return false;
            if (characterEntity.IsDodging) return false;
            if (characterEntity.IsHitting) return false;

            if (isPlayer)
            {
                return SimpleInput.GetKeyUp(KeyCode.E);
            }
            else
            {
                var distance = Vector3.SqrMagnitude(_ctx.PlayerCharacterEntity.Anim.rootPosition - characterEntity.Anim.rootPosition);
                var targetDotForward = GetDot(_ctx.PlayerCharacterEntity.Anim.transform, characterEntity.Anim.rootPosition, Vector3.forward);
                
                if (!_ctx.PlayerCharacterEntity.Anim.enabled) return false;
                if (targetDotForward < _enemyDodgeDotTrigger) return false;
                if (distance > _enemyDodgeDistance * _enemyDodgeDistance) return false;
                if (!_ctx.PlayerCharacterEntity.IsAttacking) return false;
                if (_random.Next(0, 100) > 10) return false;

                return true;
            }
        }

        public float GetDot(Transform origin, Vector3 targetPosition, Vector3 axis)
        {
            return Vector3.Dot(origin.TransformDirection(axis).normalized, (targetPosition - origin.position).normalized);
        }
    }
}

