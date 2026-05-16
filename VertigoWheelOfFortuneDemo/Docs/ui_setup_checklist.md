# UI Setup Checklist (Unity Editor)

> Superseded by `Docs/CASE_RULES.md` as the single source of truth.

## Scene Root
- `scene_wheel_of_fortune`
  - `ui_canvas_root`
  - `systems_root`

## Canvas
- Render Mode: `Screen Space - Overlay`
- Canvas Scaler:
  - UI Scale Mode: `Scale With Screen Size`
  - Reference Resolution: `1920 x 1080`
  - Screen Match Mode: `Expand`
- Add `GraphicRaycaster`.

## Required UI Names (auto-reference by OnValidate)
- `ui_button_spin_silver`
- `ui_button_cash_out`
- `ui_button_restart`
- `ui_text_zone_value`
- `ui_text_rewards_value`
- `ui_text_last_spin_value`

## Component Rules
- Changeable label objects end with `_value`.
- Unnecessary image components:
  - Disable `Raycast Target`.
  - Disable `Maskable` if not needed.
- Use `Image Type: Sliced` for panel/button sprites.
- Keep animation components under child objects, not root.
- Do not use Inspector `OnClick`; listeners are bound in code.

## Runtime Components
- Add `WheelGamePresenter` to UI root.
- Add `WheelGameBootstrap` to `systems_root` (or scene root).
- Assign `wheelGameConfig` in `WheelGameBootstrap`.

## ScriptableObject Assets To Create
1. Reward definitions:
   - `reward_gold`
   - `reward_cash`
   - `reward_chest`
2. Layout assets:
   - `layout_normal_silver` (must include exactly 1 bomb)
   - `layout_safe_silver` (no bomb)
   - `layout_super_golden` (no bomb, high value rewards)
3. Game config:
   - `wheel_game_config`
   - Add at least one zone range with normal/safe/super layout references.
