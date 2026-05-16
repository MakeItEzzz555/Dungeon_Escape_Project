# Bootstrap And Persistence System

Last audited: 2026-05-16

## Ownership

The bootstrap layer ensures the game can start from any scene without requiring a dedicated bootstrap scene.

## Primary Scripts And Assets

| Asset | Role |
|:------|:-----|
| `Assets/Scripts/Core/GameBootstrapper.cs` | Static runtime initializer. |
| `Assets/Resources/PersistentSystems.prefab` | Persistent runtime infrastructure loaded through `Resources.Load("PersistentSystems")`. |
| Persistent singleton scripts | `AudioManager`, `GlobalQuestManager`, `SceneTransitionManager`, `CameraTransitionSystem`, `UITransitionManager`, `HUDManager` where present. |

## Runtime Flow

1. `GameBootstrapper.Bootstrap()` runs at `RuntimeInitializeLoadType.BeforeSceneLoad`.
2. It registers a `SceneManager.sceneLoaded` callback.
3. It checks for a root object named `PersistentSystems`.
4. If missing, it loads and instantiates `Resources/PersistentSystems`.
5. It creates a fallback `SafetyCamera` if no `Camera.main` exists.
6. After scene load, it calls `EnsureEventSystemExists()`.
7. `EnsureEventSystemExists()` creates a `Runtime EventSystem` only if no authored EventSystem exists, including inactive objects.

## Persistent Object Rules

| Object | Persistence Mechanism |
|:-------|:----------------------|
| `PersistentSystems` instance | `DontDestroyOnLoad` from `GameBootstrapper`. |
| Standalone manager instances | Many managers call `DontDestroyOnLoad` only if `transform.parent == null`. |
| Runtime fallback camera | `DontDestroyOnLoad` when created. |
| Runtime fallback EventSystem | Scene-local, created after scene load only when no EventSystem exists. |

## Scene Integration

- `Main Menu`, `Level 1`, and `Level 2` now contain authored EventSystems using `InputSystemUIInputModule`.
- `Level 2` has an EventSystem under `GlobalManagers`.
- The bootstrap fallback must remain after-scene-load only. Running it before scene load creates duplicates because scene-authored objects are not visible yet.

## Known Risks

- `GameBootstrapper` checks only for an object named `PersistentSystems`, not for individual required child services.
- `Resources.Load` requires `Assets/Resources/PersistentSystems.prefab` to remain at that exact path/name.
- `unity-doctor.js` currently expects `The-Unity-Architect/Assets/_Project/Scripts`, so Architect tooling does not understand the current repo layout without manual inspection.
