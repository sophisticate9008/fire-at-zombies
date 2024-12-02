
using System.Collections;
using System.Reflection;
using HybridCLR;
using UnityEngine;
using YooAsset;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System;
public class ResourceManager : MonoBehaviour
{

    // [RuntimeInitializeOnLoadMethod]
    // public static void DisableOldTLS1()
    // {

    //     // 禁用 TLS 1.0 和 TLS 1.1，强制使用 TLS 1.2 或更高版本
    //     ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
    //     ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(ValidateServerCertificate);
    // }
    // private static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
    // {
    //     // 检查是否存在 SSL 错误
    //     if (sslPolicyErrors != System.Net.Security.SslPolicyErrors.None)
    //     {
    //         Debug.LogError("SSL Policy Errors: " + sslPolicyErrors);
    //         return false;  // 返回 false，表示证书验证失败
    //     }

    //     // 获取证书的公钥字符串
    //     string publicKey = certificate.GetPublicKeyString();

    //     // 预设的公钥（你可以自定义这个公钥）
    //     string expectedPublicKey = "MHYwEAYHKoZIzj0CAQYFK4EEACIDYgAENkFhFytTJe2qypTk1tpIV+9QuoRkgte7" +
    //         "BRvWHwYk9qUznYzn8QtVaGOCMBBfjWXsqqivl8q1hs4wAYl03uNOXgFu7iZ7zFP6" +
    //         "I6T3RB0+TR5fZqathfby47yOCZiAJI4g";

    //     // 比较证书公钥和预设的公钥
    //     if (publicKey.Equals(expectedPublicKey))
    //     {
    //         Debug.Log("证书验证通过");
    //         return true;  // 返回 true，表示证书验证通过
    //     }
    //     else
    //     {
    //         Debug.LogError("Invalid Public Key");
    //         return false;  // 返回 false，表示证书验证失败
    //     }
    // }
    // private static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
    // {
    //     // 这里返回 true 直接绕过证书验证
    //     return true;
    // }
    public EPlayMode PlayMode = EPlayMode.OfflinePlayMode;
    private ResourcePackage package;
    private string packageName = "DefaultPackage";
    private string dialogText = "";
    private string packageVersion;
    private ResourceDownloaderOperation downloader;

    public static ResourceManager Instance { get; private set; }
    protected virtual void Awake()
    {
        // 检查是否已经有实例存在
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // 防止创建多个实例
        }
        else
        {
            Instance = this;
            AwakeCallBack();
        }
    }
    protected void AwakeCallBack()
    {
        // DisableOldTLS1();
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        ServicePointManager.ServerCertificateValidationCallback += (p1, p2, p3, p4) => true;
        DontDestroyOnLoad(gameObject);
    }
    private void Start()
    {

        YooAssets.Initialize();
        package ??= YooAssets.CreatePackage("DefaultPackage");
        YooAssets.SetDefaultPackage(package);
        switch (PlayMode)
        {
            case EPlayMode.EditorSimulateMode:
                {
                    StartCoroutine(EditorInitPackage());
                    break;
                }
            case EPlayMode.OfflinePlayMode:
                {
                    StartCoroutine(SingInitPackage());
                    break;
                }
            case EPlayMode.HostPlayMode:
                {
                    StartCoroutine(HostInitPackage());
                    break;
                }
        }

    }
    #region 不同模式
    private IEnumerator EditorInitPackage()
    {
        //注意：如果是原生文件系统选择EDefaultBuildPipeline.RawFileBuildPipeline
        var buildPipeline = EDefaultBuildPipeline.BuiltinBuildPipeline;
        var simulateBuildResult = EditorSimulateModeHelper.SimulateBuild(buildPipeline, packageName);
        var editorFileSystem = FileSystemParameters.CreateDefaultEditorFileSystemParameters(simulateBuildResult);
        var initParameters = new EditorSimulateModeParameters();
        initParameters.EditorFileSystemParameters = editorFileSystem;
        var initOperation = package.InitializeAsync(initParameters);
        yield return initOperation;

        if (initOperation.Status == EOperationStatus.Succeed)
            Debug.Log("资源包初始化成功！");
        else
            Debug.LogError($"资源包初始化失败：{initOperation.Error}");
    }
    private IEnumerator SingInitPackage()
    {
        var createParameters = new OfflinePlayModeParameters();
        createParameters.BuildinFileSystemParameters = FileSystemParameters.CreateDefaultBuildinFileSystemParameters();
        var initializationOperation = package.InitializeAsync(createParameters);
        yield return initializationOperation;

        if (initializationOperation.Status == EOperationStatus.Succeed)
            Debug.Log("资源包初始化成功！");
        else
            Debug.LogError($"资源包初始化失败：{initializationOperation.Error}");
        StartCoroutine(UpdatePackageVersion());
    }

    private IEnumerator HostInitPackage()
    {
        // string defaultHostServer = "https://unity.wdyplus.xyz/fire_at_zombies";
        // string fallbackHostServer = "https://unity.wdyplus.xyz/fire_at_zombies";
        string defaultHostServer = "https://unity.wdyplus.xyz/fire_at_zombies";
        string fallbackHostServer = "https://unity.wdyplus.xyz/fire_at_zombies";
        IRemoteServices remoteServices = new RemoteServices(defaultHostServer, fallbackHostServer);

        var buildinFileSystem = FileSystemParameters.CreateDefaultBuildinFileSystemParameters();
        var cacheFileSystem = FileSystemParameters.CreateDefaultCacheFileSystemParameters(remoteServices);
        var initParameters = new HostPlayModeParameters
        {
            BuildinFileSystemParameters = buildinFileSystem,
            CacheFileSystemParameters = cacheFileSystem
        };

        var initializationOperation = package.InitializeAsync(initParameters);
        yield return initializationOperation;

        if (initializationOperation.Status == EOperationStatus.Succeed)
            Debug.Log("资源包初始化成功！");
        else
            Debug.LogError($"资源包初始化失败：{initializationOperation.Error}");

        StartCoroutine(UpdatePackageVersion());
    }
    private IEnumerator UpdatePackageVersion()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        var operation = package.RequestPackageVersionAsync(false);
        yield return operation;
        if (operation.Status != EOperationStatus.Succeed)
        {
            Debug.LogWarning(operation.Error);
        }
        else
        {
            Debug.Log($"Request package version : {operation.PackageVersion}");
            dialogText += $"Request package version : {operation.PackageVersion}\n";
            packageVersion = operation.PackageVersion;
            StartCoroutine(UpdateManifest());
        }
    }

    #endregion

    #region  更新清单
    private IEnumerator UpdateManifest()
    {
        yield return new WaitForSecondsRealtime(0.5f);


        var package = YooAssets.GetPackage(packageName);
        var operation = package.UpdatePackageManifestAsync(packageVersion);
        yield return operation;

        if (operation.Status != EOperationStatus.Succeed)
        {
            Debug.LogWarning(operation.Error);
            yield break;
        }
        else
        {
            yield return CreateDownloader();
        }
    }
    #endregion
    #region 下载逻辑
    IEnumerator CreateDownloader()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        var package = YooAssets.GetPackage(packageName);
        int downloadingMaxNum = 10;
        int failedTryAgain = 3;
        downloader = package.CreateResourceDownloader(downloadingMaxNum, failedTryAgain);

        if (downloader.TotalDownloadCount == 0)
        {
            StartGame();
            Debug.Log("Not found any download files !");
        }
        else
        {
            // 发现新更新文件后，挂起流程系统
            // 注意：开发者需要在下载前检测磁盘空间不足
            int totalDownloadCount = downloader.TotalDownloadCount;
            long totalDownloadBytes = downloader.TotalDownloadBytes;
            dialogText += "发现新文件数量：" + totalDownloadCount + "\n";
            dialogText += "总共大小为" + FormatStorage(totalDownloadBytes) + "\n";
            dialogText += "是否开始下载？(取消则退出游戏)\n";
            UpdateUIManager.Instance.ShowDialog(dialogText);
            downloader.OnDownloadErrorCallback = OnDownloadErrorFunction;
            downloader.OnDownloadProgressCallback = OnDownloadProgressUpdateFunction;
            downloader.OnDownloadOverCallback = OnDownloadOverFunction;
            downloader.OnStartDownloadFileCallback = OnStartDownloadFileFunction;

            bool condition = true;
            UpdateUIManager.Instance.OnUpdateConfirmed += waitOperation;
            void waitOperation(bool isConfirm)
            {
                if (isConfirm)
                {
                    downloader.BeginDownload();
                    StartCoroutine(WaitForDownload());
                }
                else
                {
                    Application.Quit();
                }
            }
            IEnumerator WaitForDownload()
            {
                yield return downloader;
                condition = false;
            }
            while (condition)
            {
                yield return null;
            }
            //检测下载结果
            if (downloader.Status == EOperationStatus.Succeed)
            {
                //下载成功
                Debug.Log("更新完成");
                StartGame();
            }
            else
            {
                //下载失败
                Debug.Log("更新失败");
            }
            // PatchEventDefine.FoundUpdateFiles.SendEventMessage(totalDownloadCount, totalDownloadBytes);

        }
    }
    /// <summary>
    /// 开始下载
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="sizeBytes"></param>
    private void OnStartDownloadFileFunction(string fileName, long sizeBytes)
    {
        Debug.Log(string.Format("开始下载：文件名：{0}，文件大小：{1}", fileName, sizeBytes));
        UpdateUIManager.Instance.UpdateStatus($"开始下载 {fileName}，大小：{sizeBytes / (1024f * 1024f):0.00} MB");
    }

    /// <summary>
    /// 下载完成
    /// </summary>
    /// <param name="isSucceed"></param>
    private void OnDownloadOverFunction(bool isSucceed)
    {
        Debug.Log("下载" + (isSucceed ? "成功" : "失败"));
        UpdateUIManager.Instance.UpdateStatus(isSucceed ? "下载完成！" : "下载失败！");
    }

    /// <summary>
    /// 更新中
    /// </summary>
    /// <param name="totalDownloadCount"></param>
    /// <param name="currentDownloadCount"></param>
    /// <param name="totalDownloadBytes"></param>
    /// <param name="currentDownloadBytes"></param>

    private long lastDownloadedBytes = 0;
    private float lastUpdateTime = 0f;
    private void OnDownloadProgressUpdateFunction(int totalDownloadCount, int currentDownloadCount, long totalDownloadBytes, long currentDownloadBytes)
    {

        Debug.Log(string.Format("文件总数：{0}，已下载文件数：{1}，下载总大小：{2}，已下载大小{3}", totalDownloadCount, currentDownloadCount, totalDownloadBytes, currentDownloadBytes));
        float progress = (float)currentDownloadBytes / totalDownloadBytes;
        float currentTime = Time.time;
        float deltaTime = currentTime - lastUpdateTime;

        if (deltaTime > 0.5f) // 每 0.5 秒更新一次速度
        {
            long bytesDownloadedSinceLastUpdate = currentDownloadBytes - lastDownloadedBytes;
            float speed = bytesDownloadedSinceLastUpdate / deltaTime; // 每秒下载的字节数
            string speedText = FormatSpeed(speed);
            UpdateUIManager.Instance.UpdateSpeed(speedText);

            // 更新上次下载的数据
            lastDownloadedBytes = currentDownloadBytes;
            lastUpdateTime = currentTime;
        }

        // 更新 UI 的进度条和进度信息
        UpdateUIManager.Instance.UpdateProgress(progress, $"下载进度：{currentDownloadCount}/{totalDownloadCount}");
    }
    private string FormatSpeed(float speed)
    {
        if (speed > 1024 * 1024)
            return $"{speed / (1024 * 1024):0.00} MB/s";
        else if (speed > 1024)
            return $"{speed / 1024:0.00} KB/s";
        else
            return $"{speed:0.00} B/s";
    }
    private string FormatStorage(float num)
    {
        if (num > 1024 * 1024)
        {
            return $"{num / (1024 * 1024):0.00} MB";
        }
        else if (num > 1024)
        {
            return $"{num / 1024:0.00} KB";
        }
        return $"{num:0.00} B";
    }
    /// <summary>
    /// 下载出错
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="error"></param>
    private void OnDownloadErrorFunction(string fileName, string error)
    {
        Debug.Log(string.Format("下载出错：文件名：{0}，错误信息：{1}", fileName, error));
    }



    /// <summary>
    /// 远端资源地址查询服务类
    /// </summary>
    private class RemoteServices : IRemoteServices
    {
        private readonly string _defaultHostServer;
        private readonly string _fallbackHostServer;

        public RemoteServices(string defaultHostServer, string fallbackHostServer)
        {
            _defaultHostServer = defaultHostServer;
            _fallbackHostServer = fallbackHostServer;
        }

        string IRemoteServices.GetRemoteMainURL(string fileName)
        {
            return $"{_defaultHostServer}/{fileName}";
        }

        string IRemoteServices.GetRemoteFallbackURL(string fileName)
        {
            return $"{_fallbackHostServer}/{fileName}";
        }

    }
    #endregion

    #region 补充元数据
    /// <summary>
    /// 为aot assembly加载原始metadata， 这个代码放aot或者热更新都行。
    /// 一旦加载后，如果AOT泛型函数对应native实现不存在，则自动替换为解释模式执行
    /// </summary>
    private IEnumerator LoadMetadataForAOTAssemblies()
    {
        HomologousImageMode mode = HomologousImageMode.SuperSet;
        string location = "System.Core.dll";
        AllAssetsHandle handle = package.LoadAllAssetsAsync<UnityEngine.TextAsset>(location);
        yield return handle;
        foreach (var assetObj in handle.AllAssetObjects)
        {
            UnityEngine.TextAsset textAsset = assetObj as UnityEngine.TextAsset;
            LoadImageErrorCode err = RuntimeApi.LoadMetadataForAOTAssembly(textAsset.bytes, mode);
            // Debug.Log($"LoadMetadataForAOTAssembly:{textAsset.name}. mode:{mode} ret:{err}");
        }
        /// 注意，补充元数据是给AOT dll补充元数据，而不是给热更新dll补充元数据。
        /// 热更新dll不缺元数据，不需要补充，如果调用LoadMetadataForAOTAssembly会返回错误
        // 加载assembly对应的dll，会自动为它hook。一旦aot泛型函数的native函数不存在，用解释器版本代码

    }
    #endregion


    #region  更新完毕
    void StartGame()
    {// 加载AOT dll的元数据
        StartCoroutine(LoadMetadataForAOTAssemblies());
        StartCoroutine(LoadHotUpdateDll());
    }
    private IEnumerator LoadHotUpdateDll()
    {
        string location = "HotUpdate.dll";
        AllAssetsHandle handle = package.LoadAllAssetsAsync<UnityEngine.TextAsset>(location);
        yield return handle;
        foreach (var assetObj in handle.AllAssetObjects)
        {
            UnityEngine.TextAsset textAsset = assetObj as UnityEngine.TextAsset;
            Assembly.Load(textAsset.bytes);
        }
        LoadScene();
        // SceneManager.LoadScene("Fight");
    }

    // IEnumerator Run_InstantiateComponentByAsset()
    // {
    //     var package = YooAssets.GetPackage("DefaultPackage");
    //     var handle = package.LoadAssetAsync<GameObject>("EntryPrefab");
    //     yield return handle;
    //     handle.Completed += Handle_Completed;
    // }

    // private void Handle_Completed(AssetHandle obj)
    // {
    //     // Debug.Log("准备实例化");
    //     // GameObject go = obj.InstantiateSync();
    //     // Debug.Log($"Prefab name is {go.name}");
    //     // GameObject prefab = YooAssets.LoadAssetSync("ExchangeBase").AssetObject as GameObject;
    //     // Instantiate(prefab);
    //     // SceneManager.LoadScene("Main");
    //     LoadScene();
    // }
    private void LoadScene()
    {
        string location = "Begin";
        var sceneMode = UnityEngine.SceneManagement.LoadSceneMode.Single;
        // bool suspendLoad = false;
        package.LoadSceneSync(location, sceneMode);
    }
    #endregion
}