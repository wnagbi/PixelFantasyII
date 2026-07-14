using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;
using Mirror;
using UnityEngine.EventSystems;
using UnityEngine.Events;

// 输入设备与 UI 导航控制器。
// 负责判断当前使用键盘鼠标还是手柄，并根据设备切换鼠标显示/锁定和默认 UI 选中项。
public class InputController : MonoBehaviour
{
    public static InputController instance;
    // 当前面板打开后希望手柄默认选中的 UI。
    public GameObject firstSelectedUI;
    private EventSystem eventSystem;
    private InputDevice currentDevice;
    private InputDevice lastDevice;
    private GameObject lastSelect;
    private void Awake()
    {
        // 缓存 EventSystem，后面频繁切换 UI 选中项时使用。
        instance = this;
        eventSystem = EventSystem.current;

    }
    private void Update()
    {
        // 每帧检测当前活跃输入设备，并根据设备更新 UI 模式。
        DetectInputDevice();
        KeyboardUI();
        //HandleUIInteraction();
    }
    void DetectInputDevice()
    {
        // 如果当前活跃设备变化，就执行一次切换处理。
        InputDevice newDevice = GetActiveInputDevice();

        if (newDevice != lastDevice) 
        {
            //Debug.Log("切换");
            lastDevice =currentDevice;
            currentDevice = newDevice;
            HandleSwitchDevice();
        }
    }

    public InputDevice GetActiveInputDevice() 
    {
        // 手柄摇杆有输入或确认键按下时，认为当前设备是手柄。
        if (Gamepad.current != null && (Gamepad.current.leftStick.ReadValue().magnitude > 0.1f ||
            Gamepad.current.buttonSouth.isPressed)) 
        {
            //Debug.Log("切换手柄");
            return Gamepad.current;

        }
        // 键盘任意键或鼠标移动时，认为当前设备是键鼠。
        if (Keyboard.current.anyKey.isPressed || Mouse.current.delta.ReadValue().magnitude > 0) 
        {
            //Debug.Log("切换键盘");
            SetGamepadUIState(false);
            return Keyboard.current;
        }
        return null;
    }

    void HandleSwitchDevice() 
    {
        // 根据设备变化决定是否进入手柄 UI 模式。
        
        bool isSwitchingToGamepad = currentDevice is Gamepad;
        bool isSwitchingToKeyboard = currentDevice is Keyboard;
        bool isSwitchingFromGamepad = lastDevice is Gamepad;
        bool isSwitchingStay = currentDevice is null;


        if (isSwitchingStay && isSwitchingFromGamepad) 
        {
            // 没有新输入但上一个设备是手柄时，保持当前 UI 选中，避免焦点丢失。
            lastSelect = eventSystem.currentSelectedGameObject;
            eventSystem.SetSelectedGameObject(lastSelect);
            return;
        }
        if (isSwitchingToGamepad && eventSystem.currentSelectedGameObject == null)
        {
            //Debug.Log("锁鼠标");
            // 切到手柄时锁鼠标并选中当前面板的默认 UI。
            SetGamepadUIState(true);
            return;
        }
    }
    void SetGamepadUIState(bool enableGamepadMode)
    {
        // 手柄模式隐藏并锁住鼠标；键鼠模式显示鼠标并清空 UI 选中。
        Cursor.visible = !enableGamepadMode;
        Cursor.lockState = enableGamepadMode ? CursorLockMode.Locked : CursorLockMode.None;

        if (enableGamepadMode  )
        {
            //Debug.Log("设置初始");
            eventSystem.SetSelectedGameObject(firstSelectedUI);
        }
        else
        {
            eventSystem.SetSelectedGameObject(null);
        }
    }
    void KeyboardUI()
    {
        // 键鼠模式下不强制保留 EventSystem 选中项，让鼠标悬停/点击控制 UI。
        if (lastDevice is Keyboard || eventSystem.currentSelectedGameObject == null && firstSelectedUI != null)
        {
            eventSystem.SetSelectedGameObject(null);
        }

    }

}
