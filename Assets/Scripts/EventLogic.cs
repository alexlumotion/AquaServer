using UnityEngine;
using UnityEngine.Video;

public class EventLogic : MonoBehaviour
{
    [SerializeField] private VideoPlayer vpBubble;
    [SerializeField] private VideoPlayer vpCastle;
    [SerializeField] private VideoClip[] bubbles;
    [SerializeField] private VideoClip[] castles;

    public bool testStart = false;
    public void Update()
    {
        if (testStart)
        {
            testStart = false;
            StartPlay();
        }
    }

    private void OnEnable()
    {
        // Підписуємось на мережеву подію — ігноруємо clientId
        NetworkReceiver.OnImageReceived += HandleNetworkImage;
    }

    private void OnDisable()
    {
        NetworkReceiver.OnImageReceived -= HandleNetworkImage;
    }

    private void HandleNetworkImage(string _ignoredClientId, byte[] imageBytes)
    {
        StartPlay();
    }

    void StartPlay()
    {
        VideoClip clipBubles = bubbles[Random.Range(0, bubbles.Length)];
        VideoClip clipCastle = castles[Random.Range(0, castles.Length)];

        vpBubble.Stop();
        vpCastle.Stop();

        vpBubble.clip = clipBubles;
        vpCastle.clip = clipCastle;

        vpBubble.Play();
        vpCastle.Play();
    }

}
