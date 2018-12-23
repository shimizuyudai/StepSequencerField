using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TrimLineView : MonoBehaviour {
    [SerializeField]
    TrimController trimController;
    [SerializeField]
    Color color;
    [SerializeField]
    Material material;
    public bool IsVisible;

    public void OnRenderObject()
    {
        if (!IsVisible) return;
        if (trimController.Points.Count < 1) return;
        material.SetPass(0);
        GL.PushMatrix();
        GL.MultMatrix(transform.localToWorldMatrix);
        GL.Begin(GL.LINE_STRIP);
        GL.Color(color);
        if (trimController.Points.Count >= 4)
        {
            //var lt = trimController.Points.OrderBy(e => e.x).Take(2).OrderBy(e => e.y).ToArray()[0];
            //var rt = trimController.Points.OrderByDescending(e => e.x).Take(2).OrderBy(e => e.y).ToArray()[0];
            //var rb = trimController.Points.OrderByDescending(e => e.x).Take(2).OrderByDescending(e => e.y).ToArray()[0];
            //var lb = trimController.Points.OrderBy(e => e.x).Take(2).OrderByDescending(e => e.y).ToArray()[0];

            var lt = trimController.Points.OrderBy(e => e.x).Take(2).OrderBy(e => e.y).ToArray()[0];
            var rt = trimController.Points.OrderByDescending(e => e.x).Take(2).OrderBy(e => e.y).ToArray()[0];
            var rb = trimController.Points.OrderByDescending(e => e.x).Take(2).OrderByDescending(e => e.y).ToArray()[0];
            var lb = trimController.Points.OrderBy(e => e.x).Take(2).OrderByDescending(e => e.y).ToArray()[0];

            for (var i = 0; i < 5; i++)
            {
                GL.Vertex(trimController.Points[i%4]);
            }
            //GL.Vertex(lt);
            //GL.Vertex(rt);
            //GL.Vertex(rb);
            //GL.Vertex(lb);
            //GL.Vertex(lt);
        }
        GL.End();
        GL.PopMatrix();
    }
    
}
