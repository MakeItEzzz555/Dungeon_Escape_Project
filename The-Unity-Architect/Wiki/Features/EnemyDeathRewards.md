# Enemy Death Rewards

Last updated: 2026-05-21

## 1. Feature Summary

| Field | Value |
|:------|:------|
| Name | Enemy Death Rewards |
| Objective | Let normal enemies and FinalBoss reveal authored reward objects on death, such as a chest that can later be opened to reveal a key. |
| Scope | Covers optional scene-authored reward activation on combatant death. It does not create random drops, loot tables, inventory, or automatic key collection. |

## 2. Business Rules

### Main Flow

1. A designer places a reward object in the scene at the exact desired transform.
2. The reward object is assigned to the enemy or FinalBoss that should reveal it.
3. At runtime start, the assigned reward object is forced inactive.
4. When the owning enemy or FinalBoss dies, the reward object is activated exactly once.
5. The activated reward owns its next behavior. For example, a chest can use chest interaction to reveal a key.
6. FinalBoss may still optionally start completion-mode run results after death, but that option should stay disabled for chest-then-key-then-checkpoint progression.

### States

| State | Purpose |
|:------|:--------|
| Hidden | Assigned reward object is inactive during gameplay until its owner dies. |
| Revealed | Assigned reward object has been activated after owner death. |
| Consumed | Optional downstream state owned by the reward itself, such as an opened chest or collected key. |

### Failure Conditions

| Condition | Result |
|:----------|:-------|
| No reward object assigned | Death proceeds normally with no reward activation. |
| Owner death is reported more than once | Reward activation runs once only. |
| Reward object is already inactive in the scene | Runtime start keeps it inactive. |
| Reward object is accidentally active in the scene | Runtime start hides it before gameplay begins. |

## 3. Technical Requirements

### Required Components

| Component | Responsibility |
|:----------|:---------------|
| Enemy or FinalBoss owner | Receives death state from shared combat health and reveals the reward once. |
| DeathRewardObject | Scene-authored reward GameObject with its own transform and follow-up behavior. |
| Health | Provides the death event that triggers reward reveal. |
| Optional chest interaction | Lets a revealed chest open later and expose an assigned item. |

### Configuration

| Setting | Purpose |
|:--------|:--------|
| Death reward object | Optional scene GameObject hidden on runtime start and activated on owner death. |
| FinalBoss run results toggle | Optional boss-only completion flow after death; leave disabled when the reward is needed for checkpoint-door progression. |
| FinalBoss results target scene | Optional boss-only destination after completion results. |
| FinalBoss results zoom target | Optional boss-only zoom target for completion results. |

### Dependencies

| Dependency | Use |
|:-----------|:----|
| Combat health | Death signal source. |
| Chest interaction | Optional second-step reward if the activated object is a chest. |
| Progression/key quest | Optional downstream key collection and level exit completion. |

## 4. Edge Cases and Exceptions

| Edge Case | Decision |
|:----------|:---------|
| A reward should appear at a precise location | Use an authored scene object instead of runtime prefab spawn. |
| The reward is a chest containing a key | Assign the chest as the death reward object, then assign the key to the chest interaction content. |
| Multiple enemies should reveal different rewards | Each enemy owns its own assigned death reward object. |
| FinalBoss already had prefab loot spawning | Replace prefab spawning with scene reward activation for consistency with chest-style authoring. |
| FinalBoss reward chest contains the level key | Do not show run results on FinalBoss death; let the player open the chest, collect the key, and finish through `CheckpointDoor`. |

## 5. Out Of Scope

- Randomized loot.
- Multiple rewards per enemy.
- Drop physics or scatter.
- Object pooling for rewards.
- New checkpoint door behavior.
- Automatically awarding keys when an enemy dies.

## Implementation Status

Approved behavior: normal enemies and FinalBoss can reveal a serialized scene reward object on death. The object is hidden at runtime start and activated once when the owner dies.
