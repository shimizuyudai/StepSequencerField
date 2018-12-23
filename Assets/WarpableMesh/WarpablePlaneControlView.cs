using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class WarpablePlaneControlView : MonoBehaviour {
    [SerializeField]
    WarpablePlane warpPlane;
    [SerializeField]
    ParticleSystem ps;
    [SerializeField]
    Color color = Color.green, centerColor = Color.red;
    [SerializeField]
    Vector3 adjustPosition;
    [SerializeField]
    bool isUseAltTexture;
    [SerializeField]
    Texture altTexture;

    Texture preTexture;

    private void Awake()
    {
        warpPlane.OnChangeMode += WarpPlane_OnChangeMode;
        ps.Stop();
    }

    private void WarpPlane_OnChangeMode(bool isEnable)
    {
        if (isEnable)
        {
            if (isUseAltTexture)
            {
                preTexture = warpPlane.Renderer.material.mainTexture;
                warpPlane.Renderer.material.mainTexture = altTexture;
            }
        }
        else
        {
            if (isUseAltTexture)
            {
                warpPlane.Renderer.material.mainTexture = preTexture;
            }            if (ps.particleCount > 0)
            {
                ps.Clear();
            }
        }
    }

    private void LateUpdate()
    {
        
        if (warpPlane.IsEnable)
        {
            var particles = new List<ParticleSystem.Particle>();
            foreach (var controlPoint in warpPlane.ControlPoints)
            {
                var p = new ParticleSystem.Particle();
                p.startSize = warpPlane.TouchDistanceThreshold * 2f;
                p.startColor = controlPoint.number == warpPlane.CornerPoints.Length ? centerColor : color;
                p.position = controlPoint.position;
                p.position += adjustPosition;
                particles.Add(p);
            }
            ps.SetParticles(particles.ToArray(), particles.Count);
        }
    }
}
