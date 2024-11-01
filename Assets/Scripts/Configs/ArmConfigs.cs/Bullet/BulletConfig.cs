


namespace ArmConfigs
{
    public class BulletConfig : ArmConfigBase, IMultipleable, IFissionable, IPenetrable, IReboundable, IBoomable
    {
        // 新增属性
        public int PenetrationLevel { get; set; } = 1;
        public int ReboundCount { get; set; } = 1;
        public BulletFissionConfig BulletFissionConfig => ConfigManager.Instance.GetConfigByClassName("BulletFission") as BulletFissionConfig;
        public int BulletFissionCount { get; set; } = 2;
        public int MultipleLevel { get; set; } = 1;
        public int RepeatLevel { get; set; } = 1;
        public float AngleDifference { get; set; } = 5f;
        public float RepeatCd { get; set; } = 0.1f;
        public ArmConfigBase FissionableChildConfig => BulletFissionConfig;


        public string FindType { get; set; } = "random";

        public ArmConfigBase BoomChildConfig => ConfigManager.Instance.GetConfigByClassName("BulletBoom") as BulletBoomConfig;

        // 构造函数
        public BulletConfig() : base()
        {
            // 延迟初始化 BulletFissionConfig，并传递当前 BulletConfig 实例

        }

        // 重写父类的 Init 方法
        public override void Init()
        {
            // 初始化 BulletConfig 的属性
            Name = "bullet";
            Description = "bullet";
            Level = 1;
            RangeFire = 7;
            Speed = 10f;
            Tlc = 1f;
            Cd = 2f;
            CritRate = 0.1f;
            Owner = Name;
            ComponentStrs.Add("穿透");
            ComponentStrs.Add("减速");
            ComponentStrs.Add("冰冻");
            ComponentStrs.Add("反弹");
            AttackCd = 1f;
            AttackCount = 30;
            
            DamageType = "ad";
            DamageExtraType = "penetrable";
            OnType = "enter";
            BuffDamageTlc = 0.1f;
            CdType = MyEnums.CdTypes.Exhaust;
        }
    }
}
