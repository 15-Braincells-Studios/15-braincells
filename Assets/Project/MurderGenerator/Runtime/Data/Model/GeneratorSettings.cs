namespace FifteenBraincells.MurderGenerator.Data
{
    /// <summary>Tuning values for the generator. Read from the "settings" block of generator_data.json.</summary>
    public sealed class GeneratorSettings
    {
        /// <summary>Chance to use an alibi-specific contradiction when one is available (else a general one).</summary>
        public float SpecificContradictionChance { get; internal set; }

        /// <summary>Chance that an innocent character gets a real alibi before the solver asks for more.</summary>
        public float InnocentAlibiChance { get; internal set; }

        public int FalseMotivesMin { get; internal set; }
        public int FalseMotivesMax { get; internal set; }
        public int RedHerringsMin { get; internal set; }
        public int RedHerringsMax { get; internal set; }

        /// <summary>Upper limit when extra red herrings are added so that everyone has a line.</summary>
        public int RedHerringsLimit { get; internal set; }

        /// <summary>How many characters a "(partial list)" names besides the real one.</summary>
        public int PartialListExtraMin { get; internal set; }

        public int PartialListExtraMax { get; internal set; }

        /// <summary>How many characters "(excluded)" names.</summary>
        public int ExcludedMin { get; internal set; }

        public int ExcludedMax { get; internal set; }

        /// <summary>How often a level is started over before generation gives up.</summary>
        public int MaxAttempts { get; internal set; }

        /// <summary>How often the solver may ask for an extra alibi to make the answer unique.</summary>
        public int MaxExtraAlibiRounds { get; internal set; }

        /// <summary>A character needs this tag for anything that needs <see cref="Need.Lifting"/>.</summary>
        public string LiftingRequiresTag { get; internal set; }
    }
}
