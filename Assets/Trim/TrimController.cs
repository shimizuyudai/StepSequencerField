using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using TrimmingUtils;
using System.Linq;
using TypeUtils.Json;

public class TrimController : TextureHolderBase
{
    [Header("左上から時計回りに頂点を選択")]
    [SerializeField]
    Camera camera;
    public Camera Camera
    {
        get {
            return this.camera;
        }
    }
    [SerializeField]
    KeyCode removePointKey, clearPointKey, saveFileKey;
    [SerializeField]
    CaptureDeviceTextureHolder captureDevice;
    [SerializeField]
    string settingsFileName;
    [SerializeField]
    float controlPointSize;

    [SerializeField]
    OrthoCameraController orthoCameraController;

    [SerializeField]
    Renderer renderer;

    int rotateCount;
    [SerializeField]
    KeyCode leftRotKey, rightRotKey;

    float orthograhicSize;

    float moveSpeed;
    float zoomSpeed;

    public override Texture GetTexture()
    {
        return base.GetTexture();
    }

    public float ControlPointSize
    {
        get {
            return controlPointSize;
        }
    }

    public int SelectedPointIndex
    {
        get;
        private set;
    }


    public int DragPointIndex
    {
        get;
        private set;
    }

    public List<Vector3> Points
    {
        get;
        private set;
    }

    private void Awake()
    {
        Points = new List<Vector3>();
        moveSpeed = orthoCameraController.moveSpeed;
        zoomSpeed = orthoCameraController.zoomSpeed;

        captureDevice.OnAvailable += CaptureDevice_OnAvailable;
        camera.orthographic = true;
        Init();
        LoadSettings();
        
    }

    //復元
    void LoadSettings()
    {
        var info = IOHandler.LoadJson<TrimmingInfo>(IOHandler.IntoStreamingAssets(settingsFileName));
        if (info == null) return;
        renderer.transform.localScale = TypeUtils.Json.Convert.Vec3ToVector3(info.Scale);
        this.rotateCount = info.RotateCount;
        renderer.transform.localEulerAngles = new Vector3(0, 0, -90f * info.RotateCount);
        camera.orthographicSize = info.OrthographicSize;
        orthograhicSize = info.OrthographicSize;
        orthoCameraController.defaultSize = orthograhicSize;
        orthoCameraController.moveSpeed = moveSpeed / orthograhicSize;
        orthoCameraController.zoomSpeed = zoomSpeed / orthograhicSize;
        Points = new List<Vector3>();
        for (var i = 0; i < info.Points.Count; i++)
        {
            Points.Add(info.Points[i].position.ToVector3());
        }
        //Points.Add(info.LeftTopPoint.position.ToVector3());
        //Points.Add(info.RightTopPoint.position.ToVector3());
        //Points.Add(info.RightBottomPoint.position.ToVector3());
        //Points.Add(info.LeftBottomPoint.position.ToVector3());
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
        var normalizedRendererSize = EMath.GetShrinkFitSize(texSize, screenSize.normalized);
        camera.orthographicSize = screenSize.normalized.y / 2f;
        orthograhicSize = screenSize.normalized.y / 2f;
        orthoCameraController.defaultSize = orthograhicSize;
        orthoCameraController.moveSpeed = moveSpeed / orthograhicSize;
        orthoCameraController.zoomSpeed = zoomSpeed / orthograhicSize;
        renderer.transform.localScale = new Vector3(normalizedRendererSize.x, normalizedRendererSize.y, 1f);
        renderer.material.mainTexture = texture;
    }

    // Update is called once per frame
    void Update()
    {
        UpdatePoints();
        //rotate();
        if (Input.GetKeyDown(saveFileKey))
        {
            Save();
        }
    }

    void rotate()
    {
        var keyPressed = false;
        if (Input.GetKeyDown(rightRotKey))
        {
            rotateCount++;
            keyPressed = true;
        }
        else if (Input.GetKeyDown(leftRotKey))
        {
            rotateCount--;
            keyPressed = true;
        }

        if (!keyPressed) return;

        rotateCount = (rotateCount + 4) % 4;
        renderer.transform.localEulerAngles = new Vector3(0f, 0f, -90f * rotateCount);
        var texSize = rotateCount % 2 == 0 ? new Vector2(renderer.material.mainTexture.width, renderer.material.mainTexture.height) : new Vector2(renderer.material.mainTexture.height, renderer.material.mainTexture.width);
        var screenSize = new Vector2(Screen.width, Screen.height);
        var normalizedRendererSize = EMath.GetShrinkFitSize(texSize, screenSize.normalized);
        renderer.transform.localScale = rotateCount % 2 == 0 ? new Vector3(normalizedRendererSize.x, normalizedRendererSize.y, 1f) : new Vector3(normalizedRendererSize.y, normalizedRendererSize.x, 1f);
    }

    public void Save()
    {
        if (Points.Count < 4) return;
        var info = createTrimmingInfo();
        var path = IOHandler.IntoStreamingAssets(settingsFileName);
        IOHandler.SaveJson(path, info);
    }

    TrimmingInfo createTrimmingInfo()
    {
        var trimmingInfo = new TrimmingInfo();
        //var ltp = this.Points.OrderBy(e => e.x).Take(2).OrderBy(e => e.y).ToArray()[0];
        //var rtp = this.Points.OrderByDescending(e => e.x).Take(2).OrderBy(e => e.y).ToArray()[0];
        //var rbp = this.Points.OrderByDescending(e => e.x).Take(2).OrderByDescending(e => e.y).ToArray()[0];
        //var lbp = this.Points.OrderBy(e => e.x).Take(2).OrderByDescending(e => e.y).ToArray()[0];

        var area = new Area(renderer.transform.position.x - renderer.transform.localScale.x / 2f,
            renderer.transform.position.y - renderer.transform.localScale.y / 2f,
            renderer.transform.localScale.x, renderer.transform.localScale.y
            );

        //var ltuv = new Vector2(EMath.Map(ltp.x, area.x, area.x + area.width, 0f, 1f), EMath.Map(ltp.y, area.y, area.y + area.height, 0f, 1f));
        //var rtuv = new Vector2(EMath.Map(rtp.x, area.x, area.x + area.width, 0f, 1f), EMath.Map(rtp.y, area.y, area.y + area.height, 0f, 1f));
        //var rbuv = new Vector2(EMath.Map(rbp.x, area.x, area.x + area.width, 0f, 1f), EMath.Map(rbp.y, area.y, area.y + area.height, 0f, 1f));
        //var lbuv = new Vector2(EMath.Map(lbp.x, area.x, area.x + area.width, 0f, 1f), EMath.Map(lbp.y, area.y, area.y + area.height, 0f, 1f));

        //var lt = new WarpablePlane.PointInfomation() { position = ltp, uv = ltuv };
        //var rt = new WarpablePlane.PointInfomation() { position = rtp, uv = rtuv };
        //var rb = new WarpablePlane.PointInfomation() { position = rbp, uv = rbuv };
        //var lb = new WarpablePlane.PointInfomation() { position = lbp, uv = lbuv };

        var trimPoints = new List<TrimPointInfo>();
        for (var i = 0; i < Points.Count; i++)
        {
            var p = Points[i];
            var uv = new Vector2(EMath.Map(p.x, area.x, area.x + area.width, 0f, 1f), EMath.Map(p.y, area.y, area.y + area.height, 0f, 1f));
            var pointInfo = new TrimPointInfo() { position = Convert.Vector3ToVec3(p), uv = Convert.Vector2ToVec2(uv) };
            trimPoints.Add(pointInfo);
        }

        //trimmingInfo.LeftTopPoint = new TrimPointInfo() { position = Convert.Vector3ToVec3(ltp), uv = Convert.Vector2ToVec2(ltuv) };
        //trimmingInfo.RightTopPoint = new TrimPointInfo() { position = Convert.Vector3ToVec3(rtp), uv = Convert.Vector2ToVec2(rtuv) };
        //trimmingInfo.RightBottomPoint = new TrimPointInfo() { position = Convert.Vector3ToVec3(rbp), uv = Convert.Vector2ToVec2(rbuv) };
        //trimmingInfo.LeftBottomPoint = new TrimPointInfo() { position = Convert.Vector3ToVec3(lbp), uv = Convert.Vector2ToVec2(lbuv) };
        trimmingInfo.Points = trimPoints;
        trimmingInfo.OrthographicSize = orthograhicSize;
        trimmingInfo.Scale = Convert.Vector3ToVec3(renderer.transform.localScale);
        trimmingInfo.RotateCount = 0;
        return trimmingInfo;
    }

    void UpdatePoints()
    {
        if (Input.GetKeyDown(clearPointKey))
        {
            Points = new List<Vector3>();
            return;
        }
        var worldMousePos = camera.ScreenToWorldPoint(Input.mousePosition);
        worldMousePos.z = this.transform.position.z;
        var isInnerTexArea = false;
        if (worldMousePos.x < renderer.transform.position.x + renderer.transform.localScale.x / 2f && worldMousePos.x > renderer.transform.position.x - renderer.transform.localScale.x / 2f)
        {
            if (worldMousePos.y < renderer.transform.position.y + renderer.transform.localScale.y / 2f && worldMousePos.y > renderer.transform.position.y - renderer.transform.localScale.y / 2f)
            {
                isInnerTexArea = true;
            }
        }

        if (Input.GetKeyDown(removePointKey))
        {
            if (SelectedPointIndex >= 0)
            {
                Points.RemoveAt(SelectedPointIndex);
                SelectedPointIndex = -1;
            }
        }

        if (!isInnerTexArea)
        {
            if (Input.GetMouseButtonDown(0))
            {
                SelectedPointIndex = -1;
                DragPointIndex = -1;
                return;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            DragPointIndex = -1;
        }

        //print(worldMousePos);
        if (Input.GetMouseButtonDown(0))
        {

            var d = controlPointSize;
            DragPointIndex = -1;
            SelectedPointIndex = -1;
            for (var i = 0; i < Points.Count; i++)
            {
                var dist = Vector2.Distance(new Vector2(Points[i].x, Points[i].y), new Vector2(worldMousePos.x, worldMousePos.y));
                if (dist < d)
                {
                    SelectedPointIndex = i;
                    DragPointIndex = i;
                    break;
                }
            }

            if (SelectedPointIndex < 0)
            {
                if (Points.Count < 4)
                {
                    Points.Add(worldMousePos);
                    print("add : " + worldMousePos);
                }
            }
        }

        if (Input.GetMouseButton(0))
        {
            if (DragPointIndex >= 0)
            {
                Points[DragPointIndex] = worldMousePos;
            }
        }


    }

    public struct ControlPoint
    {
        public Vector3 Position;
        public bool IsSelected;
    }
}
