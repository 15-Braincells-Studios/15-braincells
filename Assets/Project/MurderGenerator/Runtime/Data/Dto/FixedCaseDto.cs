using System;
using System.Collections.Generic;

// A hand-built case: a level written by a designer instead of generated from a seed.
// FixedCaseBuilder checks it against the generator data and turns it into a normal Level.
// Every field has a default so that missing JSON fields never produce nulls.
namespace FifteenBraincells.MurderGenerator.Data.Dto
{
    [Serializable]
    public sealed class FixedCaseDto
    {
        /// <summary>The number shown as the level number (Level.Seed). Only a label: nothing is generated from it.</summary>
        public int number;

        /// <summary>Method id. Only methods of kind Fixed are supported.</summary>
        public string method = "";

        /// <summary>Room id of the murder. May be left empty when the method has only one room.</summary>
        public string murderRoom = "";

        /// <summary>"18:05".</summary>
        public string murderTime = "";

        public string murderer = "";
        public string victim = "";

        /// <summary>The real motive, the fake alibi and the contradiction that breaks it (ids).</summary>
        public string motive = "";
        public string fakeAlibi = "";
        public string contradiction = "";

        /// <summary>Every clue in the level. Spoken lines keep this order (after the speaker's own alibi line).</summary>
        public List<FixedClueDto> clues = new List<FixedClueDto>();
    }

    [Serializable]
    public sealed class FixedClueDto
    {
        /// <summary>"method", "motive", "alibi", "contradiction" or "herring".</summary>
        public string type = "";

        /// <summary>The row id in generator_data.json: a method clue, motive, alibi, contradiction or red herring.</summary>
        public string id = "";

        /// <summary>Who says it, when the data leaves the speaker open (witnesses, explainers).</summary>
        public string speaker = "";

        /// <summary>Herrings: who says the explanation, when the data leaves it open.</summary>
        public string explainer = "";

        /// <summary>Alibis: the holder. Herrings: who the herring is about.</summary>
        public string character = "";

        /// <summary>Alibis: the second character, for alibis with a partner.</summary>
        public string partner = "";

        /// <summary>Alibis that need a sick character (Caring for the sick).</summary>
        public string sickCharacter = "";

        /// <summary>Alibis: which line of the alibi, starting at 1. 0 means 1.</summary>
        public int variant;

        /// <summary>Motives: who has the motive, and against whom.</summary>
        public string suspect = "";
        public string victim = "";

        /// <summary>Method clues: the characters for "(excluded)" or "(partial list)".</summary>
        public List<string> characters = new List<string>();

        /// <summary>Contradictions whose text names "{other_place}": that room's id.</summary>
        public string otherRoom = "";
    }
}
