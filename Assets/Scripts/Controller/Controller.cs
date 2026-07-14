using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;


// 手柄震动控制器。
// 其它脚本调用 StartVibration，真正是否震动取决于设置界面的 isVibration。
public class Controller : MonoBehaviour
{
    public static Controller instance;
    public Gamepad pad;
    public bool isVibration;
    private void Start()
    {
        // 记录当前手柄。手柄可能热插拔，所以震动前还会重新取 Gamepad.current。
        instance = this;
        pad = Gamepad.current;
    }

    public void StartVibration(float lowFequency, float highFequency, float duration) 
    {
        // 如果没有手柄或设置中关闭震动，直接跳过。
        pad = Gamepad.current;
        if (pad == null || !GameSettingController.instance.isVibration) return; 

        // 设置左右马达强度，并在 duration 秒后自动停止。
        pad.SetMotorSpeeds(lowFequency, highFequency);
        isVibration = true;
        Invoke("StopVibration",duration);
    }
    public void StopVibration() 
    {
        // 停止手柄震动，避免震动残留。
        pad = Gamepad.current;
        if (pad == null || !GameSettingController.instance.isVibration) return;
        pad.SetMotorSpeeds(0f, 0f);
        isVibration = false;

    }

}
