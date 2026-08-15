using System;

namespace Novels.StoryContracts
{
    public readonly struct StoryChoice
    {
        public StoryChoice(int id, string text)
        {
            Id = id;
            Text = text ?? string.Empty;
        }

        public int Id { get; }
        public string Text { get; }
    }
}
