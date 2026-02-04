using UnityEngine;

namespace Game.Character.View
{
    public class LongBowCharacter : Character
    {
        private const string _attackParam = "IsAttack";

        protected override Vector3 GetLookAtTargetPosition()
        {
            return _ctx.GetLookAtTargetPosition.Invoke(true);
        }

        protected override void OnAnimatorIK(int layerIndex)
        {
            base.OnAnimatorIK(layerIndex);
            Anim.SetBool(AnimHash(_attackParam), _ctx.GetAttackInput.Invoke());
        }
    }
}

