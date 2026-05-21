# Ubiquitous Language Glossary

This file defines canonical project terms. Code, Inspector labels, Wiki pages, and feature documents should use these names consistently.

## Audio

| Term | Definition | Owner |
|:-----|:-----------|:------|
| `AudioManager` | Persistent singleton that owns music playback, SFX playback, volume state, and audio preference persistence. | PersistentSystems |
| `MusicVolume` | Normalized 0 to 1 value applied to the music `AudioSource`. Saved with PlayerPrefs key `MusicVolume`. | `AudioManager` |
| `SFXVolume` | Normalized 0 to 1 value applied to the SFX `AudioSource`. Saved with PlayerPrefs key `SFXVolume`. | `AudioManager` |
| `Level2Music` | Optional `AudioManager` music clip override used when the active scene is `Level 2`. Falls back to `GameplayMusic` when unassigned. | `AudioManager` |
| `Level3Music` | Optional `AudioManager` music clip override used when the active scene is `Level 3`. Falls back to `GameplayMusic` when unassigned. | `AudioManager` |
| `AudioSettingsUI` | Scene or menu UI binder that reads/writes `MusicVolume` and `SFXVolume` through `AudioManager` public methods only. | Menu presentation |
| `MusicSlider` | UI Slider assigned to `AudioSettingsUI` for editing `MusicVolume`. | Menu presentation |
| `SFXSlider` | UI Slider assigned to `AudioSettingsUI` for editing `SFXVolume`. | Menu presentation |

## Existing Core Terms

| Term | Definition | Owner |
|:-----|:-----------|:------|
| `PersistentSystems` | Runtime prefab loaded from Resources that hosts global services such as audio and transitions. | Bootstrap |
| `HUD_Canvas` | Gameplay/pause UI prefab that contains HUD and pause menu presentation. | Scene UI |
| `Main Menu` | Menu scene used before gameplay begins. | Scene UI |
| `GameBootstrapper` | Static runtime initializer that loads `PersistentSystems`, creates fallback camera infrastructure, and ensures a scene EventSystem exists after scene load. | Bootstrap |
| `GlobalQuestManager` | Persistent singleton that owns collected/required key state and quest completion. | Progression |
| `CheckpointDoor` | Level completion interactable that checks quest completion and starts scene transition. | Progression |
| `SceneTransitionManager` | Persistent singleton that sequences zoom, fade, scene load, and player control restoration. | Scene Transitions |
| `RespawnTransition` | Player-facing death or fall recovery sequence that reuses camera zoom and UI fade before reloading the active scene and restoring control. | Scene Transitions |
| `CameraTransitionSystem` | Camera zoom owner used during scene transitions. | Camera |
| `UITransitionManager` | Fade overlay owner used during scene transitions. | Scene Transitions |
| `HUDManager` | UGUI gameplay HUD owner for coin/key counters, HP image displays, boss HP display, short UX messages, and run results presentation. | UI |
| `HUD_HP` | HUD panel under `HUD_Canvas` that contains the player's HP images. | UI |
| `HUD_HP_FinalBoss` | Boss HP image panel shown by `HUDManager` while FinalBoss aggro is active. If the prefab does not contain it, `HUDManager` creates a clean mirrored top-right panel at runtime and populates exactly one image slot per bound FinalBoss max health. | UI |
| `HPImage` | One UGUI Image under `HUD_HP` representing one current player hit point. | UI |
| `HPConsumable` | Trigger-based health pickup that heals the player through `Health.Heal()` and then consumes itself. | Collectibles |
| `RunResultsPanel` | Results panel under `HUD_Canvas` shown over full black after either level completion or player death/fall, with mode-specific primary action. | UI |
| `RunStats` | Per-attempt summary values shown by `RunResultsPanel`: coins collected, total coins, active gameplay time, enemies killed, and total active enemies. | Progression |
| `LevelTimer` | Per-attempt active gameplay timer that starts when player control is restored, pauses outside gameplay, stops on completion or failure, and supplies elapsed time to `RunResultsPanel`. | Progression |
| `LevelCompletionFlow` | Checkpoint-driven sequence that locks gameplay, stops the `LevelTimer`, zooms/fades to black, shows `RunResultsPanel` in completion mode, then continues to the next scene after player confirmation. | Progression |
| `DeathResultsFlow` | Death/fall sequence that locks gameplay, stops the `LevelTimer`, zooms/fades to black, shows `RunResultsPanel` in respawn mode, then reloads the active scene after player confirmation. | Progression |
| `Player` | Main player avatar, currently represented by `Player 1.prefab`. | Player |
| `PlayerController` | Player movement, state lock, fall/death, idle, footstep, and HurtBox provisioning component. | Player |
| `PlayerAnimator` | Animator bridge for player movement, idle, attack, falling, and death parameters. | Player |
| `PlayerCombat` | Player attack input and sword hitbox animation-event owner. | Player Combat |
| `HurtBox` | Shared child trigger collider used as the valid damage receiver for character `Hitbox` contact. Player, enemies, and FinalBoss should expose damage through `HurtBox`, while aggro ranges, attack ranges, and movement colliders are not damage receivers. | Combat |
| `Hitbox` | Trigger damage component that enables only during attack windows, targets `HurtBox` receivers by default, and damages each target once per enabled window even if the target was already overlapping when the hitbox activated. | Combat |
| `Health` | Shared hit point and death component used by player and enemies. | Combat |
| `EnemyAI` | Enemy state machine for idle, chase, attack, return home, and death. | Enemy |
| `IEnemyRangeReceiver` | Shared receiver contract for components that accept aggro and attack range updates from `EnemyRangeTrigger`. | Enemy |
| `EnemyRangeTrigger` | Enemy child trigger that reports aggro or attack range state to an `IEnemyRangeReceiver`, including `EnemyAI` and `FinalBossBehavior`. | Enemy |
| `FinalBoss` | Boss enemy variant that reuses shared combat, health, hitbox, and range-trigger components while owning boss-specific phase and attack-selection behavior. | Enemy |
| `FinalBossBehavior` | FinalBoss-owned behavior controller responsible for boss phases, attack selection, charge movement, cooldowns, recovery locks, boss HUD visibility, optional death loot, optional completion results, and death state. It is tuned through serialized Inspector fields rather than hard-coded constants. | Enemy |
| `FinalBossAnimationBridge` | FinalBoss presentation bridge that writes directional Animator parameters and triggers regular attack, charge attack, hurt, walking, and death animation paths. | Enemy |
| `FinalBossCombatBridge` | FinalBoss animation-event bridge that exposes regular hitbox and charge hitbox enable/disable methods to animation clips and forwards those calls to the correct hitbox components. | Enemy Combat |
| `PhaseTwo` | FinalBoss combat phase unlocked permanently when FinalBoss reaches half health. | Enemy |
| `PhaseTwoHealthThreshold` | Serialized normalized health threshold with default value `0.5`; when FinalBoss health reaches or falls below it, `PhaseTwo` unlocks permanently. | Enemy |
| `ChargeAttack` | FinalBoss special attack unlocked in `PhaseTwo`; it uses a dedicated animation path and deals more damage than the regular attack. | Enemy Combat |
| `ChargeAnimationTriggerDelay` | Serialized FinalBoss delay before sending the `ChargeAttack` Animator trigger, used to avoid the first charge trigger being swallowed by idle/hurt transition timing. | Enemy Combat |
| `ChargeAttackTrigger` | Animator trigger named `ChargeAttack` that starts the FinalBoss charge animation path. | Enemy Combat |
| `ChargeAttackCooldown` | Longer independent cooldown that controls when `ChargeAttack` can override the regular FinalBoss attack during `PhaseTwo`. | Enemy Combat |
| `ChargeHitbox` | Dedicated FinalBoss hitbox used by `ChargeAttack`; it applies higher damage than the regular attack and can damage each target only once per charge activation. | Enemy Combat |
| `ChargeDamageWindow` | Active damage period for `ChargeAttack`; `ChargeHitbox` is enabled for the full dash movement and disabled during wind-up and `ChargeRecovery`. | Enemy Combat |
| `ChargeTrail` | Presentation-only pooled sprite afterimage trail shown during the active FinalBoss `ChargeAttack` dash. It samples the current boss animation frame, has no collider, no damage, and no gameplay authority. | Enemy Combat |
| `StartChargeDash` | FinalBoss animation-event method exposed by `FinalBossCombatBridge`; starts the scripted charge dash from the charge animation at the authored event frame. | Enemy Combat |
| `ChargeDirection` | Fixed straight-line direction captured from FinalBoss to the player at the start of `ChargeAttack`; it does not retarget while the charge is active. | Enemy Combat |
| `ChargeRecovery` | Short post-charge lockout where FinalBoss cannot chase or attack, giving the player a readable punish window. | Enemy Combat |
| `ChargeObstacleImpact` | FinalBoss charge outcome where hitting a blocking obstacle immediately stops `ChargeAttack` and enters `ChargeRecovery`. | Enemy Combat |
| `ChargeInterruptRule` | FinalBoss rule where `ChargeAttack` is not cancelled by player damage during the active dash; damage during wind-up may show feedback but does not cancel the committed charge. | Enemy Combat |
| `FinalBossDeathRewards` | Optional FinalBoss death behavior that can spawn a configured loot prefab and start completion-mode `RunResultsPanel` flow to `Main Menu`. | Progression |
| `Lever_On` | Player-toggleable lever that emits a bool UnityEvent. | Interactables |
| `LeverResponder` | Adapter that converts lever bool signals into animator/gate/trap state changes. | Interactables |
| `InteractiveGate` | Gate controller that owns open/close animator parameters and physical collider state. | Interactables |
| `SpikeTrapFSM` | Trap state machine that opens damage windows from animation events and can kill the player. | Hazards |
| `FallZone` | Trigger hazard that starts the player fall sequence. | Hazards |
| `FallZoneDirectionalNudge` | Short timed glide applied in the player's last valid cardinal movement direction after `FallZone` locks normal movement and starts a fall sequence. Uses player-owned default tuning for consistent `FallZone` behavior, falls back to down when no direction exists, and does not apply to trap hazards. | Hazards |
| `YSort` | Sprite presentation component that sets sorting order based on world Y. | Presentation |
| `AnimationAssets` | Inventory of non-deluxe animation clips, controllers, and animated tile assets under `Assets/Animations`. | Presentation |
