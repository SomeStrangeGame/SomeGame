using System;
using UnityEngine;

namespace Game.SomeBattleScene1.View
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

        internal void Setup(Ctx ctx)
        {
            _ctx = ctx;
        }

        private void Update()
        {
            if (_sceneDone) return;

            _ctx.OnUpdate.Invoke(Time.deltaTime);

            if (Input.GetKeyUp(KeyCode.Escape))
            {
                _sceneDone = true;
                _ctx.OnComplete.Invoke(2);
            }
        }

        internal void Release() 
        {
            if (this != null) GameObject.Destroy(gameObject);
        }
    }
}

