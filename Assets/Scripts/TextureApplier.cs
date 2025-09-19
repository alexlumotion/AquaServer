using UnityEngine;
using System;
using System.Collections.Generic;

public class TextureApplier : MonoBehaviour
{
    [SerializeField] private List<Renderer> cubeRenderers;
    private Dictionary<string, Renderer> idToRenderer;
    private Dictionary<string, Texture2D> clientTextures = new Dictionary<string, Texture2D>();
    private HashSet<string> knownClientIds = new HashSet<string>();

    void OnEnable()
    {
        NetworkReceiver.OnImageReceived += HandleImage;
    }

    void OnDisable()
    {
        NetworkReceiver.OnImageReceived -= HandleImage;
    }

    void Start()
    {
        idToRenderer = new Dictionary<string, Renderer>()
        {
            { "client1", cubeRenderers[0] },
            { "client2", cubeRenderers[1] },
            { "client3", cubeRenderers[2] },
            { "client4", cubeRenderers[3] },
            { "client5", cubeRenderers[4] }
        };

        foreach (var clientId in idToRenderer.Keys)
        {
            clientTextures[clientId] = new Texture2D(2048, 1024, TextureFormat.RGBA32, false);
            knownClientIds.Add(clientId);
        }
    }

    private void HandleImage(string clientId, byte[] imageBytes)
    {
        if (!knownClientIds.Contains(clientId))
        {
            Debug.LogWarning($"🚫 [SERVER] Невідомий ID: {clientId}, ігноруємо");
            return;
        }

        Texture2D tex = clientTextures[clientId];
        bool loaded = tex.LoadImage(imageBytes, true);
        if (loaded)
        {
            idToRenderer[clientId].sharedMaterial.mainTexture = tex;
            Debug.Log($"✅ [SERVER] Текстура оновлена для {clientId}");
        }
        else
        {
            Debug.LogWarning("⚠️ [SERVER] Помилка при завантаженні зображення");
        }
    }

    void OnApplicationQuit()
    {
        foreach (var tex in clientTextures.Values)
        {
            UnityEngine.Object.Destroy(tex);
        }
    }
}