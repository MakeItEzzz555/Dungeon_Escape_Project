# Unity Project Graph Report

> Updated on 2026-05-17 by Codex after a second manual coverage audit.
> The stock `unity-project-graph.js` currently exports 0 indexed assets for this repo layout and overwrites this file with an empty report when run. This manual report is the authoritative project graph until the Architect tooling is adapted.

## Next AI Context Load Order

1. `CONTEXT.md`
2. `The-Unity-Architect/project-graph-report.md`
3. `The-Unity-Architect/Wiki/Systems/SystemMap.md`
4. `The-Unity-Architect/Wiki/Features/FeatureIndex.md`
5. Relevant files under `The-Unity-Architect/Wiki/Systems/` and `The-Unity-Architect/Wiki/Features/`
6. `The-Unity-Architect/Wiki/debug_report.md`

## Summary

| Type | Count |
|:-----|------:|
| Runtime C# scripts audited | 34 |
| Main project scenes audited | 3 |
| Project prefabs found under `Assets/Prefabs` | 38 |
| Runtime resource prefabs found | 1 |
| System wiki docs populated | 17 |
| Feature docs populated | 16 |
| Non-deluxe animation assets audited | 115 |
| Broken scripts reported by stock graph tool | 0 |

## Architect Tooling Results

| Tool | Result | Notes |
|:-----|:-------|:------|
| `node The-Unity-Architect/execution/unity-doctor.js` | Failed | Looks for `The-Unity-Architect/Assets/_Project/Scripts`; actual scripts live in `Assets/Scripts`. |
| `node The-Unity-Architect/execution/unity-project-graph.js` | Completed but empty | Exported 0 assets, 0 scenes, 0 scripts, then overwrote graph output. |
| `node The-Unity-Architect/execution/package-audit.js` | Failed | Expected `Assets/Hovl Studio`, which is not present. |

## Coverage Audit Result

| Area | Result |
|:-----|:-------|
| `Assets/Scripts/**/*.cs` | Covered in Systems/Features docs. `Play_Level.cs` and `Exit_Game.cs` are documented as dormant empty legacy UI scripts. |
| `Assets/Prefabs/**/*.prefab` | Covered in Systems/Features docs or Scenes/Prefabs inventory. |
| `Assets/Resources` | Covered. `PersistentSystems.prefab` is documented; `.meta` files intentionally ignored. |
| `Assets/Animations` | Covered by `AnimationAssets.md`, excluding `Assets/Animations/AnimatedTilesDeluxe`. |

## Main Scenes

| Scene | Purpose | UI Input Notes |
|:------|:--------|:---------------|
| `Assets/Scenes/Main Menu.unity` | Menu entry point | Contains `EventSystem` with `InputSystemUIInputModule`; contains main menu settings sliders. |
| `Assets/Scenes/Level 1.unity` | Gameplay level | Contains `EventSystem` with `InputSystemUIInputModule`; contains player, HUD, quest, levers, gates, traps, keys, coins, chests, checkpoint door. |
| `Assets/Scenes/Level 2.unity` | Gameplay level | Contains `EventSystem` under `GlobalManagers` with `InputSystemUIInputModule`; contains player, HUD, quest, enemies, hazards, keys, coins, gates, chests. |

## System Wiki Documents

| System | Wiki File |
|:-------|:----------|
| System Map | `The-Unity-Architect/Wiki/Systems/SystemMap.md` |
| Bootstrap and Persistence | `The-Unity-Architect/Wiki/Systems/BootstrapAndPersistence.md` |
| Input | `The-Unity-Architect/Wiki/Systems/Input.md` |
| Audio | `The-Unity-Architect/Wiki/Systems/Audio.md` |
| UI and HUD | `The-Unity-Architect/Wiki/Systems/UIAndHUD.md` |
| Player | `The-Unity-Architect/Wiki/Systems/Player.md` |
| Combat | `The-Unity-Architect/Wiki/Systems/Combat.md` |
| Enemy AI | `The-Unity-Architect/Wiki/Systems/EnemyAI.md` |
| Progression | `The-Unity-Architect/Wiki/Systems/Progression.md` |
| Collectibles | `The-Unity-Architect/Wiki/Systems/Collectibles.md` |
| Interactables | `The-Unity-Architect/Wiki/Systems/Interactables.md` |
| Traps and Hazards | `The-Unity-Architect/Wiki/Systems/TrapsAndHazards.md` |
| Scene Transitions | `The-Unity-Architect/Wiki/Systems/SceneTransitions.md` |
| Camera | `The-Unity-Architect/Wiki/Systems/Camera.md` |
| Environment and Presentation | `The-Unity-Architect/Wiki/Systems/EnvironmentAndPresentation.md` |
| Animation Assets | `The-Unity-Architect/Wiki/Systems/AnimationAssets.md` |
| Scenes and Prefabs | `The-Unity-Architect/Wiki/Systems/ScenesAndPrefabs.md` |

## Feature Wiki Documents

| Feature | Wiki File |
|:--------|:----------|
| Feature Index | `The-Unity-Architect/Wiki/Features/FeatureIndex.md` |
| Persistent Runtime Bootstrap | `The-Unity-Architect/Wiki/Features/PersistentRuntimeBootstrap.md` |
| Main Menu And Pause Menu | `The-Unity-Architect/Wiki/Features/MainMenuAndPauseMenu.md` |
| Audio Volume Settings | `The-Unity-Architect/Wiki/Features/AudioVolumeSettings.md` |
| Player Movement And State | `The-Unity-Architect/Wiki/Features/PlayerMovementAndState.md` |
| Player Combat | `The-Unity-Architect/Wiki/Features/PlayerCombat.md` |
| Enemy Encounters | `The-Unity-Architect/Wiki/Features/EnemyEncounters.md` |
| Key Quest And Level Exit | `The-Unity-Architect/Wiki/Features/KeyQuestAndLevelExit.md` |
| Coin And Key Collection | `The-Unity-Architect/Wiki/Features/CoinAndKeyCollection.md` |
| HUD And Feedback | `The-Unity-Architect/Wiki/Features/HUDAndFeedback.md` |
| Lever And Gate Interaction | `The-Unity-Architect/Wiki/Features/LeverAndGateInteraction.md` |
| Chest Interaction | `The-Unity-Architect/Wiki/Features/ChestInteraction.md` |
| Traps And Fall Hazards | `The-Unity-Architect/Wiki/Features/TrapsAndFallHazards.md` |
| Scene Transition Experience | `The-Unity-Architect/Wiki/Features/SceneTransitionExperience.md` |
| Camera Follow And Zoom | `The-Unity-Architect/Wiki/Features/CameraFollowAndZoom.md` |
| Environment Presentation | `The-Unity-Architect/Wiki/Features/EnvironmentPresentation.md` |

## Runtime Architecture

### Bootstrap

- `Scripts.Core.GameBootstrapper` runs from static runtime initialization.
- It loads `Assets/Resources/PersistentSystems.prefab` through `Resources.Load("PersistentSystems")` when no root object named `PersistentSystems` exists.
- It creates a fallback `SafetyCamera` when `Camera.main` is missing.
- It creates a scene-local `Runtime EventSystem` after scene load only when no authored EventSystem exists.

### Persistent And Global Managers

| Manager | Role |
|:--------|:-----|
| `AudioManager` | Music/SFX playback, MusicVolume/SFXVolume persistence, audio source volume application. |
| `GlobalQuestManager` | Required/collected key state and quest completion. |
| `SceneTransitionManager` | Zoom/fade/load scene transition orchestration. |
| `CameraTransitionSystem` | Transition zoom and duplicate `AudioListener` cleanup. |
| `UITransitionManager` | Fade overlay alpha and raycast blocking. |
| `HUDManager` | UGUI coin/key counters and short UX messages. |

### Gameplay Systems

| System | Primary Scripts |
|:-------|:----------------|
| Player | `PlayerController`, `PlayerAnimator`, `PlayerCombat`, `FollowPlayer`, `YSort` |
| Combat | `Health`, `Hitbox`, `PlayerCombat`, `EnemyCombat`, `EnemyAnimationEventForwarder` |
| Enemy | `EnemyAI`, `EnemyRangeTrigger`, `EnemyCombat`, `EnemyAnimationEventForwarder` |
| Progression | `GlobalQuestManager`, `CheckpointDoor`, `Collect_keys` |
| Collectibles | `Collect_coins`, `Collect_keys` |
| Interactables | `Lever_On`, `LeverResponder`, `InteractiveGate`, `door_interaction`, `open_chest`, `auto_gate` |
| Hazards | `SpikeTrapFSM`, `FallZone` |
| Presentation | `YSort`, `LightRaySequencedFX`, animation assets |

## Scene-Referenced Prefabs

| Prefab | Scenes |
|:-------|:-------|
| `Assets/Prefabs/Player/Player 1.prefab` | Level 1, Level 2 |
| `Assets/Prefabs/Managers/HUD_Canvas.prefab` | Level 1, Level 2 |
| `Assets/Prefabs/Managers/QuestManager.prefab` | Level 1, Level 2 |
| `Assets/Prefabs/Managers/PlayerAnimator.prefab` | Level 1, Level 2 |
| `Assets/Prefabs/Collectibles/BlueCoin.prefab` | Level 1, Level 2 |
| `Assets/Prefabs/Collectibles/Key 1 - GOLD - .prefab` | Level 1, Level 2 |
| `Assets/Prefabs/Doors_Gates/CheckPointDoor.prefab` | Level 1, Level 2 |
| `Assets/Prefabs/Doors_Gates/Door.prefab` | Level 1 |
| `Assets/Prefabs/Doors_Gates/MetalGate.prefab` | Level 1, Level 2 |
| `Assets/Prefabs/Doors_Gates/MetalGateInter.prefab` | Level 1, Level 2 |
| `Assets/Prefabs/Interactables/LeverSwitch.prefab` | Level 1, Level 2 |
| `Assets/Prefabs/Interactables/chest_yellow_blue.prefab` | Level 1, Level 2 |
| `Assets/Prefabs/Traps/Spike Trap.prefab` | Level 1, Level 2 |
| `Assets/Prefabs/Enemy/Enemy.prefab` | Level 2 |

## Important Script Reference Findings

- `GameBootstrapper.cs` has no scene/prefab reference because it runs statically.
- `CameraTransitionSystem.cs` and `UITransitionManager.cs` are referenced by `Assets/Resources/PersistentSystems.prefab`.
- `AudioManager.cs` is referenced by `Assets/Prefabs/Managers/AudioManager.prefab`.
- `MainMenu.cs` is referenced by Main Menu scene, Level 1 scene, and `HUD_Canvas.prefab`.
- `AudioSettingsUI.cs` is referenced by Main Menu scene and `HUD_Canvas.prefab`.
- `FallZone.cs` is directly referenced in Level 2 scene.
- `LeverResponder.cs` is referenced by scenes and by gate/trap prefabs.
- `YSort.cs` is widely referenced by player, enemy, collectibles, interactables, traps, doors/gates, and environment prefabs.
- `Exit_Game.cs` and `Play_Level.cs` currently have no scene/prefab references and contain only empty Unity lifecycle methods.

## Animation Inventory

- Full inventory: `The-Unity-Architect/Wiki/Systems/AnimationAssets.md`
- Included root: `Assets/Animations`
- Excluded folder: `Assets/Animations/AnimatedTilesDeluxe`
- Audited files: 115
  - `.anim`: 63
  - `.asset`: 30
  - `.controller`: 22

## Architecture Risks

1. Input is mixed: UGUI uses the new Input System, gameplay uses legacy `UnityEngine.Input`.
2. `PlayerController` owns movement, input, idle, audio calls, HurtBox provisioning, fall/death, and scene reload behavior.
3. Gameplay systems call `AudioManager` and `HUDManager` directly.
4. `Health` drives Animator parameters directly and uses `SendMessage("OnDeath")` for non-player death callbacks.
5. Scene transition logic manipulates player control and physics from both `CheckpointDoor` and `SceneTransitionManager`.
6. Several high-frequency runtime paths log directly to the console.
7. HurtBox ownership is not yet settled between prefab-authored setup and runtime creation.

## Script Index

| Script | Class / Type | Path |
|:-------|:-------------|:-----|
| `AudioManager.cs` | `AudioManager` | `Assets/Scripts/Managers/AudioManager.cs` |
| `AudioSettingsUI.cs` | `AudioSettingsUI` | `Assets/Scripts/UI_Menu/AudioSettingsUI.cs` |
| `CameraTransitionSystem.cs` | `Scripts.Managers.CameraTransitionSystem` | `Assets/Scripts/Managers/CameraTransitionSystem.cs` |
| `CheckpointDoor.cs` | `Scripts.Interactables.CheckpointDoor` | `Assets/Scripts/Interactables/CheckpointDoor.cs` |
| `Collect_coins.cs` | `Collect_coins` | `Assets/Scripts/Collectibles/Collect_coins.cs` |
| `Collect_keys.cs` | `Collect_keys` | `Assets/Scripts/Collectibles/Collect_keys.cs` |
| `EnemyAI.cs` | `EnemyAI`, `EnemyState` | `Assets/Scripts/Enemy/EnemyAI.cs` |
| `EnemyAnimationEventForwarder.cs` | `EnemyAnimationEventForwarder` | `Assets/Scripts/Enemy/EnemyAnimationEventForwarder.cs` |
| `EnemyCombat.cs` | `EnemyCombat` | `Assets/Scripts/Enemy/EnemyCombat.cs` |
| `EnemyRangeTrigger.cs` | `EnemyRangeTrigger`, `RangeType` | `Assets/Scripts/Enemy/EnemyRangeTrigger.cs` |
| `Exit_Game.cs` | `Exit_Game` | `Assets/Scripts/UI_Menu/Exit_Game.cs` |
| `FallZone.cs` | `Scripts.Interactables.FallZone` | `Assets/Scripts/Interactables/FallZone.cs` |
| `FollowPlayer.cs` | `FollowPlayer` | `Assets/Scripts/FollowPlayer.cs` |
| `GameBootstrapper.cs` | `Scripts.Core.GameBootstrapper` | `Assets/Scripts/Core/GameBootstrapper.cs` |
| `GlobalQuestManager.cs` | `Scripts.Managers.GlobalQuestManager` | `Assets/Scripts/Managers/GlobalQuestManager.cs` |
| `HUDManager.cs` | `HUDManager` | `Assets/Scripts/Managers/HUDManager.cs` |
| `Health.cs` | `Health` | `Assets/Scripts/Core/Health.cs` |
| `Hitbox.cs` | `Hitbox` | `Assets/Scripts/Core/Hitbox.cs` |
| `InteractiveGate.cs` | `InteractiveGate` | `Assets/Scripts/Interactables/InteractiveGate.cs` |
| `LeverResponder.cs` | `LeverResponder` | `Assets/Scripts/Interactables/LeverResponder.cs` |
| `Lever_on.cs` | `Lever_On`, `LeverEvent` | `Assets/Scripts/Interactables/Lever_on.cs` |
| `LightRaysPulse.cs` | `LightRaySequencedFX` | `Assets/Scripts/LightRaysPulse.cs` |
| `MainMenu.cs` | `MainMenu` | `Assets/Scripts/UI_Menu/MainMenu.cs` |
| `Play_Level.cs` | `Play_Level` | `Assets/Scripts/UI_Menu/Play_Level.cs` |
| `PlayerAnimator.cs` | `PlayerAnimator` | `Assets/Scripts/Player/PlayerAnimator.cs` |
| `PlayerCombat.cs` | `PlayerCombat` | `Assets/Scripts/Player/PlayerCombat.cs` |
| `PlayerController.cs` | `PlayerController` | `Assets/Scripts/Player/PlayerController.cs` |
| `SceneTransitionManager.cs` | `Scripts.Managers.SceneTransitionManager`, `TransitionState` | `Assets/Scripts/Managers/SceneTransitionManager.cs` |
| `SpikeTrapFSM.cs` | `SpikeTrapFSM`, `TrapState` | `Assets/Scripts/Interactables/SpikeTrapFSM.cs` |
| `UITransitionManager.cs` | `Scripts.Managers.UITransitionManager` | `Assets/Scripts/Managers/UITransitionManager.cs` |
| `YSort.cs` | `YSort` | `Assets/Scripts/YSort.cs` |
| `auto_gate.cs` | `auto_gate` | `Assets/Scripts/auto_gate.cs` |
| `door_interaction.cs` | `door_interaction` | `Assets/Scripts/Interactables/door_interaction.cs` |
| `open_chest.cs` | `open_chest` | `Assets/Scripts/Interactables/open_chest.cs` |
