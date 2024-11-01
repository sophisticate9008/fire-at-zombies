using System;
using ArmsChild;
using ArmConfigs;
using FightBases;
using R3;
using UnityEngine;
namespace MyComponents
{
    public class PenetrableComponent : ComponentBase, IPenetrable
    {

        private readonly ReactiveProperty<int> _penetrationLevel = new();
        public PenetrableComponent(string componentName, string type, GameObject selfObj) : base(componentName, type, selfObj)
        {

        }
        public override void Init()
        {
            base.Init();
            PenetrationLevel = (ConfigManager.Instance.GetConfigByClassName("Global") as GlobalConfig).AllPenetrationLevel;
            PenetrationLevel += (SelfObj.GetComponent<ArmChildBase>().Config as IPenetrable).PenetrationLevel;
        }
        public int PenetrationLevel
        {
            get => _penetrationLevel.Value;
            set
            {
                _penetrationLevel.Value = value;
            }
        }
        public void HandleDestruction()
        {
            SelfObj.GetComponent<ArmChildBase>().ReturnToPool();
        }

        public override void Exec(GameObject enemyObj)
        {
            PenetrationLevel -= enemyObj.GetComponent<EnemyBase>().Config.Blocks;
            if (PenetrationLevel <= 0)
            {
                HandleDestruction();
            }
        }
    }

}
