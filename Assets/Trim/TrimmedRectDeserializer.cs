using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrimmedRectDeserializer : TextureHolderBase
{
    [SerializeField]
    string settingsFileName;
    [SerializeField]
    WarpablePlane warpablePlane;
    [SerializeField]
    CaptureDeviceTextureHolder captureDevice;
    [SerializeField]
    TypeUtils.IntVec2 resolution;

    public override Texture GetTexture()
    {
        return result;
    }

    //一度カメラで撮影（元の状態）してからWarpablePlaneで補正する
    //CaptureCamera:Quadの撮影, TrimCamera:補正済みのWarpablePlaneの撮影
    [SerializeField]
    Camera captureCamera;
    RenderTexture result;

    [Header("Trimming時と同じ状態を復元する")]
    [SerializeField]
    bool isDebug;

    private void Awake()
    {
        captureDevice.OnAvailable += CaptureDevice_OnAvailable;
        captureCamera.orthographic = true;
        result = new RenderTexture(resolution.x, resolution.y, 0);
    }

    private void CaptureDevice_OnAvailable()
    {
        Init();
    }

    void Init()
    {
        var texture = captureDevice.GetTexture();
        if (texture == null) return;
        var texSize = new Vector2(texture.width, texture.height);
        var screenSize = new Vector2(Screen.width, Screen.height);
        var info = IOHandler.LoadJson<TrimmingUtils.TrimmingInfo>(IOHandler.IntoStreamingAssets(settingsFileName));
        if (info == null) return;
        var lt = info.Points[0].ToPointInfomation();
        var rt = info.Points[1].ToPointInfomation();
        var rb = info.Points[2].ToPointInfomation();
        var lb = info.Points[3].ToPointInfomation();
        
        warpablePlane.Init(lt, rt, rb, lb, 10, 10);

        
        var t = (float)(resolution.x + resolution.y);
        
        var normalizedResolution = new Vector2(resolution.x / t, resolution.y / t);
        var fitSize = EMath.GetShrinkFitSize(normalizedResolution, screenSize.normalized);
        print(normalizedResolution);
        var leftTop = new Vector2(-fitSize.x / 2f, -fitSize.y / 2f);
        var rightTop = new Vector2(fitSize.x / 2f, -fitSize.y / 2f);
        var rightBottom = new Vector2(fitSize.x / 2f, fitSize.y / 2f);
        var leftBottom = new Vector2(-fitSize.x / 2f, fitSize.y / 2f);
        if (!isDebug)
        {
            warpablePlane.Refresh(leftTop, rightTop, rightBottom, leftBottom);
            captureCamera.targetTexture = result;
        }
        
        warpablePlane.Renderer.material.mainTexture = texture;
        
        captureCamera.orthographicSize = !isDebug ? screenSize.normalized.y/2 : info.OrthographicSize;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
