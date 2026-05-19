# Respawn Transition

## 1. Feature Summary

| Field | Value |
|:------|:------|
| Name | Respawn Transition |
| Objective | Make every player death or fall recovery use the same zoom and fade language as level scene transitions, so respawn feels intentional instead of abruptly reloading the scene. |
| Scope | Applies to player death from enemies, death from traps, and falling into water or other fall zones. It does not add checkpoints, lives, health redesign, save data, or hazard rule changes. |

## 2. Business Rules

### Main Flow

1. Player is killed by an enemy, killed by a trap, or enters a fall zone.
2. Player control is locked immediately.
3. The correct failure animation remains visible first: death uses the death animation, and falling uses the fall animation state.
4. After the configured animation read time, the camera zooms in on the player.
5. The screen fades to black.
6. The active scene reloads.
7. The newly loaded player is reset and remains control-locked while presentation finishes.
8. The screen fades from black.
9. The camera zooms out from transition zoom to normal gameplay zoom.
10. Player control is restored.

### States

| State | Meaning |
|:------|:--------|
| Idle | No respawn transition is active. |
| FailureAnimation | Player is dead or falling, input is locked, and the failure animation is being shown. |
| ZoomingIn | Camera is zooming toward the player before the reload fade. |
| FadingToBlack | UI fade overlay is moving to black. |
| ReloadingScene | The active scene is being reloaded. |
| FadingFromBlack | UI fade overlay is returning to transparent after reload. |
| ZoomingOut | Camera is returning to normal gameplay zoom. |
| RestoringControl | The reloaded player is reset and control is restored. |

### Failure Conditions

- If a respawn transition is already active, later enemy, trap, or fall triggers must be ignored.
- If camera transition infrastructure is unavailable, the scene reload must still complete.
- If UI fade infrastructure is unavailable, the scene reload must still complete.
- If the reloaded scene does not contain a player, the transition must fail visibly in logs rather than silently leaving controls in an unknown state.

## 3. Technical Requirements

### Required Components

| Component | Responsibility |
|:----------|:---------------|
| Respawn Transition Orchestrator | Owns the full death/fall recovery sequence and prevents duplicate respawn transitions. |
| Player State Owner | Starts death or fall animation, locks movement, and delegates recovery presentation to the orchestrator. |
| Camera Zoom Owner | Reuses the existing transition zoom behavior and targets the player during respawn transitions. |
| UI Fade Owner | Reuses the existing fade-to-black and fade-from-black overlay behavior. |

### Configuration

| Value | Purpose |
|:------|:--------|
| Death animation read time | Delay before zoom/fade begins after a standard death. |
| Fall animation read time | Delay before zoom/fade begins after a fall or water hazard. |
| Fade in duration | Time to fade to black before scene reload. |
| Fade out duration | Time to fade from black after scene reload. |
| Zoom in duration | Time to zoom toward the player before reload. |
| Zoom out duration | Time to return to gameplay zoom after reload. |

### Dependencies

- Player movement and animation state.
- Enemy damage/death path.
- Trap death path.
- Fall zone path.
- Camera zoom transition system.
- UI fade transition system.
- Active scene reload behavior.

## 4. Edge Cases and Exceptions

- Enemy, trap, and fall deaths must converge on the same respawn transition behavior.
- Fall deaths must preserve the fall animation state before zoom and fade begin.
- The zoom target is always the player, not the enemy, trap, or fall-zone object.
- Respawn transitions are non-interruptible once started.
- Active-scene reload remains the respawn method for this iteration.
- Existing level-completion scene transitions remain separate from respawn transitions, even though they share camera and fade presentation components.

## 5. Out of Scope

- Checkpoint respawning or teleporting without scene reload.
- Per-hazard transition variants.
- Replacing UGUI fade overlay with UI Toolkit.
- Reworking health, damage routing, or trap state machines.
- Adding lives, game over screens, save files, or persistent death counters.

## Task Breakdown

### Issue 1 - Respawn Transition Orchestration

- Type: Script
- Description: Add a respawn transition path to the existing scene transition owner that waits for failure animation read time, zooms in, fades to black, reloads the active scene, fades out, zooms out, and restores player control.
- Blocked By: None
- Affected Components: `SceneTransitionManager`
- Acceptance Criteria: Calling the respawn transition while a player exists reloads the active scene through the same zoom/fade presentation used by level transitions.

### Issue 2 - Player Death And Fall Delegation

- Type: Script
- Description: Route player death and fall recovery through the respawn transition instead of directly reloading the active scene.
- Blocked By: Issue 1
- Affected Components: `PlayerController`
- Acceptance Criteria: Enemy death, trap death, and fall-zone death all preserve their failure animation, then use zoom/fade respawn.

### Issue 3 - Documentation And Validation

- Type: Script
- Description: Update the wiki/system notes and run compile/static checks.
- Blocked By: Issue 2
- Affected Components: Architect Wiki, Unity C# project
- Acceptance Criteria: The feature and system docs match the implemented behavior, and the project compiles without new C# errors.

## Implementation Notes

- Implemented in `SceneTransitionManager.BeginRespawnTransition(Transform, float)`.
- `SceneTransitionManager` now enters `FailureAnimation` immediately so `IsTransitioning` is true during the animation read window.
- `PlayerController` delegates death and fall recovery to the respawn transition and keeps a direct reload fallback if the transition manager is unavailable.

## Approval

Approved by the user on 2026-05-19 and implemented as the active respawn recovery flow.
