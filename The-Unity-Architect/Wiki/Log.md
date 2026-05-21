# Wiki Log

## 2026-05-21

- Created feature GDD: [Sword Consumable And Player Charge Attack](./Features/SwordConsumableAndPlayerChargeAttack.md), defining the scene-local `SwordConsumable` unlock and `PlayerChargeAttack` animator/dash/hitbox contract.
- Created and implemented feature GDD: [Player Dash](./Features/PlayerDash.md), adding Left Shift dash movement, cooldown, wall stop, stuck guard, fall/death cancellation, optional `IsDashing` animation bridge, and reused `ChargeTrail` presentation.
- Implemented script support for `SwordConsumable` and `PlayerChargeAttack`, including `Q` input, movement lock, cooldown, obstacle stop, charge hitbox animation events, charge trail hooks, fallback safety, and the `ChargeAttack` Animator trigger parameter.
- Changed `PlayerChargeAttack` dash startup to be animation-event authored by default, with `StartDash` and `StartChargeDash` both supported and dash fallback left as opt-in.
- Fixed fall/death failure interruption so water/fall zones and lethal hazards cancel regular/charged attacks, clear hitboxes/trails, and force `Fall_Dive` or death presentation immediately.
- Created feature GDD: [Enemy Death Rewards](./Features/EnemyDeathRewards.md), defining `DeathRewardObject` as the canonical scene-authored reward model for normal enemies and FinalBoss.
- Implemented enemy and FinalBoss `DeathRewardObject` activation: assigned reward objects are hidden on runtime start and revealed once on death.
- Changed the FinalBoss run-results-on-death default to disabled so reward chest/key progression can continue to the final `CheckpointDoor`.
- Added FinalBoss aggro-driven `HUD_HP_FinalBoss`, optional death reward reveal, and optional boss-death completion results targeting `Main Menu`.
- Capped `HUD_HP_FinalBoss` to the bound FinalBoss `Health.maxHealth` and added `AudioManager` scene-specific music overrides for `Level 2` and `Level 3`.
- Hardened `HUD_HP_FinalBoss` runtime creation so it no longer clones extra player HUD children, removes duplicate boss panels, and creates exact health slots. Updated `Hitbox` to resolve trigger-stay contacts once per activation so already-overlapping melee targets are not missed.
- Replaced incremental FinalBoss HUD slot add/remove with deterministic runtime rebuild: existing boss HP children are disabled, cleared, and recreated as exact direct `Image` slots from the bound FinalBoss `maxHealth`.
- Restored runtime `HUD_HP_FinalBoss` creation and added scheduled FinalBoss charge animation triggering so the first charge waits for the `ChargeAttack` trigger path before dash movement is allowed.

## 2026-05-20

- Created feature GDD: [FinalBoss](./Features/FinalBoss.md).
- Added FinalBoss phase, charge, hitbox, and animation contract terms to the root `CONTEXT.md` glossary.
- Linked FinalBoss from the wiki index and feature index.
- Refined FinalBoss documentation with canonical `FinalBossAnimationBridge` naming and existing attack SFX animation-event compatibility.
- Implemented FinalBoss script support: `IEnemyRangeReceiver`, shared `EnemyRangeTrigger` receiver routing, `FinalBossBehavior`, `FinalBossAnimationBridge`, and `FinalBossCombatBridge`.
- Updated Enemy AI, Combat, FinalBoss, and Scenes/Prefabs wiki notes for FinalBoss script support and remaining Editor wiring.
- Added FinalBoss charge dash animation-event support through `StartChargeDash` with serialized fallback timing.
- Created feature GDD: [FallZone Directional Nudge](./Features/FallZoneDirectionalNudge.md).
- Added `FallZoneDirectionalNudge` to the root `CONTEXT.md` glossary.
- Linked FallZone Directional Nudge from the wiki index and feature index.
- Implemented `FallZoneDirectionalNudge` through player-owned last valid cardinal direction and a short fall-sequence glide.

## 2026-05-18

- Created feature GDD: [Respawn Transition](./Features/RespawnTransition.md)
- Added `RespawnTransition` to the root `CONTEXT.md` glossary.
- Linked Respawn Transition from the wiki index and feature index.

## 2026-05-19

- Implemented [Respawn Transition](./Features/RespawnTransition.md) through `SceneTransitionManager.BeginRespawnTransition`.
- Updated [Scene Transition System](./Systems/SceneTransitions.md) and [Player System](./Systems/Player.md) to document death/fall respawn zoom and fade behavior.
- Fixed pause exit and returned-main-menu play transitions through `SceneTransitionManager`; logged [ADR: Menu Exit Transition Ownership](./ADR/2026-05-19-menu-exit-transition-ownership.md).
- Implemented [Player HP HUD](./Features/PlayerHPHUD.md), including health-change HUD binding, configurable trap damage, and fall-zone HP depletion.
- Created feature GDD: [Run Results Panel](./Features/RunResultsPanel.md).
- Added `RunResultsPanel`, `RunStats`, `LevelTimer`, `LevelCompletionFlow`, and `DeathResultsFlow` to the root `CONTEXT.md` glossary.

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
