# Scenes And Prefabs Inventory

Last audited: 2026-05-16

This file captures scene and prefab wiring discovered from `.unity`, `.prefab`, and `.meta` reference inspection. It is not a replacement for the Unity Inspector, but it gives future AI sessions a reliable starting map.

## Main Scenes

| Scene | Primary Role | Notable Systems |
|:------|:-------------|:----------------|
| `Assets/Scenes/Main Menu.unity` | Entry menu | Main menu UI, settings panel, AudioSettingsUI, EventSystem, main camera. |
| `Assets/Scenes/Level 1.unity` | Gameplay level | Player, HUD_Canvas, QuestManager, EventSystem, levers, gates, traps, keys, coins, chests, checkpoint door, environment. |
| `Assets/Scenes/Level 2.unity` | Gameplay level | Player, HUD_Canvas, QuestManager, EventSystem under GlobalManagers, enemies, levers, gates, traps, fall zones, keys, coins, chests, environment. |

## Scene-Referenced Gameplay Prefabs

| Prefab | Main Use | Scene References Found |
|:-------|:---------|:-----------------------|
| `Assets/Prefabs/Player/Player 1.prefab` | Player avatar | Level 1, Level 2 |
| `Assets/Prefabs/Managers/HUD_Canvas.prefab` | HUD and pause menu | Level 1, Level 2 |
| `Assets/Prefabs/Managers/QuestManager.prefab` | Quest/key manager | Level 1, Level 2 |
| `Assets/Prefabs/Managers/PlayerAnimator.prefab` | Scene player animator object/reference | Level 1, Level 2 |
| `Assets/Prefabs/Collectibles/BlueCoin.prefab` | Coins | Level 1, Level 2 |
| `Assets/Prefabs/Collectibles/Key 1 - GOLD - .prefab` | Keys | Level 1, Level 2 |
| `Assets/Prefabs/Doors_Gates/CheckPointDoor.prefab` | Level transition door | Level 1, Level 2 |
| `Assets/Prefabs/Doors_Gates/Door.prefab` | Legacy door | Level 1 |
| `Assets/Prefabs/Doors_Gates/MetalGate.prefab` | Legacy/auto gate | Level 1, Level 2 |
| `Assets/Prefabs/Doors_Gates/MetalGateInter.prefab` | Interactive gate | Level 1, Level 2 |
| `Assets/Prefabs/Interactables/LeverSwitch.prefab` | Lever | Level 1, Level 2 |
| `Assets/Prefabs/Interactables/chest_yellow_blue.prefab` | Chest | Level 1, Level 2 |
| `Assets/Prefabs/Traps/Spike Trap.prefab` | Spike trap | Level 1, Level 2 |
| `Assets/Prefabs/Enemy/Enemy.prefab` | Enemy | Level 2 |

## Scene-Referenced Environment Prefabs

| Prefab | Scene References Found |
|:-------|:-----------------------|
| `Assets/Prefabs/Env Objects/Knight_Statue.prefab` | Level 1, Level 2 |
| `Assets/Prefabs/Env Objects/LightRays.prefab` | Level 1, Level 2 |
| `Assets/Prefabs/Env Objects/oven_fire.prefab` | Level 1, Level 2 |
| `Assets/Prefabs/Env Objects/Ripple.prefab` | Level 1, Level 2 |
| `Assets/Prefabs/Env Objects/Rocks.prefab` | Level 2 |
| `Assets/Prefabs/Env Objects/Barrels/Barrel_metal.prefab` | Level 2 |
| `Assets/Prefabs/Env Objects/Barrels/Barrel_wood.prefab` | Level 2 |
| `Assets/Prefabs/Env Objects/Pillars/HalfPillar_Left.prefab` | Level 1, Level 2 |
| `Assets/Prefabs/Env Objects/Pillars/HalfPillar_Right.prefab` | Level 1, Level 2 |
| `Assets/Prefabs/Env Objects/Pillars/Light_Pillar.prefab` | Level 1, Level 2 |
| `Assets/Prefabs/Env Objects/Pillars/TorchPillar.prefab` | Level 1 |
| `Assets/Prefabs/Env Objects/Pillars/TorchPillar_plain.prefab` | Level 2 |
| `Assets/Prefabs/Env Objects/WaterFalls/BigWaterFall.prefab` | Level 2 |
| `Assets/Prefabs/Env Objects/WaterFalls/waterfall_small.prefab` | Level 1, Level 2 |

## Prefabs With No Direct Scene Reference Found

These may be instantiated by code, kept for future work, or unused:

- `Assets/Prefabs/Camera/CameraTransition.prefab`
- `Assets/Prefabs/Camera/Main Camera.prefab`
- `Assets/Prefabs/Camera/SceneTransManager.prefab`
- `Assets/Prefabs/Managers/AudioManager.prefab`
- `Assets/Prefabs/Managers/UI_Manager.prefab`
- `Assets/Prefabs/Traps/RollingTrap.prefab`
- `Assets/Prefabs/Env Objects/Torch Yellow.prefab`
- `Assets/Prefabs/Env Objects/Torch_blue.prefab`
- `Assets/Prefabs/Env Objects/WaterFalls/SmallWaterFall.prefab`
- `Assets/Prefabs/Env Objects/WaterFalls/waterfall_large.prefab`

## Resources

| Resource | Role |
|:---------|:-----|
| `Assets/Resources/PersistentSystems.prefab` | Runtime-loaded persistent systems prefab. Referenced by `GameBootstrapper` through `Resources.Load("PersistentSystems")`. |

## Script Reference Notes

- `GameBootstrapper` has no scene/prefab serialized reference because it runs from static runtime initialization.
- `UITransitionManager` and `CameraTransitionSystem` are referenced by `Assets/Resources/PersistentSystems.prefab`.
- `MainMenu` is referenced by Main Menu scene, Level 1 scene, and `HUD_Canvas.prefab`.
- `AudioSettingsUI` is referenced by Main Menu scene and `HUD_Canvas.prefab`.
- `FallZone` is directly referenced in Level 2 scene.
- `LeverResponder` has both prefab and scene references, because some lever targets appear to be wired directly in scenes.
