using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// 游戏内几个面板打开时的默认 UI 选中项管理。
// 主要服务手柄/键盘导航：打开面板后立刻选中第一个按钮。
public class GameController : MonoBehaviour
{
    [SerializeField] private GameObject EscFirst;
    [SerializeField] private GameObject settingFirst;
    [SerializeField] private GameObject selectFirst;


    public void EscOpen() 
    {
        // 打开暂停面板时选中暂停菜单的第一个控件。
        EventSystem.current.SetSelectedGameObject(EscFirst);
        InputController.instance.firstSelectedUI = EscFirst;
    }
    public void SettingOpen() 
    {
        // 打开设置面板时选中设置菜单的第一个控件。
        EventSystem.current.SetSelectedGameObject(settingFirst);
        InputController.instance.firstSelectedUI = settingFirst;
    }
    public void SelectOpen() 
    {
        // 打开选择面板时选中选择菜单的第一个控件。
        EventSystem.current.SetSelectedGameObject(selectFirst);
        InputController.instance.firstSelectedUI = selectFirst;
    }
}
