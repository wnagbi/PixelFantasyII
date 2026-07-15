using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// This class is used to store the data of player. It is a singleton class.
/// If you want to add new attributes, you can add them here. 
/// Don't forget to add a constructor and its limits and set the maximum value, if you need:)
/// </summary>
public class PlayerData : SingleBaseManager<PlayerData>
{
    // The maximum value of each attribute
    private const int MAX_GOLD = 999999;
    private const int MAX_CRYSTAL = 999999;
    private const int MAX_LEVEL = 999;
    private const int MAX_EXP = 999999;
    private const int MAX_KILLNUM = 999999;

    private const float MAX_HEALTH = 9999;
    private const float MAX_ATTACK = 999;
    private const float MAX_DEFENSE = 999;
    private const float MAX_SPEED = 99;

    // The attributes of player
    [Header("Player Info")]
    private int gold;   // the number of gold
    private int crystal;    // the number of crystal(global)
    private int level;  // the level of player
    private int exp;    // the number of experience
    private int killNum;    // the number of killed monsters
    private int x_pos;  // the x position of player
    private int y_pos;  // the y position of player
    private int skill1;
    private int score = 100;
    private int extraAttack;

    [Header("Player Base Attributes")]
    private float baseMaxHealth = 100;    // the basic curHealth of player
    //private float baseAttack = 10;    // the basic attack of player
    private float baseDefense = 5;    // the basic defense of player
    private float baseSpeed = 5;    // the speed of player

    [Header("Player Current Attributes")]
    private float currentMaxHealth; // currentMaxHealth = baseMaxHealth + level * 10 + healthBuff;  // the maximum curHealth of player
    private float currentHealth;    // the current curHealth of player
    //private float currentAttack;    // currentAttack = baseAttack + level * 2 + attackBuff;  // the current attack of player
    private float currentDefense;   // currentDefense = baseDefense + level * 1 + defenseBuff;  // the current defense of player
    private float currentSpeed;   // currentSpeed = speed + speedBuff;  // the current speed of player




    [Header("Game Setting")]
    private int resolutionIndex;
    private int languageIndex;
    // Constructor:
    // Goid, Crystal, Level, Exp, KillNum,
    // HealthBuff, AttackBuff, DefenseBuff, SpeedBuff,
    // MaxHealth, CurrentHealth, CurrentAttack, CurrentDefense, CurrentSpeed,
    // X_pos, Y_pos

    // 旧血量事件保留用于兼容；新代码优先订阅 GameEvents.HealthChanged。
    public event Action<float,float> OnHealthChanged;

    private void SetHealth(float value)
    {
        // 统一走 CurrentHealth 属性，保证扣血/回血都会触发血量事件。
        CurrentHealth = value;
    }

    private void NotifyHealthChanged()
    {
        // 同时触发旧事件和全局事件，避免旧 UI 失效，也让新事件系统能工作。
        OnHealthChanged?.Invoke(currentHealth, CurrentMaxHealth);
        GameEvents.RaiseHealthChanged(currentHealth, CurrentMaxHealth);
    }

    public void AddHealth(float value)
    {
        SetHealth(CurrentHealth + value);
    }

    public void TakeDamage(float value)
    {
        SetHealth(currentHealth - value);
    }
    
    public int ExtraDamge 
    {
        get { return extraAttack; }
        set { extraAttack = value; }
    }
    public int Gold
    {
        get { return gold; }
        set 
        {
            if (value < 0)
                gold = 0;
            else if (value > MAX_GOLD)
                gold = MAX_GOLD;
            else
                gold = value;
        }
    }
    public int Score
    {
        get { return score; }
        set { score = value;}
    }

    public int Crystal
    {
        get { return crystal; }
        set 
        {
            if (value < 0)
                crystal = 0;
            else if (value > MAX_CRYSTAL)
                crystal = MAX_CRYSTAL;
            else
                crystal = value;
        }
    }

    public int Exp
    {
        get { return exp; }
        set
        {
            if (value < 0)
                exp = 0;
            else if (value > MAX_EXP)
                exp = MAX_EXP;
            else
                exp = value;
        }
    }
    public int Level 
    { 
        get { return level; }
        set
        {
            int oldLevel = level;
            if (value < 0)
                level = 0;
            else if (value > MAX_LEVEL)
                level = MAX_LEVEL;
            else
                level = value;

            if (oldLevel != level)
            {
                // 等级变化只发事件，不在这里直接操作 UI。
                GameEvents.RaiseLevelChanged(level);
            }
        }
    }

    public int KillNum
    {
        get { return killNum; }
        set
        {
            if (value < 0)
                killNum = 0;
            else if (value > MAX_KILLNUM)
                killNum = MAX_KILLNUM;
            else
                killNum = value;
        }
    }
    
    //public float HealthBuff
    //{
    //    get { return healthBuff; }
    //    set { healthBuff = value; }
    //}

    //public float AttackBuff
    //{
    //    get { return attackBuff; }
    //    set { attackBuff = value; }
    //}

    //public float DefenseBuff
    //{
    //    get { return defenseBuff; }
    //    set { defenseBuff = value; }
    //}

    //public float SpeedBuff
    //{
    //    get { return speedBuff; }
    //    set { speedBuff = value; }
    //}


    public float CurrentMaxHealth
    {
        get { return currentMaxHealth; }
        set 
        {
            float oldMaxHealth = currentMaxHealth;
            if (value < 0)
                currentMaxHealth = 0;
            else if(value > MAX_HEALTH)
                currentMaxHealth = MAX_HEALTH;
            else
                currentMaxHealth = value; 

            if (!Mathf.Approximately(oldMaxHealth, currentMaxHealth))
            {
                // 最大血量下降时，当前血量不能超过新的最大值。
                if (currentHealth > currentMaxHealth)
                {
                    currentHealth = currentMaxHealth;
                }
                NotifyHealthChanged();
            }
        }
    }

    public float CurrentHealth
    {
        get { return currentHealth; }
        set
        {
            float oldHealth = currentHealth;
            if (value < 0)
                currentHealth = 0;
            else if (value > currentMaxHealth)
                currentHealth = currentMaxHealth;
            else
                currentHealth = value;

            if (!Mathf.Approximately(oldHealth, currentHealth))
            {
                // 血量真的变化时才通知，避免重复刷新血条动画。
                NotifyHealthChanged();
            }
        }
    }

    //public float CurrentAttack
    //{
    //    get { return currentAttack; }
    //    set
    //    {
    //        if (value < 0)
    //            currentAttack = 0;
    //        else if (value > MAX_ATTACK)
    //            currentAttack = MAX_ATTACK;
    //        else
    //            currentAttack = value;
    //    }
    //}

    public float CurrentDefense
    {
        get { return currentDefense; }
        set
        {
            if (value < 0)
                currentDefense = 0;
            else if (value > MAX_DEFENSE)
                currentDefense = MAX_DEFENSE;
            else
                currentDefense = value;
        }
    }

    public float CurrentSpeed
    {
        get { return currentSpeed; }
        set
        {
            if (value < 0)
                currentSpeed = 0;
            else if (value > MAX_SPEED)
                currentSpeed = MAX_SPEED;
            else
                currentSpeed = value;
        }
    }

    public int X_pos
    {
        get { return x_pos; }
        set { x_pos = value; }
    }

    public int Y_pos
    {
        get { return y_pos; }
        set { y_pos = value; }
    }



    public void UpdateAllData()
    {
        // 玩家升级成长参数从 Lua 配置读取。
        // 这里仍然由 C# 计算并写回 PlayerData，Lua 只负责提供可热更的成长系数。
        float healthGrowth = LuaConfig.GetFloat("config.player_config", "level_health_growth", 10f);
        float speedGrowth = LuaConfig.GetFloat("config.player_config", "level_speed_growth", 1f);
        float defenseGrowth = LuaConfig.GetFloat("config.player_config", "level_defense_growth", 1f);

        currentMaxHealth = baseMaxHealth + level * healthGrowth;
        currentSpeed = baseSpeed + level * speedGrowth;
        currentDefense = baseDefense + level * defenseGrowth;
        // 最大血量变化后主动刷新血条比例。
        NotifyHealthChanged();
    }

    /// <summary>
    /// This function is used to init data for a new game.
    /// Including set all attributes to default value.
    /// </summary>
    public void InitData()  // init data for a new game
    {
        PlayerPrefs.SetInt("Level", 1);
        PlayerPrefs.SetInt("Exp", 0);
        KillNum = 0;
        // 本局击杀数和时间现在由 RunData 管理，不再写 PlayerPrefs。
        RunData.Reset();
        PlayerPrefs.SetFloat("MaxHealth", baseMaxHealth);
        PlayerPrefs.SetFloat("CurrentHealth", baseMaxHealth);
        //PlayerPrefs.SetFloat("CurrentAttack", baseAttack);
        PlayerPrefs.SetFloat("CurrentDefense", baseDefense);
        PlayerPrefs.SetFloat("CurrentSpeed", baseSpeed);
    }

    /// <summary>
    /// This function is used to save data of player.
    /// Save all attributes to PlayerPrefs.
    /// </summary>
    public void SaveAllData()
    {
        PlayerPrefs.SetInt("Gold", Gold);
        PlayerPrefs.SetInt("Crystal", Crystal);
        PlayerPrefs.SetInt("Level", level);
        PlayerPrefs.SetInt("Exp", Exp);
        PlayerPrefs.SetFloat("MaxHealth", currentMaxHealth);
        PlayerPrefs.SetFloat("CurrentHealth", CurrentHealth);
        //PlayerPrefs.SetFloat("CurrentAttack", CurrentAttack);
        PlayerPrefs.SetFloat("CurrentDefense", CurrentDefense);
        PlayerPrefs.SetFloat("CurrentSpeed", CurrentSpeed);
    }


    /// <summary>
    /// This function is used to load data of player.
    /// </summary>
    /// 
    public void LoadAllData()
    {
        Gold = PlayerPrefs.GetInt("Gold", gold);
        Crystal = PlayerPrefs.GetInt("Crystal", crystal);
        level = PlayerPrefs.GetInt("Level", level);
        Exp = PlayerPrefs.GetInt("Exp", exp);
        KillNum = RunData.KillCount;
        currentMaxHealth = PlayerPrefs.GetFloat("MaxHealth", currentMaxHealth);
        CurrentHealth = PlayerPrefs.GetFloat("CurrentHealth", currentHealth);
        //CurrentAttack = PlayerPrefs.GetFloat("CurrentAttack", currentAttack);
        CurrentDefense = PlayerPrefs.GetFloat("CurrentDefense", currentDefense);
        CurrentSpeed = PlayerPrefs.GetFloat("CurrentSpeed", currentSpeed);
    }

    public void SaveKillNum() 
    {
        KillNum = RunData.KillCount;
    }
    public void ChangeSkill(int ID) 
    {
        PlayerSaveStore.UnlockSkill(ID);
    }







}
