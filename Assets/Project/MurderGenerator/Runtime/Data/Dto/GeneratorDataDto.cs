using System;
using System.Collections.Generic;

// Plain serialisable mirrors of generator_data.json.
// They only carry data; DatabaseBuilder validates them and turns them into the model classes.
// Every field has a default so that missing JSON fields never produce nulls (JsonUtility and
// System.Text.Json both keep the initialiser when a field is absent).
namespace FifteenBraincells.MurderGenerator.Data.Dto
{
    [Serializable]
    public sealed class GeneratorDataDto
    {
        public SettingsDto settings = new SettingsDto();
        public List<CharacterDto> characters = new List<CharacterDto>();
        public List<RoomDto> rooms = new List<RoomDto>();
        public List<MethodDto> methods = new List<MethodDto>();
        public List<ClueGroupDto> clueGroups = new List<ClueGroupDto>();
        public List<MethodClueDto> methodClues = new List<MethodClueDto>();
        public List<MotiveDto> motives = new List<MotiveDto>();
        public List<AlibiDto> alibis = new List<AlibiDto>();
        public List<ContradictionDto> contradictions = new List<ContradictionDto>();
        public List<RedHerringDto> redHerrings = new List<RedHerringDto>();
        public List<VictimPlanDto> victimPlans = new List<VictimPlanDto>();

        /// <summary>Rooms that can be closed for a level. Rooms in one entry are always closed together.</summary>
        public List<ClosableRoomDto> closableRooms = new List<ClosableRoomDto>();
    }

    [Serializable]
    public sealed class SettingsDto
    {
        public float specificContradictionChance = 0.5f;
        public float innocentAlibiChance = 0.6f;
        public int falseMotivesMin = 1;
        public int falseMotivesMax = 2;
        public int redHerringsMin = 1;
        public int redHerringsMax = 2;
        public int redHerringsLimit = 5;
        public int partialListExtraMin = 2;
        public int partialListExtraMax = 4;
        public int excludedMin = 1;
        public int excludedMax = 2;
        public int maxAttempts = 60;
        public int maxExtraAlibiRounds = 12;
        public string liftingRequiresTag = "strong";
        public int timeStepMinutes = 5;
        public string soloEarliest = "17:30";
        public string soloLatest = "20:30";
        public int soloMarginMinutes = 30;
        public string defaultBedtime = "19:00";
        public float decoyChance = 0.5f;
        public int looseContradictionsMin = 1;
        public int looseContradictionsMax = 2;
        public float secondLooseContradictionChance = 0.25f;
        public int maxLinesPerCharacter = 2;
        public bool weightedMotives = true;
        public bool weightedAlibis = true;
    }

    [Serializable]
    public sealed class TagFilterDto
    {
        public List<string> anyOf = new List<string>();
        public List<string> allOf = new List<string>();
        public List<string> noneOf = new List<string>();
    }

    [Serializable]
    public sealed class SpeakerDto
    {
        public string kind = "None";
        public string character = "";
    }

    [Serializable]
    public sealed class RoomRefDto
    {
        public string room = "";
        public string owner = "";
    }

    [Serializable]
    public sealed class CharacterDto
    {
        public string id = "";
        public string nameKey = "";
        public string referenceKey = "";
        public string role = "";
        public string sex = "";
        public string homeRoom = "";
        public List<string> tags = new List<string>();
    }

    [Serializable]
    public sealed class RoomDto
    {
        public string id = "";
        public string nameKey = "";
    }

    [Serializable]
    public sealed class MethodDto
    {
        public string id = "";
        public string nameKey = "";
        public string kind = "Fixed";
        public string windowStart = "";
        public string windowEnd = "";
        public List<string> rooms = new List<string>();
        public TagFilterDto suspects = new TagFilterDto();
        public TagFilterDto victims = new TagFilterDto();
        public int sleepDelayMin;
        public int sleepDelayMax;
        public bool noOtherOutsideAlibi;
        public bool allowDecoys;
    }

    [Serializable]
    public sealed class ClueGroupDto
    {
        public string group = "";
        public float showChance;
    }

    [Serializable]
    public sealed class ClueEffectDto
    {
        public string kind = "None";
        public string role = "Suspect";
        public List<string> characters = new List<string>();
    }

    [Serializable]
    public sealed class MethodClueDto
    {
        public string id = "";
        public string method = "";
        public string group = "";
        public string form = "";
        public RoomRefDto room = new RoomRefDto();
        public SpeakerDto speaker = new SpeakerDto();
        public string clarity = "";
        public string textKey = "";
        public ClueEffectDto effect = new ClueEffectDto();
        public List<string> notSuspect = new List<string>();
        public List<string> notVictim = new List<string>();
        public TagFilterDto excludedFilter = new TagFilterDto();
        public bool excludedGetNoOtherAlibi;
        public bool listedGetNoOtherAlibi;
        public bool listOmitsSpeaker;
        public List<string> excludedNoAlibiRooms = new List<string>();
        public List<string> excludedNotBlockedFor = new List<string>();
        public List<string> noOtherAlibiFor = new List<string>();
        public List<string> suspectHasTags = new List<string>();
        public bool excludedNotOnPartialList;
        public List<string> mentions = new List<string>();
        public List<string> blocks = new List<string>();
    }

    [Serializable]
    public sealed class LineDto
    {
        public string form = "";
        public SpeakerDto speaker = new SpeakerDto();
        public RoomRefDto room = new RoomRefDto();
        public string textKey = "";
    }

    [Serializable]
    public sealed class MotiveDto
    {
        public string id = "";
        public TagFilterDto suspects = new TagFilterDto();
        public TagFilterDto victims = new TagFilterDto();
        public bool sameRole;
        public bool differentSex;
        public string notBothTag = "";
        public List<string> mentions = new List<string>();
        public LineDto clue = new LineDto();
    }

    [Serializable]
    public sealed class AlibiPartnerDto
    {
        public string kind = "None";
        public string character = "";
        public TagFilterDto pool = new TagFilterDto();
        public string placeholder = "";
    }

    [Serializable]
    public sealed class AlibiVariantDto
    {
        public string form = "";
        public SpeakerDto speaker = new SpeakerDto();
        public RoomRefDto room = new RoomRefDto();
        public string holderTag = "";
        public List<string> mentions = new List<string>();
        public string textKey = "";
    }

    [Serializable]
    public sealed class AlibiDto
    {
        public string id = "";
        public string nameKey = "";
        public TagFilterDto holders = new TagFilterDto();
        public AlibiPartnerDto partner = new AlibiPartnerDto();
        public string start = "";
        public string end = "";
        public bool outside;
        public string room = "";
        public string placeKey = "";
        public List<string> needs = new List<string>();
        public List<string> notWithMethods = new List<string>();
        public string requiresOtherHolderOf = "";
        public bool oneInSameRoom;
        public List<AlibiVariantDto> variants = new List<AlibiVariantDto>();
    }

    [Serializable]
    public sealed class ContradictionDto
    {
        public string id = "";
        public bool specific;
        public string alibiTag = "";
        public string alibi = "";
        public string form = "";
        public RoomRefDto room = new RoomRefDto();
        public SpeakerDto speaker = new SpeakerDto();
        public string textKey = "";
        public List<string> notSuspect = new List<string>();
        public bool allOtherAlibisInside;
        public bool noOtherAlibiAtAlibiPlace;
        public string noOtherAlibiInRoom = "";
        public bool noOtherOutsideAlibi;
        public string noOtherHolderOfAlibi = "";
        public List<string> suspectHasTags = new List<string>();
        public List<string> onlyForAlibis = new List<string>();
        public List<string> notForAlibis = new List<string>();
        public List<string> notWithMethods = new List<string>();
        public bool onlyForBathroomAlibis;
        public bool noOtherAlibiInBathroom;
        public bool mentionsSuspect;
        public bool notForOutsideAlibis;
        public List<string> mentions = new List<string>();
    }

    [Serializable]
    public sealed class VictimPlanDto
    {
        public string id = "";
        public string method = "";
        public TagFilterDto holders = new TagFilterDto();
        public string alibi = "";
        public bool solo;
        public string time = "";
        public string sameAsAlibi = "";
        public List<string> mentions = new List<string>();
        public LineDto clue = new LineDto();
    }

    [Serializable]
    public sealed class HerringLineDto
    {
        public string form = "";
        public SpeakerDto speaker = new SpeakerDto();
        public RoomRefDto room = new RoomRefDto();
        public List<string> fixedExplainerOptions = new List<string>();
        public string textKey = "";
    }

    [Serializable]
    public sealed class RedHerringDto
    {
        public string id = "";
        public TagFilterDto subjects = new TagFilterDto();
        public List<string> needs = new List<string>();
        public List<string> mentions = new List<string>();
        public HerringLineDto suspicion = new HerringLineDto();
        public HerringLineDto explanation = new HerringLineDto();
    }

    /// <summary>One switch in the "closed rooms" option: one room, or rooms that are always closed together.</summary>
    /// <remarks>JsonUtility cannot read nested arrays, so each entry is an object.</remarks>
    [Serializable]
    public sealed class ClosableRoomDto
    {
        public List<string> rooms = new List<string>();
    }
}
