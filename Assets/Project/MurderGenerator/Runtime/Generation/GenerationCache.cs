using System.Collections.Generic;
using System.Linq;
using FifteenBraincells.MurderGenerator.Data;

namespace FifteenBraincells.MurderGenerator.Generation
{
    /// <summary>A murderer and victim who fit a method, with the motives that fit them.</summary>
    internal sealed class Crime
    {
        public Crime(Method method, Character suspect, Character victim, List<Motive> motives)
        {
            Method = method;
            Suspect = suspect;
            Victim = victim;
            Motives = motives;
        }

        public Method Method { get; }
        public Character Suspect { get; }
        public Character Victim { get; }
        public List<Motive> Motives { get; }
    }

    /// <summary>
    /// Facts that only depend on the data, worked out once per <see cref="LevelGenerator"/>: which crimes each
    /// method allows and on how many pairs each motive fits. Used to spread the choices evenly.
    /// </summary>
    internal sealed class GenerationCache
    {
        public GenerationCache(GeneratorDatabase db)
        {
            var cast = db.Characters;
            MotiveFit = db.Motives.ToDictionary(m => m, m => cast.Sum(s => cast.Count(v => m.Fits(s, v))));
            foreach (var method in db.Methods)
            {
                var crimes = new List<Crime>();
                foreach (var suspect in cast.Where(method.Suspects.Matches))
                {
                    foreach (var victim in cast.Where(v => method.Victims.Matches(v) && PlansFor(db, method, v).Count > 0))
                    {
                        var motives = db.Motives.Where(m => m.Fits(suspect, victim)).ToList();
                        if (motives.Count > 0)
                        {
                            crimes.Add(new Crime(method, suspect, victim, motives));
                        }
                    }
                }

                Crimes[method] = crimes;
            }
        }

        /// <summary>On how many (murderer, victim) pairs a motive fits, ignoring the method.</summary>
        public Dictionary<Motive, int> MotiveFit { get; }

        public Dictionary<Method, List<Crime>> Crimes { get; } = new Dictionary<Method, List<Crime>>();

        public double MotiveWeight(Motive motive) => 1.0 / System.Math.Max(1, MotiveFit[motive]);

        /// <summary>The plans a character can have with this method. For Fixed methods: a list with one null.</summary>
        public static List<VictimPlan> PlansFor(GeneratorDatabase db, Method method, Character character)
        {
            if (method.Kind == MethodKind.Fixed)
            {
                return new List<VictimPlan> { null };
            }

            return db.GetPlans(method)
                .Where(p => p.Fits(character) && (!p.IsSolo || SoloTimes(db.Settings, p).Count > 0))
                .ToList();
        }

        /// <summary>The times a murder can happen inside a solo plan.</summary>
        public static List<ClockTime> SoloTimes(GeneratorSettings s, VictimPlan plan) =>
            Steps(s,
                System.Math.Max(s.SoloEarliest.Minutes, plan.Alibi.Start.Minutes + s.SoloMarginMinutes),
                System.Math.Min(s.SoloLatest.Minutes, plan.Alibi.End.Minutes - s.SoloMarginMinutes));

        /// <summary>Every time from first to last (both included) in steps of <see cref="GeneratorSettings.TimeStepMinutes"/>.</summary>
        public static List<ClockTime> Steps(GeneratorSettings s, int first, int last)
        {
            var step = System.Math.Max(1, s.TimeStepMinutes);
            var times = new List<ClockTime>();
            for (var m = (first + step - 1) / step * step; m <= last; m += step)
            {
                times.Add(new ClockTime(m));
            }

            return times;
        }
    }
}
