using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeserializedTrimView : MonoBehaviour
{
    [SerializeField]
    TrimmedRectDeserializer deserializer;
    Texture2D tex;
    [SerializeField]
    TextureController textureController;
    [SerializeField]
    Renderer renderer;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        renderer.material.mainTexture = deserializer.GetTexture();
    }
}
