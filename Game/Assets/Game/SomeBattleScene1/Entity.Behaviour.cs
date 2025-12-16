using System;
using System.Linq;
using UnityEngine;

namespace Game.SomeBattleScene1
{
    public sealed partial class Entity
    {
        private const float _defaultLookAtOffset = 5f;

        private const float _enemyMoveDistance = 1.9f;

        private const float _enemyAttackDistance = 2.2f;

        private const float _enemyDodgeDotTrigger = 0.1f;
        private const float _enemyDodgeDistance = 2.1f;

        private Vector3 GetTargetPosition(Character.Entity characterEntity, bool isPlayer)
        {
            var targetPos = characterEntity.CharacterTransform.position;

            if (isPlayer)
            {
                targetPos += Vector3.right * SimpleInput.GetAxis("Horizontal");
                targetPos += Vector3.forward * SimpleInput.GetAxis("Vertical");
            }
            else
            {
                var heading = _playerCharacterEntity.CharacterTransform.position - characterEntity.CharacterTransform.position;
                var distance = heading.magnitude;
                var direction = heading / distance;
                targetPos = _playerCharacterEntity.CharacterTransform.position - direction * _enemyMoveDistance;
            }
            return targetPos;
        }

        private Vector3 GetLookAtTargetPosition(Character.Entity characterEntity, bool isPlayer)
        {
            var lookAtPoint = characterEntity.CharacterTransform.position + characterEntity.CharacterTransform.forward * _defaultLookAtOffset;

            if (isPlayer)
            {
                var minDistance = float.MaxValue;
                foreach (var enemyEntity in _enemyCharacterEntites)
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
                lookAtPoint = _playerCharacterEntity.ChestTransform.position;
            }

            return lookAtPoint;
        }

        private bool GetAttackInput(Character.Entity characterEntity, bool isPlayer)
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
                if (!_playerCharacterEntity.Anim.enabled) return false;
                if (_playerCharacterEntity.IsHitting) return false;

                var distance = Vector3.SqrMagnitude(_playerCharacterEntity.Anim.rootPosition - characterEntity.Anim.rootPosition);
                if (distance > _enemyAttackDistance * _enemyAttackDistance) return false;
                if (_enemyCharacterEntites.Any(e => e.Anim.enabled && e.IsAttacking)) return false;

                return true;
            }
        }

        private bool GetDodgeInput(Character.Entity characterEntity, bool isPlayer)
        {
            var isDodge = true;
            isDodge &= characterEntity.Anim.enabled;
            isDodge &= !characterEntity.IsDodging;

            if (isPlayer)
            {
                isDodge &= SimpleInput.GetKeyUp(KeyCode.E);
            }
            else
            {
                var distance = Vector3.Distance(_playerCharacterEntity.Anim.rootPosition, characterEntity.Anim.rootPosition);
                var targetDotForward = GetDot(_playerCharacterEntity.Anim.transform, characterEntity.Anim.rootPosition, Vector3.forward);
                
                isDodge &= _playerCharacterEntity.Anim.enabled;
                isDodge &= targetDotForward > _enemyDodgeDotTrigger;
                isDodge &= distance < _enemyDodgeDistance;
                isDodge &= _playerCharacterEntity.IsAttacking;
                isDodge &= false;
            }

            return isDodge;
        }
    }
}

