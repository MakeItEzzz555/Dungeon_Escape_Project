# ADR: Menu Exit Transition Ownership

Date: 2026-05-19

## Status

Accepted

## Context

Pause menu exit used `MainMenu.ReturnToMainMenu()` to call `SceneManager.LoadScene("Main Menu")` directly, while `RunResultsPanel` exit used `SceneTransitionManager`. The direct path bypassed fade/camera cleanup and relied on brittle prefab button wiring. The persistent `AudioManager` also had no scene-load enforcement and the runtime prefab had no `mainMenuMusic` clip assigned.

## Decision

`SceneTransitionManager` owns both gameplay-to-menu exit and menu-to-gameplay play transitions. `MainMenu` delegates to it when available and keeps direct scene loading only as a fallback. `AudioManager` enforces music by scene on `sceneLoaded`, and `PersistentSystems.prefab` assigns the main menu music clip.

## Consequences

- Pause exit and results exit now use the same persistent transition owner.
- Main menu music is restored after any scene load into `Main Menu`.
- Starting gameplay from a returned main menu resets the camera to transition zoom and runs the expected zoom-out restore path.
- `MainMenu` still binds its pause exit button at runtime to tolerate broken serialized prefab references.
