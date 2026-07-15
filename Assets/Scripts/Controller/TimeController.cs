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
    void Start()
    {
        // 新一局开始时清空击杀数和时间，确保 HUD、任务、结算从同一个初始状态开始。
        RunData.Reset();
        // 记录本局开始时间。
        startTime = Time.time;
        nameString.TableEntryReference = "TimeText";
    }
    private void Update()
    {
        // 到达目标生存时间时触发外部绑定事件。
        if ((int)runTime == tpTime)
        {
            //Debug.Log("生存成功");
            tpEvent.Invoke();
        }
        else 
        {
            // 计算从本局开始到现在经过的时间。
            runTime = Time.time - startTime; // 计算当时游戏时间
            string minutes = ((int)runTime / 60).ToString("00");
            string seconds = (runTime % 60).ToString("00");
            text.text = $"{nameString.GetLocalizedString()} : " + minutes + " : " + seconds;
            // 不再写 PlayerPrefs.Time，本局时间只保存在运行时数据里。
            RunData.SetRunTime(runTime);
        }
       
    }
}
