using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace Game.Battle.View
{
    internal sealed class Scene : MonoBehaviour
    {
        internal struct Ctx
        {
            internal GameObject PlayerCharacterGO;
            internal List<GameObject> EnemyCharactersGO;
            internal Action<float> OnUpdate;
            internal Action<int> OnComplete;
        }

        [SerializeField] private LayerMask _navMeshLayers;
        [SerializeField] private GameObject _playerCharacterPoint;
        [SerializeField] private GameObject[] _enemyCharacterPoints;

        private bool _sceneDone = false;

        private Ctx _ctx;

        internal GameObject PlayerCharacterPoint => _playerCharacterPoint;
        internal GameObject[] EnemyCharacterPoints => _enemyCharacterPoints;

        private Animator _playerAnim;
        private Animator[] _enemyAnims;

        private NavMeshDataInstance _navMeshDataInstance;

        internal void Setup(Ctx ctx)
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

        internal void Release() 
        {
            if (this != null) GameObject.Destroy(gameObject);
        }
    }
}

