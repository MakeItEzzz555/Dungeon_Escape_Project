# ADR: FinalBoss Death Transition Ownership

Date: 2026-05-21

## Context

FinalBoss death results previously waited inside `FinalBossBehavior` before calling `SceneTransitionManager`. That preserved the death animation read time, but it meant the shared transition system was idle during the wait. The visible result was a brief empty-scene flicker before the black fade and `RunResultsPanel`.

## Decision

`SceneTransitionManager` owns the FinalBoss death completion presentation window. FinalBoss starts the completion transition immediately and passes `bossDeathTransitionDelay` as a presentation window. The fade starts at `presentationDuration - fadeInDuration` so the screen reaches full black at the end of the death read window. `CameraTransitionSystem` may hold the zoom target during that window, then releases it after the screen is fully black.

## Consequences

- FinalBoss death uses the same completion results infrastructure as checkpoint doors.
- `SceneTransitionManager.IsTransitioning` becomes true as soon as FinalBoss death results begin.
- The camera can stay framed on the FinalBoss during the death animation instead of snapping back to the player before fade.
- The held camera target must be released before scene load or restoration paths continue.
- FinalBoss death zoom targets must be world transforms. UI `RectTransform` targets are rejected at runtime and fall back to the FinalBoss transform.
