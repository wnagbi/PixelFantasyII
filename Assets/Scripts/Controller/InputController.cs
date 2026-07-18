using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public enum PlayerInputDeviceType
{
    // 键盘和鼠标共用一套 UI 提示模式。
    KeyboardMouse,
    // 手柄模式会隐藏鼠标并恢复 EventSystem 默认选中项。
    Gamepad
}

// 记录当前输入设备，并根据“手柄 / 键盘 / 鼠标”调整 UI 焦点和鼠标显示。
public class InputController : MonoBehaviour
{
    public static InputController instance;
    public static PlayerInputDeviceType CurrentDeviceType { get; private set; } = PlayerInputDeviceType.KeyboardMouse;
    public static event Action<PlayerInputDeviceType> DeviceChanged;

    // 当前面板打开后，手柄或键盘导航默认选中的 UI。
    public GameObject firstSelectedUI;

    private EventSystem eventSystem;
    private InputDevice currentDevice;
    private bool lastKeyboardMouseInputWasMouse;

    private void Awake()
    {
        // static 枚举跨场景保留上一次设备类型，新场景启动时立即恢复对应 UI 模式。
        instance = this;
        eventSystem = EventSystem.current;
        currentDevice = GetDeviceFromType(CurrentDeviceType);
        ApplyDeviceMode(CurrentDeviceType);
    }

    private void Update()
    {
        // 输入设备活动状态和 EventSystem 焦点需要逐帧检测，属于必要的实时循环。
        DetectInputDevice();
        RefreshUiSelection();
    }

    public InputDevice GetCurrentDevice()
    {
        return currentDevice;
    }

    public InputDevice GetActiveInputDevice()
    {
        // 只有本帧真的出现有效输入才返回设备；没有输入时保持上一次模式。
        if (Gamepad.current != null
            && (Gamepad.current.leftStick.ReadValue().magnitude > 0.1f
                || Gamepad.current.buttonSouth.isPressed))
        {
            return Gamepad.current;
        }

        if (HasMouseInput())
        {
            lastKeyboardMouseInputWasMouse = true;
            return Keyboard.current;
        }

        if (Keyboard.current != null && Keyboard.current.anyKey.isPressed)
        {
            lastKeyboardMouseInputWasMouse = false;
            return Keyboard.current;
        }

        return null;
    }

    public static bool IsGamepadMode()
    {
        return CurrentDeviceType == PlayerInputDeviceType.Gamepad;
    }

    public static bool IsKeyboardMouseMode()
    {
        return CurrentDeviceType == PlayerInputDeviceType.KeyboardMouse;
    }

    private void DetectInputDevice()
    {
        InputDevice activeDevice = GetActiveInputDevice();
        if (activeDevice is Gamepad)
        {
            SetCurrentDeviceType(PlayerInputDeviceType.Gamepad);
        }
        else if (activeDevice is Keyboard)
        {
            SetCurrentDeviceType(PlayerInputDeviceType.KeyboardMouse);
        }
    }

    private void SetCurrentDeviceType(PlayerInputDeviceType deviceType)
    {
        // 类型未变化时不重复广播，但鼠标模式仍需刷新显示/锁定状态。
        if (CurrentDeviceType == deviceType)
        {
            if (deviceType == PlayerInputDeviceType.KeyboardMouse)
            {
                ApplyDeviceMode(deviceType);
            }

            return;
        }

        CurrentDeviceType = deviceType;
        currentDevice = GetDeviceFromType(deviceType);
        ApplyDeviceMode(deviceType);
        DeviceChanged?.Invoke(CurrentDeviceType);
    }

    private InputDevice GetDeviceFromType(PlayerInputDeviceType deviceType)
    {
        return deviceType == PlayerInputDeviceType.Gamepad ? Gamepad.current : Keyboard.current;
    }

    private void ApplyDeviceMode(PlayerInputDeviceType deviceType)
    {
        bool enableGamepadMode = deviceType == PlayerInputDeviceType.Gamepad;
        bool showMouse = deviceType == PlayerInputDeviceType.KeyboardMouse && lastKeyboardMouseInputWasMouse;

        // 只有真实鼠标输入时才显示鼠标。键盘游玩和手柄游玩都隐藏鼠标。
        Cursor.visible = showMouse;
        Cursor.lockState = showMouse ? CursorLockMode.None : CursorLockMode.Locked;

        if (eventSystem == null)
        {
            return;
        }

        if (enableGamepadMode)
        {
            if (firstSelectedUI != null && eventSystem.currentSelectedGameObject == null)
            {
                eventSystem.SetSelectedGameObject(firstSelectedUI);
            }
        }
        else if (showMouse)
        {
            eventSystem.SetSelectedGameObject(null);
        }
    }

    private void RefreshUiSelection()
    {
        // 手柄模式下如果面板切换导致选中项为空，恢复 firstSelectedUI 以便继续导航。
        if (eventSystem == null)
        {
            return;
        }

        if (CurrentDeviceType == PlayerInputDeviceType.KeyboardMouse)
        {
            if (lastKeyboardMouseInputWasMouse && eventSystem.currentSelectedGameObject != null)
            {
                eventSystem.SetSelectedGameObject(null);
            }

            return;
        }

        if (firstSelectedUI != null && eventSystem.currentSelectedGameObject == null)
        {
            eventSystem.SetSelectedGameObject(firstSelectedUI);
        }
    }

    private bool HasMouseInput()
    {
        // 鼠标移动、点击或滚轮都视为切换到鼠标模式的有效输入。
        if (Mouse.current == null)
        {
            return false;
        }

        return Mouse.current.delta.ReadValue().sqrMagnitude > 0.01f
            || Mouse.current.leftButton.wasPressedThisFrame
            || Mouse.current.rightButton.wasPressedThisFrame
            || Mouse.current.middleButton.wasPressedThisFrame
            || Mathf.Abs(Mouse.current.scroll.ReadValue().y) > 0.01f;
    }
}
