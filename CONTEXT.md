# Ubiquitous Language Glossary

This file defines canonical project terms. Code, Inspector labels, Wiki pages, and feature documents should use these names consistently.

## Audio

| Term | Definition | Owner |
|:-----|:-----------|:------|
| `AudioManager` | Persistent singleton that owns music playback, SFX playback, volume state, and audio preference persistence. | PersistentSystems |
| `MusicVolume` | Normalized 0 to 1 value applied to the music `AudioSource`. Saved with PlayerPrefs key `MusicVolume`. | `AudioManager` |
| `SFXVolume` | Normalized 0 to 1 value applied to the SFX `AudioSource`. Saved with PlayerPrefs key `SFXVolume`. | `AudioManager` |
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
| `HUDManager` | UGUI gameplay HUD owner for coin/key counters and short UX messages. | UI |
| `HUD_HP` | HUD panel under `HUD_Canvas` that contains the player's HP images. | UI |
| `HPImage` | One UGUI Image under `HUD_HP` representing one current player hit point. | UI |
| `Player` | Main player avatar, currently represented by `Player 1.prefab`. | Player |
| `PlayerController` | Player movement, state lock, fall/death, idle, footstep, and HurtBox provisioning component. | Player |
| `PlayerAnimator` | Animator bridge for player movement, idle, attack, falling, and death parameters. | Player |
| `PlayerCombat` | Player attack input and sword hitbox animation-event owner. | Player Combat |
| `HurtBox` | Player child trigger collider used for enemy range detection and combat targeting. | Player Combat |
| `Hitbox` | Trigger damage component that enables only during attack windows. | Combat |
| `Health` | Shared hit point and death component used by player and enemies. | Combat |
| `EnemyAI` | Enemy state machine for idle, chase, attack, return home, and death. | Enemy |
| `EnemyRangeTrigger` | Enemy child trigger that reports aggro or attack range state to `EnemyAI`. | Enemy |
| `Lever_On` | Player-toggleable lever that emits a bool UnityEvent. | Interactables |
| `LeverResponder` | Adapter that converts lever bool signals into animator/gate/trap state changes. | Interactables |
| `InteractiveGate` | Gate controller that owns open/close animator parameters and physical collider state. | Interactables |
| `SpikeTrapFSM` | Trap state machine that opens damage windows from animation events and can kill the player. | Hazards |
| `FallZone` | Trigger hazard that starts the player fall sequence. | Hazards |
| `YSort` | Sprite presentation component that sets sorting order based on world Y. | Presentation |
| `AnimationAssets` | Inventory of non-deluxe animation clips, controllers, and animated tile assets under `Assets/Animations`. | Presentation |
