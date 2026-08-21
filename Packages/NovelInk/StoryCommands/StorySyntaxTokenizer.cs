using System.Collections.Generic;
using System.Text;

namespace Novels.StoryCommands
{
    internal static class StorySyntaxTokenizer
    {
        internal static int IndexOfUnescaped(string source, char separator)
        {
            var escaped = false;
            for (var index = 0; index < source.Length; index++)
            {
                var current = source[index];
                if (!escaped && current == separator)
                    return index;
                if (current == '\\' && !escaped)
                {
                    escaped = true;
                    continue;
                }
                escaped = false;
            }
            return -1;
        }

        internal static string[] SplitArguments(string source)
        {
            var result = new List<string>();
            var current = new StringBuilder();
            var escaped = false;
            foreach (var character in source)
            {
                if (!escaped && character == ',')
                {
                    AddArgument(result, current);
                    continue;
                }
                if (!escaped && character == '\\')
                {
                    escaped = true;
                    continue;
                }
                current.Append(character);
                escaped = false;
            }
            if (escaped)
                current.Append('\\');
            AddArgument(result, current);
            return result.ToArray();
        }

        private static void AddArgument(
            ICollection<string> target,
            StringBuilder source)
        {
            var argument = source.ToString().Trim();
            source.Clear();
            if (argument.Length > 0)
                target.Add(argument);
        }
    }
}
