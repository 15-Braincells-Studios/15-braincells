using System.Linq;
using System.Text;
using FifteenBraincells.MurderGenerator.Output;

namespace FifteenBraincells.MurderGenerator.Text
{
    /// <summary>A readable overview of a level, for debugging and for checking the content.</summary>
    public static class LevelReport
    {
        public static string Describe(Level level, ClueTextRenderer renderer)
        {
            var solution = level.Solution;
            var text = new StringBuilder();
            text.AppendLine($"Level {level.Seed} (attempt {level.Attempts})");
            text.AppendLine($"  Method:        {solution.Method.Id} at {solution.Method.Time} in {solution.MurderRoom}");
            text.AppendLine($"  Murderer:      {solution.Suspect.Id}");
            text.AppendLine($"  Victim:        {solution.Victim.Id}");
            text.AppendLine($"  Motive:        {solution.Motive.Id}");
            text.AppendLine($"  Fake alibi:    {solution.FakeAlibi.Id}");
            text.AppendLine($"  Contradiction: {solution.Contradiction.Id}");
            if (level.Blocked.Count > 0)
            {
                text.AppendLine("  Blocked today: " + string.Join(", ", level.Blocked.Select(b => $"{b.Key.Id} ({b.Value})")));
            }

            text.AppendLine();
            text.AppendLine("What the characters say:");
            foreach (var character in level.Clues.Where(c => c.IsSpoken).Select(c => c.Speaker).Distinct().OrderBy(c => c.Id))
            {
                text.AppendLine($"  {renderer.RenderName(character)}");
                foreach (var clue in level.LinesOf(character))
                {
                    text.AppendLine($"    - {renderer.Render(clue)}  [{clue.Category}: {clue.SourceId}]");
                }
            }

            text.AppendLine();
            text.AppendLine("What lies in the rooms:");
            foreach (var room in level.Clues.Where(c => !c.IsSpoken).GroupBy(c => c.Room.ToString()).OrderBy(g => g.Key))
            {
                text.AppendLine($"  {room.Key}");
                foreach (var clue in room)
                {
                    text.AppendLine($"    - {renderer.Render(clue)}  [{clue.Category}: {clue.SourceId}]");
                }
            }

            return text.ToString();
        }
    }
}
