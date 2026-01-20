using UnityEngine;

namespace CodeStrippingFix
{
    public class SelfRemover : MonoBehaviour
    {
        private void Start()
        {
            Destroy(gameObject);
        }
    }

}
