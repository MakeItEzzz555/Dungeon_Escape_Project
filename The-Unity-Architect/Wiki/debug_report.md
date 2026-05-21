# Debug Report

## FinalBoss Death Results Flicker

Date: 2026-05-21

### Symptom

- After FinalBoss death, the death animation finishes and the scene briefly shows an empty background before `RunResultsPanel` fades in.
- The boss-death completion path does not feel like checkpoint completion because the camera zoom/fade transition starts late.

### Confirmed Data

- `Level 3.unity` has `FinalBossBehavior.showRunResultsOnDeath` enabled and targets `Main Menu`.
- `FinalBossBehavior.OnDeath()` revealed the optional `DeathRewardObject`, waited `bossDeathTransitionDelay`, then called `SceneTransitionManager.BeginLevelCompletionTransition()`.
- During that delay, `SceneTransitionManager.currentState` stayed `None`, so no transition zoom or fade overlay was active.
- `RunResultsPanel` is designed to fade in only after the fade overlay is fully black.

### Root Cause

FinalBoss death delayed before entering the shared `LevelCompletionFlow`. That left the transition infrastructure idle during the boss death presentation window, creating a visible uncovered gap before the black fade and results panel.

### Fix Applied

- Added `SceneTransitionManager.BeginLevelCompletionTransitionWithPresentationWindow()` so FinalBoss can enter the shared completion transition immediately while still delaying fade until the death animation read window completes.
- Added `CameraTransitionSystem.StartZoomInAndHoldTarget()` plus `ReleaseHeldTarget()` so the camera can zoom to and hold the FinalBoss target through the death presentation window, then release once the screen is black.
- Updated `FinalBossBehavior` to call the new completion presentation-window path immediately on death instead of waiting in a local coroutine before starting the transition.

### Follow-up Finding

- Unity did recompile the changed scripts: the Editor log reported `Mono: successfully reloaded assembly`.
- FinalBoss death clips are approximately `1.52s`, with `Death_Down` at `1.683s`.
- The serialized `bossDeathTransitionDelay` is `1.68s`, and the transition fade duration is `0.5s`.
- Waiting the full death window before starting fade still leaves the final invisible/dead animation frame visible for the fade duration.
- Runtime logs showed `CameraTransitionSystem` switching the follow target to `ResultsPanel` during FinalBoss death.
- `Assets/Scenes/Level 3.unity` had `FinalBossBehavior.resultsZoomTarget` assigned to a stripped `RectTransform` from the `HUD_Canvas` prefab's `ResultsPanel` instead of the FinalBoss world transform.

### Follow-up Fix Applied

- `SceneTransitionManager` now starts the fade at `presentationDuration - fadeInDuration`, so the screen reaches full black at the end of the boss death presentation window instead of after it.
- `Level 3.unity` now assigns `resultsZoomTarget` to the FinalBoss root transform.
- `FinalBossBehavior.ResolveResultsZoomTarget()` rejects UI `RectTransform` targets and falls back to the FinalBoss transform, preventing future HUD targets from driving camera transitions.

## Pause Menu Buttons Not Responding

Date: 2026-05-16

### Symptom

- Pressing `ESC` opens the pause menu.
- Pause menu buttons do not work, including Resume, Settings, and Exit.

### Confirmed Data

- `FollowPlayer.Update()` still receives `ESC`, so game script keyboard input is running.
- `Level 1.unity` has an active `EventSystem`.
- `Level 1.unity` uses `UnityEngine.InputSystem.UI.InputSystemUIInputModule`, not the legacy `StandaloneInputModule`.
- `HUD_Canvas.prefab` has a `GraphicRaycaster`.
- `Resume_bttn` is interactable and has an OnClick call to `MainMenu.ResumeGame`.
- `Settings_bttn` is interactable and has OnClick calls that activate `settings_panel` and deactivate `MainMenu`.
- `Close_bttn` is interactable and has OnClick calls that deactivate `settings_panel` and activate `MainMenu`.
- `Exit_bttn` has a broken OnClick target: `{fileID: 0}` with `MainMenu.QuitGame`.

### Current Hypotheses

1. If a full-screen UI graphic from recent manual slider setup is blocking raycasts, then buttons will not highlight on hover and no OnClick will fire.
2. If the EventSystem/InputSystem module is not receiving pointer clicks at runtime, then buttons will not highlight and sliders will not drag.
3. If button bindings are broken, then buttons will highlight/press but their expected methods will not run.
4. If the pause menu canvas or parent CanvasGroup is non-interactable or blocking state is wrong at runtime, then buttons may be visible but not receive pointer events.

### Next Experiment

In Play Mode, open the pause menu and hover/click `Resume_bttn`:

- If the button does not visually highlight, investigate raycast blockers/EventSystem.
- If the button highlights/presses but does not resume, investigate OnClick binding/target.

### Fix Applied

- `MainMenu.PauseGame()` now saves the pre-pause cursor state, unlocks the cursor, and makes it visible while the pause menu is open.
- `MainMenu.ResumeGame()` now restores the saved cursor state after unpausing.
- `MainMenu.PauseGame()` now ensures the Input System update mode is `ProcessEventsInDynamicUpdate` so UI input is compatible with `Time.timeScale = 0`.

### Follow-up Finding

The cursor/update-mode fix did not resolve the issue. A stronger root cause was found in `PersistentSystems.prefab`:

- `Fade_Overlay_Canvas` has sorting order `100`.
- `HUD_Canvas` and the pause menu canvas have sorting order `0`.
- `FadeOverlay` starts transparent (`alpha = 0`) but still has `Raycast Target = true`.
- Because it is persistent and sorted above the pause UI, it can invisibly consume all mouse hover/click raycasts.

### Follow-up Fix Applied

- `UITransitionManager.Awake()` now disables `fadeOverlay.raycastTarget` when the overlay is idle.
- `UITransitionManager.FadeRoutine()` enables raycast blocking only while fading/black, then disables it again when the target alpha returns to transparent.

### Remaining Known Issue

- `Exit_bttn` in `HUD_Canvas.prefab` still has a broken OnClick target (`{fileID: 0}`) and must be rewired in the Inspector to call `MainMenu.QuitGame`.

## Level 2 Pause Menu Buttons Not Responding

Date: 2026-05-16

### Symptom

- Pause menu works in Main Menu -> Level 1 flow.
- Music/SFX settings persist correctly across scenes.
- In Level 2, whether reached by transition or started directly, the pause menu appears but does not receive mouse hover/click input.

### Confirmed Data

- `Main Menu.unity` contains an `EventSystem` with `InputSystemUIInputModule`.
- `Level 1.unity` contains an `EventSystem` with `InputSystemUIInputModule`.
- Earlier scan: `Level 2.unity` did not contain an `EventSystem`.
- `ESC` can still open the pause menu because `FollowPlayer.Update()` reads keyboard input directly.
- UGUI buttons/sliders require an active `EventSystem` to receive pointer hover/click/drag events.

### Root Cause

Level 2 was missing the scene UI input infrastructure. Scene transition destroys the Level 1 `EventSystem`, and Level 2 does not provide a replacement. Starting directly in Level 2 has the same problem.

### Fix Applied

- `GameBootstrapper.PostBootstrap()` now calls `EnsureEventSystemExists()`.
- `EnsureEventSystemExists()` creates a scene-local `EventSystem` only when none exists.
- With the Input System enabled, the created EventSystem uses `InputSystemUIInputModule` and assigns default UI actions.
- Existing scene-authored EventSystems in Main Menu and Level 1 are preserved.

## Level 2 Duplicate EventSystem After Manual Fix

Date: 2026-05-16

### Symptom

- A manual `EventSystem` was added under `GlobalManagers` in `Level 2`.
- In Play Mode, `GameBootstrapper` still created another EventSystem, resulting in duplicates.

### Confirmed Data

- `Level 2.unity` now contains an active `EventSystem`.
- The manual EventSystem uses `UnityEngine.InputSystem.UI.InputSystemUIInputModule`.
- `GameBootstrapper.Bootstrap()` was running `EnsureEventSystemExists()` during `RuntimeInitializeLoadType.BeforeSceneLoad`.
- At `BeforeSceneLoad`, scene-authored objects are not available to `FindObjectsByType`, so the bootstrapper could not see the EventSystem under `GlobalManagers` yet.

### Root Cause

The fallback EventSystem check was executing too early. It was intended as a safety net for scenes without UI input infrastructure, but running before scene load made it blind to scene-authored EventSystems.

### Fix Applied

- Removed fallback EventSystem creation from the `BeforeSceneLoad` bootstrap path.
- Registered a `SceneManager.sceneLoaded` hook.
- `EnsureEventSystemExists()` now runs after scene load, when scene-authored EventSystems are visible.
- The fallback object is named `Runtime EventSystem` to make runtime-created safety infrastructure obvious.
- The lookup now includes inactive objects, so the bootstrapper will not create a duplicate just because an authored EventSystem is temporarily inactive.

## HP HUD Icons Not Hiding On Trap Damage

Date: 2026-05-19

### Symptom

- Trap damage applies correctly to the player Health component.
- HP images under `HUD_HP` remain visible after trap hits, even when traps deal 5 damage.

### Confirmed Data

- `SpikeTrapFSM` calls `Health.TakeDamage(trapDamageAmount, transform)` and damage logs show HP changing.
- `Health.TakeDamage()` fires `OnHealthChanged` for non-lethal damage and death.
- `HUDManager` subscribes to the player Health event and also polls `currentHealth/maxHealth`.
- `HUD_Canvas.prefab` had `HUDManager.hudHpPanel` assigned to fileID `8906961554848753430`, which is `HUD_UX_Panel`, not `HUD_HP`.
- The actual `HUD_HP` RectTransform is fileID `4617025985266312981`.

### Root Cause

`HUDManager.CacheHpImages()` trusted the non-null serialized `hudHpPanel` reference. Because the prefab pointed at `HUD_UX_Panel`, the manager cached and toggled Images under the wrong panel while the actual HP icons remained untouched.

### Fix Applied

- Rewired `HUD_Canvas.prefab` so `HUDManager.hudHpPanel` points to the actual `HUD_HP` RectTransform.
- Hardened `HUDManager.CacheHpImages()` to reject any assigned panel whose name is not `HUD_HP` and rebind by name.

## Fall_Dive Blocked By Player Attack States

Date: 2026-05-21

### Symptom

- Entering a fall/water hazard during `PlayerChargeAttack` left the player in charge animation until the charge finished.
- After that failure, the player could move on the hazard area and regular/charged attacks no longer became available.
- Entering a fall hazard during regular attack started the failure flow, but the attack animation continued instead of immediately showing `Fall_Dive`.

### Confirmed Data

- `FallZone.OnTriggerEnter2D()` called `Health.DepleteHealth()` before `PlayerController.StartFallSequence()`.
- `PlayerController.StartFallSequence()` returned early whenever `IsLocked` was true.
- `IsLocked` included `abilityMovementLocked`, which is set during `PlayerChargeAttack`.
- The saved `PlayerAnimatorController.controller` only had a saved transition from `Run Tree` to `Fall_Dive`; no saved Any State transition to `Fall_Dive` existed on disk.
- Regular and charged attacks both relied on their animation states/events to finish naturally.

### Root Cause

`PlayerChargeAttack` used the same lock gate that `StartFallSequence()` treated as a reason to ignore fall. The fall zone consumed itself and depleted HP, but the gameplay fall state never started. Regular attacks had a related presentation failure because the Animator did not have a guaranteed saved interruption path from attack states into `Fall_Dive`.

### Fix Applied

- `PlayerController.StartFallSequence()` now allows fall to override `abilityMovementLocked`.
- `PlayerController` cancels regular attack and charge attack before fall/death presentation.
- `PlayerCombat.CancelAttack()` disables the sword hitbox and clears attack triggers.
- `PlayerChargeAttack.CancelChargeAttack()` disables charge hitbox/trail and releases movement ownership.
- `PlayerAnimator.InterruptToFallDive()` and `InterruptToDeath()` clear action triggers and force the saved failure state on layer 0.
