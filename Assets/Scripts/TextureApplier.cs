using UnityEngine;
using System;
using System.Collections.Generic;

public class TextureApplier : MonoBehaviour
{
    // Кеш текстур по конкретному Renderer
    private readonly Dictionary<Renderer, Texture2D> texByRenderer = new Dictionary<Renderer, Texture2D>();

    private void OnEnable()
    {
        FishLogic.OnImageForRenderer += HandleImageForRenderer;
    }

    private void OnDisable()
    {
        FishLogic.OnImageForRenderer -= HandleImageForRenderer;
    }

    private void HandleImageForRenderer(Renderer rend, byte[] imageBytes)
    {
        if (rend == null || imageBytes == null || imageBytes.Length == 0) return;

        if (!texByRenderer.TryGetValue(rend, out var tex) || tex == null)
        {
            tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            texByRenderer[rend] = tex;
        }

        if (tex.LoadImage(imageBytes, true))
        {
            var mat = rend.sharedMaterial;
            if (mat != null)
            {
                if (mat.HasProperty("_BaseMap"))      mat.SetTexture("_BaseMap", tex);
                else if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", tex);
                else                                   mat.mainTexture = tex;
            }
        }
    }

    private void OnApplicationQuit()
    {
        foreach (var kv in texByRenderer)
            if (kv.Value != null) Destroy(kv.Value);
        texByRenderer.Clear();
    }
}