using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public enum PlayerInputDeviceType
{
    KeyboardMouse,
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
        instance = this;
        eventSystem = EventSystem.current;
        currentDevice = GetDeviceFromType(CurrentDeviceType);
        ApplyDeviceMode(CurrentDeviceType);
    }

    private void Update()
    {
        DetectInputDevice();
        RefreshUiSelection();
    }

    public InputDevice GetCurrentDevice()
    {
        return currentDevice;
    }

    public InputDevice GetActiveInputDevice()
    {
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
