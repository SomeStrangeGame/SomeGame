using System;

namespace Novels.BubbleContracts
{
    public static class BubbleTriggers
    {
        public const string Wardrobe = "some wardrobe trigger";
        public const string Choose = "some choose trigger";

        public static BubblePresentationKind Resolve(string value)
        {
            if (string.Equals(value, Wardrobe, StringComparison.Ordinal))
                return BubblePresentationKind.Wardrobe;
            if (string.Equals(value, Choose, StringComparison.Ordinal))
                return BubblePresentationKind.Choose;
            return BubblePresentationKind.Dialogue;
        }
    }

    public enum BubblePresentationKind
    {
        Dialogue,
        Wardrobe,
        Choose,
    }

    public static class BubbleTextKeys
    {
        public const string Disclaimer = "bubble.disclaimer";
        public const string Hint = "bubble.hint";
    }

    public sealed class WardrobePresentation
    {
        public WardrobePresentation(Action onCompleted)
        {
            OnCompleted = onCompleted ?? throw new ArgumentNullException(nameof(onCompleted));
        }

        public Action OnCompleted { get; }
    }

    public sealed class ChoosePresentation
    {
        public ChoosePresentation(Action onCompleted)
        {
            OnCompleted = onCompleted ?? throw new ArgumentNullException(nameof(onCompleted));
        }

        public Action OnCompleted { get; }
    }

    public sealed class BubblePresentation
    {
        public BubblePresentation(
            string name,
            StoryContracts.StorySpeakerRole speakerRole,
            StoryContracts.DialoguePresentation dialoguePresentation,
            BubbleText text,
            BubbleChoice[] choices,
            Action onBackgroundClick)
        {
            Name = name ?? string.Empty;
            SpeakerRole = speakerRole;
            DialoguePresentation = dialoguePresentation;
            Text = text;
            Choices = choices ?? Array.Empty<BubbleChoice>();
            OnBackgroundClick = onBackgroundClick;
        }

        public string Name { get; }
        public StoryContracts.StorySpeakerRole SpeakerRole { get; }
        public StoryContracts.DialoguePresentation DialoguePresentation { get; }
        public BubbleText Text { get; }
        public BubbleChoice[] Choices { get; }
        public Action OnBackgroundClick { get; }
    }

    public readonly struct BubbleText
    {
        public BubbleText(string header, string text)
        {
            Header = header ?? string.Empty;
            Text = text ?? string.Empty;
        }

        public string Header { get; }
        public string Text { get; }
    }

    public readonly struct BubbleChoice
    {
        public BubbleChoice(int id, string text, Action<int> onClick)
        {
            Id = id;
            Text = text ?? string.Empty;
            OnClick = onClick;
        }

        public int Id { get; }
        public string Text { get; }
        public Action<int> OnClick { get; }
    }
}
