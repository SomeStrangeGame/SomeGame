using UnityEngine;

namespace Game.Chapter_1.View
{
    internal sealed class Screen : MonoBehaviour
    {
        internal void Release() 
        {
            if (this != null) GameObject.Destroy(gameObject);
        }
    }

}

