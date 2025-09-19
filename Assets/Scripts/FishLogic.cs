using UnityEngine;
using System;
using System.Collections.Generic;

public class FishLogic : MonoBehaviour
{
    [SerializeField] private List<FishController> fish = new List<FishController>(12);

    // Подія для TextureApplier: передаємо РЕНДЕРЕР і БАЙТИ
    public static event Action<Renderer, byte[]> OnImageForRenderer;

    private List<FishController> order = new List<FishController>(12);
    private int activeCount = 0;   // скільки вже активовано (макс 12)
    private int ringIndex = 0;     // індекс для кільцевого перевикористання

    private System.Random rng;

    private void OnEnable()
    {
        // Підписуємось на мережеву подію — ігноруємо clientId
        NetworkReceiver.OnImageReceived += HandleNetworkImage;
    }

    private void OnDisable()
    {
        NetworkReceiver.OnImageReceived -= HandleNetworkImage;
    }

    private void Start()
    {
        // Створюємо випадковий порядок (перемішування на старті)
        order.Clear();
        foreach (var f in fish)
            if (f != null) order.Add(f);

        rng = new System.Random();
        for (int i = order.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (order[i], order[j]) = (order[j], order[i]);
        }

        activeCount = 0;
        ringIndex = 0;

        // На старті всі неактивні
        foreach (var f in order) f?.Deactivate();
    }

    private void HandleNetworkImage(string _ignoredClientId, byte[] imageBytes)
    {
        if (order.Count == 0 || imageBytes == null || imageBytes.Length == 0) return;

        FishController target;

        if (activeCount < order.Count)
        {
            // Ще є вільні рибки — активуємо наступну
            target = order[activeCount];
            target.Activate();
            activeCount++;
        }
        else
        {
            // Ліміт досягнуто — кільцеве перевикористання: деактивуємо й знов активуємо
            target = order[ringIndex];
            target.Deactivate();
            target.Activate();
            ringIndex = (ringIndex + 1) % order.Count;
        }

        var rend = target.TargetRenderer;
        if (rend != null)
        {
            OnImageForRenderer?.Invoke(rend, imageBytes);
        }
    }
}