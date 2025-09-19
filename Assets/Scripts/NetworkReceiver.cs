using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;

public class NetworkReceiver : MonoBehaviour
{
    public int port = 5005;
    private TcpListener listener;
    private Queue<Action> mainThreadActions = new Queue<Action>();

    // Єдина подія для передачі даних далі
    public static event Action<string, byte[]> OnImageReceived;

    void Start()
    {
        listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        Debug.Log($"🟢 [SERVER] Слухаємо порт {port}...");
        listener.BeginAcceptTcpClient(OnClientConnected, null);
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
                        OnImageReceived?.Invoke(clientId, imageBytes);
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
    }
}