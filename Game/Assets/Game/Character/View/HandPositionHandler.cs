using System;
using UnityEngine;

namespace Game.Character.View
{
    internal sealed class HandPositionHandler : MonoBehaviour
    {
        internal Vector3 Pos => transform.position;
        internal Quaternion Rot => transform.rotation;
    }
}
