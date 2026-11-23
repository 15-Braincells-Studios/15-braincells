# Torch Lighting

A wall torch with a stylised flame, embers, smoke and a flickering light.

## Using it
Drag `Assets/Project/Prefabs/Props/Lights/TorchWall.prefab` into the scene and put it against a wall.
The wall bracket is on the **+X side** of the prefab (same orientation as the Blender model), so turn the torch until +X points into the wall.
Nothing else is needed: the flicker and the shadow manager start by themselves in Play mode.

## What is in the prefab
| Child | What it does |
|---|---|
| Model | The torch from `Art/Models/Lights/TorchWall.fbx` (nested, so model updates from Art come through). Has a **Mesh Collider**. Casts no shadow, so it doesn't block its own light. |
| Flame | A Quad with the `TorchFlame` material (`15Braincells/StylizedFlame` shader). Always faces the camera, stays upright. |
| Light | Warm point light, a bit in front of the flame (away from the wall). |
| Embers / Smoke | Small particle systems (`15Braincells/SoftParticle` shader). |

## Scripts (`Assets/Project/Scripts/Lighting`)
**TorchFlicker** (on the prefab root)
- Flickers the light brightness, range and colour with smooth Perlin noise, and wobbles the light a few millimetres so the shadows sway.
- The flame height and brightness follow the same flicker, so the fire and the light match.
- Every torch gets a random seed, so torches never flicker in sync.
- `IntensityMultiplier` can be changed by other scripts later (wind, the player walking past, lighting/blowing out a torch).
- Settings are in the Inspector (Base Intensity, Base Range, colours, Flicker Speed, variations).

**TorchShadowManager** (created automatically, don't place it)
- Point light shadows are expensive and URP only has room for a few (shadow atlas is 2048).
- Only the **4 torches closest to the camera** within 20 m cast shadows; they fade in and out. All other torches still give light.
- Change `maxShadowedTorches` / `maxShadowDistance` on the TorchShadowManager object in Play mode to test.

## Tuning the look
- Flame colours, band thresholds, width and wobble: on the `Material/Lights/TorchFlame` material.
- The flame looks best with **Bloom** enabled in the scene's Volume.
- In the editor (not playing) every torch shows shadows, because the manager only runs in Play mode.
