# Audio Volume Settings

## 1. Feature Summary

| Field | Value |
|:------|:------|
| Name | Audio Volume Settings |
| Objective | Let players adjust music and SFX volume from both Main Menu and Pause Menu while keeping `AudioManager` as the single source of truth. |
| Scope | Adds persistent normalized volume values and reusable menu slider binding. Does not redesign audio playback, transitions, HUD behavior, or menu navigation. |

## 2. Business Rules

### Main Flow

1. Player opens a settings panel in Main Menu or Pause Menu.
2. The settings panel displays Music and SFX slider values from the current persistent audio state.
3. Moving the Music slider immediately changes background music volume.
4. Moving the SFX slider immediately changes future SFX playback volume.
5. Both values are saved and survive scene transitions and future play sessions.
6. Opening another settings panel later shows the same saved values.

### States

| State | Meaning |
|:------|:--------|
| Default Volume | No saved setting exists, so volume starts at 1. |
| Saved Volume | PlayerPrefs contains a stored 0 to 1 value. |
| Missing AudioManager | UI is present, but persistent audio is unavailable. UI must fail safely. |
| Missing Slider Reference | One or both slider fields are not assigned. The assigned slider should still function. |

### Failure Conditions

- Missing `AudioManager` must not throw runtime exceptions.
- Missing slider assignments must not throw runtime exceptions.
- Reopening pause/settings panels must not stack duplicate slider listeners.
- Menus must not manipulate `AudioSource` children directly.

## 3. Technical Requirements

### Required Components

| Component | Responsibility |
|:----------|:---------------|
| Persistent Audio Service | Owns volume state, clamps values, applies volume to audio sources, saves and loads preferences. |
| Audio Settings UI Binder | Owns slider synchronization and listener registration for menu presentation. |
| Music Slider | Presents and edits the normalized music volume. |
| SFX Slider | Presents and edits the normalized SFX volume. |

### Configuration

| Value | Range / Default |
|:------|:----------------|
| Music Volume | 0 to 1, default 1 |
| SFX Volume | 0 to 1, default 1 |

### Dependencies

- Persistent audio service.
- UGUI Slider controls in Main Menu settings panel.
- UGUI Slider controls in Pause Menu settings panel.
- PlayerPrefs keys `MusicVolume` and `SFXVolume`.

## 4. Edge Cases and Exceptions

- If the audio service is created before the settings UI, sliders must initialize from the service.
- If the settings UI is enabled repeatedly, listeners must be removed before or during disable.
- If values are changed in Main Menu, Pause Menu must reflect them when opened later.
- If a duplicate audio manager appears, the surviving persistent instance remains the source of truth.
- If Unity MCP scene editing is unavailable or unsafe, UI hierarchy setup is performed manually in the Unity Editor.

## 5. Out of Scope

- Audio mixers, mixer snapshots, mute toggles, master volume, or accessibility presets.
- Replacing UGUI with UI Toolkit.
- Reworking Pause Menu input.
- Rewriting `AudioManager` singleton ownership.
- Editing scene or prefab YAML by hand.

## Task Breakdown

### Issue 1 - Persistent Audio Volume API

- Type: Script
- Description: Add normalized Music/SFX volume state and PlayerPrefs persistence to the persistent audio service.
- Blocked By: None
- Affected Components: `AudioManager`
- Acceptance Criteria: Music volume affects current music source immediately, SFX volume affects future one-shots, and values reload after restarting play mode.

### Issue 2 - Reusable Audio Settings UI Binder

- Type: Script
- Description: Add a menu presentation component that binds Music/SFX sliders to the persistent audio service public API.
- Blocked By: Issue 1
- Affected Components: `AudioSettingsUI`
- Acceptance Criteria: Enabling a settings panel syncs sliders from audio state, slider movement applies instantly, and repeated enable/disable cycles do not duplicate listeners.

### Issue 3 - Main Menu And Pause Menu Inspector Wiring

- Type: Editor
- Description: Add or assign two UGUI sliders in each settings panel and attach the binder to each panel.
- Blocked By: Issue 2
- Affected Components: Main Menu settings panel, Pause Menu settings panel
- Acceptance Criteria: Both menus control the same persistent values and open with synchronized slider positions.

## Approval

This GDD reflects the approved implementation prompt for the current task.
