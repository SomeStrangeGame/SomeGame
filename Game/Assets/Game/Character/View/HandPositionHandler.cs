using System;
using UnityEngine;

namespace Game.Character.View
{
    internal sealed class HandPositionHandler : MonoBehaviour
    {
        public Vector3 Pos => transform.position;
        public Quaternion Rot => transform.rotation;
    }
}
