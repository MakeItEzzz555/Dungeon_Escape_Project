# Feature Index

Last audited: 2026-05-16

This folder documents current game features from the player/runtime behavior point of view. For ownership and code architecture, use the linked system docs.

## Current Features

| Feature | Feature Doc | Related Systems |
|:--------|:------------|:----------------|
| Persistent Runtime Bootstrap | [PersistentRuntimeBootstrap.md](./PersistentRuntimeBootstrap.md) | [Bootstrap And Persistence](../Systems/BootstrapAndPersistence.md) |
| Main Menu And Pause Menu | [MainMenuAndPauseMenu.md](./MainMenuAndPauseMenu.md) | [UI And HUD](../Systems/UIAndHUD.md), [Input](../Systems/Input.md) |
| Audio Volume Settings | [AudioVolumeSettings.md](./AudioVolumeSettings.md) | [Audio](../Systems/Audio.md) |
| Player Movement And State | [PlayerMovementAndState.md](./PlayerMovementAndState.md) | [Player](../Systems/Player.md), [Input](../Systems/Input.md) |
| Player Combat | [PlayerCombat.md](./PlayerCombat.md) | [Player](../Systems/Player.md), [Combat](../Systems/Combat.md) |
| Enemy Encounters | [EnemyEncounters.md](./EnemyEncounters.md) | [Enemy AI](../Systems/EnemyAI.md), [Combat](../Systems/Combat.md) |
| Key Quest And Level Exit | [KeyQuestAndLevelExit.md](./KeyQuestAndLevelExit.md) | [Progression](../Systems/Progression.md), [Scene Transitions](../Systems/SceneTransitions.md) |
| Coin And Key Collection | [CoinAndKeyCollection.md](./CoinAndKeyCollection.md) | [Collectibles](../Systems/Collectibles.md), [UI And HUD](../Systems/UIAndHUD.md) |
| HUD And Feedback | [HUDAndFeedback.md](./HUDAndFeedback.md) | [UI And HUD](../Systems/UIAndHUD.md) |
| Player HP HUD | [PlayerHPHUD.md](./PlayerHPHUD.md) | [UI And HUD](../Systems/UIAndHUD.md), [Combat](../Systems/Combat.md), [Traps And Hazards](../Systems/TrapsAndHazards.md) |
| Lever And Gate Interaction | [LeverAndGateInteraction.md](./LeverAndGateInteraction.md) | [Interactables](../Systems/Interactables.md) |
| Chest Interaction | [ChestInteraction.md](./ChestInteraction.md) | [Interactables](../Systems/Interactables.md), [Collectibles](../Systems/Collectibles.md) |
| Traps And Fall Hazards | [TrapsAndFallHazards.md](./TrapsAndFallHazards.md) | [Traps And Hazards](../Systems/TrapsAndHazards.md), [Player](../Systems/Player.md) |
| Respawn Transition | [RespawnTransition.md](./RespawnTransition.md) | [Scene Transitions](../Systems/SceneTransitions.md), [Camera](../Systems/Camera.md), [Player](../Systems/Player.md) |
| Scene Transition Experience | [SceneTransitionExperience.md](./SceneTransitionExperience.md) | [Scene Transitions](../Systems/SceneTransitions.md), [Camera](../Systems/Camera.md) |
| Camera Follow And Zoom | [CameraFollowAndZoom.md](./CameraFollowAndZoom.md) | [Camera](../Systems/Camera.md) |
| Environment Presentation | [EnvironmentPresentation.md](./EnvironmentPresentation.md) | [Environment And Presentation](../Systems/EnvironmentAndPresentation.md) |

## Documentation Rule

When a new feature is approved, create or update a feature document here and cross-link it to its owning system document under `Wiki/Systems/`.
