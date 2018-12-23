using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class VirtualStepSequencerField : MonoBehaviour
{
    [SerializeField]
    StepSequencer stepSequencer;
    [SerializeField]
    TextureHolderBase textureHolder;
    [SerializeField]
    ImageFilterGroup filterGroup;

    Texture2D tex;
    RenderTexture rt;
    [SerializeField]
    [Range(0, 1)]
    float rate;

    private void Awake()
    {
        stepSequencer.InitializedEvent += StepSequencer_InitializedEvent;
        stepSequencer.AttackEvent += StepSequencer_AttackEvent;
    }

    private void StepSequencer_AttackEvent(int column, int[] activeElementIds)
    {
        var str = string.Empty;
        foreach (var elm in activeElementIds)
        {
            str += elm + ", ";
        }
        //print(column + " : " + str);
    }

    private void StepSequencer_InitializedEvent()
    {
        //throw new System.NotImplementedException();
    }

    void SecureTextures(Texture texture)
    {
        if (tex != null)
        {
            if (tex.width != texture.width || tex.height != texture.height)
            {
                DestroyImmediate(tex);
                DestroyImmediate(rt);
                tex = null;
                rt = null;
            }
        }


        if (tex == null)
        {
            tex = new Texture2D(texture.width, texture.height);
            rt = new RenderTexture(texture.width, texture.height, 0);
        }
    }

    public void Refresh(Texture texture)
    {
        if (texture == null) return;
        filterGroup.Filter(texture);
        var t = filterGroup.GetTexture();
        SecureTextures(t);
        TextureUtils.Texture2Texture2D(t, tex, rt);
        Refresh(tex);
    }

    public void Refresh(Texture2D texture)
    {
        var size = new TypeUtils.IntVec2(texture.width / stepSequencer.Column, texture.height / stepSequencer.Row);
        var colors = texture.GetPixels();
        var texSize = new TypeUtils.IntVec2(texture.width, texture.height);
        var num = stepSequencer.Column * stepSequencer.Row;
        var threshold = (int)((size.x * size.y) * rate);
        //print(threshold);
        var pixelCounts = new int[num];
        Parallel.For(0, num, i =>
          {
              var x = i % stepSequencer.Column;
              var y = i / stepSequencer.Column;
              var px = x * size.x;
              var py = y * size.y;
              int count = 0;
              for (var cy = 0; cy < size.y; cy++)
              {
                  for (var cx = 0; cx < size.x; cx++)
                  {
                      var index = (px + cx) + texSize.x * (py + cy);
                      var color = colors[index];
                      if (color.r > 0.1f)
                      {
                          count++;
                      }
                  }
              }
              pixelCounts[i] = count;
          });

        for (var i = 0; i < pixelCounts.Length; i++)
        {
            var x = i % stepSequencer.Column;
            var y = i / stepSequencer.Column;
            stepSequencer.SetActiveElemnt(x, y, pixelCounts[i] > threshold);
        }
        //for (var y = 0; y < stepSequencer.Row; y++)
        //{
        //    for (var x = 0; x < stepSequencer.Column; x++)
        //    {
        //        var px = x * size.x;
        //        var py = y * size.y;
        //        for (var cy = 0; cy < size.y; cy++)
        //        {
        //            for (var cx = 0; cx < size.y; cx++)
        //            {
        //                var i = (px + cx) + texSize.x * (py + cy);
        //                var color = colors[i];
        //            }
        //        }
        //        //Parallel.For(0, size.x, cy =>
        //        //{
        //        //    Parallel.For(0, size.y, cx =>
        //        //    {
        //        //        var i = (px + cx) + texSize.x * (py + cy);
        //        //        var color = colors[i];
        //        //    });
        //        //});
        //    }
        //}
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Refresh(textureHolder.GetTexture());
    }
}
