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


    /// <summary>
    /// 打开暂停面板时选中暂停菜单的第一个控件。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 GameController 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public void EscOpen() 
    {
        // 打开暂停面板时选中暂停菜单的第一个控件。
        FocusUi(EscFirst);
    }
    /// <summary>
    /// 打开设置面板时选中设置菜单的第一个控件。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 GameController 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public void SettingOpen() 
    {
        // 打开设置面板时选中设置菜单的第一个控件。
        FocusUi(settingFirst);
    }
    /// <summary>
    /// 打开选择面板时选中选择菜单的第一个控件。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 GameController 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public void SelectOpen() 
    {
        // 打开选择面板时选中选择菜单的第一个控件。
        FocusUi(selectFirst);
    }

    /// <summary>
    /// 进入 UI 模式并为键盘或手柄设置当前面板默认焦点。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 GameController 内部流程调用，并依赖当前组件已经完成初始化。
    /// </remarks>
    private static void FocusUi(GameObject defaultSelection)
    {
        if (InputController.instance != null)
        {
            InputController.instance.EnterUiMode(defaultSelection);
            return;
        }

        // 没有输入控制器的测试场景仍可使用 EventSystem 默认导航。
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(defaultSelection);
        }
    }
}
