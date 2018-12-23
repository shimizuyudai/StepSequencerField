using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class TrimCornerPointView : MonoBehaviour {
    [SerializeField]
    ParticleSystem ps;
    Color selectedColor;
    [SerializeField]
    [Range(0,1)]
    float alpha;
    [SerializeField]
    TrimController trimController;
    private bool isVisible;
    public bool IsVisible
    {
        get {
            return isVisible;
        }
        set {
            ps.Clear();
            isVisible = false;
        }
    }

    Color[] colors;

    private void Awake()
    {
        ps.Stop();
        colors = new Color[4];
        colors[0] = Color.red;
        colors[1] = Color.green;
        colors[2] = Color.blue;
        colors[3] = Color.black;
        for (var i = 0; i < colors.Length; i++)
        {
            colors[i].a = alpha;
        }
        selectedColor = Color.white;
        selectedColor.a = alpha;
    }

    private void Update()
    {
        ps.Clear();
        var points = new List<ParticleSystem.Particle>();
        for(var i = 0; i < trimController.Points.Count; i++)
        {
            var p = new ParticleSystem.Particle();
            p.startColor = i == trimController.SelectedPointIndex ? selectedColor : colors[i];
            p.startSize = trimController.ControlPointSize;
            p.position = trimController.Points[i];
            points.Add(p);
        }
        ps.SetParticles(points.ToArray(), trimController.Points.Count);
    }
}
