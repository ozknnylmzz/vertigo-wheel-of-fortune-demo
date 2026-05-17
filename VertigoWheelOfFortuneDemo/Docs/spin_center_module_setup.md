# Spin Center Module Setup

## Scope
This module only configures `ui_panel_spin_center` visuals from ScriptableObject data.

## Scripts
- `SpinCenterConfigAsset`
- `SpinCenterSetup`
- `SpinCenterView`
- `SpinCenterSliceView`

## Required UI Object Names
- `ui_image_spin_base`
- `ui_image_spin_indicator`
- `ui_text_spin_title_value`
- `ui_text_spin_subtitle_value`
- `ui_container_wheel_slices`
- slice item objects should include `SpinCenterSliceView` and usually be named like `ui_item_wheel_slice_00`.

## How To Use
1. Add `SpinCenterView` to `ui_panel_spin_center`.
2. Add `SpinCenterSliceView` to each slice item.
3. Add `SpinCenterSetup` to `ui_panel_spin_center` (or a parent).
4. Create asset:
   - `Create > Vertigo > Wheel Of Fortune > Spin Center Config`
5. In the asset, fill 3 dataset fields directly:
   - `bronze`
   - `silver`
   - `golden`
6. For each dataset fill:
   - bronze/silver/golden wheel sprites
   - title/subtitle texts
   - slice icon + amount text per slice
7. Assign the config asset to `SpinCenterSetup`.
8. Runtime tier rule is automatic by level:
   - level `30/60` -> golden tier
   - level `%5 == 0` -> silver tier
   - other levels -> bronze tier
9. `SpinCenterSetup` clamps level to `1..60` (60 is max/final).
10. Press Play or run `Apply Current Selection` from component context menu.
