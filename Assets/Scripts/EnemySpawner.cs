using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{

    public float initalSpawnInterval;
    public float minSpawnInterval;
    public float accelerationFactor;
    public float maxDifficultyTime;

    public Transform player;

    public GameObject enemyToSpawn;
    public string enemyPoolName = "Silm";
    private float spawnCounter;

    private Vector3 minSpawn;
    private Vector3 maxSpawn;

    public Vector3 maxSpawnOffset;
    public Vector3 minSpawnOffset;
    private float gameTime;
    private bool missingPlayerWarningShown;
    private void Start()
    {
        // 刷怪基础参数优先从 Lua 配置读取。
        // 如果 config.stage_config.lua 缺字段或报错，就保留 Inspector 中填写的默认值。
        initalSpawnInterval = LuaConfig.GetFloat("config.stage_config", "enemy_spawn", "initial_interval", initalSpawnInterval);
        minSpawnInterval = LuaConfig.GetFloat("config.stage_config", "enemy_spawn", "min_interval", minSpawnInterval);
        accelerationFactor = LuaConfig.GetFloat("config.stage_config", "enemy_spawn", "acceleration_factor", accelerationFactor);
        maxDifficultyTime = LuaConfig.GetFloat("config.stage_config", "enemy_spawn", "max_difficulty_time", maxDifficultyTime);
        enemyPoolName = LuaConfig.GetString("config.stage_config", "enemy_spawn", "pool_name", enemyPoolName);
        spawnCounter = initalSpawnInterval;
        TryBindPlayer();
    }
    private void Update()
    {
        if (player == null && !TryBindPlayer())
        {
            return;
        }
            
            
        spawnCounter -= Time.deltaTime;
        gameTime += Time.deltaTime;
        minSpawn = minSpawnOffset+ player.transform.position;
        maxSpawn = maxSpawnOffset + player.transform.position;
        if (spawnCounter <= 0)
        {
            spawnCounter = GetCurrentSpawnInterval();            

            // 每次刷怪前询问 Lua 本轮要从哪个对象池取敌人。
            // 这样可以热更不同时间段/难度下刷出的敌人类型。
            if (LuaConfig.TryCallString("hotfix.stage.stage_rule", "GetSpawnPoolName", this, enemyPoolName, out string luaPoolName)
                && !string.IsNullOrEmpty(luaPoolName))
            {
                enemyPoolName = luaPoolName;
            }
            GameObject silm = ObjPoolManager.instance.GetObj(enemyPoolName);
            if (silm != null)
            {
                silm.transform.position = SelectSpawnPoint();
            }
        }
        //Debug.Log(GetCurrentSpawnInterval());
        
    }

    private bool TryBindPlayer()
    {
        if (PlayerRuntimeRegistry.TryGetPlayerTransform(out player))
        {
            return true;
        }

        if (!missingPlayerWarningShown)
        {
            Debug.LogWarning("[EnemySpawner] Player is not registered yet. Spawn will wait.", this);
            missingPlayerWarningShown = true;
        }

        return false;
    }

    public float GetCurrentSpawnInterval()  //刷怪间隔计算
    {
        float t = Mathf.Clamp01(gameTime / maxDifficultyTime);
        float difficulty = Mathf.Pow(t, accelerationFactor*10);
        float currentInterval = Mathf.Lerp(initalSpawnInterval,minSpawnInterval, difficulty);
        return currentInterval;
    }


    public Vector3 SelectSpawnPoint() //怪物刷新点计算
    {
        Vector3 spawnPoint = Vector3.zero;
        bool spawnVerticalEdge = Random.Range(0f, 1f) > 0.5f;
        if (spawnVerticalEdge)
        {
            spawnPoint.y = Random.Range(minSpawn.y, maxSpawn.y);
            if (Random.Range(0f, 1f) > 0.5f)
                spawnPoint.x = maxSpawn.x;
            else
                spawnPoint.x = minSpawn.x;
        }
        else 
        {
            spawnPoint.x = Random.Range(minSpawn.x, maxSpawn.x);
            if (Random.Range(0f, 1f) > 0.5f)
                spawnPoint.y = maxSpawn.y;
            else
                spawnPoint.y = minSpawn.y;
        }


        return spawnPoint;
    }

}
