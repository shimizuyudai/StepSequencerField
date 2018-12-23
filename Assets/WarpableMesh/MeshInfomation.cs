using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TypeUtils.Json;

public class MeshInfomation {
    public List<Vec3> Vertices;
    public List<Vec2> Uv;
    public List<int> Triangles;

    public MeshInfomation()
    {

    }

    public MeshInfomation(Mesh mesh)
    {
        Vertices = new List<Vec3>();
        Uv = new List<Vec2>();
        Triangles = new List<int>();
        for (var i = 0; i < mesh.vertices.Length; i++)
        {
            Vertices.Add(Convert.Vector3ToVec3(mesh.vertices[i]));
        }

        for (var i = 0; i < mesh.uv.Length; i++)
        {
            Uv.Add(Convert.Vector2ToVec2(mesh.uv[i]));
        }

        for (var i = 0; i < mesh.triangles.Length; i++)
        {
            Triangles.Add(mesh.triangles[i]);
        }
    }

    public Mesh ToMesh()
    {
        var mesh = new Mesh();
        var vertices = new List<Vector3>();
        for (var i = 0; i < Vertices.Count; i++)
        {
            vertices.Add(Convert.Vec3ToVector3(Vertices[i]));
        }
        mesh.vertices = vertices.ToArray();

        var uv = new List<Vector2>();
        for (var i = 0; i < Vertices.Count; i++)
        {
            uv.Add(Convert.Vec2ToVector2(Uv[i]));
        }
        mesh.uv = uv.ToArray();

        var triangles = new List<int>();
        for (var i = 0; i < Triangles.Count; i++)
        {
            triangles.Add(Triangles[i]);
        }
        mesh.triangles = triangles.ToArray();

        mesh.RecalculateNormals();

        return mesh;


    }
}
