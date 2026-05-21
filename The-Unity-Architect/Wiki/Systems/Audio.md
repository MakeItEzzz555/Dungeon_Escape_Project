# Audio System

Last audited: 2026-05-21

## Ownership

`AudioManager` is the single source of truth for audio playback, volume state, and persistence.

Menu UI must communicate with `AudioManager` through public methods only. Menu UI must not reference or manipulate child `AudioSource` objects.

## Runtime Asset Ownership

| Asset | Role |
|:------|:-----|
| `Assets/Prefabs/Managers/AudioManager.prefab` | Authored audio manager prefab with music and SFX sources. |
| `Assets/Resources/PersistentSystems.prefab` | Runtime persistent root. Current direct script references include transition/camera systems; audio manager prefab also exists separately under manager prefabs. |
| `Assets/Scripts/Managers/AudioManager.cs` | Singleton runtime owner for playback, volume state, and persistence. |
| `Assets/Scripts/UI_Menu/AudioSettingsUI.cs` | UGUI slider binder for menu settings panels. |

## Persistent State

| State | Storage |
|:------|:--------|
| `MusicVolume` | PlayerPrefs key `MusicVolume` |
| `SFXVolume` | PlayerPrefs key `SFXVolume` |

Both values are normalized from 0 to 1 and default to 1 when no saved value exists.

## Presentation Binding

`AudioSettingsUI` is a reusable menu binder for UGUI sliders. It owns slider listener registration and synchronization, but it does not own audio state.

`AudioSettingsUI` is referenced by:

- `Assets/Scenes/Main Menu.unity`
- `Assets/Prefabs/Managers/HUD_Canvas.prefab`

## Playback API

| API | Purpose |
|:----|:--------|
| `PlayMusic(AudioClip clip, bool loop = true)` | Core music playback path. Prevents restarting the same clip if already playing. |
| `PlayMainMenuMusic()` | Starts the main menu loop. |
| `PlayGameplayMusic()` | Starts the active scene's gameplay loop. `Level 2` and `Level 3` use their dedicated clips when assigned, otherwise this falls back to `gameplayMusic`. |
| `PlayDeath()` | Death SFX wrapper. |
| `PlayWalk()` | Footstep SFX wrapper. |
| `PlayOpenChest()` | Chest SFX wrapper; current chest code calls `PlayInteract()` instead. |
| `PlayCoin()` | Coin pickup SFX wrapper. |
| `PlayKey()` | Key pickup SFX wrapper. |
| `PlayInteract()` | General interaction SFX wrapper. |
| `PlayPlayerAttack1/2/3()` | Player attack animation-event SFX wrappers. |
| `PlayEnemySwordSwing1/2()` | Enemy animation-event SFX wrappers. |

## Scene Music Enforcement

`AudioManager` subscribes to `SceneManager.sceneLoaded` and applies scene music centrally. `Main Menu` plays `mainMenuMusic`; `Level 2` plays `level2Music` when assigned; `Level 3` plays `level3Music` when assigned; all other gameplay scenes fall back to `gameplayMusic`. Explicit calls from transition code remain valid because `PlayGameplayMusic()` resolves the active scene before choosing a clip.

## Callers

| Caller | Calls |
|:-------|:------|
| `PlayerController` | `PlayWalk()` |
| `PlayerCombat` | `PlayPlayerAttack1/2/3()` |
| `EnemyAnimationEventForwarder` | `PlayEnemySwordSwing1/2()` |
| `Collect_coins` | `PlayCoin()` |
| `Collect_keys` | `PlayKey()` |
| `Lever_On` | `PlayInteract()` |
| `door_interaction` | `PlayInteract()` |
| `open_chest` | `PlayInteract()` |
| `MainMenu` | `PlayGameplayMusic()` after starting Level 1 |
| `AudioSettingsUI` | `SetMusicVolume()`, `SetSFXVolume()`, `GetMusicVolume()`, `GetSFXVolume()` |

## Known Risks

- Audio calls are made directly from gameplay scripts. This is functional but couples simulation scripts to presentation/audio.
- `PlayOpenChest()` exists but `open_chest` currently uses `PlayInteract()`.
