using System;
using System.Linq;
using UnityEngine;

namespace Game.Chapter_1.View
{
    internal sealed class Scene : MonoBehaviour
    {
        internal struct Ctx
        {
            internal Action<float> OnUpdate;
            internal Action<int> OnComplete;
        }

        [SerializeField] private GameObject _playerCharacter;
        [SerializeField] private GameObject[] _enemyCharacters;

        private bool _sceneDone = false;

        private Ctx _ctx;

        internal GameObject PlayerCharacter => _playerCharacter;
        internal GameObject[] EnemyCharacters => _enemyCharacters;

        private Animator _playerAnim;
        private Animator[] _enemyAnims;

        internal void Setup(Ctx ctx)
        {
            _ctx = ctx;

            _playerAnim = PlayerCharacter.GetComponent<Animator>();
            _enemyAnims = EnemyCharacters.Select(c => c.GetComponent<Animator>()).ToArray();
        }

        private void Update()
        {
            if (_sceneDone) return;

            _ctx.OnUpdate.Invoke(Time.deltaTime);

            if (Input.GetKeyUp(KeyCode.Escape))
            {
                _sceneDone = true;
                _ctx.OnComplete.Invoke(0);
            }

            if (!_playerAnim.enabled)
            {
                _sceneDone = true;
                _ctx.OnComplete.Invoke(0);
            }

            if (_enemyAnims.All(e => !e.enabled))
            {
                _sceneDone = true;
                _ctx.OnComplete.Invoke(1);
            }
        }

        internal void Release() 
        {
            if (this != null) GameObject.Destroy(gameObject);
        }
    }
}

