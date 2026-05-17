# Spin Center Module Setup

## Scope
This module only configures `ui_panel_spin_center` visuals from ScriptableObject data.

## Scripts
- `SpinCenterConfigAsset`
- `SpinCenterSpinSettingsAsset`
- `SpinCenterSetup`
- `SpinCenterSpinController`
- `SpinCenterView`
- `SpinCenterSliceView`

## Required UI Object Names
- `ui_image_spin_base`
- `ui_image_spin_indicator`
- `ui_text_spin_title`
- `ui_text_spin_rewards_info`
- `ui_container_wheel_slices`
- `ui_transform_wheel_animator`
- `ui_button_spin`
- slice item objects should include `SpinCenterSliceView` and usually be named like `ui_item_wheel_slice_00`.

## How To Use
1. Add `SpinCenterView` to `ui_panel_spin_center`.
2. Add `SpinCenterSliceView` to each slice item.
3. Add `SpinCenterSetup` to `ui_panel_spin_center` (or a parent).
4. Add `SpinCenterSpinController` to `ui_panel_spin_center` (or same parent as setup).
   - Button click is subscribed in code (`OnEnable/OnDisable`), not Inspector `OnClick`.
5. Create asset:
   - `Create > Vertigo > Wheel Of Fortune > Spin Center Config`
6. Create spin settings asset:
   - `Create > Vertigo > Wheel Of Fortune > Spin Center Spin Settings`
7. In `SpinCenterConfigAsset`, fill 3 dataset fields directly:
   - `bronze`
   - `silver`
   - `golden`
8. For each dataset fill:
   - bronze/silver/golden wheel sprites
   - title + `rewardInfoAmountValue` (example: `x10`)
   - `titleColor` + `rewardInfoColor`
   - slice icon + amount text per slice
9. Assign the config asset to `SpinCenterSetup`.
10. Assign `SpinCenterSpinSettingsAsset` to `SpinCenterSpinController`.
11. Runtime tier rule is automatic by level:
   - level `30/60` -> golden tier
   - level `%5 == 0` -> silver tier
   - other levels -> bronze tier
12. `SpinCenterSetup` clamps level to `1..60` (60 is max/final).
13. Press Play or run `Apply Current Selection` from component context menu.
14. `SpinCenterSpinController` uses DOTween `Sequence` for spin animation.
15. Runtime integration can listen to:
   - `SpinStarted`
   - `SpinCompleted(float stopAngle)`
16. In `SpinCenterSpinSettingsAsset`, keep `snapToSliceCenter = true` and `sliceCount = 8` to stop exactly at slice centers.
