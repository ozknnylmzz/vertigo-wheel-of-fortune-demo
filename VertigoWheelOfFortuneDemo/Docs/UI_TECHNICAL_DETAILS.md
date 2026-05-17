# UI Technical Details

## Baseline
- Canvas Scaler mode: `Scale With Screen Size`
- Screen Match Mode: `Expand`
- Use `TextMeshPro` for all texts

## Naming
- Changeable text/value fields should end with `_value`
- Use root-to-specific naming style
  - Example: `ui_image_spin_silver`

## Buttons / Events
- Do not wire button events from Unity Inspector `OnClick`
- Button references must be auto-assigned from `OnValidate` code

## UI Components
- Disable unnecessary `Raycast Target` and `Maskable` flags on images
- Use `Image Type: Sliced` when panel/button sprites require slicing

## Layout / Transforms
- UI animators should be on dedicated child transforms, not root transforms
- Anchors and pivots must be set correctly for multi-aspect compatibility
- Do not stretch images
