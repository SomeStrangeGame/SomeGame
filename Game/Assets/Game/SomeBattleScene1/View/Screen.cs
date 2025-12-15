using UnityEngine;

namespace Game.SomeBattleScene1.View
{
    internal sealed class Screen : MonoBehaviour
    {
        internal struct Ctx
        {
            
        }

        private Ctx _ctx;

        internal void Setup (Ctx ctx)
        {
            _ctx = ctx;
        }

        internal void Release() 
        {
            if (this != null) GameObject.Destroy(gameObject);
        }
    }

}

