# System Map

Last audited: 2026-05-16

This is the top-level map for the current Unity project. Future AI sessions should load this file first, then follow the linked system files and `The-Unity-Architect/project-graph-report.md`.

Feature-level behavior docs live in [FeatureIndex.md](../Features/FeatureIndex.md).

## Project Shape

| Area | Current State |
|:-----|:--------------|
| Unity version target | Unity 6 / URP, based on package set and project settings. |
| Runtime scripts | 34 C# scripts under `Assets/Scripts`. |
| Main scenes | `Main Menu`, `Level 1`, `Level 2`. |
| Main prefabs | 38 prefabs under `Assets/Prefabs`, plus `Assets/Resources/PersistentSystems.prefab`. |
| UI stack | UGUI canvases, TextMesh Pro, `InputSystemUIInputModule`. |
| Gameplay input | Legacy `UnityEngine.Input` calls in scripts. Project active input handler is `2`, meaning both legacy and new input paths are enabled. |
| Rendering | URP, 2D lights, sprite renderers, tilemaps. |

## Main Runtime Systems

| System | Wiki File | Primary Scripts / Assets |
|:-------|:----------|:-------------------------|
| Bootstrap and Persistence | [BootstrapAndPersistence.md](./BootstrapAndPersistence.md) | `GameBootstrapper`, persistent manager singletons |
| Input | [Input.md](./Input.md) | `PlayerController`, `PlayerCombat`, `FollowPlayer`, interactable scripts, UGUI EventSystems |
| Audio | [Audio.md](./Audio.md) | `AudioManager`, `AudioSettingsUI` |
| UI and HUD | [UIAndHUD.md](./UIAndHUD.md) | `MainMenu`, `HUDManager`, `AudioSettingsUI` |
| Player | [Player.md](./Player.md) | `PlayerController`, `PlayerAnimator`, `PlayerCombat`, `FollowPlayer`, `YSort` |
| Combat | [Combat.md](./Combat.md) | `Health`, `Hitbox`, `PlayerCombat`, `EnemyCombat` |
| Enemy AI | [EnemyAI.md](./EnemyAI.md) | `EnemyAI`, `EnemyRangeTrigger`, `EnemyCombat`, `EnemyAnimationEventForwarder` |
| Progression | [Progression.md](./Progression.md) | `GlobalQuestManager`, `CheckpointDoor`, `Collect_keys` |
| Collectibles | [Collectibles.md](./Collectibles.md) | `Collect_coins`, `Collect_keys` |
| Interactables | [Interactables.md](./Interactables.md) | `Lever_On`, `LeverResponder`, `InteractiveGate`, `door_interaction`, `open_chest`, `auto_gate` |
| Traps and Hazards | [TrapsAndHazards.md](./TrapsAndHazards.md) | `SpikeTrapFSM`, `FallZone` |
| Scene Transitions | [SceneTransitions.md](./SceneTransitions.md) | `SceneTransitionManager`, `UITransitionManager`, `CameraTransitionSystem`, `CheckpointDoor` |
| Camera | [Camera.md](./Camera.md) | `FollowPlayer`, `CameraTransitionSystem` |
| Environment and Presentation | [EnvironmentAndPresentation.md](./EnvironmentAndPresentation.md) | `YSort`, `LightRaySequencedFX`, environment prefabs |
| Animation Assets | [AnimationAssets.md](./AnimationAssets.md) | `Assets/Animations` excluding `AnimatedTilesDeluxe` |
| Scenes and Prefabs | [ScenesAndPrefabs.md](./ScenesAndPrefabs.md) | Scene/prefab wiring inventory |

## Scene Responsibilities

| Scene | Purpose | Important Runtime Notes |
|:------|:--------|:------------------------|
| `Assets/Scenes/Main Menu.unity` | Menu entry point | Contains `MainMenu`, settings UI, `AudioSettingsUI`, EventSystem with `InputSystemUIInputModule`. |
| `Assets/Scenes/Level 1.unity` | First gameplay level | Contains player, HUD canvas, quest manager, EventSystem, levers, gates, traps, chests, keys, coins, checkpoint door. |
| `Assets/Scenes/Level 2.unity` | Second gameplay level | Contains player, HUD canvas, quest manager, enemies, hazards, waterfall fall zones, EventSystem under `GlobalManagers`. |

## Important Architectural Boundaries

- `AudioManager` owns all audio state and persistence.
- `AudioSettingsUI` is a binder only and must not access `AudioSource` children.
- `GlobalQuestManager` owns key progress and quest completion state.
- `CheckpointDoor` owns level-completion interaction and delegates actual transition to `SceneTransitionManager`.
- `SceneTransitionManager` orchestrates transition flow but delegates camera zoom and fade to dedicated systems.
- `HUDManager` owns HUD counters and UX message presentation.
- `Health` and `Hitbox` are the shared combat core used by player and enemies.

## Known Architecture Debt

- Gameplay input is legacy `Input` while UGUI uses the new Input System module.
- `PlayerController` owns movement, input, idle state, footstep audio calls, fall/death flow, scene reload, and HurtBox provisioning.
- Gameplay objects call `HUDManager` and `AudioManager` directly.
- `Health` drives animator parameters directly.
- Several runtime paths log frequently and should be gated before production.
- HurtBox ownership is split between prefab instructions and runtime creation by `PlayerController.EnsureHurtBox()`.
