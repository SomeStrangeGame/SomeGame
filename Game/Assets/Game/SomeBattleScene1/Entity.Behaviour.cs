using System;
using System.Linq;
using UnityEngine;

namespace Game.SomeBattleScene1
{
    public sealed partial class Entity
    {
        private readonly System.Random _behaviourRandom = new(DateTime.UtcNow.Second);

        private const float _defaultLookAtOffset = 5f;

        private const float _enemyMoveDistance = 1.9f;

        private const float _enemyAttackDistance = 2.2f;

        private const float _enemyDodgeDotTrigger = 0.1f;
        private const float _enemyDodgeDistance = 2.1f;

        private bool RandomBool => _behaviourRandom.Next(-10, 10) > 0;

        private Vector3 GetTargetPosition(Character.Entity characterEntity, bool isPlayer)
        {
            var targetPos = characterEntity.Anim.rootPosition;

            if (isPlayer)
            {
                targetPos += Vector3.right * Input.GetAxis("Horizontal");
                targetPos += Vector3.forward * Input.GetAxis("Vertical");
            }
            else
            {
                var heading = _playerCharacterEntity.Anim.rootPosition - characterEntity.Anim.rootPosition;
                var distance = heading.magnitude;
                var direction = heading / distance;
                targetPos = _playerCharacterEntity.Anim.rootPosition - direction * _enemyMoveDistance;
            }
            return targetPos;
        }

        private Vector3 GetLookAtTargetPosition(Character.Entity characterEntity, bool isPlayer)
        {
            var lookAtPoint = characterEntity.Anim.rootPosition + characterEntity.Anim.transform.forward * _defaultLookAtOffset;

            if (isPlayer)
            {
                var minDistance = float.MaxValue;
                foreach (var enemyEntity in _enemyCharacterEntites)
                {
                    if (!enemyEntity.Anim.enabled) continue;

                    var distance = Vector3.Distance(enemyEntity.Anim.rootPosition, characterEntity.Anim.rootPosition);
                    if (distance > minDistance) continue;

                    minDistance = distance;
                    lookAtPoint = enemyEntity.Anim.GetBoneTransform(HumanBodyBones.Chest).position;
                }
            }
            else
            {
                lookAtPoint = _playerCharacterEntity.Anim.GetBoneTransform(HumanBodyBones.Chest).position;
            }

            return lookAtPoint;
        }

        private bool GetAttackInput(Character.Entity characterEntity, bool isPlayer)
        {
            if (characterEntity.IsAttacking) return false;

            if (isPlayer)
            {
                return Input.GetKeyUp(KeyCode.Space);
            }
            else
            {
                var distance = Vector3.Distance(_playerCharacterEntity.Anim.rootPosition, characterEntity.Anim.rootPosition);

                var isAttack = true;
                isAttack &= distance < _enemyAttackDistance;
                isAttack &= RandomBool;
                isAttack &= !_enemyCharacterEntites.Any(e => e.IsAttacking);

                return isAttack;
            }
        }

        private bool GetDodgeInput(Character.Entity characterEntity, bool isPlayer)
        {
            if (characterEntity.IsDodging) return false;

            if (isPlayer)
            {
                return Input.GetKeyUp(KeyCode.E);
            }
            else
            {
                var distance = Vector3.Distance(_playerCharacterEntity.Anim.rootPosition, characterEntity.Anim.rootPosition);
                var targetDotForward = GetDot(_playerCharacterEntity.Anim.transform, characterEntity.Anim.bodyPosition, Vector3.forward);
                
                var isDodge = true;
                isDodge &= targetDotForward > _enemyDodgeDotTrigger;
                isDodge &= distance < _enemyDodgeDistance;
                isDodge &= _playerCharacterEntity.IsAttacking;

                return isDodge;
            }
        }
    }
}

