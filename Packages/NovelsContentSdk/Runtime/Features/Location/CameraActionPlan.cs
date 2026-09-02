namespace Novels.Location
{
    public static class CameraActionCapabilities
    {
        public static bool IsSupported(StoryContracts.StoryCameraAction action) =>
            CameraActionPlan.TryCreate(action, out _);
    }

    internal enum CameraActionPresentation
    {
        Motion,
        TimedEffect,
        TransientEffect,
    }

    internal readonly struct CameraActionPlan
    {
        private CameraActionPlan(
            CameraActionPresentation presentation,
            View.LocationScreen.CameraEffect motion,
            View.LocationScreen.Effect effect,
            int effectHoldDurationMilliseconds)
        {
            Presentation = presentation;
            Motion = motion;
            Effect = effect;
            EffectHoldDurationMilliseconds = effectHoldDurationMilliseconds;
        }

        internal CameraActionPresentation Presentation { get; }
        internal View.LocationScreen.CameraEffect Motion { get; }
        internal View.LocationScreen.Effect Effect { get; }
        internal int EffectHoldDurationMilliseconds { get; }

        internal static bool TryCreate(
            StoryContracts.StoryCameraAction action,
            out CameraActionPlan plan)
        {
            switch (action)
            {
                case StoryContracts.StoryCameraAction.FadeIn:
                    plan = TimedEffect(View.LocationScreen.Effect.Dark, 1000);
                    return true;
                case StoryContracts.StoryCameraAction.PanLeftToRight:
                    plan = MotionPlan(View.LocationScreen.CameraEffect.LeftRight);
                    return true;
                case StoryContracts.StoryCameraAction.PanRightToLeft:
                    plan = MotionPlan(View.LocationScreen.CameraEffect.RightLeft);
                    return true;
                case StoryContracts.StoryCameraAction.MoveToCenter:
                    plan = MotionPlan(View.LocationScreen.CameraEffect.ToCenter);
                    return true;
                case StoryContracts.StoryCameraAction.MoveToLeft:
                    plan = MotionPlan(View.LocationScreen.CameraEffect.ToLeft);
                    return true;
                case StoryContracts.StoryCameraAction.MoveToRight:
                    plan = MotionPlan(View.LocationScreen.CameraEffect.ToRight);
                    return true;
                case StoryContracts.StoryCameraAction.Shake:
                    plan = MotionPlan(View.LocationScreen.CameraEffect.Shaking);
                    return true;
                case StoryContracts.StoryCameraAction.Injury:
                    plan = TransientEffect(View.LocationScreen.Effect.Dark);
                    return true;
                case StoryContracts.StoryCameraAction.Splashes:
                    plan = TransientEffect(View.LocationScreen.Effect.Light);
                    return true;
                default:
                    plan = default;
                    return false;
            }
        }

        private static CameraActionPlan MotionPlan(View.LocationScreen.CameraEffect motion) =>
            new(CameraActionPresentation.Motion, motion, default, 0);

        private static CameraActionPlan TimedEffect(
            View.LocationScreen.Effect effect,
            int holdDurationMilliseconds) =>
            new(CameraActionPresentation.TimedEffect, default, effect, holdDurationMilliseconds);

        private static CameraActionPlan TransientEffect(View.LocationScreen.Effect effect) =>
            new(CameraActionPresentation.TransientEffect, default, effect, 0);
    }
}
