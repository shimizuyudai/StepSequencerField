using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureController : MonoBehaviour
{
    public void SecureTexture(Texture refTex, Texture2D tex)
    {
        if (refTex == null) return;
        if (tex != null)
        {
            if (tex.width != refTex.width || tex.height != refTex.height)
            {
                tex.Resize(refTex.width, refTex.height);
            }
        }
        else
        {
            tex = new Texture2D(refTex.width, refTex.height);
        }
    }

    public void SecureTexture(Texture refTex, RenderTexture tex)
    {
        if (refTex == null) return;
        var d = 0;
        if (tex != null)
        {
            d = tex.depth;
            if (tex.width != refTex.width || tex.height != refTex.height)
            {
                DestroyImmediate(tex);
                tex = null;
            }
        }

        if (tex == null)
        {
            tex = new RenderTexture(refTex.width,refTex.height,d);
        }
    }
}
