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
    /// <summary>
    /// 标题主面板默认选中项。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 TitlePanelController 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public void TitleOpen()
    {
        // 标题主面板默认选中项。
        FocusUi(titileFirst);
    }
    /// <summary>
    /// 设置面板默认选中项。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 TitlePanelController 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public void SettingOpen() 
    {
        // 设置面板默认选中项。
        FocusUi(settingFirst);
    }
    /// <summary>
    /// 地图选择面板默认选中项。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 TitlePanelController 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public void SelectOpen() 
    {
        // 地图选择面板默认选中项。
        FocusUi(selectFirst);
    }
    /// <summary>
    /// 教程/指南面板默认选中项。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 TitlePanelController 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public void SelectGuidle() 
    {
        // 教程/指南面板默认选中项。
        FocusUi(guidleFirst);
    }

    /// <summary>
    /// 进入 UI 模式并为键盘或手柄设置当前面板默认焦点。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 TitlePanelController 内部流程调用，并依赖当前组件已经完成初始化。
    /// </remarks>
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

    /// <summary>
    /// 重置按钮 Animator 到 Normal，避免切换面板后按钮停留在高亮/按下状态。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 TitlePanelController 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public void InitButton() 
    {
        // 重置按钮 Animator 到 Normal，避免切换面板后按钮停留在高亮/按下状态。
        foreach (var ani in anis) 
        {
            ani.Play("Normal");
        }
    }
    /// <summary>
    /// 打包后退出游戏；编辑器里不会真正关闭 Unity。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 TitlePanelController 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public void QuitGame() 
    {
        // 打包后退出游戏；编辑器里不会真正关闭 Unity。
        Application.Quit();
    }
}


