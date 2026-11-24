using System.Collections.Generic;
using System.Linq;
using FifteenBraincells.MurderGenerator.Data;
using FifteenBraincells.MurderGenerator.Generation;
using FifteenBraincells.MurderGenerator.Output;
using NUnit.Framework;

namespace FifteenBraincells.MurderGenerator.Tests
{
    /// <summary>Closed rooms: nothing in a level may take place in a closed room or talk about it.</summary>
    [TestFixture]
    public sealed class ClosedRoomTests
    {
        private const int LevelsPerChoice = 150;

        private static GeneratorDatabase Db => TestData.Database;

        private static IEnumerable<string[]> Choices() =>
            Db.ClosableRooms.Select(g => g.ToArray()).Append(Db.ClosableRooms.SelectMany(g => g).ToArray());

        [Test]
        public void ClosedRoomsLeaveNoTrace()
        {
            var problems = new List<string>();
            foreach (var choice in Choices())
            {
                var db = Db.WithClosedRooms(choice);
                var closed = new HashSet<string>(db.ClosedRoomIds);
                var generator = new LevelGenerator(db);
                var openVariants = new HashSet<string>(db.Alibis.SelectMany(a => a.Variants).Where(db.IsOpen).Select(v => v.Line.TextKey));
                var rows = new HashSet<string>(db.MethodClues.Select(c => c.Id)
                    .Concat(db.Motives.Select(m => m.Id))
                    .Concat(db.Contradictions.Select(c => c.Id))
                    .Concat(db.RedHerrings.Select(h => h.Id))
                    .Concat(db.Alibis.Select(a => a.Id)));
                for (var seed = 1; seed <= LevelsPerChoice; seed++)
                {
                    var level = generator.Generate(seed);
                    var where = $"closed {string.Join("+", choice)}, seed {seed}";
                    if (closed.Contains(level.Solution.MurderRoom.Room.Id))
                    {
                        problems.Add($"{where}: murder in {level.Solution.MurderRoom}");
                    }

                    foreach (var clue in level.Clues)
                    {
                        if (clue.Category == ClueCategory.ClosedRoom)
                        {
                            continue;
                        }

                        if (clue.Room != null && closed.Contains(clue.Room.Room.Id))
                        {
                            problems.Add($"{where}: {clue} lies in {clue.Room}");
                        }

                        if (clue.Category != ClueCategory.Plan && !rows.Contains(clue.SourceId))
                        {
                            problems.Add($"{where}: {clue} comes from a row that should be left out");
                        }

                        if (clue.Category == ClueCategory.Alibi && !openVariants.Contains(clue.TextKey))
                        {
                            problems.Add($"{where}: {clue} uses a line about a closed room");
                        }

                        foreach (var room in clue.Args.Values.OfType<RoomArg>())
                        {
                            if (closed.Contains(room.Room.Room.Id))
                            {
                                problems.Add($"{where}: {clue} names {room.Room}");
                            }
                        }
                    }

                    foreach (var alibi in level.Alibis.Where(a => !a.Alibi.IsOutside))
                    {
                        if (closed.Contains(alibi.Alibi.RoomFor(alibi.Holder)))
                        {
                            problems.Add($"{where}: {alibi.Holder.Id} has an alibi in {alibi.Alibi.RoomFor(alibi.Holder)}");
                        }
                    }

                    var notes = level.Clues.Where(c => c.Category == ClueCategory.ClosedRoom).Select(c => c.Room.Room.Id).ToList();
                    if (!closed.SetEquals(notes) || notes.Count != closed.Count)
                    {
                        problems.Add($"{where}: door notes {string.Join(",", notes)}");
                    }
                }
            }

            Assert.IsEmpty(problems);
        }

        [Test]
        public void BathroomsCloseTogether()
        {
            var db = Db.WithClosedRooms(new[] { RoomRef.RoyalBathroom });
            Assert.IsTrue(db.IsClosed(RoomRef.StaffBathroom), "closing one bathroom closes both");
        }

        [Test]
        public void OnlyClosableRoomsCanBeClosed()
        {
            Assert.Throws<System.ArgumentException>(() => Db.WithClosedRooms(new[] { "kitchen" }));
        }

        [Test]
        public void ClosingRoomsRemovesTheMethodsThatNeedThem()
        {
            var db = Db.WithClosedRooms(new[] { "library", "study_room" });
            Assert.IsTrue(db.Methods.All(m => !m.RoomIds.Contains("library") && !m.RoomIds.Contains("study_room")),
                "a method in a closed room is left out");
            Assert.IsTrue(db.Methods.Count < Db.Methods.Count && db.Methods.Count > 0, "the other methods stay");
        }
    }
}
