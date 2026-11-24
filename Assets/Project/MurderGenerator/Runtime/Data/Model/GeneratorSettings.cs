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

        /// <summary>Murder times are picked in steps of this many minutes.</summary>
        public int TimeStepMinutes { get; internal set; }

        /// <summary>Solo methods: the time of murder lies between these, and this many minutes inside the plan.</summary>
        public ClockTime SoloEarliest { get; internal set; }

        public ClockTime SoloLatest { get; internal set; }
        public int SoloMarginMinutes { get; internal set; }

        /// <summary>Sleep methods: the bedtime of a sleep plan that names none (Sick, Early to bed).</summary>
        public ClockTime DefaultBedtime { get; internal set; }

        /// <summary>Chance that a method that allows decoys gets one.</summary>
        public float DecoyChance { get; internal set; }

        /// <summary>False-lead contradictions: about an alibi nobody has, so they refute nothing.</summary>
        public int LooseContradictionsMin { get; internal set; }

        public int LooseContradictionsMax { get; internal set; }

        /// <summary>Chance for each loose contradiction above the minimum.</summary>
        public float SecondLooseContradictionChance { get; internal set; }

        /// <summary>No character says more than this many things in a level.</summary>
        public int MaxLinesPerCharacter { get; internal set; }

        /// <summary>A motive that fits many pairs gets a smaller chance; victim and murderer are picked first.</summary>
        public bool WeightedMotives { get; internal set; }

        /// <summary>An alibi that many characters may hold gets a smaller chance.</summary>
        public bool WeightedAlibis { get; internal set; }
    }
}
