using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// UI 导航关系数据组件。
// 用来在 Inspector 中记录当前 UI 元素上下左右相邻的 UI 对象。
public class UIElementLinks : MonoBehaviour
{
    public GameObject up;
    public GameObject down;
    public GameObject left;
    public GameObject right;
}

