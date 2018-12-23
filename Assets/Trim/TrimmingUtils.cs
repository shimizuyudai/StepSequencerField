using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TypeUtils.Json;

namespace TrimmingUtils
{
    class Area
    {
        public float x, y, width, height;
        public Area(float x, float y, float width, float height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }

    }

    public class TrimmingInfo
    {
        public List<TrimPointInfo> Points;
        public float OrthographicSize;
        public int RotateCount;//Quadの回転数(90度単位)
        public Vec3 Scale;//Quadのスケール
    }

    public class DeserializeTrimSetting
    {

    }

    public class TrimPointInfo
    {
        public Vec3 position;
        public Vec2 uv;

        public WarpablePlane.PointInfomation ToPointInfomation()
        {
            return new WarpablePlane.PointInfomation { position = Convert.Vec3ToVector3(position), uv = Convert.Vec2ToVector2(uv) };
        }
    }

    //public class TrimmingSettings
    //{
    //    public Vec2 Size;
    //    public Vec3 Scale;
    //    public MeshInfomation MeshInfomation;
    //    public int RotateCount;
    //}
}