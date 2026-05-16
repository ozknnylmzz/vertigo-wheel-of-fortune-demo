# Vertigo Wheel Of Fortune - Case Rules

## 1) Game Rules (Core Gameplay)
- Player spins a wheel instead of selecting cards.
- Each zone has a wheel with multiple reward slices and one bomb slice.
- Bomb result removes all collected rewards and ends the run.
- Player can restart after bomb.
- Every 5th zone is `Safe Zone`:
  - Silver spin.
  - No bomb.
- Every 30th zone is `Super Zone`:
  - Golden spin.
  - No bomb.
  - Special rewards.
- Player may cash out only when:
  - Wheel is not spinning.
  - Current zone is `Safe` or `Super`.

## 2) Technical Rules (Must Apply Everywhere)
- Use reusable, maintainable, scalable, testable architecture.
- Use OOP + SOLID principles.
- Prefer ScriptableObjects for editable game content.
- Wheel slice content must be editable from Unity Editor.

## 3) UI Technical Rules
- `Canvas Scaler`:
  - `Scale With Screen Size`
  - `Screen Match Mode: Expand`
  - Reference resolution `1920 x 1080`
- Use `TextMeshPro` for texts.
- For dynamic/changeable UI text names, suffix with `_value`.
- Naming style: root general to specific.
  - Example: `ui_image_spin_silver`
- Do not use Inspector `OnClick` wiring.
- Button references must be auto-assigned with `OnValidate`.
- Disable unnecessary `Raycast Target` and unnecessary `Maskable` on images.
- Use `Image Type: Sliced` when using panel/button frame sprites.
- Do not put UI animator components on root transforms; use dedicated child transforms.
- Keep anchors/pivots correct for multi-aspect compatibility.
- Do not stretch images.

## 4) Required Deliverables
- Working Android APK (Release) uploaded on GitHub release.
- Gameplay video.
- Screenshots in all required aspect ratios:
  - `20:9`
  - `16:9`
  - `4:3`

## 5) Recommended / Plus Points
- Unity `2021 LTS`.
- DOTween usage.
- Sprite Atlas usage.
- Proper ScriptableObject usage.

## 6) Project Scene/Setup Baseline
- Active gameplay scene: `GamePlayScene`.
- Keep `SampleScene` as backup until final cleanup.
- Suggested scene roots:
  - `scene_wheel_of_fortune`
  - `ui_canvas_root`
  - `systems_root`

## 7) Current Implementation Contract (This Repo)
- Core flow uses:
  - `ZoneRules`
  - `WheelSpinEngine`
  - `WheelSession`
- Config flow uses:
  - `WheelGameConfigAsset`
  - `WheelSpinLayoutAsset`
  - `RewardDefinitionAsset`
- UI flow uses:
  - `WheelGamePresenter` (button/text auto-binding via `OnValidate`)
  - `WheelGameBootstrap` (runtime wiring)

## 8) Working Agreement For Next Steps
- We speak Turkish in chat.
- We keep code/comments/identifiers in English.
- Every new task is checked against this file before implementation.
