using System.Collections;
using System.Collections.Generic;
using System.Reflection;
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

    void Start()
    {
        // 缓存 Button 并初始化当前状态。
        button = GetComponent<Button>();
        // 初始状态
        currentState = button.interactable ? ButtonState.Normal : ButtonState.Disabled;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // 鼠标进入按钮时进入高亮状态。
        if (button.interactable) 
        {
            currentState = ButtonState.Highlighted;
            playMusic.Invoke();
        }
 
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // 鼠标离开按钮时回到普通状态。
        if (button.interactable)
            currentState = ButtonState.Normal;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // 鼠标按下时进入按下状态。
        if (button.interactable)
            currentState = ButtonState.Pressed;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // 鼠标抬起后回到高亮状态。
        if (button.interactable)
            currentState = ButtonState.Highlighted; // 假设鼠标仍在按钮上
    }
    public void OnSelect(BaseEventData eventData)
    {
        // 手柄/键盘导航选中按钮时进入 Selected，并播放音效。
        if (button == null)
            return;
        if (button.interactable)
        {
            currentState = ButtonState.Selected;
            playMusic.Invoke();
        }     
    }

    public void OnDeselect(BaseEventData eventData)
    {
        // 失去 UI 选中焦点时回到普通状态。
        if (button == null)
            return;
        if (button.interactable)
            currentState = ButtonState.Normal;
        
    }
    void Update()
    {
        // 如果按钮被禁用，同步记录 Disabled 状态。
        // 处理禁用状态
        if (!button.interactable)
            currentState = ButtonState.Disabled;
    }
}

