# Player HurtBox Setup Instructions

## Problem Summary
Enemy AttackRange never detects the player because the player's physical BoxCollider2D only covers the feet/body base. The enemy approaches from above/offset positions where the physical collider doesn't overlap.

## Solution
Add a dedicated HurtBox trigger child to Player 1 that covers the full player body area.

---

## Step-by-Step Instructions

### 1. Open Unity Editor
- Open the Dungeon_Escape_Project in Unity
- Navigate to `Assets/Prefabs/Player/Player 1.prefab`
- Double-click to open the prefab for editing

### 2. Create HurtBox Child GameObject
In the Prefab editing mode:

1. **Right-click on "Player 1"** in the Hierarchy
2. Select **Create Empty**
3. Rename the new child to **"HurtBox"**

### 3. Configure HurtBox Transform
With HurtBox selected:

1. **Reset Transform** (Right-click Transform component → Reset)
   - Position: (0, 0, 0)
   - Rotation: (0, 0, 0)
   - Scale: (1, 1, 1)

2. **Adjust Position** (if needed to center on player torso)
   - Position Y: approximately 0.2 to 0.3 (depends on sprite)
   - This should center the hurtbox on the player's body/torso area

### 4. Add BoxCollider2D to HurtBox
With HurtBox selected:

1. Click **Add Component**
2. Search for **BoxCollider2D**
3. Add it to the HurtBox GameObject

### 5. Configure BoxCollider2D
With the BoxCollider2D component visible in Inspector:

1. **Enable "Is Trigger"** ✓ (MUST be checked)
2. **Set Size:**
   - Size X: `0.8` (covers player width)
   - Size Y: `1.0` (covers full player height from feet to head)
3. **Set Offset:**
   - Offset X: `0`
   - Offset Y: `0.3` (adjust to center on player body)

**IMPORTANT:**
- The collider should visually cover the player's full body when you see the green outline in Scene view
- It should be LARGER than the existing feet collider on Player 1 root
- It MUST be a trigger (Is Trigger = true)

### 6. Verify Setup

**HurtBox GameObject should have:**
- ✓ Transform component only
- ✓ BoxCollider2D component
- ✓ BoxCollider2D → Is Trigger = TRUE
- ✗ NO Rigidbody2D
- ✗ NO Hitbox.cs script
- ✗ NO Health.cs script

**Player 1 root should still have:**
- ✓ Transform
- ✓ Animator
- ✓ SpriteRenderer
- ✓ Rigidbody2D (gravityScale = 0)
- ✓ BoxCollider2D (Is Trigger = FALSE) - the feet collider
- ✓ PlayerController script
- ✓ YSort script
- ✓ Health.cs script (should be here)
- ✓ PlayerCombat script (if present)

### 7. Save the Prefab
1. Click **Save** in the Prefab editing toolbar
2. Exit Prefab editing mode

### 8. Update All Scenes
The prefab changes should automatically propagate to all scenes:
- Level 1.unity
- Level 2.unity

**Verify in each scene:**
1. Open the scene
2. Select Player 1 in Hierarchy
3. Expand Player 1 to see HurtBox child
4. Verify HurtBox has the trigger collider configured

---

## What This Fixes

### Before (Broken):
```
Enemy AttackRange (trigger)
    ↓
    Tries to detect Player physical collider
    ↓
    Physical collider only at player feet
    ↓
    Enemy approaches from above/side
    ↓
    ❌ No overlap detected
    ↓
    ❌ Enemy never enters Attacking state
```

### After (Working):
```
Enemy AttackRange (trigger)
    ↓
    Detects Player HurtBox (trigger)
    ↓
    HurtBox covers full player body
    ↓
    Enemy approaches from any direction
    ↓
    ✓ Reliable overlap detected
    ↓
    ✓ Enemy enters Attacking state
    ↓
    ✓ Enemy attack animation triggers
    ↓
    ✓ Enemy AttackHitBox damages player
```

---

## Code Changes Already Applied

### EnemyRangeTrigger.cs
Enhanced player detection to recognize:
- Player root collider (tag="Player")
- Player child triggers like HurtBox (checks root tag)
- Any child with PlayerController in parent hierarchy

```csharp
private bool IsPlayer(Collider2D other)
{
    return other.CompareTag("Player")
        || other.transform.root.CompareTag("Player")
        || other.GetComponentInParent<PlayerController>() != null;
}
```

### Debug Logs
All necessary debug logs already present in:
- EnemyRangeTrigger.cs (ENTER/STAY/EXIT events)
- EnemyAI.cs (state changes, aggro/attack range flags)

---

## Testing After Setup

1. **Open Level 2 scene**
2. **Press Play**
3. **Watch Console for debug logs:**
   ```
   [EnemyRangeTrigger] ENTER Aggro: HurtBox, tag=Untagged
   [EnemyRangeTrigger] STAY Aggro: HurtBox, tag=Untagged
   [EnemyAI] SetPlayerInAggroRange(True)
   [EnemyAI] State changed: IdleAtHome -> Chasing
   ```

4. **Move player closer to enemy**
5. **Watch for AttackRange detection:**
   ```
   [EnemyRangeTrigger] ENTER Attack: HurtBox, tag=Untagged
   [EnemyAI] SetPlayerInAttackRange(True)
   [EnemyAI] State changed: Chasing -> Attacking
   [EnemyAI] Attack() called
   ```

6. **Verify enemy attack animation plays**
7. **Verify player takes damage when enemy hitbox activates**

---

## Important Notes

### DO NOT:
- ❌ Add Hitbox.cs to HurtBox (it's only for detection, not dealing damage)
- ❌ Add Rigidbody2D to HurtBox (would interfere with physics)
- ❌ Tag HurtBox as "Player" (not required, detection works via parent)
- ❌ Make HurtBox collider non-trigger (must be trigger)
- ❌ Remove or modify existing Player 1 root collider
- ❌ Remove or modify SwordHitBox (player attack system)

### Expected Behavior:
- ✓ Enemy chases player from AggroRange
- ✓ Enemy stops and attacks inside AttackRange
- ✓ Enemy attack animation triggers
- ✓ Player takes damage from enemy attacks
- ✓ Player combat still works (SwordHitBox unchanged)
- ✓ Player death system still works (Health.cs on root)
- ✓ All blend trees and animations still work

---

## Troubleshooting

### If enemy still doesn't attack:

1. **Check HurtBox collider in Scene view**
   - Is it visible as green outline?
   - Does it cover the full player body?
   - Is "Is Trigger" enabled?

2. **Check Console logs**
   - Do you see ENTER/STAY messages for AttackRange?
   - Does playerInAttackRange become true?
   - Does Attack() get called?

3. **Check Enemy AttackRange size**
   - Select Enemy in Hierarchy
   - Find AttackRange child
   - Verify CircleCollider2D radius (should be ~1.5-2.0)
   - Increase if needed for testing

4. **Check Layer Collision Matrix**
   - Edit → Project Settings → Physics 2D
   - Verify Default layer can detect Default layer

### If player stops taking damage:

- Health.cs must remain on Player 1 root
- Hitbox.cs on enemy AttackHitBox should find Health via GetComponentInParent
- Do NOT add Health.cs to HurtBox

---

## Visual Reference

```
Player 1 (root)                          [Tag: Player]
├── BoxCollider2D (feet)                 [Trigger: FALSE]
├── Rigidbody2D
├── Health.cs                            ← Stays here!
├── PlayerController.cs
├── PlayerCombat.cs
├── Animator
├── SpriteRenderer
│
├── HurtBox (new child)                  [Tag: Untagged]
│   └── BoxCollider2D (full body)        [Trigger: TRUE]
│
└── SwordHitBox (existing child)
    ├── BoxCollider2D                    [Trigger: TRUE]
    └── Hitbox.cs
```

---

## Summary

This fix adds a dedicated detection volume for the player without changing any existing systems. The HurtBox exists purely for enemy range detection and damage reception, while all existing player mechanics (movement, combat, death) remain unchanged.
