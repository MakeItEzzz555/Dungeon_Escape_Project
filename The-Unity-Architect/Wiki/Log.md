# Wiki Log

## 2026-05-16

- Created feature GDD: [Audio Volume Settings](./Features/AudioVolumeSettings.md)
- Created system note: [Audio System](./Systems/Audio.md)
- Created root `CONTEXT.md` glossary with audio settings terms.
- Performed full manual Architect wiki audit after `unity-doctor.js`, `unity-project-graph.js`, and `package-audit.js` reported repo-layout/tooling limitations.
- Added full system documentation set under `Wiki/Systems/`:
  - [System Map](./Systems/SystemMap.md)
  - [Bootstrap And Persistence](./Systems/BootstrapAndPersistence.md)
  - [Input](./Systems/Input.md)
  - [UI And HUD](./Systems/UIAndHUD.md)
  - [Player](./Systems/Player.md)
  - [Combat](./Systems/Combat.md)
  - [Enemy AI](./Systems/EnemyAI.md)
  - [Progression](./Systems/Progression.md)
  - [Collectibles](./Systems/Collectibles.md)
  - [Interactables](./Systems/Interactables.md)
  - [Traps And Hazards](./Systems/TrapsAndHazards.md)
  - [Scene Transitions](./Systems/SceneTransitions.md)
  - [Camera](./Systems/Camera.md)
  - [Environment And Presentation](./Systems/EnvironmentAndPresentation.md)
  - [Scenes And Prefabs](./Systems/ScenesAndPrefabs.md)
- Expanded `CONTEXT.md` with canonical terms for current systems.
- Refreshed `The-Unity-Architect/project-graph-report.md` and `project-graph.json` as manual authoritative graph files.
- Added current feature documentation under `Wiki/Features/`:
  - [Feature Index](./Features/FeatureIndex.md)
  - [Persistent Runtime Bootstrap](./Features/PersistentRuntimeBootstrap.md)
  - [Main Menu And Pause Menu](./Features/MainMenuAndPauseMenu.md)
  - [Player Movement And State](./Features/PlayerMovementAndState.md)
  - [Player Combat](./Features/PlayerCombat.md)
  - [Enemy Encounters](./Features/EnemyEncounters.md)
  - [Key Quest And Level Exit](./Features/KeyQuestAndLevelExit.md)
  - [Coin And Key Collection](./Features/CoinAndKeyCollection.md)
  - [HUD And Feedback](./Features/HUDAndFeedback.md)
  - [Lever And Gate Interaction](./Features/LeverAndGateInteraction.md)
  - [Chest Interaction](./Features/ChestInteraction.md)
  - [Traps And Fall Hazards](./Features/TrapsAndFallHazards.md)
  - [Scene Transition Experience](./Features/SceneTransitionExperience.md)
  - [Camera Follow And Zoom](./Features/CameraFollowAndZoom.md)
  - [Environment Presentation](./Features/EnvironmentPresentation.md)

## 2026-05-17

- Performed a second documentation coverage audit against current scripts, prefabs, resources, and `Assets/Animations`.
- Re-ran Architect tools:
  - `unity-doctor.js` still fails on the hardcoded `The-Unity-Architect/Assets/_Project/Scripts` path.
  - `unity-project-graph.js` still exports 0 indexed assets and overwrites graph output with an empty report.
  - `package-audit.js` still expects missing `Assets/Hovl Studio`.
- Confirmed all `Assets/Prefabs/**/*.prefab` files are represented in Systems/Features docs.
- Confirmed all non-meta `Assets/Resources` files are represented in Systems/Features docs.
- Added dormant UI script notes for `Play_Level.cs` and `Exit_Game.cs`.
- Added [Animation Assets Inventory](./Systems/AnimationAssets.md), covering 115 non-deluxe animation assets under `Assets/Animations` and excluding `Assets/Animations/AnimatedTilesDeluxe`.
