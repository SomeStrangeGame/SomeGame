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
        PersistentEffect,
        TransientEffect,
    }

    internal readonly struct CameraActionPlan
    {
        private CameraActionPlan(
            CameraActionPresentation presentation,
            View.Screen.CameraEffect motion,
            View.Screen.Effect effect)
        {
            Presentation = presentation;
            Motion = motion;
            Effect = effect;
        }

        internal CameraActionPresentation Presentation { get; }
        internal View.Screen.CameraEffect Motion { get; }
        internal View.Screen.Effect Effect { get; }

        internal static bool TryCreate(
            StoryContracts.StoryCameraAction action,
            out CameraActionPlan plan)
        {
            switch (action)
            {
                case StoryContracts.StoryCameraAction.FadeIn:
                    plan = PersistentEffect(View.Screen.Effect.Dark);
                    return true;
                case StoryContracts.StoryCameraAction.PanLeftToRight:
                    plan = MotionPlan(View.Screen.CameraEffect.LeftRight);
                    return true;
                case StoryContracts.StoryCameraAction.PanRightToLeft:
                    plan = MotionPlan(View.Screen.CameraEffect.RightLeft);
                    return true;
                case StoryContracts.StoryCameraAction.MoveToCenter:
                    plan = MotionPlan(View.Screen.CameraEffect.ToCenter);
                    return true;
                case StoryContracts.StoryCameraAction.MoveToLeft:
                    plan = MotionPlan(View.Screen.CameraEffect.ToLeft);
                    return true;
                case StoryContracts.StoryCameraAction.Shake:
                    plan = MotionPlan(View.Screen.CameraEffect.Shaking);
                    return true;
                case StoryContracts.StoryCameraAction.Injury:
                    plan = TransientEffect(View.Screen.Effect.Dark);
                    return true;
                case StoryContracts.StoryCameraAction.Splashes:
                    plan = TransientEffect(View.Screen.Effect.Light);
                    return true;
                default:
                    plan = default;
                    return false;
            }
        }

        private static CameraActionPlan MotionPlan(View.Screen.CameraEffect motion) =>
            new(CameraActionPresentation.Motion, motion, default);

        private static CameraActionPlan PersistentEffect(View.Screen.Effect effect) =>
            new(CameraActionPresentation.PersistentEffect, default, effect);

        private static CameraActionPlan TransientEffect(View.Screen.Effect effect) =>
            new(CameraActionPresentation.TransientEffect, default, effect);
    }
}
