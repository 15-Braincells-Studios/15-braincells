# Murder Generator

A procedural level generator for the "prevent the murder" deduction game, written for **Unity 6.3 LTS**.
It builds one level from data: who plans to kill whom, how, why, and every clue the player can find in
the afternoon. A built-in solver guarantees that each level has exactly one answer.

This is a technical proof of concept. There is no UI: the output is a `Level` object, and a small demo
component prints it to the Console.

## Contents

```
MurderGenerator/
  Data/
    generator_data.json      all content and rules (methods, clues, motives, alibis, ...)
    strings_en.json          all English text, in the team's {group, key, value} format
  Runtime/                   the generator (no UnityEngine reference: plain C#)
    Data/                    JSON mirrors (Dto), the content model, validation
    Generation/              LevelGenerator and the steps of building a level
    Solving/                 the solver
    Output/                  Level, Clue, ClueEffect, text arguments
    Text/                    string table, text rendering, a debug report
  Unity/                     JSON loading with JsonUtility, and a demo MonoBehaviour
  Tests/Editor/              EditMode tests (Unity Test Framework)
```

Three assembly definitions keep the layers apart:

| Assembly | References | Why |
|---|---|---|
| `FifteenBraincells.MurderGenerator` | nothing (`noEngineReferences`) | The rules can be tested and reused without Unity |
| `FifteenBraincells.MurderGenerator.Unity` | runtime | The only code that touches Unity |
| `FifteenBraincells.MurderGenerator.Tests` | runtime, Unity, test runner | Editor-only tests |

No packages are needed.

## Getting started

1. Copy the `MurderGenerator` folder into `Assets/`.
2. Add **Murder Generator Demo** to any GameObject. Assign `generator_data.json` and `strings_en.json`.
3. Press Play. You can also use **Generate level** in the component's context menu. The level appears in
   the Console: the solution, every character's lines, and the objects in each room.
4. Run the tests in **Window > General > Test Runner > EditMode**.

In code:

```csharp
var strings  = GeneratorDataLoader.LoadStrings(stringsJson);
var database = GeneratorDataLoader.LoadDatabase(generatorDataJson, strings); // validates everything
var renderer = new ClueTextRenderer(strings);

Level level = new LevelGenerator(database).Generate(seed: 2611);

foreach (var line in level.LinesOf(database.GetCharacter("chef")))
    Debug.Log(renderer.Render(line));            // what the Chef says when you talk to him

bool solved = Solver.Solve(foundClues, database.Characters).IsSolved;   // has the player got enough?
```

### Evidence

When the player solves a case, they name the murderer and the victim and hand in five pieces of evidence. In every
level exactly five clues are the right evidence. `Clue.Evidence` says which one a clue is (`EvidenceRole.None` for
all others):

| `EvidenceRole` | The clue |
|---|---|
| `WhenAndWhere` | the method clue of group A |
| `Method` | the method clue of group B |
| `FakeAlibi` | the murderer's fake alibi |
| `Contradiction` | the contradiction that breaks that fake alibi (never a loose false lead) |
| `Motive` | the murderer's real motive against the victim (never a false lead) |

```csharp
IReadOnlyList<Clue> right = level.Evidence;              // the five, in the order above
bool ok = level.IsCorrectEvidence(playerSelection);      // exactly these five, in any order, nothing else
Clue motive = level.EvidenceFor(EvidenceRole.Motive);
```

Groups A and B are always shown in every level (whatever the Excel chance says), because they are evidence.
`LevelReport` marks the five with `EVIDENCE`.

### Closed rooms

Some rooms can be closed for a level: the armory, the alchemy room, the study, the library and the bathrooms
(both bathrooms always close together). `database.ClosableRooms` lists them.

```csharp
var closed = database.WithClosedRooms(new[] { "armory", "library" });  // a filtered copy, build it once per choice
Level level = new LevelGenerator(closed).Generate(seed: 2611);
```

A closed room is left out of the game completely. No method, alibi, plan, clue, motive, red herring or
contradiction takes place there or talks about it, and it is never filled in as a place in a text. A method that
needs the room disappears: the heavy book (library), the study fire (study), and the drowning (bathrooms). Each
level gets one clue of category `ClosedRoom` per closed room: a note on its door (`closed_room_note`, "Closed
today."). It proves nothing. Things a character keeps in a closed home room (the Weapons master's armory, the
Librarian's library) lie in the staff room instead.

The converter marks every row with the closable rooms it takes place in or talks about (`mentions`); a plain "bath"
or "tub" counts as both bathrooms.

## How a level is built

`LevelGenerator` runs `LevelAttempt` until an attempt succeeds. With the current data, 99% of levels
succeed on the first attempt. Each step is in its own file:

| Step | File | What happens |
|---|---|---|
| 1 | `LevelAttempt.Crime.cs` | Pick the victim, then the murderer (equal chances for every character), then a method (one that fits few pairs gets a bigger chance) and a motive (one that fits few pairs gets a bigger chance). Then the time and murder room: from the method's time window, or from the victim's plan. Add method clues: each clue group (A-D) is rolled against its chance. Add the real motive plus 1-2 false leads. |
| 1b | `LevelAttempt.Plans.cs` | With a plan method: the victim's plan for tonight, sometimes a decoy with a plan of the same kind, and their plan lines. |
| 2 | `LevelAttempt.Alibis.cs` | Give the suspect a fake alibi. An alibi that few characters may hold gets a bigger chance. |
| 3 | `LevelAttempt.Contradiction.cs` | Pick the clue that breaks the fake alibi (alibi-specific with `specificContradictionChance`, 75%). Then give innocent characters real alibis, and add 1-2 loose contradictions: alibi-specific ones about an alibi nobody has tonight, as a false lead. |
| 4 | `LevelAttempt.RedHerrings.cs` | Add 1-2 suspicious-looking things about innocent characters, each with its explanation. |
| 5 | `LevelAttempt.Speakers.cs` | While the solver still finds another answer, give one of those wrong people an alibi. Then fill the unnamed witnesses and explainers, so that every character has at least one line and nobody more than two. |

### Method kinds

| Kind | Time | Murder room |
|---|---|---|
| `Fixed` | A random time in the method's window, in steps of 5 minutes | From the method |
| `Plan` | When the victim's plan starts (an alibi used as a plan: its From time) | From the method (`victim_bathroom`: the royal bathroom for Royalty, the staff bathroom for Staff) |
| `Solo` | A random time inside the victim's solo alibi, at least 30 minutes from its start and end | That alibi's room |
| `Sleep` | 15 to 30 minutes after the victim's bedtime, at most 20:00 | The victim's bedroom, or the infirmary with the plan Sick |

A plan is not an alibi: nobody with a plan gets an alibi, and an alibi used as a plan by a method (an outside alibi
for the ambush, a bath) is never an alibi with that method.

Rules that hold in every level. Each one is checked by a test.

- Exactly one murderer and victim fit the clues, and they are the real ones. Without the contradiction,
  the fake alibi still clears the murderer.
- Every character has at least one line and at most two. The line about their own evening (their alibi or
  plan) comes first: `Level.LinesOf` returns them in that order.
- The victim never has a real alibi. Nobody with a plan has an alibi.
- A character named by an `(excluded)` clue gets that clue's **Blocks** for the day. A blocked character
  never gets an alibi or red herring that **Needs** the same thing, and that includes partners. Lifting
  also requires the tag in `liftingRequiresTag`.
- Someone blocked for Sight or Afternoon is never an unnamed witness. A witness is never the person the
  line is about.
- **Presence lists** (`listedGetNoOtherAlibi`) say who will be at the murder place, so nobody on them gets
  an alibi elsewhere. The same goes for `excludedGetNoOtherAlibi`.
- Every condition from the Excel is enforced. The contradiction's conditions are checked again whenever a
  later alibi is added, so a later alibi cannot make the contradiction untrue.

## The data

The JSON is converted from `Generation Tables.xlsx` (the converter is kept outside this folder).

### `strings_en.json`

This file uses the team's format, with one entry per line and the entries grouped:

```json
{"group": "method_clue", "key": "clue_poisoned_remedy_a2", "value": "At a quarter past seven everyone comes for their evening remedy. ..."}
```

Spoken lines do not contain the speaker's name, because in the game you only hear a line from the person
you are talking to. Placeholders are named, not numbered, so the text stays readable:

| Placeholder | Filled with |
|---|---|
| `{suspect}` `{victim}` `{character}` `{partner}` `{tutor}` `{sick_character}` | a character, e.g. "the King" |
| `{list}` `{partial_list}` `{excluded}` | a list, e.g. "the Chef, the Queen and me" |
| `{murder_place}` `{alibi_place}` `{other_place}` | a room, e.g. "the library" or "the King's bedroom" |
| `{murder_time}` | a time: in words in spoken lines ("ten past seven", from the `time_*` strings), in digits on notes |

### `generator_data.json`

Every free-text rule from the Excel is now a field. The main building blocks:

- **Tag filters** (`anyOf`, `allOf`, `noneOf`) select characters. A character's id also works as a tag,
  so `{"anyOf": ["king", "queen"]}` means "the King or the Queen". An empty filter means anyone.
- **Speaker** `kind` is one of the following:
  - `Fixed`: a named character.
  - `Witness` or `Explainer`: chosen by the generator.
  - `Self`: the alibi holder.
  - `Partner`: the alibi's second character.
  - `None`: an object in a room.
- **Room** is a room id. It can also be one of these special values: `murder_place`, `alibi_place`, or
  `bedroom` / `home` with an `owner` (`suspect`, `victim`, `subject`, `holder`).
- **Method clue effects**:
  - `FullList`, `PartialList`, `Excluded`: these follow the placeholders.
  - `ExcludeFixed`, `OnlyFixed`: these name the characters directly, for clues that rule people out
    without a placeholder.
- **Needs/Blocks** are `Hands`, `Writing`, `Lifting`, `Sight`, `Wine`, `Afternoon`, `Outside` (every outside
  alibi needs it), `Evening` (someone with an alibi or plan never gets a red herring that needs it) and `Bath`.
- **Victim plans** (`victimPlans`) belong to a method. A plan has its own line and time, or uses an alibi.
- **Settings** hold all tuning values: chances, counts and attempt limits.

The data is validated when it is loaded. Every unknown id, tag, room, method, alibi, missing text key,
bad time or invalid combination is collected. All of them are reported together in one
`GeneratorDataException`, so a typo never fails silently in play.

## Design decisions

- **JsonUtility, not Newtonsoft.** The data was designed so that Unity's built-in parser can read it: no
  dictionaries and no polymorphism. Enums are written as strings and parsed during validation, which gives
  readable JSON and clear error messages without adding a package. The JSON sits behind the plain DTO
  classes in `Runtime/Data/Dto`, so changing the parser later only touches `GeneratorDataLoader`.
- **DTO → validated model.** The generator never sees raw JSON. It works on immutable model objects that
  hold references instead of string ids.
- **Own random number generator** (`Rng`, SplitMix64). A seed gives exactly the same level on every
  platform and .NET version. `System.Random` does not promise that.
- **Semantic text arguments.** A clue stores "character: King" rather than "the King". Turning that into
  text is the job of `ClueTextRenderer` with a string table.
- **Partial class per step.** The steps share a lot of state about one attempt. Splitting the class by step
  keeps each file about one subject without passing that state between many small classes.

### Localisation

The generator never produces finished text. Everything language-specific comes from the string table:
- names with their article ("the Chef");
- room names;
- list separators and "me";
- every clue line.

A new language is a new `strings_xx.json` with the same keys.

Two things are still English-only and are kept in one place, `ClueTextRenderer`:
- the word order of lists, with the speaker last as "me";
- capitalising the first letter of a sentence.

## Verification

All 32 tests pass, including the closed-room tests: every closable room on its own and all of them at once,
150 levels each, without a single clue, alibi, argument or murder in a closed room. There are also two
standalone checks:
- **Stress test:** 10,000 random seeds give 0 failures. A level takes 1.04 attempts on average and about
  2.5 ms to generate. Every row in the data appears in at least one level.
- **Spread:** over 5,000 levels every character is murderer in 9.3-10.8% and victim in 8.9-11.1% of the
  levels; no motive is used more than 1.5 times and no alibi more than 2.2 times as often as the median one.
- **Compile check:** the runtime compiles as C# 9 / netstandard2.1 with warnings as errors.

## Conversion notes

These are decisions made while converting the Excel, for the team to review.

- **Descriptions became real lines.** Some Excel cells described a clue instead of giving its text, for
  example "Doctor saying character is sick" and "Duty roster". These were written out as real lines or
  notes. This affects the alibis Buying ingredients, Sick, Fencing Lessons, State visit, Lessons with
  tutor, Night watch, Early to bed, Evening off and Royal accounts, and the red herrings Poison, Dagger,
  Burned letter, Red stains and Lab notes.
- **Places for outside alibis.** Each outside alibi got a place for `{alibi_place}`, such as "the tavern"
  or "the village church". Before, the generator wrote the name of the alibi there.
- **Rooms for motive objects.** Motives that are letters or documents got a room:
  - a letter lies in the home room of the person who received it;
  - an order or will lies in the study;
  - a lab journal lies in the infirmary.
- **Merged places.** "Pantry door", "Ballroom door", "Front door", "Entrance Hall" and "Parlor balustrade"
  now point to their room.
- **No location for witness lines.** A witness line has no room any more, because the player hears it from
  the speaker.
