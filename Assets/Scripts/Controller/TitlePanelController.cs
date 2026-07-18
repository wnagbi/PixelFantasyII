using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

// 标题界面面板导航控制器。
// 打开不同面板时设置默认选中 UI，保证手柄/键盘导航有焦点。
public class TitlePanelController : MonoBehaviour
{
    [SerializeField] private GameObject titileFirst;
    [SerializeField] private GameObject settingFirst;
    [SerializeField] private GameObject selectFirst;
    [SerializeField] private GameObject guidleFirst;
    [SerializeField] private Animator[] anis;
    private void Start()
    {
        // 进入标题界面时默认选中标题主菜单按钮。
        TitleOpen();
        
    }
    public void TitleOpen()
    {
        // 标题主面板默认选中项。
        FocusUi(titileFirst);
    }
    public void SettingOpen() 
    {
        // 设置面板默认选中项。
        FocusUi(settingFirst);
    }
    public void SelectOpen() 
    {
        // 地图选择面板默认选中项。
        FocusUi(selectFirst);
    }
    public void SelectGuidle() 
    {
        // 教程/指南面板默认选中项。
        FocusUi(guidleFirst);
    }

    private static void FocusUi(GameObject defaultSelection)
    {
        if (InputController.instance != null)
        {
            InputController.instance.EnterUiMode(defaultSelection);
            return;
        }

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(defaultSelection);
        }
    }

    public void InitButton() 
    {
        // 重置按钮 Animator 到 Normal，避免切换面板后按钮停留在高亮/按下状态。
        foreach (var ani in anis) 
        {
            ani.Play("Normal");
        }
    }
    public void QuitGame() 
    {
        // 打包后退出游戏；编辑器里不会真正关闭 Unity。
        Application.Quit();
    }
}


