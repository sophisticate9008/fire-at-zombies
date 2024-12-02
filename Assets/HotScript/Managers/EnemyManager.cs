using System.Collections;
using System.Collections.Generic;
using FightBases;
using UnityEngine;
// 管理怪物生成
public class EnemyManager : ManagerBase<EnemyManager>
{
    public List<AnimationCurve> spawnRateCurves; // 每种怪物的生成速率曲线
    private List<string> enemyTypes = new() { "NormalMonster" }; // 支持的怪物类型
    private List<EnemyConfigBase> enemyConfigBases = new(); // 怪物配置
    private List<GameObject> monsterPrefabs = new(); // 怪物预制体
    private int maxCount = 1000; // 最大生成数量
    private int currentCount = 0; // 当前生成数量
    public float fixInterval = 6f; // 固定时间间隔
    public float noiseScale = 0.8f; // 噪声比例
    [SerializeField]
    private float ViewportXCoordinate = 0.8f; // 视口生成的y坐标
    public int liveCount = 0;//存活数量
    private Camera mainCamera;


    private void Start()
    {
        mainCamera = Camera.main;
        foreach (string item in enemyTypes)
        {
            EnemyConfigBase configBase = ConfigManager.Instance.GetConfigByClassName(item) as EnemyConfigBase;
            enemyConfigBases.Add(configBase);
            monsterPrefabs.Add(configBase.Prefab);
            StartCoroutine(SpawnMonsters(item)); // 启动特定怪物类型的生成协程
        }
    }

    private IEnumerator SpawnMonsters(string enemyType)
    {
        int enemyIndex = enemyTypes.IndexOf(enemyType);
        AnimationCurve currentCurve = spawnRateCurves[enemyIndex];

        while (currentCount < maxCount)
        {
            float normalizedInput = Mathf.Clamp01((float)currentCount / maxCount);
            float spawnInterval = (1 - currentCurve.Evaluate(normalizedInput) * noiseScale) * fixInterval; // 使用当前怪物的生成曲线

            // 只有在生成值大于0时才生成怪物
            if (spawnInterval > 0)
            {
                SpawnMonster();
            }

            // 等待下次生成
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnMonster()
    {
        currentCount++; // 增加共享数量
        liveCount++;
        // 随机生成视口坐标中的x坐标（0到设置之间）
        float viewportXCoordinate = Random.Range(0f, ViewportXCoordinate);

        // 将视口坐标转换为世界坐标
        Vector3 worldPosition = mainCamera.ViewportToWorldPoint(new Vector3(viewportXCoordinate, 1f, mainCamera.nearClipPlane));

        // 设置生成位置
        Vector2 spawnPosition = new Vector2(worldPosition.x, worldPosition.y);

        // 随机选择怪物
        int monsterIndex = Random.Range(0, monsterPrefabs.Count);
        GameObject theEnemy = ObjectPoolManager.Instance.GetFromPool(enemyTypes[monsterIndex] + "Pool", monsterPrefabs[monsterIndex]);

        // 设置怪物位置并初始化
        theEnemy.transform.position = spawnPosition;
        theEnemy.GetComponent<EnemyBase>().Init();
    }
}
