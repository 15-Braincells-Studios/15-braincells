using System;
using System.Collections.Generic;
using System.Linq;
using FifteenBraincells.MurderGenerator.Data.Dto;
using FifteenBraincells.MurderGenerator.Text;

namespace FifteenBraincells.MurderGenerator.Data
{
    /// <summary>
    /// Turns the JSON data into a <see cref="GeneratorDatabase"/>. Every reference (character, tag, room,
    /// method, alibi, text key) is checked. All problems are collected and reported together, so one
    /// load shows everything that is wrong with the data.
    /// </summary>
    public sealed class DatabaseBuilder
    {
        private readonly List<string> _errors = new List<string>();
        private readonly GeneratorDataDto _dto;
        private readonly IStringTable _strings;

        private Dictionary<string, Character> _characters;
        private HashSet<string> _tags;
        private Dictionary<string, Room> _rooms;
        private Dictionary<string, Method> _methods;
        private Dictionary<string, Alibi> _alibis;

        private DatabaseBuilder(GeneratorDataDto dto, IStringTable strings)
        {
            _dto = dto;
            _strings = strings;
        }

        /// <summary>Builds the database.</summary>
        /// <param name="dto">The deserialised generator_data.json.</param>
        /// <param name="strings">Optional. When given, every text key must exist in it.</param>
        /// <exception cref="GeneratorDataException">The data has one or more problems.</exception>
        public static GeneratorDatabase Build(GeneratorDataDto dto, IStringTable strings = null)
        {
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto));
            }

            var builder = new DatabaseBuilder(dto, strings);
            var database = builder.BuildDatabase();
            if (builder._errors.Count > 0)
            {
                throw new GeneratorDataException(builder._errors);
            }

            return database;
        }

        private GeneratorDatabase BuildDatabase()
        {
            var rooms = _dto.rooms.Select(BuildRoom).ToList();
            _rooms = ToLookup(rooms, r => r.Id, "room");

            var characters = _dto.characters.Select(BuildCharacter).ToList();
            _characters = ToLookup(characters, c => c.Id, "character");
            _tags = new HashSet<string>(characters.SelectMany(c => c.Tags));

            var methods = _dto.methods.Select(BuildMethod).ToList();
            _methods = ToLookup(methods, m => m.Id, "method");

            // Alibis refer to each other ("requires someone with Sick"), so create them first and link after.
            var alibis = _dto.alibis.Select(BuildAlibi).ToList();
            _alibis = ToLookup(alibis, a => a.Id, "alibi");
            for (var i = 0; i < alibis.Count; i++)
            {
                var other = _dto.alibis[i].requiresOtherHolderOf;
                alibis[i].RequiresOtherHolderOf = other.Length == 0 ? null : AlibiRef(other, $"alibi '{alibis[i].Id}'");
            }

            return new GeneratorDatabase(
                BuildSettings(_dto.settings),
                characters,
                rooms,
                methods,
                _dto.clueGroups.Select(BuildClueGroup).ToList(),
                _dto.methodClues.Select(BuildMethodClue).ToList(),
                _dto.motives.Select(BuildMotive).ToList(),
                alibis,
                _dto.contradictions.Select(BuildContradiction).ToList(),
                _dto.redHerrings.Select(BuildRedHerring).ToList());
        }

        // ------------------------------------------------------------------ entities

        private GeneratorSettings BuildSettings(SettingsDto s)
        {
            Check(s.falseMotivesMin <= s.falseMotivesMax, "settings: falseMotivesMin is above falseMotivesMax");
            Check(s.redHerringsMin <= s.redHerringsMax, "settings: redHerringsMin is above redHerringsMax");
            Check(s.partialListExtraMin <= s.partialListExtraMax, "settings: partialListExtraMin is above partialListExtraMax");
            Check(s.excludedMin <= s.excludedMax, "settings: excludedMin is above excludedMax");
            Check(s.maxAttempts > 0, "settings: maxAttempts must be above 0");
            return new GeneratorSettings
            {
                SpecificContradictionChance = s.specificContradictionChance,
                InnocentAlibiChance = s.innocentAlibiChance,
                FalseMotivesMin = s.falseMotivesMin,
                FalseMotivesMax = s.falseMotivesMax,
                RedHerringsMin = s.redHerringsMin,
                RedHerringsMax = s.redHerringsMax,
                RedHerringsLimit = s.redHerringsLimit,
                PartialListExtraMin = s.partialListExtraMin,
                PartialListExtraMax = s.partialListExtraMax,
                ExcludedMin = s.excludedMin,
                ExcludedMax = s.excludedMax,
                MaxAttempts = s.maxAttempts,
                MaxExtraAlibiRounds = s.maxExtraAlibiRounds,
                LiftingRequiresTag = Tag(s.liftingRequiresTag, "settings.liftingRequiresTag"),
            };
        }

        private Room BuildRoom(RoomDto dto) => new Room { Id = dto.id, NameKey = TextKey(dto.nameKey, $"room '{dto.id}'") };

        private Character BuildCharacter(CharacterDto dto)
        {
            var context = $"character '{dto.id}'";
            if (dto.homeRoom != RoomRef.Bedroom)
            {
                RoomById(dto.homeRoom, context);
            }

            return new Character(dto.id, TextKey(dto.nameKey, context), TextKey(dto.referenceKey, context),
                dto.role, dto.sex, dto.homeRoom, dto.tags);
        }

        private Method BuildMethod(MethodDto dto)
        {
            var context = $"method '{dto.id}'";
            Check(dto.rooms.Count > 0, $"{context}: has no rooms");
            return new Method
            {
                Id = dto.id,
                NameKey = TextKey(dto.nameKey, context),
                Time = Time(dto.time, context),
                RoomIds = dto.rooms.Select(r => RoomById(r, context)?.Id).ToList(),
                Suspects = Filter(dto.suspects, context),
                Victims = Filter(dto.victims, context),
            };
        }

        private ClueGroupRule BuildClueGroup(ClueGroupDto dto) => new ClueGroupRule
        {
            Group = Parse<ClueGroup>(dto.group, "clue group"),
            ShowChance = dto.showChance,
        };

        private MethodClue BuildMethodClue(MethodClueDto dto)
        {
            var context = $"method clue '{dto.id}'";
            var effect = Parse<MethodClueEffect>(dto.effect.kind, context);
            var clue = new MethodClue
            {
                Id = dto.id,
                Method = MethodRef(dto.method, context),
                Group = Parse<ClueGroup>(dto.group, context),
                Line = Line(dto.form, dto.speaker, dto.room, dto.textKey, context),
                Clarity = dto.clarity,
                Effect = effect,
                EffectRole = Parse<CrimeRole>(dto.effect.role, context),
                EffectCharacters = Characters(dto.effect.characters, context),
                NotSuspect = Characters(dto.notSuspect, context),
                NotVictim = Characters(dto.notVictim, context),
                ExcludedFilter = Filter(dto.excludedFilter, context),
                ExcludedGetNoOtherAlibi = dto.excludedGetNoOtherAlibi,
                ListedGetNoOtherAlibi = dto.listedGetNoOtherAlibi,
                ListOmitsSpeaker = dto.listOmitsSpeaker,
                Blocks = Needs(dto.blocks, context),
            };

            var namesCharacters = effect == MethodClueEffect.ExcludeFixed || effect == MethodClueEffect.OnlyFixed;
            Check(namesCharacters == clue.EffectCharacters.Count > 0,
                $"{context}: effect {effect} {(namesCharacters ? "needs" : "must not have")} effect characters");
            return clue;
        }

        private Motive BuildMotive(MotiveDto dto)
        {
            var context = $"motive '{dto.id}'";
            return new Motive
            {
                Id = dto.id,
                Suspects = Filter(dto.suspects, context),
                Victims = Filter(dto.victims, context),
                SameRole = dto.sameRole,
                DifferentSex = dto.differentSex,
                Clue = Line(dto.clue.form, dto.clue.speaker, dto.clue.room, dto.clue.textKey, context),
            };
        }

        private Alibi BuildAlibi(AlibiDto dto)
        {
            var context = $"alibi '{dto.id}'";
            var partnerKind = Parse<PartnerKind>(dto.partner.kind, context);
            var partner = partnerKind == PartnerKind.None
                ? AlibiPartner.None
                : new AlibiPartner
                {
                    Kind = partnerKind,
                    Character = partnerKind == PartnerKind.Fixed ? CharacterRef(dto.partner.character, context) : null,
                    Pool = Filter(dto.partner.pool, context),
                    Placeholder = dto.partner.placeholder,
                };
            Check(partnerKind == PartnerKind.None || partner.Placeholder.Length > 0, $"{context}: partner has no placeholder");

            var alibi = new Alibi
            {
                Id = dto.id,
                NameKey = TextKey(dto.nameKey, context),
                Holders = Filter(dto.holders, context),
                Partner = partner,
                Start = Time(dto.start, context),
                End = Time(dto.end, context),
                IsOutside = dto.outside,
                RoomId = dto.outside ? "" : AlibiRoom(dto.room, context),
                PlaceKey = dto.outside ? TextKey(dto.placeKey, context) : "",
                Needs = Needs(dto.needs, context),
                NotWithMethods = dto.notWithMethods.Select(m => MethodRef(m, context)).ToList(),
                Variants = dto.variants.Select(v => new AlibiVariant
                {
                    Line = Line(v.form, v.speaker, v.room, v.textKey, context),
                    HolderTag = v.holderTag.Length == 0 ? "" : Tag(v.holderTag, context),
                }).ToList(),
            };
            Check(alibi.Variants.Count > 0, $"{context}: has no variants");
            Check(alibi.Start.Minutes < alibi.End.Minutes, $"{context}: ends before it starts");
            return alibi;
        }

        private Contradiction BuildContradiction(ContradictionDto dto)
        {
            var context = $"contradiction '{dto.id}'";
            Check(dto.specific || dto.alibiTag == "Any" || dto.alibiTag == "Inside" || dto.alibiTag == "Outside",
                $"{context}: alibiTag must be Any, Inside or Outside");
            return new Contradiction
            {
                Id = dto.id,
                IsSpecific = dto.specific,
                Alibi = dto.specific ? AlibiRef(dto.alibi, context) : null,
                AlibiTag = dto.alibiTag,
                Line = Line(dto.form, dto.speaker, dto.room, dto.textKey, context),
                Rules = new ContradictionRules
                {
                    NotSuspect = Characters(dto.notSuspect, context),
                    AllOtherAlibisInside = dto.allOtherAlibisInside,
                    NoOtherAlibiAtAlibiPlace = dto.noOtherAlibiAtAlibiPlace,
                    NoOtherAlibiInRoom = dto.noOtherAlibiInRoom.Length == 0 ? null : RoomById(dto.noOtherAlibiInRoom, context),
                    NoOtherOutsideAlibi = dto.noOtherOutsideAlibi,
                    NoOtherHolderOf = dto.noOtherHolderOfAlibi.Length == 0 ? null : AlibiRef(dto.noOtherHolderOfAlibi, context),
                    SuspectHasTags = dto.suspectHasTags.Select(t => Tag(t, context)).ToList(),
                    OnlyForAlibis = dto.onlyForAlibis.Select(a => AlibiRef(a, context)).ToList(),
                    NotForAlibis = dto.notForAlibis.Select(a => AlibiRef(a, context)).ToList(),
                    NotWithMethods = dto.notWithMethods.Select(m => MethodRef(m, context)).ToList(),
                },
            };
        }

        private RedHerring BuildRedHerring(RedHerringDto dto)
        {
            var context = $"red herring '{dto.id}'";
            return new RedHerring
            {
                Id = dto.id,
                Subjects = Filter(dto.subjects, context),
                Needs = Needs(dto.needs, context),
                Suspicion = HerringLine(dto.suspicion, context),
                Explanation = HerringLine(dto.explanation, context),
            };
        }

        private HerringLine HerringLine(HerringLineDto dto, string context) => new HerringLine
        {
            Line = Line(dto.form, dto.speaker, dto.room, dto.textKey, context),
            FixedExplainerOptions = Characters(dto.fixedExplainerOptions, context),
        };

        // ------------------------------------------------------------------ shared parts

        private LineSpec Line(string form, SpeakerDto speaker, RoomRefDto room, string textKey, string context)
        {
            var line = new LineSpec
            {
                Form = Parse<ClueForm>(form, context),
                Speaker = Speaker(speaker, context),
                Room = Room(room, context),
                TextKey = TextKey(textKey, context),
            };
            Check(line.Speaker.IsSpoken == (line.Form == ClueForm.Witness),
                $"{context}: a Witness line needs a speaker and an object must not have one");
            Check(line.Form == ClueForm.Witness || line.Room.IsSet, $"{context}: an object needs a room");
            return line;
        }

        private SpeakerSpec Speaker(SpeakerDto dto, string context)
        {
            var kind = Parse<SpeakerKind>(dto.kind, context);
            if (kind == SpeakerKind.None)
            {
                return SpeakerSpec.Nobody;
            }

            return new SpeakerSpec(kind, kind == SpeakerKind.Fixed ? CharacterRef(dto.character, context) : null);
        }

        private RoomRef Room(RoomRefDto dto, string context)
        {
            if (dto.room.Length == 0)
            {
                return RoomRef.Nowhere;
            }

            var owner = dto.owner.Length == 0 ? RoomOwner.None : Parse<RoomOwner>(Capitalise(dto.owner), context);
            var special = dto.room == RoomRef.MurderPlace || dto.room == RoomRef.AlibiPlace || dto.room == RoomRef.Home;
            if (!special)
            {
                RoomById(dto.room, context);
            }

            var needsOwner = dto.room == RoomRef.Bedroom || dto.room == RoomRef.Home;
            Check(needsOwner == (owner != RoomOwner.None), $"{context}: room '{dto.room}' {(needsOwner ? "needs" : "must not have")} an owner");
            return new RoomRef(dto.room, owner);
        }

        private TagFilter Filter(TagFilterDto dto, string context)
        {
            foreach (var tag in dto.anyOf.Concat(dto.allOf).Concat(dto.noneOf))
            {
                Tag(tag, context);
            }

            return new TagFilter(dto.anyOf.ToList(), dto.allOf.ToList(), dto.noneOf.ToList());
        }

        private Need Needs(IEnumerable<string> names, string context) =>
            names.Aggregate(Need.None, (all, name) => all | Parse<Need>(name, context));

        private ClockTime Time(string text, string context)
        {
            Check(ClockTime.TryParse(text, out var time), $"{context}: '{text}' is not a time (HH:mm)");
            return time;
        }

        private string AlibiRoom(string roomId, string context) =>
            roomId == RoomRef.Bedroom ? roomId : RoomById(roomId, context)?.Id ?? "";

        private IReadOnlyList<Character> Characters(IEnumerable<string> ids, string context) =>
            ids.Select(id => CharacterRef(id, context)).Where(c => c != null).ToList();

        // ------------------------------------------------------------------ lookups that record errors

        private Character CharacterRef(string id, string context) => Lookup(_characters, id, "character", context);

        private Method MethodRef(string id, string context) => Lookup(_methods, id, "method", context);

        private Alibi AlibiRef(string id, string context) => Lookup(_alibis, id, "alibi", context);

        private Room RoomById(string id, string context) => Lookup(_rooms, id, "room", context);

        private string Tag(string tag, string context)
        {
            Check(_tags.Contains(tag), $"{context}: unknown tag '{tag}'");
            return tag;
        }

        private string TextKey(string key, string context)
        {
            Check(key.Length > 0, $"{context}: missing text key");
            Check(_strings == null || key.Length == 0 || _strings.Contains(key), $"{context}: text key '{key}' is not in the string table");
            return key;
        }

        private T Parse<T>(string value, string context) where T : struct, Enum
        {
            if (Enum.TryParse(value, false, out T result) && Enum.IsDefined(typeof(T), result))
            {
                return result;
            }

            _errors.Add($"{context}: '{value}' is not a valid {typeof(T).Name}");
            return default;
        }

        private TValue Lookup<TValue>(Dictionary<string, TValue> lookup, string id, string what, string context)
            where TValue : class
        {
            if (lookup.TryGetValue(id ?? "", out var value))
            {
                return value;
            }

            _errors.Add($"{context}: unknown {what} '{id}'");
            return null;
        }

        private Dictionary<string, T> ToLookup<T>(IEnumerable<T> items, Func<T, string> id, string what)
        {
            var lookup = new Dictionary<string, T>();
            foreach (var item in items)
            {
                if (!lookup.ContainsKey(id(item)))
                {
                    lookup.Add(id(item), item);
                }
                else
                {
                    _errors.Add($"duplicate {what} id '{id(item)}'");
                }
            }

            return lookup;
        }

        private void Check(bool condition, string error)
        {
            if (!condition)
            {
                _errors.Add(error);
            }
        }

        private static string Capitalise(string text) =>
            text.Length == 0 ? text : char.ToUpperInvariant(text[0]) + text.Substring(1);
    }
}
