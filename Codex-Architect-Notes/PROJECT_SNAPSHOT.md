# Dungeon Escape Project Snapshot

Created: 2026-05-16

## Purpose

This folder is reserved for Codex working notes. It is not gameplay content and should not be treated as source of truth over `The-Unity-Architect/Wiki/`, `CONTEXT.md`, or approved GDDs.

## Architect Operating Rule

For any new mechanic, system, or feature request:

1. Read `The-Unity-Architect/skills/unity-feature-pipeline/SKILL.md`.
2. Start Phase 1 using `The-Unity-Architect/skills/unity-feature-pipeline/01-quest-giver.md`.
3. Do not write implementation code until the glossary, GDD, and issue breakdown are accepted.
4. During implementation, switch to `The-Unity-Architect/skills/unity-architecture-and-best-practices/SKILL.md`.

## Current Project Shape

- Engine stack: Unity 6-style project using URP `17.3.0`, 2D packages, Tilemap, Aseprite, TextMesh Pro, UGUI, and Input System `1.18.0`.
- Main scenes found: `Assets/Scenes/Main Menu.unity`, `Assets/Scenes/Level 1.unity`, `Assets/Scenes/Level 2.unity`.
- Core gameplay scripts live in `Assets/Scripts/`, not the Architect default `Assets/_Project/Scripts/`.
- Persistent runtime systems are bootstrapped from `Assets/Resources/PersistentSystems.prefab` through `GameBootstrapper`.
- Current Wiki is mostly empty: `The-Unity-Architect/Wiki/Index.md` exists, but no `Features`, `ADR`, `Systems`, or `Lore` docs are populated yet.
- No root `CONTEXT.md` was found at the time of this snapshot.

## Gameplay Systems Observed

- Player:
  - `PlayerController` handles movement, idle/standby timing, footstep audio calls, fall/death lockout, scene reload, and dynamically creates a `HurtBox` child if missing.
  - `PlayerCombat` triggers attacks from `F`, forwards animation events to the sword `Hitbox`, and plays attack SFX.
  - `PlayerAnimator` exists and is referenced by controller/combat, but was not fully audited in this snapshot.
- Combat:
  - `Health` tracks `maxHealth`, `currentHealth`, hurt animation direction, death animation, and forwards player death into `PlayerController.StartDeathSequence`.
  - `Hitbox` enables/disables a trigger collider, tracks already-hit `Health` targets per swing, and prevents owner self-damage.
- Enemy:
  - `EnemyAI` uses a simple state enum: `IdleAtHome`, `Chasing`, `Attacking`, `ReturningHome`, `Dead`.
  - `EnemyRangeTrigger` drives aggro/attack range booleans and recognizes player root colliders plus child triggers like `HurtBox`.
  - `EnemyCombat` and animation event forwarding exist but were not deeply audited in this snapshot.
- Progression:
  - `GlobalQuestManager` tracks required/collected keys and quest completion.
  - `CheckpointDoor` gates scene transition until keys are collected, locks player physics, then calls `SceneTransitionManager`.
- UI/Transition:
  - `HUDManager` uses UGUI/TextMesh Pro, reveals coin/key HUD after discovery, and shows short UX messages.
  - `SceneTransitionManager` coordinates player lockout, camera zoom, UI fade, async scene load, player reset, fade out, and zoom out.

## Diagnostics Run

- `node The-Unity-Architect/execution/unity-doctor.js`
  - Failed because the tool expects `The-Unity-Architect/Assets/_Project/Scripts`; this project uses `Assets/Scripts`.
- `node The-Unity-Architect/execution/unity-project-graph.js`
  - Completed but reported `0 assets indexed` and `0 files analyzed`; output files were created under `The-Unity-Architect/`.
- `python The-Unity-Architect/execution/parse_editor_log.py`
  - Completed after forcing UTF-8 console output.
  - Important Unity log issue found: `StandaloneInputModule` is trying to read legacy `UnityEngine.Input` while project active input handling is set to the Input System package.
  - Many repeated Unity AI Assistant API errors are unrelated to gameplay.

## Immediate Architectural Notes

- The project currently mixes old `UnityEngine.Input` calls with the Input System package setting. This is already producing editor log errors through UGUI `StandaloneInputModule`.
- Several scripts are doing multiple jobs at once. `PlayerController` currently owns movement, idle, footstep audio calls, death/fall sequences, and runtime HurtBox creation. Future work should avoid adding more responsibility there without a design pass.
- Simulation and presentation are partially coupled. Examples: `Health` directly drives animator parameters, `PlayerController` calls `AudioManager`, and `InteractiveGate` writes HUD messages.
- There are many debug logs in high-frequency paths such as trigger stay and idle reset. Useful for diagnosis, but should be cleaned or gated before production.
- `ADD_PLAYER_HURTBOX_INSTRUCTIONS.md` says the HurtBox should be prefab-authored, but `PlayerController` now also creates it dynamically. That design should be reconciled during the next combat/hit detection pass.

## Next Prompt Protocol

When the user gives the first missing feature or next addition:

1. Determine whether it is a new mechanic/system/feature.
2. If yes, start Architect Phase 1 and ask exactly one hard design question at a time.
3. Update/create `CONTEXT.md` as canonical language emerges.
4. Create/approve GDD and issue breakdown before touching scripts.
