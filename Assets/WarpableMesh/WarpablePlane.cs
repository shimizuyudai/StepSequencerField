using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using OpenCVForUnity;
using System.Linq;
using System.IO;
using Newtonsoft.Json;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]

public class WarpablePlane : MonoBehaviour
{
    MeshCollider meshCollider;
    [SerializeField]
    bool isStandalone;

    [SerializeField]
    KeyCode toggleKey, exportKey, resetKey;
    [SerializeField]
    string fileName;
    bool isEnable;
    public event System.Action<bool> OnChangeMode;



    public bool IsEnable
    {
        get {
            return this.isEnable;
        }
        private set {
            if (isEnable != value)
            {
                if (this.OnChangeMode != null) OnChangeMode(value);
            }
            this.isEnable = value;
        }
    }

    [SerializeField]
    Camera cam;


    public Vector2 size;
    public int SegmentX, SegmentY;
    private ControlPoint[] defaultCornerPoints;
    //左上、右上、左下、右下
    public ControlPoint[] CornerPoints
    {
        get;
        private set;
    }

    public ControlPoint[] ControlPoints;

    private Vector3[] defaultVertices;

    private int selectedNumber;

    [SerializeField]
    float touchDistanceThreshold;
    public float TouchDistanceThreshold
    {
        get {
            return touchDistanceThreshold;
        }
    }

    public Mesh Mesh { get { return meshFilter.mesh; } }

    public bool IsWarp
    {
        get;
        set;
    }

    public bool IsVisible
    {
        get {
            return renderer.enabled;
        }
        set {
            renderer.enabled = value;
        }
    }


    [SerializeField]
    Renderer renderer;
    public Renderer Renderer
    {
        get {
            return renderer;
        }
    }
    [SerializeField]
    MeshFilter meshFilter;
    public MeshFilter MeshFilter
    {
        get {
            return meshFilter;
        }
    }

    Vector3 preMousePosition;
    public bool HasInitialized
    {
        get;
        private set;
    }


    [SerializeField]
    bool isAutoLoad;

    private void Awake()
    {
        if (this.meshFilter == null)
        {
            this.meshFilter = GetComponent<MeshFilter>();
        }
        if (this.renderer == null)
        {
            this.renderer = GetComponent<Renderer>();
        }

        if (isStandalone)
        {
            Init(Vector3.zero, this.size, SegmentX, SegmentY);
            if (isAutoLoad)
            {
                LoadSettings(fileName);
            }
        }

    }

    void Start()
    {

        preMousePosition = Input.mousePosition;
    }

    public void Init(Mesh mesh)
    {
        this.meshFilter.mesh = mesh;
        mesh.RecalculateNormals();
        HasInitialized = true;
        refresh();
    }

    public void Init(PointInfomation leftTop, PointInfomation rightTop, PointInfomation rightBottom, PointInfomation leftBottom, int segmentX, int segmentY)
    {
        var points = new PointInfomation[]
        {
            leftTop,rightTop,rightBottom,leftBottom
        };
        var mesh = new Mesh();
        var vertices = new List<Vector3>();
        var uvs = new List<Vector2>();
        for (var y = 0; y < segmentY; y++)
        {
            for (var x = 0; x < segmentX; x++)
            {
                var p = new Vector2((float)x / (float)(segmentX - 1), 1f - (float)y / (float)(segmentY - 1));
                var pos = leftTop.position * (1f - p.x) * (1f - p.y) + rightTop.position * (p.x) * (1f - p.y)
                    + rightBottom.position * (p.x) * (p.y) + leftBottom.position * (1f - p.x) * (p.y);
                var uv = leftTop.uv * (1f - p.x) * (1f - p.y) + rightTop.uv * (p.x) * (1f - p.y)
                    + rightBottom.uv * (p.x) * (p.y) + leftBottom.uv * (1f - p.x) * (p.y);
                uv.y = 1 - uv.y;
                //print(uv);
                vertices.Add(pos);
                uvs.Add(uv);
            }
        }
        defaultVertices = vertices.ToArray();



        var lt = vertices[0];
        var rt = vertices[segmentX - 1];
        var rb = vertices[segmentX * (segmentY - 1) + segmentX - 1];
        var lb = vertices[segmentX * (segmentY - 1)];
        var center = (lt + rt + rb + lb) / 4f;

        CornerPoints = new ControlPoint[] {
            new ControlPoint (0, lt),
            new ControlPoint (1, rt),
            new ControlPoint (2, rb),
            new ControlPoint (3, lb)
        };


        ControlPoints = new ControlPoint[] {
            new ControlPoint(0, lt),
            new ControlPoint(1, rt),
            new ControlPoint(2, rb),
            new ControlPoint(3, lb),
            new ControlPoint(4, center)
        };

        defaultCornerPoints = new ControlPoint[] {
            new ControlPoint (0, lt),
            new ControlPoint (1, rt),
            new ControlPoint (2, rb),
            new ControlPoint (3, lb)
        };

        mesh.vertices = vertices.ToArray();
        mesh.uv = uvs.ToArray();

        var triangles = new List<int>();
        for (var y = 0; y < segmentY - 1; y++)
        {
            for (var x = 0; x < segmentX - 1; x++)
            {
                if (x + 1 < segmentX && y + 1 < segmentY)
                {
                    var index = x + (y * segmentX);
                    triangles.Add(index);
                    triangles.Add(index + segmentX);
                    triangles.Add(index + 1);

                    triangles.Add(index + segmentX);
                    triangles.Add(index + segmentX + 1);
                    triangles.Add(index + 1);

                }
            }
        }
        mesh.triangles = triangles.ToArray();

        Init(mesh);
    }

    public void Init(Vector3 position, Vector2 size, int segmentX, int segmentY, Camera camera)
    {
        this.cam = camera;
        Init(position, size, segmentX, segmentY);
    }

    public void Init(Vector3 position, Vector2 size, int segmentX, int segmentY)
    {
        var mesh = new Mesh();
        var startPos = position + new Vector3(-size.x / 2.0f, size.y / 2.0f);
        var leftTop = new PointInfomation
        {
            position = new Vector3(position.x - size.x / 2, position.y + size.y / 2f, position.z),
            uv = new Vector2(0f, 0f)
        };
        var rightTop = new PointInfomation
        {
            position = new Vector3(position.x + size.x / 2, position.y + size.y / 2f, position.z),
            uv = new Vector2(1f, 0f)
        };
        var rightBottom = new PointInfomation
        {
            position = new Vector3(position.x + size.x / 2, position.y - size.y / 2f, position.z),
            uv = new Vector2(1f, 1f)
        };
        var leftBottom = new PointInfomation
        {
            position = new Vector3(position.x - size.x / 2, position.y - size.y / 2f, position.z),
            uv = new Vector2(0f, 1f)
        };
        Init(leftTop, rightTop, rightBottom, leftBottom, segmentX, segmentY);
    }

    public void Reset()
    {
        if (this.MeshFilter.mesh == null) return;
        if (CornerPoints == null || defaultCornerPoints == null) return;
        for (var i = 0; i < defaultCornerPoints.Length; i++)
        {
            CornerPoints[i].position = defaultCornerPoints[i].position;
        }
        refresh();
    }

    public void UpdateMesh()
    {
        meshFilter.mesh.RecalculateBounds();
        meshFilter.mesh.RecalculateNormals();
    }


    public void LoadSettings(string fileName)
    {
        var path = Path.Combine(Application.streamingAssetsPath, fileName);
        if (File.Exists(path))
        {
            var json = File.ReadAllText(path);
            var corners = JsonConvert.DeserializeObject<CornerPointInfomation[]>(json);
            Restore(corners);
        }
    }

    public void Restore(CornerPointInfomation[] corners)
    {
        foreach (var corner in corners)
        {
            var selectedCornerPoint = CornerPoints.FirstOrDefault(e => e.number == corner.number);
            if (selectedCornerPoint != null)
            {
                selectedCornerPoint.position = new Vector2(corner.position.x, corner.position.y);
            }
        }
        refresh();
    }

    public void Save(string fileName)
    {
        var json = GetJson();
        var path = Path.Combine(Application.streamingAssetsPath, fileName);
        File.WriteAllText(path, json);
    }

    public string GetJson()
    {
        var json = JsonConvert.SerializeObject(CornerPointInfomations);
        return json;
    }

    public CornerPointInfomation[] CornerPointInfomations
    {
        get {
            return CornerPoints.Select(e => new CornerPointInfomation(e.number, new TypeUtils.Json.Vec2(e.position.x, e.position.y))).ToArray();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!isStandalone) return;
        if (!HasInitialized) return;

        if (Input.GetKeyDown(resetKey))
        {
            Reset();
        }

        if (Input.GetKeyDown(toggleKey))
        {
            IsEnable = !IsEnable;
        }

        if (Input.GetKeyDown(exportKey))
        {
            Save(fileName);
        }

        var worldMousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        worldMousePos.z = 0f;

        if (IsEnable)
        {
            //---自身で変形する---
            if (Input.GetMouseButtonDown(0))
            {
                if (!IsWarp)
                {
                    this.IsWarp = OnPointerDown(worldMousePos);
                }
            }
            else if (Input.GetMouseButtonUp(0))
            {
                if (IsWarp)
                {
                    OnPointerUp();
                }
            }

            if (IsWarp)
            {
                Warp(worldMousePos - preMousePosition);
            }
        }


        preMousePosition = worldMousePos;
    }

    public void OnPointerUp()
    {
        IsWarp = false;
    }

    public bool OnPointerDown(Vector3 pos)
    {
        var isTouch = false;
        var orderedControlPoints = ControlPoints.OrderBy(e => Vector3.Distance(e.position, pos));
        if (Vector2.Distance(orderedControlPoints.First().position, pos) < touchDistanceThreshold)
        {
            selectedNumber = orderedControlPoints.First().number;
            isTouch = true;
        }
        return isTouch;
    }

    public void Warp(Vector2 velocity)
    {
        //頂点の移動
        var selectedPoint = ControlPoints.FirstOrDefault(e => e.number == selectedNumber);
        if (selectedPoint != null)
        {
            if (selectedPoint != null)
            {
                if (selectedPoint.number == CornerPoints.Length)
                {
                    foreach (var cornerPoint in CornerPoints)
                    {
                        cornerPoint.position += velocity;
                    }
                }
                else
                {
                    var selectedCornerPoint = CornerPoints.FirstOrDefault(e => e.number == selectedNumber);
                    if (selectedCornerPoint != null)
                    {
                        selectedCornerPoint.position += velocity;
                    }
                }
            }
        }
        refresh();
    }

    private void refresh()
    {
        var defaultPoints = defaultCornerPoints.Select(e => e.point).ToArray();
        var destPoints = CornerPoints.Select(e => e.point).ToArray();
        using (var defaultCornerMat = new MatOfPoint2f(defaultPoints))
        using (var destCornerMat = new MatOfPoint2f(destPoints))
        using (var defaultMat = new MatOfPoint2f(defaultVertices.Select(e => new Point(e.x, e.y)).ToArray()))
        using (var destMat = new MatOfPoint2f(meshFilter.mesh.vertices.Select(e => new Point(e.x, e.y)).ToArray()))
        {
            var h = Calib3d.findHomography(defaultCornerMat, destCornerMat);
            OpenCVForUnity.Core.perspectiveTransform(defaultMat, destMat, h);
            var vertices = destMat.toList().Select(e => new Vector3((float)e.x, (float)e.y, 0f)).ToList();//resultPoints.Select (e => new Vector3((float)e.x,(float)e.y,0f)).ToList();
            meshFilter.mesh.SetVertices(vertices);
        }
        var centerPoint = ControlPoints.FirstOrDefault(e => e.number == CornerPoints.Length);
        var centerPos = Vector2.zero;
        for (var i = 0; i < CornerPoints.Length; i++)
        {
            var cornerPoint = CornerPoints[i];
            ControlPoints[i].position = cornerPoint.position;
            centerPos += cornerPoint.position;
        }

        centerPoint.position = centerPos / (float)CornerPoints.Length;


    }

    //左上、右上、右下、左下
    public void Refresh(Vector2 leftTop, Vector2 rightTop, Vector2 leftBottom, Vector2 rightBottom)
    {
        var positions = new List<Vector2>()
        {
            leftTop,rightTop,leftBottom,rightBottom
        };
        for (var i = 0; i < CornerPoints.Length; i++)
        {
            CornerPoints[i].position = positions[i];
        }
        refresh();
    }

    public void ClearSettings()
    {
        var path = Path.Combine(Application.streamingAssetsPath, fileName);
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    public class ControlPoint
    {
        public Vector2 position;
        public int number;

        public Point point
        {
            get {
                return new Point() { x = this.position.x, y = this.position.y };
            }
        }

        public ControlPoint(int number, Vector2 position)
        {
            this.number = number;
            this.position = position;
        }
    }


    public class CornerPointInfomation
    {
        public int index;
        public int number;
        public TypeUtils.Json.Vec2 position;

        public CornerPointInfomation(int number, TypeUtils.Json.Vec2 position)
        {
            this.number = number;
            this.position = position;
        }
    }

    public class PointInfomation
    {
        public Vector3 position;
        public Vector2 uv;
    }
}




