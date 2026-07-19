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
    private bool isUiMode;

    private void Awake()
    {
        // 部分旧场景同时在场景控制器和玩家 Prefab 上挂了 InputController。
        // 只允许第一个实例工作，避免两个 Update 反复覆盖光标状态。
        if (instance != null && instance != this)
        {
            enabled = false;
            return;
        }

        // static 枚举跨场景保留上一次设备类型，新场景启动时立即恢复对应 UI 模式。
        instance = this;
        eventSystem = EventSystem.current;
        isUiMode = firstSelectedUI != null;
        lastKeyboardMouseInputWasMouse = isUiMode && CurrentDeviceType == PlayerInputDeviceType.KeyboardMouse;
        currentDevice = GetDeviceFromType(CurrentDeviceType);
        ApplyDeviceMode(CurrentDeviceType);
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    private void Update()
    {
        // 输入设备活动状态和 EventSystem 焦点需要逐帧检测，属于必要的实时循环。
        DetectInputDevice();
        RefreshUiSelection();
    }

    /// <summary>
    /// 获取最近一次确认的输入设备实例。
    /// </summary>
    /// <remarks>
    /// 使用注意：对应设备未连接时可能返回 null；UI 模式判断应优先使用 CurrentDeviceType。
    /// </remarks>
    public InputDevice GetCurrentDevice()
    {
        return currentDevice;
    }

    /// <summary>
    /// 检测当前帧真正产生有效输入的键盘、鼠标或手柄设备。
    /// </summary>
    /// <remarks>
    /// 使用注意：没有新输入时返回 null；战斗模式忽略鼠标移动和滚轮，但保留鼠标按键。
    /// </remarks>
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

    /// <summary>
    /// 判断当前 UI 提示和导航是否处于手柄模式。
    /// </summary>
    /// <remarks>
    /// 使用注意：读取的是跨场景保留的设备类型，不表示当前帧一定产生了手柄输入。
    /// </remarks>
    public static bool IsGamepadMode()
    {
        return CurrentDeviceType == PlayerInputDeviceType.Gamepad;
    }

    /// <summary>
    /// 判断当前 UI 提示是否处于键盘鼠标模式。
    /// </summary>
    /// <remarks>
    /// 使用注意：键盘和鼠标共用该模式，焦点行为还取决于最近输入是否来自鼠标。
    /// </remarks>
    public static bool IsKeyboardMouseMode()
    {
        return CurrentDeviceType == PlayerInputDeviceType.KeyboardMouse;
    }

    /// <summary>
    /// 判断当前是否允许 UI 鼠标和导航焦点交互。
    /// </summary>
    /// <remarks>
    /// 使用注意：UI 模式只表示面板交互状态，鼠标是否显示还取决于当前设备类型。
    /// </remarks>
    public bool IsUiMode()
    {
        return isUiMode;
    }

    /// <summary>
    /// 进入 UI 交互模式，并可指定键盘或手柄的默认选中对象。
    /// </summary>
    /// <remarks>
    /// 使用注意：面板激活后再调用；默认对象应是当前激活且可交互的 Selectable。
    /// </remarks>
    public void EnterUiMode(GameObject defaultSelectedUi = null)
    {
        // UI 模式决定光标是否允许出现；输入设备只决定鼠标和手柄的表现差异。
        if (defaultSelectedUi != null)
        {
            firstSelectedUI = defaultSelectedUi;
        }

        isUiMode = true;
        ApplyDeviceMode(CurrentDeviceType);
    }

    /// <summary>
    /// 退出 UI 交互模式，清空导航焦点并重新锁定、隐藏鼠标。
    /// </summary>
    /// <remarks>
    /// 使用注意：关闭暂停或升级选择面板后调用，不要在仍需点击的界面中提前退出。
    /// </remarks>
    public void ExitUiMode()
    {
        isUiMode = false;
        firstSelectedUI = null;
        ApplyDeviceMode(CurrentDeviceType);
    }

    /// <summary>
    /// 根据布尔值统一切换 UI 与战斗输入模式。
    /// </summary>
    /// <remarks>
    /// 使用注意：需要指定默认焦点时应直接调用 EnterUiMode(defaultSelectedUi)。
    /// </remarks>
    public void SetUiMode(bool enabled)
    {
        if (enabled)
        {
            EnterUiMode();
        }
        else
        {
            ExitUiMode();
        }
    }

    /// <summary>
    /// 根据当前帧有效输入更新保存的设备类型。
    /// </summary>
    /// <remarks>
    /// 使用注意：无输入时必须保持原设备类型，不能切换成空状态。
    /// </remarks>
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

    /// <summary>
    /// 切换当前设备类型并同步光标、焦点和设备变化事件。
    /// </summary>
    /// <remarks>
    /// 使用注意：相同类型不重复广播 DeviceChanged，避免 UI 图标反复刷新。
    /// </remarks>
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

    /// <summary>
    /// 把持久化设备枚举解析为当前 Input System 设备实例。
    /// </summary>
    /// <remarks>
    /// 使用注意：目标设备未连接时允许返回 null，调用方不能直接解引用。
    /// </remarks>
    private InputDevice GetDeviceFromType(PlayerInputDeviceType deviceType)
    {
        return deviceType == PlayerInputDeviceType.Gamepad ? Gamepad.current : Keyboard.current;
    }

    /// <summary>
    /// 将设备类型和 UI 模式应用到 Cursor 与 EventSystem 焦点。
    /// </summary>
    /// <remarks>
    /// 使用注意：鼠标模式清空导航选中，键盘或手柄模式仅在焦点为空时恢复默认项。
    /// </remarks>
    private void ApplyDeviceMode(PlayerInputDeviceType deviceType)
    {
        bool enableGamepadMode = deviceType == PlayerInputDeviceType.Gamepad;
        bool showMouse = isUiMode && deviceType == PlayerInputDeviceType.KeyboardMouse;

        // 战斗模式始终隐藏鼠标；只有进入 UI 且当前使用键鼠时才显示。
        Cursor.visible = showMouse;
        Cursor.lockState = showMouse ? CursorLockMode.None : CursorLockMode.Locked;

        if (eventSystem == null)
        {
            return;
        }

        if (!isUiMode)
        {
            eventSystem.SetSelectedGameObject(null);
        }
        else if (enableGamepadMode || !lastKeyboardMouseInputWasMouse)
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

    /// <summary>
    /// 维护 UI 导航焦点，防止面板切换后手柄失去可导航对象。
    /// </summary>
    /// <remarks>
    /// 使用注意：鼠标模式清空焦点后必须立即返回，避免默认按钮被逐帧重复选中。
    /// </remarks>
    private void RefreshUiSelection()
    {
        // 手柄模式下如果面板切换导致选中项为空，恢复 firstSelectedUI 以便继续导航。
        if (eventSystem == null || !isUiMode)
        {
            return;
        }

        if (CurrentDeviceType == PlayerInputDeviceType.KeyboardMouse)
        {
            if (lastKeyboardMouseInputWasMouse)
            {
                // 鼠标模式不需要 EventSystem 的导航焦点。
                // 无论当前是否已有选中项都必须直接返回，否则“当前为空”会继续执行
                // 下方的默认焦点恢复，形成 清空 -> 选中 -> 清空 的逐帧循环。
                if (eventSystem.currentSelectedGameObject != null)
                {
                    eventSystem.SetSelectedGameObject(null);
                }

                return;
            }
        }

        if (firstSelectedUI != null && eventSystem.currentSelectedGameObject == null)
        {
            eventSystem.SetSelectedGameObject(firstSelectedUI);
        }
    }

    /// <summary>
    /// 判断鼠标在当前模式下是否产生了应当切换设备提示的有效输入。
    /// </summary>
    /// <remarks>
    /// 使用注意：战斗模式只接受鼠标按键；UI 模式额外接受移动和滚轮。
    /// </remarks>
    private bool HasMouseInput()
    {
        if (Mouse.current == null)
        {
            return false;
        }

        bool buttonPressed = Mouse.current.leftButton.wasPressedThisFrame
            || Mouse.current.rightButton.wasPressedThisFrame
            || Mouse.current.middleButton.wasPressedThisFrame;

        // 战斗中忽略鼠标移动和滚轮，避免碰到鼠标就切换图标或显示光标。
        // 鼠标按键仍可能绑定攻击，因此按键本身继续视为有效输入。
        if (!isUiMode)
        {
            return buttonPressed;
        }

        return buttonPressed
            || Mouse.current.delta.ReadValue().sqrMagnitude > 0.01f
            || Mathf.Abs(Mouse.current.scroll.ReadValue().y) > 0.01f;
    }
}
