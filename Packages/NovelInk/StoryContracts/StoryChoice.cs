using System;

namespace Novels.StoryContracts
{
    public readonly struct StoryChoice
    {
        public StoryChoice(int id, string text, string icon = null)
        {
            Id = id;
            Text = text ?? string.Empty;
            Icon = icon ?? string.Empty;
        }

        public int Id { get; }
        public string Text { get; }
        public string Icon { get; }
    }

    public readonly struct StoryDecision
    {
        private StoryDecision(bool hasChoice, int choiceId)
        {
            HasChoice = hasChoice;
            ChoiceId = choiceId;
        }

        public bool HasChoice { get; }
        public int ChoiceId { get; }

        public static StoryDecision Advance => new(false, default);

        public static StoryDecision Choice(int choiceId)
        {
            if (choiceId < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(choiceId),
                    choiceId,
                    "Choice ID must not be negative.");
            }
            return new StoryDecision(true, choiceId);
        }
    }
}
