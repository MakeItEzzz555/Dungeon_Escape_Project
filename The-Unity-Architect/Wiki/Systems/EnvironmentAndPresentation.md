# Environment And Presentation System

Last audited: 2026-05-16

## Ownership

This layer covers non-gameplay visual presentation: sprite sorting, light rays, animated environment props, tilemaps, waterfalls, torches, pillars, barrels, rocks, statues, and decorative effects.

## Primary Scripts And Assets

| Asset | Role |
|:------|:-----|
| `Assets/Scripts/YSort.cs` | Sets sprite sorting order based on bottom Y position. |
| `Assets/Scripts/LightRaysPulse.cs` | Defines `LightRaySequencedFX`, a sequenced two-sprite light ray effect. |
| `Assets/Prefabs/Env Objects/LightRays.prefab` | Light ray effect prefab. |
| `Assets/Prefabs/Env Objects/...` | Environment props and animated decorative prefabs. |
| `Assets/Animations/...` | Animator controllers and animated tile assets. |

Full non-deluxe animation inventory: [AnimationAssets.md](./AnimationAssets.md).

## YSort

`YSort` requires a `SpriteRenderer`. In `LateUpdate`, it reads `sr.bounds.min.y` and sets sorting order:

```text
baseOrder - RoundToInt(bottomY * 100) + offset
```

This makes lower objects render in front of higher objects.

`YSort` is widely referenced by player, enemy, collectibles, gates, traps, props, doors, and scene objects.

## LightRaySequencedFX

`LightRaySequencedFX` alternates two `SpriteRenderer` rays:

- Ray A fade in
- Ray A hold
- Ray A fade out
- Ray B fade in
- Ray B hold
- Ray B fade out

It also applies Perlin noise and sinusoidal drift to local position.

## Scene Environment Groups

`Level 1` includes root/group names such as:

- `Background`
- `Midground`
- `Foreground`
- `Environment`
- `Doors`
- `Gates`
- `Pillars`
- `Torches`
- `ovens`
- `Chests`
- `Traps`
- `Waterfalls`
- `Loot`

`Level 2` includes root/group names such as:

- `background`
- `midground`
- `foreground`
- `Environment`
- `Set`
- `Set (1)`
- `Set (2)`
- `Gates`
- `Interactables`
- `Barrels`
- `Levers`
- `LightRays`
- `Keys`
- `Coins`
- `Statues`
- `LightPillars`
- `Ovens`
- `BigWaterFalls`
- `Rocks`
- `Traps`
- `Chests`

## Known Risks

- Some presentation scripts use `Update` for visual animation, which is acceptable for low counts but should be profiled if many instances are active.
- Environment assets include many imported asset-pack files. The Architect package audit script currently expects an absent `Assets/Hovl Studio` folder and fails.
