# Animation Assets Inventory

Last audited: 2026-05-17

This document inventories animation-related assets under `Assets/Animations`, excluding the imported deluxe subfolder. The user requested ignoring `AnimationsDeluxe`; the actual folder present in the project is `Assets/Animations/AnimatedTilesDeluxe`, so that folder is excluded here.

## Summary

| Type | Count |
|:-----|------:|
| `.anim` clips | 63 |
| `.asset` animated tile assets | 30 |
| `.controller` animator controllers | 22 |
| Total audited animation assets | 115 |

## Folder Summary

| Folder | Count | Notes |
|:-------|------:|:------|
| `Assets/Animations/AnimatedTiles` | 30 | Animated tile assets for flames, torches, water, waterfalls, and rotating spike trap. |
| `Assets/Animations/Chests` | 3 | Chest controller and chest open/idle clips. |
| `Assets/Animations/Coins` | 3 | Blue coin controller, idle animation, and collection clip. |
| `Assets/Animations/Doors` | 4 | Door controller and open/close/idle clips. |
| `Assets/Animations/Enemy` | 6 | Enemy controller and attack/death/hurt/idle/walk clips. |
| `Assets/Animations/Gates_Doors` | 5 | Metal gate controllers and open/down/up clips. |
| `Assets/Animations/Keys/Key GOLD` | 3 | Gold key controller, idle clip, and collect clip. |
| `Assets/Animations/Light` | 2 | Light rays controller and clip. |
| `Assets/Animations/LightPillar` | 2 | Light pillar controller and clip. |
| `Assets/Animations/Player` | 30 | Player directional idle/run/hurt/death/attack clips, fall clip, standby clip. |
| `Assets/Animations/SwitchLever` | 3 | Lever controller and on/off clips. |
| `Assets/Animations/Torches` | 10 | Torch and flame controllers/clips. |
| `Assets/Animations/Trap` | 4 | Rolling and spike trap controllers/clips. |
| `Assets/Animations/Waterfalls` | 10 | Waterfall/ripple controllers and clips. |

## Runtime Feature Mapping

| Feature/System | Animation Folders |
|:---------------|:------------------|
| Player movement/combat/death | `Assets/Animations/Player` |
| Enemy encounters | `Assets/Animations/Enemy` |
| Coin/key collection | `Assets/Animations/Coins`, `Assets/Animations/Keys/Key GOLD` |
| Doors/gates/levers | `Assets/Animations/Doors`, `Assets/Animations/Gates_Doors`, `Assets/Animations/SwitchLever` |
| Chests | `Assets/Animations/Chests` |
| Traps and hazards | `Assets/Animations/Trap`, `Assets/Animations/AnimatedTiles/rotating_spike_trap.asset` |
| Environment presentation | `Assets/Animations/AnimatedTiles`, `Assets/Animations/Light`, `Assets/Animations/LightPillar`, `Assets/Animations/Torches`, `Assets/Animations/Waterfalls` |

## Full Inventory

### Animated Tiles

- `Assets/Animations/AnimatedTiles/Flames/Blue_Flame.asset`
- `Assets/Animations/AnimatedTiles/rotating_spike_trap.asset`
- `Assets/Animations/AnimatedTiles/Torch_Pillars/Light_Background/b_left_corner.asset`
- `Assets/Animations/AnimatedTiles/Torch_Pillars/Light_Background/b_right_corner.asset`
- `Assets/Animations/AnimatedTiles/Torch_Pillars/Light_Background/bot_left_corner.asset`
- `Assets/Animations/AnimatedTiles/Torch_Pillars/Light_Background/bot_right_corner.asset`
- `Assets/Animations/AnimatedTiles/Torch_Pillars/Light_Background/Bot_torch_Pillar.asset`
- `Assets/Animations/AnimatedTiles/Torch_Pillars/Light_Background/Mid_torch_pillar.asset`
- `Assets/Animations/AnimatedTiles/Torch_Pillars/Light_Background/top_corner_small_left.asset`
- `Assets/Animations/AnimatedTiles/Torch_Pillars/Light_Background/top_corner_small_right.asset`
- `Assets/Animations/AnimatedTiles/Torch_Pillars/Light_Background/top_left_corner.asset`
- `Assets/Animations/AnimatedTiles/Torch_Pillars/Light_Background/top_mid.asset`
- `Assets/Animations/AnimatedTiles/Torch_Pillars/Light_Background/top_right_corner.asset`
- `Assets/Animations/AnimatedTiles/Torch_Pillars/Light_Background/Top_torch_pillar.asset`
- `Assets/Animations/AnimatedTiles/Torch_Pillars/Without_BackLight/bot_pillar.asset`
- `Assets/Animations/AnimatedTiles/Torch_Pillars/Without_BackLight/mid_pillar.asset`
- `Assets/Animations/AnimatedTiles/Torch_Pillars/Without_BackLight/top_flame.asset`
- `Assets/Animations/AnimatedTiles/transparent_top_water.asset`
- `Assets/Animations/AnimatedTiles/water_full.asset`
- `Assets/Animations/AnimatedTiles/water_transparent.asset`
- `Assets/Animations/AnimatedTiles/Waterfalls/small_bot_waterfall.asset`
- `Assets/Animations/AnimatedTiles/Waterfalls/small_top_waterfall.asset`
- `Assets/Animations/AnimatedTiles/Waterfalls/small_waterfall.asset`
- `Assets/Animations/AnimatedTiles/Waterfalls/waterfall__top_1.asset`
- `Assets/Animations/AnimatedTiles/Waterfalls/waterfall_bot_1.asset`
- `Assets/Animations/AnimatedTiles/Waterfalls/waterfall_bot_left_top.asset`
- `Assets/Animations/AnimatedTiles/Waterfalls/waterfall_bot_right_top.asset`
- `Assets/Animations/AnimatedTiles/Waterfalls/waterfall_left_bot.asset`
- `Assets/Animations/AnimatedTiles/Waterfalls/waterfall_mid_1.asset`
- `Assets/Animations/AnimatedTiles/Waterfalls/waterfall_right_bot_1.asset`

### Chests

- `Assets/Animations/Chests/Chest_Controller.controller`
- `Assets/Animations/Chests/Idle_yb_chest.anim`
- `Assets/Animations/Chests/Yellow_blue_chest.anim`

### Coins

- `Assets/Animations/Coins/blue_coin_anim.anim`
- `Assets/Animations/Coins/BlueCoin Sheet_0.controller`
- `Assets/Animations/Coins/coin_collected.anim`

### Doors

- `Assets/Animations/Doors/close_door.anim`
- `Assets/Animations/Doors/door_controller.controller`
- `Assets/Animations/Doors/idle_closed.anim`
- `Assets/Animations/Doors/open_door.anim`

### Enemy

- `Assets/Animations/Enemy/attack_cmb.anim`
- `Assets/Animations/Enemy/death.anim`
- `Assets/Animations/Enemy/Enemy_ctrl.controller`
- `Assets/Animations/Enemy/hurt.anim`
- `Assets/Animations/Enemy/idle.anim`
- `Assets/Animations/Enemy/walk.anim`

### Gates And Doors

- `Assets/Animations/Gates_Doors/metal_gate_Clip.anim`
- `Assets/Animations/Gates_Doors/metal_gate_controller_new.controller`
- `Assets/Animations/Gates_Doors/metal_gate_down.anim`
- `Assets/Animations/Gates_Doors/metal_gate_up.anim`
- `Assets/Animations/Gates_Doors/metal_gate_UpDown.controller`

### Keys

- `Assets/Animations/Keys/Key GOLD/collect_key.anim`
- `Assets/Animations/Keys/Key GOLD/Key 4 - GOLD.anim`
- `Assets/Animations/Keys/Key GOLD/Key 4 - GOLD.controller`

### Light

- `Assets/Animations/Light/LightRays.controller`
- `Assets/Animations/Light/LightRaysClip.anim`

### Light Pillar

- `Assets/Animations/LightPillar/light_pillar.anim`
- `Assets/Animations/LightPillar/light_pillar.controller`

### Player

- `Assets/Animations/Player/attack1/attack1_down.anim`
- `Assets/Animations/Player/attack1/attack1_left.anim`
- `Assets/Animations/Player/attack1/attack1_right.anim`
- `Assets/Animations/Player/attack1/attack1_up.anim`
- `Assets/Animations/Player/attack2/attack2_down.anim`
- `Assets/Animations/Player/attack2/attack2_left.anim`
- `Assets/Animations/Player/attack2/attack2_right.anim`
- `Assets/Animations/Player/attack2/attack2_up.anim`
- `Assets/Animations/Player/AttackCombo/attackCmb_down.anim`
- `Assets/Animations/Player/AttackCombo/attackCmb_left.anim`
- `Assets/Animations/Player/AttackCombo/attackCmb_right.anim`
- `Assets/Animations/Player/AttackCombo/attackCmb_up.anim`
- `Assets/Animations/Player/Death/Death_Down.anim`
- `Assets/Animations/Player/Death/Death_left.anim`
- `Assets/Animations/Player/Death/Death_right.anim`
- `Assets/Animations/Player/Death/Death_up.anim`
- `Assets/Animations/Player/Fall_Dive.anim`
- `Assets/Animations/Player/hurt/hurt_down.anim`
- `Assets/Animations/Player/hurt/hurt_left.anim`
- `Assets/Animations/Player/hurt/hurt_right.anim`
- `Assets/Animations/Player/hurt/hurt_up.anim`
- `Assets/Animations/Player/Idle/Idle_Down.anim`
- `Assets/Animations/Player/Idle/Idle_Left.anim`
- `Assets/Animations/Player/Idle/Idle_Right.anim`
- `Assets/Animations/Player/Idle/Idle_Up.anim`
- `Assets/Animations/Player/Run/Run_Down.anim`
- `Assets/Animations/Player/Run/Run_Left.anim`
- `Assets/Animations/Player/Run/Run_Right.anim`
- `Assets/Animations/Player/Run/Run_Up.anim`
- `Assets/Animations/Player/StandBy_idle_down.anim`

### Switch Lever

- `Assets/Animations/SwitchLever/Lever_anim.anim`
- `Assets/Animations/SwitchLever/Lever_controller.controller`
- `Assets/Animations/SwitchLever/Lever_off_idle.anim`

### Torches

- `Assets/Animations/Torches/light_plain.anim`
- `Assets/Animations/Torches/light_plain_ctrl.controller`
- `Assets/Animations/Torches/oven_torch.controller`
- `Assets/Animations/Torches/oven_torch_fire.anim`
- `Assets/Animations/Torches/Torch Yellow_0.controller`
- `Assets/Animations/Torches/Torch.anim`
- `Assets/Animations/Torches/torch_blue.anim`
- `Assets/Animations/Torches/torch_blue.controller`
- `Assets/Animations/Torches/torch_pillar.controller`
- `Assets/Animations/Torches/torch_pillar_blue.anim`

### Trap

- `Assets/Animations/Trap/Rolling_clip.anim`
- `Assets/Animations/Trap/RollingTrap.controller`
- `Assets/Animations/Trap/SpikeTrap_Anim.anim`
- `Assets/Animations/Trap/TP001_0.controller`

### Waterfalls

- `Assets/Animations/Waterfalls/BigContr.controller`
- `Assets/Animations/Waterfalls/BigWaterFall.anim`
- `Assets/Animations/Waterfalls/large_waterfall.controller`
- `Assets/Animations/Waterfalls/ripple_clip.anim`
- `Assets/Animations/Waterfalls/ripple_ctrl.controller`
- `Assets/Animations/Waterfalls/SmallCtrl.controller`
- `Assets/Animations/Waterfalls/SmallWaterFall.anim`
- `Assets/Animations/Waterfalls/Water_0.controller`
- `Assets/Animations/Waterfalls/waterfall_large.anim`
- `Assets/Animations/Waterfalls/Waterfall_small.anim`

## Excluded Folder

- `Assets/Animations/AnimatedTilesDeluxe`

This folder is intentionally excluded from the inventory per the user's request to ignore the deluxe animations subfolder.
