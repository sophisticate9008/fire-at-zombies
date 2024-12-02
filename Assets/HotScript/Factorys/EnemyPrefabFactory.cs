using FightBases;
using UnityEngine;
using YooAsset;

namespace Factorys
{
    public class EnemyPrefabFactory
    {
        public static GameObject Create(string enemyName, string enemyType)
        {
            // 加载预制体
            GameObject prefabO = YooAssets.LoadAssetSync(enemyName).AssetObject as GameObject;
            GameObject prefab = GameObject.Instantiate(prefabO);
            // 添加组件
            EnemyBase enemyBase = prefab.AddComponent(CommonUtil.GetTypeByName(enemyName)) as EnemyBase;
            return prefab;
        }


    }
}