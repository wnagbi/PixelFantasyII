using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

// 任务控制器。
// XML 负责本地化文本，Lua 可以热更任务目标和完成奖励，C# 保留 UI 刷新和默认击杀任务回退。
public class TaskController : MonoBehaviour
{
    public static TaskController instance;
    public List<Task> tasks;
    public Task curTask;
    public Text taskName;
    public Text taskDescription;
    public Text taskStatus;
    public Text taskRewards;
    public bool isHaveTask;
    [HideInInspector] public int taskIndex = 1;
    public UnityEvent selectWeapon;
    public int status;
    public TextAsset xmlTaskFile;
    private int num;

    private void Awake()
    {
        InitTaskText();
        instance = this;
        PlayerPrefs.SetInt("KillNum", 0);
    }

    private void Start()
    {
        LoadTasks(taskIndex);
    }

    private void Update()
    {
        if (tasks.Count == 0)
        {
            return;
        }

        ChangeTasks(taskIndex);
        CheckTask();
    }

    public void LoadTasks(int index)
    {
        if (index == 0)
        {
            return;
        }

        LoadTaskFromXml(index);
        LoadTaskText();
        InitTask();
        isHaveTask = true;
    }

    public void ChangeTasks(int index)
    {
        if (index == 0)
        {
            return;
        }

        LoadTaskFromXml(index);
        LoadTaskText();
    }

    private void LoadTaskFromXml(int index)
    {
        XmlDocument xml = new XmlDocument();
        xml.LoadXml(xmlTaskFile.text);

        XmlNodeList nodes;
        if (LocalizationSettings.SelectedLocale == LocalizationSettings.AvailableLocales.Locales[0])
        {
            nodes = xml.SelectNodes($"/TaskList/Task{index}/Task_English");
        }
        else
        {
            nodes = xml.SelectNodes($"/TaskList/Task{index}/Task_Chinese");
        }

        foreach (XmlNode node in nodes)
        {
            tasks.Clear();
            Task task = new Task();
            task.ID = int.Parse(node.SelectSingleNode("ID").InnerText);
            task.Name = node.SelectSingleNode("Name").InnerText;
            task.Description = node.SelectSingleNode("Description").InnerText;
            task.Goal = int.Parse(node.SelectSingleNode("Goal").InnerText);
            task.Reward = node.SelectSingleNode("Reward").InnerText;
            tasks.Add(task);
        }
    }

    public void LoadTaskText()
    {
        if (LocalizationSettings.SelectedLocale == LocalizationSettings.AvailableLocales.Locales[0])
        {
            taskName.text = "Task: " + tasks[0].Name;
            taskDescription.text = "Requirement: " + tasks[0].Description;
            taskRewards.text = "Reward: " + tasks[0].Reward;
            taskStatus.text = "Progress: " + $"{status} / {tasks[0].Goal}";
        }
        else
        {
            taskName.text = "\u4efb\u52a1: " + tasks[0].Name;
            taskDescription.text = "\u8981\u6c42: " + tasks[0].Description;
            taskRewards.text = "\u5956\u52b1: " + tasks[0].Reward;
            taskStatus.text = "\u8fdb\u5ea6: " + $"{status} / {tasks[0].Goal}";
        }
    }

    public void InitTaskText()
    {
        taskName.text = "";
        taskDescription.text = "";
        taskRewards.text = "";
        taskStatus.text = "";
        status = 0;
        num = 0;
    }

    public void FinishTask()
    {
        tasks.Clear();
        isHaveTask = false;
    }

    public void InitTask()
    {
        if (tasks.Count == 0)
        {
            return;
        }

        switch (tasks[0].ID)
        {
            case 1:
                num = PlayerPrefs.GetInt("KillNum");
                break;
            case 2:
                break;
        }
    }

    public void CheckTask()
    {
        if (tasks.Count == 0)
        {
            return;
        }

        switch (tasks[0].ID)
        {
            case 1:
                if (num + 1 == PlayerPrefs.GetInt("KillNum"))
                {
                    num = PlayerPrefs.GetInt("KillNum");
                    status++;
                    LoadTaskText();
                }

                int targetGoal = tasks[0].Goal;

                // 任务目标数优先询问 Lua。
                // XML 仍负责文本显示，Lua 负责可热更的完成条件数值。
                if (LuaConfig.TryCallInt("hotfix.task.task_rule", "GetTaskGoal", this, tasks[0].ID, out int luaGoal))
                {
                    targetGoal = luaGoal;
                }

                if (status >= targetGoal)
                {
                    // 任务完成奖励先交给 Lua。
                    // Lua 返回 handled=true 时表示奖励已经处理；否则回退到原来的选武器奖励。
                    if (!LuaConfig.TryCallBool("hotfix.task.task_rule", "OnTaskComplete", this, tasks[0].ID, out bool handled) || !handled)
                    {
                        selectWeapon.Invoke();
                    }

                    InitTaskText();
                    FinishTask();
                }
                break;
            case 2:
                break;
        }
    }
}

[System.Serializable]
// 单条任务数据。
// LoadTaskFromXml 会把 XML 中的 ID/Name/Description/Goal/Reward 填到这个对象里。
public class Task
{
    public int ID { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int Goal { get; set; }
    public string Reward { get; set; }
}
