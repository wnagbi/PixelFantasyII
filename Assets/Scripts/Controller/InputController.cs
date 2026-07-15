using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public enum PlayerInputDeviceType
{
    KeyboardMouse,
    Gamepad
}

// 输入设备与 UI 导航控制器。
// 负责记录当前使用键鼠还是手柄，并根据设备切换鼠标显示/锁定和默认 UI 选中项。
public class InputController : MonoBehaviour
{
    public static InputController instance;
    public static PlayerInputDeviceType CurrentDeviceType { get; private set; } = PlayerInputDeviceType.KeyboardMouse;
    public static event Action<PlayerInputDeviceType> DeviceChanged;

    // 当前面板打开后希望手柄默认选中的 UI。
    public GameObject firstSelectedUI;

    private EventSystem eventSystem;
    private InputDevice currentDevice;

    private void Awake()
    {
        // 缓存 EventSystem，后面频繁切换 UI 选中项时使用。
        instance = this;
        eventSystem = EventSystem.current;
        currentDevice = GetDeviceFromType(CurrentDeviceType);
        ApplyDeviceMode(CurrentDeviceType);
    }

    private void Update()
    {
        // 每帧只检测“是否出现新的有效输入”；没有输入时保留上一次设备状态。
        DetectInputDevice();
        KeyboardUI();
    }

    public InputDevice GetCurrentDevice()
    {
        // 返回上一次有效输入设备，而不是这一帧是否有输入。
        return currentDevice;
    }

    public InputDevice GetActiveInputDevice()
    {
        // 手柄摇杆有输入或确认键按下时，认为当前设备是手柄。
        if (Gamepad.current != null && (Gamepad.current.leftStick.ReadValue().magnitude > 0.1f ||
            Gamepad.current.buttonSouth.isPressed))
        {
            return Gamepad.current;
        }

        // 键盘任意键或鼠标移动时，认为当前设备是键鼠。
        if ((Keyboard.current != null && Keyboard.current.anyKey.isPressed) ||
            (Mouse.current != null && Mouse.current.delta.ReadValue().magnitude > 0))
        {
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

        // 手柄模式隐藏并锁住鼠标；键鼠模式显示鼠标并清空 UI 选中。
        Cursor.visible = !enableGamepadMode;
        Cursor.lockState = enableGamepadMode ? CursorLockMode.Locked : CursorLockMode.None;

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
        else
        {
            eventSystem.SetSelectedGameObject(null);
        }
    }

    private void KeyboardUI()
    {
        if (eventSystem == null)
        {
            return;
        }

        // 键鼠模式下不强制保留 EventSystem 选中项，让鼠标悬停/点击控制 UI。
        if (CurrentDeviceType == PlayerInputDeviceType.KeyboardMouse)
        {
            if (eventSystem.currentSelectedGameObject != null)
            {
                eventSystem.SetSelectedGameObject(null);
            }

            return;
        }

        // 手柄模式下如果当前没有选中项，尝试恢复当前面板的默认选中 UI。
        if (firstSelectedUI != null && eventSystem.currentSelectedGameObject == null)
        {
            eventSystem.SetSelectedGameObject(firstSelectedUI);
        }
    }
}
