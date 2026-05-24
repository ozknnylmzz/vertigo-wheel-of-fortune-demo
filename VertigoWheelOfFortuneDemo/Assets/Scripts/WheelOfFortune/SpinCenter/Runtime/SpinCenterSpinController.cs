using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Vertigo.WheelOfFortune.GameFlow.Runtime;
using Vertigo.WheelOfFortune.SpinCenter.Data;

namespace Vertigo.WheelOfFortune.SpinCenter.Runtime
{
    public sealed class SpinCenterSpinController : MonoBehaviour
    {
        private const string SpinButtonName = "ui_button_spin";
        private const string WheelAnimatorName = "ui_transform_wheel_animator";
        private static readonly AnimationCurve DefaultEaseCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("References")]
        [SerializeField] private Button ui_button_spin;
        [SerializeField] private Transform ui_transform_wheel_animator;
        [SerializeField] private SpinCenterSpinSettingsAsset spinSettings;

        public bool IsSpinning { get; private set; }
        public float LastStopAngle { get; private set; }

        public event Action SpinStarted;
        public event Action<float> SpinCompleted;

        private Sequence spinSequence;
        private bool isGameFlowSubscribed;
        private bool spinFastForwarded;
        private int spinStartedFrame;

        private void Awake()
        {
            AutoAssignReferences();
        }

        private void OnEnable()
        {
            SubscribeButton();
            TrySubscribeGameFlow();
            SetButtonInteractable(CanStartSpin());
        }

        private void Start()
        {
            TrySubscribeGameFlow();
            SetButtonInteractable(CanStartSpin());
        }

        private void OnDisable()
        {
            TryUnsubscribeGameFlow();
            UnsubscribeButton();
            StopSpinIfRunning();
            SetButtonInteractable(true);
        }

        private void Update()
        {
            if (!CanFastForwardSpin() || !HasScreenClickStarted())
            {
                return;
            }

            FastForwardSpin();
        }

        private void OnValidate()
        {
            AutoAssignReferences();
        }

        public bool TryStartSpin()
        {
            if (!CanStartSpin())
            {
                return false;
            }

            StartSpinSequence();
            return true;
        }

        [ContextMenu("Spin Once")]
        private void SpinOnceFromContext()
        {
            TryStartSpin();
        }

        private void StartSpinSequence()
        {
            IsSpinning = true;
            spinFastForwarded = false;
            spinStartedFrame = Time.frameCount;
            SetButtonInteractable(false);
            SpinStarted?.Invoke();
            WheelGameEventBus.PublishSpinFastForwardMultiplierChanged(1f);
            WheelGameFlowManager.Instance.BeginSpin();

            int turns = UnityEngine.Random.Range(GetMinSpinTurns(), GetMaxSpinTurns() + 1);
            float extraAngle = UnityEngine.Random.Range(GetMinExtraStopAngle(), GetMaxExtraStopAngle());
            if (ShouldSnapToSliceCenter())
            {
                extraAngle = SnapAngleToSliceCenter(extraAngle, GetSliceCount());
            }

            float totalAngle = turns * 360f + extraAngle;

            Tween rotateTween = ui_transform_wheel_animator
                .DOLocalRotate(new Vector3(0f, 0f, -totalAngle), GetSpinDurationSeconds(), RotateMode.LocalAxisAdd)
                .SetEase(GetSpinEaseCurve());

            spinSequence = DOTween.Sequence()
                .SetTarget(this)
                .Append(rotateTween)
                .OnComplete(OnSpinSequenceComplete)
                .OnKill(OnSpinSequenceKilled);
        }

        private void AutoAssignReferences()
        {
            if (ui_button_spin == null)
            {
                Transform buttonTransform = FindChildRecursive(transform, SpinButtonName);
                if (buttonTransform != null)
                {
                    ui_button_spin = buttonTransform.GetComponent<Button>();
                }
            }

            if (ui_transform_wheel_animator == null)
            {
                ui_transform_wheel_animator = FindChildRecursive(transform, WheelAnimatorName);
            }
        }

        private void SubscribeButton()
        {
            if (ui_button_spin == null)
            {
                return;
            }

            ui_button_spin.onClick.RemoveListener(OnSpinButtonClicked);
            ui_button_spin.onClick.AddListener(OnSpinButtonClicked);
        }

        private void UnsubscribeButton()
        {
            if (ui_button_spin == null)
            {
                return;
            }

            ui_button_spin.onClick.RemoveListener(OnSpinButtonClicked);
        }

        private void OnSpinButtonClicked()
        {
            TryStartSpin();
        }

        private void HandleGameStateChanged(WheelGameState _)
        {
            SetButtonInteractable(CanStartSpin());
        }

        private void TrySubscribeGameFlow()
        {
            if (isGameFlowSubscribed || WheelGameFlowManager.Instance == null)
            {
                return;
            }

            WheelGameFlowManager.Instance.GameStateChanged += HandleGameStateChanged;
            isGameFlowSubscribed = true;
        }

        private void TryUnsubscribeGameFlow()
        {
            if (!isGameFlowSubscribed || WheelGameFlowManager.Instance == null)
            {
                isGameFlowSubscribed = false;
                return;
            }

            WheelGameFlowManager.Instance.GameStateChanged -= HandleGameStateChanged;
            isGameFlowSubscribed = false;
        }

        private void StopSpinIfRunning()
        {
            if (spinSequence == null || !spinSequence.IsActive())
            {
                return;
            }

            spinSequence.Kill(false);
        }

        private void SetButtonInteractable(bool canInteract)
        {
            if (!ShouldBlockButtonDuringSpin() || ui_button_spin == null)
            {
                return;
            }

            ui_button_spin.interactable = canInteract;
        }

        private void OnSpinSequenceComplete()
        {
            if (ShouldSnapToSliceCenter())
            {
                SnapWheelToNearestSliceCenter();
            }

            LastStopAngle = NormalizeAngle(ui_transform_wheel_animator.localEulerAngles.z);
            SpinCompleted?.Invoke(LastStopAngle);
            WheelGameEventBus.PublishSpinCompleted(LastStopAngle);
            ResetSpinState();
        }

        private void OnSpinSequenceKilled()
        {
            ResetSpinState();
        }

        private void ResetSpinState()
        {
            spinSequence = null;
            IsSpinning = false;
            spinFastForwarded = false;
            SetButtonInteractable(CanStartSpin());
        }

        private bool CanStartSpin()
        {
            return !IsSpinning &&
                   ui_transform_wheel_animator != null &&
                   WheelGameFlowManager.Instance != null &&
                   WheelGameFlowManager.Instance.CurrentState != WheelGameState.Spinning &&
                   !IsMaxRoundReached();
        }

        private static bool IsMaxRoundReached()
        {
            WheelGameFlowManager flowManager = WheelGameFlowManager.Instance;
            return flowManager == null ||
                   flowManager.CurrentState == WheelGameState.Win;
        }

        private float GetSpinDurationSeconds()
        {
            return spinSettings != null ? spinSettings.SpinDurationSeconds : 2.6f;
        }

        private int GetMinSpinTurns()
        {
            return spinSettings != null ? spinSettings.MinSpinTurns : 4;
        }

        private int GetMaxSpinTurns()
        {
            return spinSettings != null ? spinSettings.MaxSpinTurns : 7;
        }

        private float GetMinExtraStopAngle()
        {
            return spinSettings != null ? spinSettings.MinExtraStopAngle : 0f;
        }

        private float GetMaxExtraStopAngle()
        {
            return spinSettings != null ? spinSettings.MaxExtraStopAngle : 360f;
        }

        private bool ShouldBlockButtonDuringSpin()
        {
            return spinSettings == null || spinSettings.BlockButtonDuringSpin;
        }

        private bool ShouldSnapToSliceCenter()
        {
            return spinSettings == null || spinSettings.SnapToSliceCenter;
        }

        private float GetFastForwardMultiplier()
        {
            return spinSettings != null ? spinSettings.FastForwardMultiplier : 3f;
        }

        private int GetSliceCount()
        {
            return spinSettings != null ? Mathf.Max(2, spinSettings.SliceCount) : 8;
        }

        private AnimationCurve GetSpinEaseCurve()
        {
            if (spinSettings != null && spinSettings.SpinEaseCurve != null && spinSettings.SpinEaseCurve.length > 0)
            {
                return spinSettings.SpinEaseCurve;
            }

            return DefaultEaseCurve;
        }

        private bool CanFastForwardSpin()
        {
            return IsSpinning &&
                   !spinFastForwarded &&
                   Time.frameCount != spinStartedFrame &&
                   spinSequence != null &&
                   spinSequence.IsActive();
        }

        private void FastForwardSpin()
        {
            spinFastForwarded = true;
            float fastForwardMultiplier = GetFastForwardMultiplier();
            WheelGameEventBus.PublishSpinFastForwardMultiplierChanged(fastForwardMultiplier);

            spinSequence.timeScale = fastForwardMultiplier;
        }

        private static bool HasScreenClickStarted()
        {
            if (Input.GetMouseButtonDown(0))
            {
                return true;
            }

            for (int i = 0; i < Input.touchCount; i++)
            {
                if (Input.GetTouch(i).phase == TouchPhase.Began)
                {
                    return true;
                }
            }

            return false;
        }

        private static float SnapAngleToSliceCenter(float angle, int sliceCount)
        {
            float step = 360f / Mathf.Max(2, sliceCount);
            int stepIndex = Mathf.RoundToInt(angle / step);
            return stepIndex * step;
        }

        private void SnapWheelToNearestSliceCenter()
        {
            if (ui_transform_wheel_animator == null)
            {
                return;
            }

            int sliceCount = GetSliceCount();
            float currentZ = NormalizeAngle(ui_transform_wheel_animator.localEulerAngles.z);
            float snappedZ = SnapAngleToSliceCenter(currentZ, sliceCount);
            ui_transform_wheel_animator.localEulerAngles = new Vector3(0f, 0f, snappedZ);
        }

        private static float NormalizeAngle(float angle)
        {
            angle %= 360f;
            if (angle < 0f)
            {
                angle += 360f;
            }

            return angle;
        }

        private static Transform FindChildRecursive(Transform root, string targetName)
        {
            if (root == null)
            {
                return null;
            }

            if (root.name == targetName)
            {
                return root;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform child = root.GetChild(i);
                Transform result = FindChildRecursive(child, targetName);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }
    }
}
