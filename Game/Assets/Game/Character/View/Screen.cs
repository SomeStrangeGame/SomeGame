using UnityEngine;

namespace Game.Character.View
{
    public sealed class Screen : MonoBehaviour
    {
        public void Release() 
        {
            if (this != null) GameObject.Destroy(gameObject);
        }
    }

}

