using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// UI Button 状态检测器。
// 统一监听鼠标和手柄/键盘选择事件，记录按钮当前状态并在高亮/选中时播放音效。
public class ButtonStateDetector : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler,
    ISelectHandler,     
    IDeselectHandler    
{
    public UnityEvent playMusic;
    public enum ButtonState { Normal, Highlighted, Pressed, Disabled, Selected }
    public ButtonState currentState = ButtonState.Normal;

    private Button button;

    private void Awake()
    {
        // 缓存 Button 并初始化当前状态。
        button = GetComponent<Button>();
        RefreshInteractableState();
    }

    private void OnEnable()
    {
        RefreshInteractableState();
    }

    /// <summary>
    /// 处理指针进入 UI 元素时的高亮和反馈。
    /// </summary>
    /// <remarks>
    /// 使用注意：由 EventSystem 自动回调；目标对象需要配置对应的 UI 组件和事件系统。
    /// </remarks>
    public void OnPointerEnter(PointerEventData eventData)
    {
        // 鼠标进入按钮时进入高亮状态。
        if (CanInteract())
        {
            currentState = ButtonState.Highlighted;
            playMusic.Invoke();
        }
 
    }

    /// <summary>
    /// 处理指针离开 UI 元素时的状态恢复。
    /// </summary>
    /// <remarks>
    /// 使用注意：由 EventSystem 自动回调；目标对象需要配置对应的 UI 组件和事件系统。
    /// </remarks>
    public void OnPointerExit(PointerEventData eventData)
    {
        // 鼠标离开按钮时回到普通状态。
        if (CanInteract())
            currentState = ButtonState.Normal;
    }

    /// <summary>
    /// 处理指针按下 UI 元素时的按压状态。
    /// </summary>
    /// <remarks>
    /// 使用注意：由 EventSystem 自动回调；目标对象需要配置对应的 UI 组件和事件系统。
    /// </remarks>
    public void OnPointerDown(PointerEventData eventData)
    {
        // 鼠标按下时进入按下状态。
        if (CanInteract())
            currentState = ButtonState.Pressed;
    }

    /// <summary>
    /// 处理指针在 UI 元素上抬起时的状态恢复。
    /// </summary>
    /// <remarks>
    /// 使用注意：由 EventSystem 自动回调；目标对象需要配置对应的 UI 组件和事件系统。
    /// </remarks>
    public void OnPointerUp(PointerEventData eventData)
    {
        // 鼠标抬起后回到高亮状态。
        if (CanInteract())
            currentState = ButtonState.Highlighted; // 假设鼠标仍在按钮上
    }
    /// <summary>
    /// 处理 EventSystem 选中当前 UI 元素时的反馈。
    /// </summary>
    /// <remarks>
    /// 使用注意：由 EventSystem 自动回调；目标对象需要配置对应的 UI 组件和事件系统。
    /// </remarks>
    public void OnSelect(BaseEventData eventData)
    {
        // 手柄/键盘导航选中按钮时进入 Selected，并播放音效。
        if (CanInteract())
        {
            currentState = ButtonState.Selected;
            playMusic.Invoke();
        }     
    }

    /// <summary>
    /// 处理 EventSystem 取消选中当前 UI 元素时的状态恢复。
    /// </summary>
    /// <remarks>
    /// 使用注意：由 EventSystem 自动回调；目标对象需要配置对应的 UI 组件和事件系统。
    /// </remarks>
    public void OnDeselect(BaseEventData eventData)
    {
        // 失去 UI 选中焦点时回到普通状态。
        if (CanInteract())
            currentState = ButtonState.Normal;
    }

    /// <summary>
    /// 设置 ButtonStateDetector 中与 SetInteractable 对应的状态或数据。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 ButtonStateDetector 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public void SetInteractable(bool interactable)
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }

        if (button == null)
        {
            return;
        }

        button.interactable = interactable;
        RefreshInteractableState();
    }

    /// <summary>
    /// 根据最新数据刷新 ButtonStateDetector 的状态或显示。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 ButtonStateDetector 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public void RefreshInteractableState()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }

        if (button == null)
        {
            return;
        }

        if (!button.interactable)
        {
            currentState = ButtonState.Disabled;
        }
        else if (currentState == ButtonState.Disabled)
        {
            currentState = ButtonState.Normal;
        }
    }

    /// <summary>
    /// 判断当前条件是否允许执行 CanInteract 对应的操作。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 ButtonStateDetector 内部流程调用，并依赖当前组件已经完成初始化。
    /// </remarks>
    private bool CanInteract()
    {
        if (button == null || !button.interactable)
        {
            currentState = ButtonState.Disabled;
            return false;
        }

        return true;
    }
}

