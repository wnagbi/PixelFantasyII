using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// 按钮音效控制。
// 当前用于按钮动画事件或 UI 事件中播放点击音效。
public class ButtonControl : MonoBehaviour
{
    private Animator ani;
    public AudioClip AudioClip;
    //public UnityEvent tp;
    private void Awake()
    {
        // 预留 Animator 引用，后续如果需要根据动画状态播放音效可以直接使用。
        ani = GetComponent<Animator>();

    }

    public void WaitAnimator() 
    {
            // 播放按钮音效。
            AudioController.instance.PlaySE(AudioClip);
    }
}
