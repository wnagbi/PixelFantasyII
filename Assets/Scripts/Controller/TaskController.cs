using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

/// <summary>
/// 任务控制器。
/// XML 负责文本，Lua 可以热更目标和奖励，任务进度由 GameEvents.KillCountChanged 推进。
/// </summary>
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

    // 记录上一次处理过的击杀数，用来计算这次事件新增了多少击杀。
    private int num;

    private void Awake()
    {
        InitTaskText();
        instance = this;
    }

    private void OnEnable()
    {
        // 击杀任务不再每帧轮询 KillNum，而是在击杀数变化时推进。
        GameEvents.KillCountChanged += OnKillCountChanged;
    }

    private void OnDisable()
    {
        GameEvents.KillCountChanged -= OnKillCountChanged;
    }

    private void Start()
    {
        LoadTasks(taskIndex);
    }

    public void LoadTasks(int index)
    {
        if (index == 0)
        {
            return;
        }

        // 读取当前语言对应的任务文本和默认目标。
        LoadTaskFromXml(index);
        InitTask();
        LoadTaskText();
        isHaveTask = true;
    }

    public void ChangeTasks(int index)
    {
        if (index == 0)
        {
            return;
        }

        // 保留这个接口，方便以后外部切换任务时主动刷新文本。
        LoadTaskFromXml(index);
        LoadTaskText();
    }

    private void LoadTaskFromXml(int index)
    {
        // 当前项目仍用 XML 存任务显示文本；规则层可以继续交给 Lua 热更。
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
        if (tasks.Count == 0)
        {
            return;
        }

        // 显示目标值时也走 Lua，保证 UI 和实际完成条件一致。
        int targetGoal = GetCurrentTaskGoal();
        if (LocalizationSettings.SelectedLocale == LocalizationSettings.AvailableLocales.Locales[0])
        {
            taskName.text = "Task: " + tasks[0].Name;
            taskDescription.text = "Requirement: " + tasks[0].Description;
            taskRewards.text = "Reward: " + tasks[0].Reward;
            taskStatus.text = "Progress: " + $"{status} / {targetGoal}";
        }
        else
        {
            taskName.text = "\u4efb\u52a1: " + tasks[0].Name;
            taskDescription.text = "\u8981\u6c42: " + tasks[0].Description;
            taskRewards.text = "\u5956\u52b1: " + tasks[0].Reward;
            taskStatus.text = "\u8fdb\u5ea6: " + $"{status} / {targetGoal}";
        }
    }

    public void InitTaskText()
    {
        // 没有任务时清空 UI，并把进度基准重置为当前局击杀数。
        taskName.text = "";
        taskDescription.text = "";
        taskRewards.text = "";
        taskStatus.text = "";
        status = 0;
        num = RunData.KillCount;
    }

    public void FinishTask()
    {
        // 当前任务完成后清掉列表，避免后续击杀事件继续推进已完成任务。
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
                // 击杀任务从当前击杀数开始计算，避免加载任务前的击杀被重复计算。
                num = RunData.KillCount;
                break;
            case 2:
                break;
        }
    }

    public void CheckTask()
    {
        // 保留旧接口，兼容可能存在的 Inspector 或脚本调用。
        if (tasks.Count == 0)
        {
            return;
        }

        if (tasks[0].ID == 1)
        {
            OnKillCountChanged(RunData.KillCount);
        }
    }

    private void OnKillCountChanged(int killCount)
    {
        if (tasks.Count == 0 || tasks[0].ID != 1)
        {
            return;
        }

        if (killCount < num)
        {
            // 新一局 Reset 后击杀数会变小，此时同步基准值，不推进任务。
            num = killCount;
            return;
        }

        int delta = killCount - num;
        if (delta <= 0)
        {
            return;
        }

        num = killCount;
        status += delta;
        int targetGoal = GetCurrentTaskGoal();
        // 任务进度变化也广播出去，后续如果有独立任务 UI 可以直接订阅。
        GameEvents.RaiseTaskProgressChanged(tasks[0].ID, status, targetGoal);
        LoadTaskText();

        if (status >= targetGoal)
        {
            CompleteCurrentTask();
        }
    }

    private int GetCurrentTaskGoal()
    {
        int targetGoal = tasks[0].Goal;
        // 任务目标数优先询问 Lua，Lua 缺失或报错时回退 XML 目标值。
        if (LuaConfig.TryCallInt("hotfix.task.task_rule", "GetTaskGoal", this, tasks[0].ID, out int luaGoal))
        {
            targetGoal = luaGoal;
        }

        return targetGoal;
    }

    private void CompleteCurrentTask()
    {
        // 完成奖励优先由 Lua 处理；Lua 未处理时回退到原本的选武器奖励。
        if (!LuaConfig.TryCallBool("hotfix.task.task_rule", "OnTaskComplete", this, tasks[0].ID, out bool handled) || !handled)
        {
            selectWeapon.Invoke();
        }

        InitTaskText();
        FinishTask();
    }
}

/// <summary>
/// 单条任务数据。文本来自 XML，运行时进度由 TaskController 维护。
/// </summary>
[System.Serializable]
public class Task
{
    public int ID { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int Goal { get; set; }
    public string Reward { get; set; }
}
