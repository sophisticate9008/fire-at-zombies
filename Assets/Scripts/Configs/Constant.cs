using System.IO;
using UnityEngine;

public class Constant
{
    public static readonly string ConfigsPath = Path.Combine(Application.persistentDataPath, "Configs");
    public static readonly string DataPath = Path.Combine(Application.persistentDataPath, "Data");
    public static readonly string SelfPrefabResPath = "Prefabs/Self/";
    public static readonly string EnemyPrefabResPath = "Prefabs/Enemy/";
    public static readonly string PrefabResOther = "Prefabs/Other/";
    public static readonly int JewelMaxId = 10;
    public static Vector2 leftBottomViewBoundary = new(0, 0.2f);
    public static Vector2 rightTopViewBoundary = new(1, 1f);
    public static Vector2 leftBottomBoundary = Camera.main.ViewportToWorldPoint(new Vector3(0, 0.2f, Camera.main.nearClipPlane)); // 设定左下角的边界
    public static Vector2 rightTopBoundary = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, Camera.main.nearClipPlane)); // 设定右上角的边界
}