

public class NormalMonsterConfig: EnemyConfigBase{
    
    public NormalMonsterConfig():base() {
        Life = 1500;
        Speed = 0.15f;
        Blocks = 1;
        RangeFire = 0;
        CharacterType = "elite";
    }
}