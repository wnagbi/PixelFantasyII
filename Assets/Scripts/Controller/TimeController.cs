using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;
using UnityEngine.UI;

// 本局计时器。
// 负责显示生存时间，到达 tpTime 时触发通关/传送事件，并把时间写入 RunData 供结算读取。
public class TimeController : MonoBehaviour
{
    public Text text;
    private float startTime;
    public float runTime;
    public float tpTime;
    public UnityEvent tpEvent;
    public LocalizedString nameString;
    private int lastDisplayedSecond = -1;
    private bool completionTriggered;
    private string timeLabel = string.Empty;

    private void Awake()
    {
        nameString.TableEntryReference = "TimeText";
    }

    private void OnEnable()
    {
        // 首次加载和切换语言时缓存前缀，不在 Update 中重复查询本地化表。
        nameString.StringChanged += OnTimeLabelChanged;
    }

    private void OnDisable()
    {
        nameString.StringChanged -= OnTimeLabelChanged;
    }

    private void Start()
    {
        // 新一局开始时清空击杀数和时间，确保 HUD、任务、结算从同一个初始状态开始。
        RunData.Reset();
        // 记录本局开始时间。
        startTime = Time.time;
        runTime = 0f;
        completionTriggered = false;
        RefreshTimeText(0);
    }

    private void Update()
    {
        // 时间值保持逐帧精度，结算界面仍可读取准确的 RunData.RunTime。
        runTime = Mathf.Max(0f, Time.time - startTime);
        RunData.SetRunTime(runTime);

        // 文本只在整数秒变化时刷新，避免每帧字符串格式化和 UI rebuild。
        int wholeSeconds = Mathf.FloorToInt(runTime);
        if (wholeSeconds != lastDisplayedSecond)
        {
            RefreshTimeText(wholeSeconds);
        }

        // 使用 >= 防止低帧率跨过目标时刻，并保证事件只触发一次。
        if (!completionTriggered && runTime >= tpTime)
        {
            completionTriggered = true;
            tpEvent?.Invoke();
        }
    }

    private void OnTimeLabelChanged(string localizedText)
    {
        timeLabel = localizedText;
        RefreshTimeText(Mathf.Max(0, Mathf.FloorToInt(runTime)));
    }

    private void RefreshTimeText(int wholeSeconds)
    {
        lastDisplayedSecond = wholeSeconds;
        if (text == null)
        {
            return;
        }

        string minutes = (wholeSeconds / 60).ToString("00");
        string seconds = (wholeSeconds % 60).ToString("00");
        text.text = $"{timeLabel} : {minutes} : {seconds}";
    }
}
