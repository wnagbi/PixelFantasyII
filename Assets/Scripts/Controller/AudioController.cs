using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 全局音频播放入口。
// 当前负责背景音乐 AudioSource 引用和一次性音效播放。
public class AudioController : MonoBehaviour
{
    public static AudioController instance;
    public AudioSource BGMScoure;
    public AudioSource SESource;
    private void Awake()
    {
        // 其它脚本通过 AudioController.instance 播放音效。
        instance = this;
    }

    /// <summary>
    /// 播放单个音效。当前实现会覆盖 SESource.clip，所以同一时刻只播放一个 SE。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 AudioController 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public void PlaySE(AudioClip audio) 
    {
        // 播放单个音效。当前实现会覆盖 SESource.clip，所以同一时刻只播放一个 SE。
        SESource.clip = audio;
        SESource.Play();
    }
}
