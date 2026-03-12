using UnityEngine;

namespace BattleStory.Character.View
{
    internal sealed class HandPositionHandler : MonoBehaviour
    {
        internal Vector3 Pos => transform.position;
        internal Quaternion Rot => transform.rotation;
    }
}
