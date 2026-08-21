namespace Novels
{
    internal sealed class EpisodePresentation
    {
        internal Loading.Entity Loading { get; set; }
        internal Audio.AudioController Audio { get; set; }
        internal Bubble.BubbleController Bubble { get; set; }
        internal Character.CharacterController Character { get; set; }
        internal Choose.ChooseController Choose { get; set; }
        internal Location.LocationController Location { get; set; }
        internal Notification.NotificationController Notification { get; set; }
        internal Wardrobe.WardrobeController Wardrobe { get; set; }
    }
}
