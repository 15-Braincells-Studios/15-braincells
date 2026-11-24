using System;
using System.Collections.Generic;
using System.Linq;

namespace FifteenBraincells.MurderGenerator.Data
{
    /// <summary>Builds a database without the rows that take place in, or talk about, a closed room.</summary>
    internal static class ClosedRoomFilter
    {
        public static GeneratorDatabase Apply(GeneratorDatabase db, IEnumerable<string> roomIds)
        {
            var closed = new HashSet<string>(db.ClosedRoomIds);
            foreach (var id in roomIds ?? Enumerable.Empty<string>())
            {
                var group = db.ClosableRooms.FirstOrDefault(g => g.Contains(id));
                if (group == null)
                {
                    throw new ArgumentException($"Room '{id}' cannot be closed. Closable: " +
                                                string.Join(", ", db.ClosableRooms.SelectMany(g => g)));
                }

                closed.UnionWith(group);
            }

            bool IsClosed(string roomId) => IsRoomClosed(closed, roomId);
            bool Mentions(IReadOnlyList<string> rooms) => rooms != null && rooms.Any(closed.Contains);
            bool LineOk(LineSpec line) => line == null || !IsClosed(line.Room.RoomId);

            // An alibi stays when it is not in a closed room and at least one of its lines is still open.
            // The closed lines are skipped by the generator (GeneratorDatabase.IsOpen).
            var alibis = db.Alibis
                .Where(a => (a.IsOutside || !IsClosed(a.RoomId))
                            && a.Variants.Any(v => !Mentions(v.MentionedRooms) && LineOk(v.Line)))
                .ToList();
            var alive = new HashSet<Alibi>(alibis);

            var clues = db.MethodClues.Where(c => !Mentions(c.MentionedRooms) && LineOk(c.Line)).ToList();
            var plans = db.VictimPlans
                .Where(p => !Mentions(p.MentionedRooms) && (p.Alibi == null || alive.Contains(p.Alibi)))
                .ToList();

            // A method needs an open room, a plan (if it uses plans) and a clue that says when and where.
            var methods = db.Methods.Where(m =>
                    (m.RoomIds.Count == 0 || m.RoomIds.Any(r => !IsClosed(r)))
                    && (m.Kind == MethodKind.Fixed || plans.Any(p => p.Method == m))
                    && clues.Any(c => c.Method == m && c.Group == ClueGroup.WhenAndWhere))
                .ToList();
            clues = clues.Where(c => methods.Contains(c.Method)).ToList();
            plans = plans.Where(p => methods.Contains(p.Method)).ToList();

            return new GeneratorDatabase(
                db.Settings,
                db.Characters,
                db.Rooms,
                methods,
                db.ClueGroups,
                clues,
                db.Motives.Where(m => !Mentions(m.MentionedRooms) && LineOk(m.Clue)).ToList(),
                alibis,
                db.Contradictions
                    .Where(c => !Mentions(c.MentionedRooms) && LineOk(c.Line) && (!c.IsSpecific || alive.Contains(c.Alibi)))
                    .ToList(),
                db.RedHerrings
                    .Where(h => !Mentions(h.MentionedRooms) && LineOk(h.Suspicion.Line) && LineOk(h.Explanation.Line))
                    .ToList(),
                plans,
                db.ClosableRooms,
                closed.ToList());
        }

        /// <summary>A group bathroom (alibi) or victim bathroom (method) only counts as closed when both bathrooms are.</summary>
        public static bool IsRoomClosed(ICollection<string> closed, string roomId) =>
            roomId == RoomRef.GroupBathroom || roomId == RoomRef.VictimBathroom
                ? closed.Contains(RoomRef.RoyalBathroom) && closed.Contains(RoomRef.StaffBathroom)
                : closed.Contains(roomId);
    }
}
