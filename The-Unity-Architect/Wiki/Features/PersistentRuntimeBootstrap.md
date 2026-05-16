# Persistent Runtime Bootstrap

Last audited: 2026-05-16

## Feature Summary

| Field | Value |
|:------|:------|
| Player-visible purpose | The game can be started from Main Menu, Level 1, or Level 2 without manually opening a bootstrap scene. |
| Runtime purpose | Ensure persistent services, camera safety, and UI input fallback exist at runtime. |
| Related system | [Bootstrap And Persistence](../Systems/BootstrapAndPersistence.md) |

## Current Behavior

1. Runtime initialization runs before scene load.
2. If no root object named `PersistentSystems` exists, the bootstrapper loads `Resources/PersistentSystems`.
3. If no main camera exists, a fallback `SafetyCamera` is created.
4. After scene load, if no EventSystem exists, a scene-local `Runtime EventSystem` is created.

## Implementation References

| Asset | Role |
|:------|:-----|
| `Assets/Scripts/Core/GameBootstrapper.cs` | Static bootstrap initializer. |
| `Assets/Resources/PersistentSystems.prefab` | Runtime-loaded persistent infrastructure prefab. |

## Acceptance Criteria Already Met

- Starting directly from a gameplay scene can create missing persistent systems.
- Scene-authored EventSystems are preserved.
- The fallback EventSystem runs after scene load to avoid duplicates.

## Known Gaps

- The bootstrapper checks for `PersistentSystems` by object name.
- It does not validate every required service under `PersistentSystems`.
