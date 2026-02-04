using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace Game.Battle.View
{
    public sealed class Scene : MonoBehaviour
    {
        [Serializable]
        public struct CharacterPoint
        {
            [SerializeField] private Transform _characterPoint;
            [SerializeField] private int _health;

            public readonly Transform Point => _characterPoint;
            public readonly int Health => _health;
        }

        public struct Ctx
        {
            public GameObject PlayerCharacterGO;
            public List<GameObject> EnemyCharactersGO;
            public Action<float> OnUpdate;
            public Action<int> OnComplete;
        }

        [SerializeField] private LayerMask _navMeshLayers;
        [SerializeField] private CharacterPoint _playerCharacterPoint;
        [SerializeField] private CharacterPoint[] _enemyCharacterPoints;

        private bool _sceneDone = false;

        private Ctx _ctx;

        public CharacterPoint PlayerCharacterPoint => _playerCharacterPoint;
        public CharacterPoint[] EnemyCharacterPoints => _enemyCharacterPoints;

        private Animator _playerAnim;
        private Animator[] _enemyAnims;

        private NavMeshDataInstance _navMeshDataInstance;

        public void Setup(Ctx ctx)
        {
            _ctx = ctx;

            _playerAnim = _ctx.PlayerCharacterGO.GetComponent<Animator>();
            _enemyAnims = _ctx.EnemyCharactersGO.Select(c => c.GetComponent<Animator>()).ToArray();
        }

        private void OnEnable()
        {
            CreateNavMesh();
        }

        private void OnDisable()
        {
            RemoveNavMesh();
        }

        private void CreateNavMesh() 
        {
            var buildSources = new List<NavMeshBuildSource>();
            NavMeshBuilder.CollectSources(transform, _navMeshLayers, NavMeshCollectGeometry.PhysicsColliders, 0, new List<NavMeshBuildMarkup>(), buildSources);
            var boundsSize = new Vector3(50, 50, 50);
            var bounds = new Bounds(transform.position, boundsSize);
            var navData = NavMeshBuilder.BuildNavMeshData(NavMesh.GetSettingsByID(0), buildSources, bounds, Vector3.down, Quaternion.Euler(Vector3.up));
            _navMeshDataInstance = NavMesh.AddNavMeshData(navData);
        }

        private void RemoveNavMesh() 
        {
            _navMeshDataInstance.Remove();
        }

        private void Update()
        {
            if (_sceneDone) return;
            if (_ctx.OnUpdate == null) return;

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
    }
}

