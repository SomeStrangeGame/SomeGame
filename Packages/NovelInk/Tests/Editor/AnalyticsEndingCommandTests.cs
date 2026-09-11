using NUnit.Framework;

namespace Novels.StoryCommands.Tests
{
    public sealed class AnalyticsEndingCommandTests
    {
        [TestCase("analytics-ending: keeper", "keeper")]
        [TestCase("Аналитика-концовка: quiet-road", "quiet-road")]
        public void Parse_AcceptsStableEndingId(string source, string expected)
        {
            var result = new Entity().Parse(source, false);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Command, Is.TypeOf<AnalyticsEndingStoryCommand>());
            Assert.That(
                ((AnalyticsEndingStoryCommand)result.Command).Data.EndingId,
                Is.EqualTo(expected));
        }

        [TestCase("analytics-ending:")]
        [TestCase("analytics-ending: Has Spaces")]
        [TestCase("analytics-ending: КОНЦОВКА")]
        public void Parse_RejectsUnstableEndingId(string source)
        {
            var result = new Entity().Parse(source, false);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error.Code, Is.EqualTo("INVALID_ENDING_ID"));
        }
    }
}
