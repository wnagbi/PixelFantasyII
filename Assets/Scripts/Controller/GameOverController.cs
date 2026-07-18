using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

// 结算界面控制器。
// 从 RunData 读取本局时间和击杀数，本局得分累加到 save_data.json。
public class GameOverController : MonoBehaviour
{
    public int scoreX;
    public Text suriveTime;
    public Text scoreText;
    public Text killNumText;

    public LocalizedString survivalString;
    public LocalizedString scoreString;
    public LocalizedString killNumString;

    private void OnEnable()
    {
        // Cursor 状态会跨场景保留。战斗场景可能因键盘/手柄模式将其锁定并隐藏，
        // 结算界面属于纯 UI 场景，因此启用时必须主动恢复鼠标交互。
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Awake()
    {
        // 本局时间只存在运行时，不作为长期存档保存。
        float time = RunData.RunTime;
        string minutes = ((int)time / 60).ToString("00");
        string seconds = (time % 60).ToString("00");
        suriveTime.text = $"{survivalString.GetLocalizedString()}" + minutes + " : " + seconds;

        // 击杀数由 Enemy -> RunData.AddKill() 统一累加。
        int killNum = RunData.KillCount;
        int runScore = killNum * scoreX;

        // 显示本局得分。
        scoreString.TableEntryReference = "ScoreText";
        scoreText.text = $"{scoreString.GetLocalizedString()}" + runScore.ToString();

        // 显示本局击杀数。
        killNumString.TableEntryReference = "KillNumText";
        killNumText.text = $"{killNumString.GetLocalizedString()}" + killNum.ToString();

        // 把本局得分累加到玩家长期存档。
        PlayerSaveStore.AddScore(runScore);
    }
}
