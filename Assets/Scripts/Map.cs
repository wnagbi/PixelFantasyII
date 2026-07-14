using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 标题界面/选择界面中的地图按钮。
// 点击某张地图时，点亮当前 Outline，并把 StartGameController 的目标场景名改成这张地图。
public class Map : MonoBehaviour
{
    // 要启动的场景名或地图标识。
    public string mapName;
    private Outline outline;

    // 其它地图按钮的 Outline，用于切换时关闭其它选中状态。
    public Outline[] otherMap;

    private void Awake()
    {
        outline = GetComponent<Outline>();
    }

    public void ChangeOutline() 
    {
        // 当前地图显示选中边框，并记录本次要进入的地图。
        outline.enabled = true;
        StartGameController.instance.startGameTarget = mapName;

        // 关闭其它地图的选中边框，保证界面上只有一个地图处于选中状态。
        foreach(var map in otherMap) 
        {
            map.enabled = false;
        }
        
    }

}
