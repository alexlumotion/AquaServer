using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;
using System.Collections.Generic;
using System.Text;

public class TextureReceiverServer : MonoBehaviour
{
    public int port = 5005;
    private TcpListener listener;
    private Queue<Action> mainThreadActions = new Queue<Action>();

    [SerializeField] private List<Renderer> cubeRenderers;
    private Dictionary<string, Renderer> idToRenderer;
    private Dictionary<string, Texture2D> clientTextures = new Dictionary<string, Texture2D>();
    private HashSet<string> knownClientIds = new HashSet<string>();

    void Start()
    {
        listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        Debug.Log($"🟢 [SERVER] Слухаємо порт {port}...");
        listener.BeginAcceptTcpClient(OnClientConnected, null);

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

    void OnClientConnected(IAsyncResult ar)
    {
        if (listener == null) return;

        try
        {
            using (TcpClient client = listener.EndAcceptTcpClient(ar))
            using (NetworkStream stream = client.GetStream())
            {
                Debug.Log("📡 [SERVER] Клієнт підʼєднався");

                byte[] idLengthBytes = new byte[4];
                stream.Read(idLengthBytes, 0, 4);
                int idLength = BitConverter.ToInt32(idLengthBytes, 0);

                byte[] idBytes = new byte[idLength];
                stream.Read(idBytes, 0, idLength);
                string clientId = Encoding.UTF8.GetString(idBytes);

                if (!knownClientIds.Contains(clientId))
                {
                    Debug.LogWarning($"🚫 [SERVER] Невідомий ID: {clientId}, ігноруємо");
                    return;
                }

                byte[] imageLengthBytes = new byte[4];
                stream.Read(imageLengthBytes, 0, 4);
                int imageLength = BitConverter.ToInt32(imageLengthBytes, 0);

                byte[] imageBytes = new byte[imageLength];
                int read = 0;
                while (read < imageLength)
                    read += stream.Read(imageBytes, read, imageLength - read);

                lock (mainThreadActions)
                {
                    mainThreadActions.Enqueue(() =>
                    {
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
                    });
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ [SERVER] Помилка: {ex.Message}");
        }

        if (listener != null && listener.Server != null && listener.Server.IsBound)
        {
            listener.BeginAcceptTcpClient(OnClientConnected, null);
        }
    }

    void Update()
    {
        lock (mainThreadActions)
        {
            while (mainThreadActions.Count > 0)
                mainThreadActions.Dequeue().Invoke();
        }
    }

    void OnApplicationQuit()
    {
        if (listener != null)
        {
            Debug.Log("[SERVER] Сервер зупинено");
            listener.Stop();
            listener = null;
        }

        foreach (var tex in clientTextures.Values)
        {
            UnityEngine.Object.Destroy(tex);
        }
    }
}
