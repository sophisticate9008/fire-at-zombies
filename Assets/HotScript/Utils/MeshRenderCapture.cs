using UnityEngine;

public class MeshCapture : MonoBehaviour
{
    public static MeshCapture Instance { get; private set; } // 单例实例
    public Camera renderCamera; // 渲染的摄像机

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 使实例在场景切换时不销毁
        }
        else
        {
            Destroy(gameObject); // 销毁额外的实例
        }
    }

    /// <summary>
    /// 捕获传入的 MeshRenderer 对应的图像
    /// </summary>
    /// <param name="targetMeshRenderer">目标 MeshRenderer</param>
    /// <returns>捕获的图像（Texture2D）</returns>
    public Texture2D CaptureMeshImage(MeshRenderer targetMeshRenderer)
    {
        if (renderCamera == null)
        {
            Debug.LogError("RenderCamera is not assigned.");
            return null;
        }

        // 获取 MeshRenderer 的包围盒
        Bounds bounds = targetMeshRenderer.bounds;

        // 将包围盒的四个角转换为屏幕空间（2D 只关心 X 和 Y）
        Vector3[] corners = new Vector3[4];
        corners[0] = renderCamera.WorldToScreenPoint(bounds.center + new Vector3(bounds.extents.x, bounds.extents.y, 0)); // 右上
        corners[1] = renderCamera.WorldToScreenPoint(bounds.center + new Vector3(bounds.extents.x, -bounds.extents.y, 0)); // 右下
        corners[2] = renderCamera.WorldToScreenPoint(bounds.center + new Vector3(-bounds.extents.x, bounds.extents.y, 0)); // 左上
        corners[3] = renderCamera.WorldToScreenPoint(bounds.center + new Vector3(-bounds.extents.x, -bounds.extents.y, 0)); // 左下

        // 计算屏幕空间的最小和最大点
        Vector3 min = corners[0];
        Vector3 max = corners[0];
        foreach (Vector3 corner in corners)
        {
            min = Vector3.Min(min, corner);
            max = Vector3.Max(max, corner);
        }

        // 计算包围盒在屏幕上的像素宽高
        int width = Mathf.RoundToInt(max.x - min.x);
        int height = Mathf.RoundToInt(max.y - min.y);

        // 创建 RenderTexture
        RenderTexture renderTexture = new RenderTexture(width, height, 24);
        renderCamera.targetTexture = renderTexture;

        // 调整摄像机的位置和朝向，使其对准目标对象
        renderCamera.transform.position = bounds.center - renderCamera.transform.forward * bounds.size.magnitude;
        renderCamera.transform.LookAt(bounds.center);

        // 渲染摄像机视图
        RenderTexture.active = renderTexture;
        renderCamera.Render();

        // 将 RenderTexture 转为 Texture2D
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGB24, false);
        texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        texture.Apply();

        // 清理资源
        renderCamera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(renderTexture);

        return texture;
    }
}
