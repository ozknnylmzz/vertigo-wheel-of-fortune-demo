# Vertigo Wheel Of Fortune - Implementation Plan

> Superseded by `Docs/CASE_RULES.md` as the single source of truth.

## Mandatory Constraints (Applied To All Steps)
- Unity target: `2021.3 LTS` (requested by case).
- UI: `Canvas Scaler -> Scale With Screen Size -> Expand`.
- Text: `TextMeshPro` only.
- Naming: root-to-specific, for mutable labels suffix `_value`.
- No Unity Inspector `OnClick` events.
- Button references auto-assigned in `OnValidate`.
- Unnecessary `Raycast Target` disabled.
- UI animation components on child transforms, not root.
- Use sliced sprites where needed.

## Delivery Checklist
- Android APK build (Release).
- Gameplay video.
- Screenshots: `20:9`, `16:9`, `4:3`.
- GitHub repository + release upload.

## Phase Plan
1. Project foundation and code architecture.
2. Core wheel game rules and deterministic outcome flow.
3. ScriptableObject-driven zone and slice content.
4. UI binding layer (no inspector event wiring).
5. Spin animation, reward flow, bomb flow, safe/super zone flow.
6. Aspect ratio validation and visual polish.
7. Android release build and capture assets.

## First Technical Target
- Build a reusable core with:
  - Zone type rules (`Normal`, `Safe`, `Super`).
  - Slice models (`Reward`, `Bomb`).
  - Reward stash tracking and lose-on-bomb behavior.
  - Exit/take-reward gating by zone and wheel state.
