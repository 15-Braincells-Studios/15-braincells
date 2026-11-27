# Dragon Statue

A small dragon-on-a-tower statue the player can pick up and inspect. The tower has a little door; in the inspect view the player right-clicks it to open it. One statue hides the **Cellar Key** behind its door.

## Prefabs (`Assets/Project/Prefabs/Inspect`)
| Prefab | What it is |
|---|---|
| `DragonStatue.prefab` | The normal statue. Inspectable (tag `inspect`, ItemSO `ScriptableObjects/Inspect/DragonStatue`). The door opens but there is nothing behind it. |
| `DragonStatue_CellarKey.prefab` | Variant of the one above with a hidden key behind the door (`Model/CellarKey`, a `Clue` with the `HiddenKey` ItemSO "Cellar Key"). |

Inside the prefab:
| Child | What it does |
|---|---|
| Model | The dragon mesh (scale 0.07) with a Mesh Collider, so the inspect raycast hits it. |
| Model/HatchHinge | Empty at the door's hinge line. Has the **StatueHatch** script. |
| Model/HatchHinge/Hatch | The door mesh with a Box Collider (the collider is what the player clicks). |
| Model/CellarKey | Only in the key variant. Turned off until the door opens. |

## In the scene (`A MapScene`, FRIDAY TEST map)
5 statues, each in its own group with a table:
- `Map/ground floor/props ground floor/Dragon statues/` → Dragon statue 1 (Cellar Key), 2, 3, 4, and `Empty tables` (3 spare tables).
- `Map/Upstairs/Dragon statues/` → Dragon statue 5.

The original map (`Map_Rebuild`) still has the old statuettes and was not changed.

## How it plays
1. Player inspects the statue (E, like any inspectable).
2. Hovering the door shows the clue cursor; **right-click** opens it (the same click that takes clues).
3. The door swings open on the copy in the inspect view **and** on the real statue, so it stays open after closing the inspect view.
4. On the key statue the key appears in the doorway. Right-click it to take it; it goes to the inventory as "Cellar Key".
5. The Cellar Key opens the locked door `ground floor/dining room/Door_Stone (1)`.

## Moving the key to another statue
Swap the prefabs: replace the key statue with `DragonStatue.prefab` and another statue with `DragonStatue_CellarKey.prefab` (keep the position). Nothing else needs changing.

## Scripts
**StatueHatch** (`Scripts/Inspect/StatueHatch.cs`, new)
- Put it on the hinge object; the door mesh is a child with a collider.
- `Hinge Axis` / `Open Angle` / `Swing Speed`: how the door turns (default: around up, -110°).
- `Reveal On Open`: objects that get turned on when the door opens (the key).
- Its open state is serialized, so the inspect copy starts in the same state as the real statue.

**ItemInspector** (`Scripts/Inspect/ItemInspector.cs`, changed)
- The hover check now returns the collider under the mouse and looks for a `Clue` **or** a closed `StatueHatch` on it.
- Right-click takes the clue if there is one, otherwise opens the hatch (`OpenHatch`).
- Hatches on the copy and the real prop are paired by order, the same way clues are.
- Nothing changes for props without a StatueHatch.
