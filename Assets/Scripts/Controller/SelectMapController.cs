using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

// 地图选择界面的分数显示。
// 从 save_data.json 读取累计分数，并用本地化文本刷新到 UI。
public class SelectMapController : MonoBehaviour
{
    public Text scoreText;
    public LocalizedString nameString;

    private void Start()
    {
        // 指定本地化表中的分数字段。
        nameString.TableEntryReference = "ScoreText";
        //PlayerData.getInstance().InitData();
    }

    private void Update()
    {
        // 每帧刷新总分显示。后续可以优化成进入界面或分数变化时刷新。
        scoreText.text = $"{nameString.GetLocalizedString()}" + PlayerSaveStore.Current.score.ToString();
    }
}