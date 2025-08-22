using UnityEngine;

[RequireComponent(typeof(Camera))]
public class TexturePainter : MonoBehaviour
{
    [Header("Ціль для малювання")]
    public Renderer targetRenderer;               // Обʼєкт, на якому малюємо
    public Texture2D originalTexture;             // Оригінал (readonly)
    private Texture2D textureToPaint;             // Клон, на якому малюємо

    [Header("Налаштування пензля")]
    public Color brushColor = Color.red;
    [Range(1, 64)]
    public int brushSize = 8;

    [Header("Параметри")]
    public string paintableTag = "Paintable";

    private Camera mainCamera;

    void Start()
    {
        mainCamera = GetComponent<Camera>();
        InitChecks();
        CloneTexture();
        AssignTextureToMaterial();
        EnsureMeshCollider();
        //DrawTestArea(); // можна увімкнути для тесту
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Debug.Log("🖱 Натиснута ліва кнопка миші");
            TryPaintUnderCursor();
        }
    }

    // 🧩 Перевірка вхідних параметрів
    void InitChecks()
    {
        if (mainCamera == null)
            Debug.LogError("🚫 TexturePainter: Камера не знайдена.");

        if (originalTexture == null)
            Debug.LogError("🚫 TexturePainter: Не вказана originalTexture.");

        if (targetRenderer == null)
            Debug.LogError("🚫 TexturePainter: Не вказано Renderer.");
    }

    // 🧬 Клонування текстури
    void CloneTexture()
    {
        textureToPaint = new Texture2D(
            originalTexture.width,
            originalTexture.height,
            TextureFormat.RGBA32,
            false
        );

        textureToPaint.SetPixels(originalTexture.GetPixels());
        textureToPaint.Apply();

        Debug.Log($"🧬 Створено клон текстури '{originalTexture.name}' → '{textureToPaint.name}'");
    }

    // 🎨 Привʼязка клону до матеріалу
    void AssignTextureToMaterial()
    {
        targetRenderer.material.mainTexture = textureToPaint;
        Debug.Log("✅ Привʼязано textureToPaint до material.mainTexture");
    }

    // 🧩 MeshCollider setup
    void EnsureMeshCollider()
    {
        MeshCollider meshCol = targetRenderer.GetComponent<MeshCollider>();
        MeshFilter meshFilt = targetRenderer.GetComponent<MeshFilter>();

        if (meshCol != null && meshFilt != null && meshCol.sharedMesh == null)
        {
            meshCol.sharedMesh = meshFilt.sharedMesh;
            Debug.Log("🧩 MeshCollider.mesh встановлено з MeshFilter.sharedMesh");
        }
    }

    // 🎯 Обробка кліку миші та малювання
    void TryPaintUnderCursor()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Debug.Log($"🎯 Потрапили в: {hit.collider.name}");
            Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.green, 2f);
            Debug.Log($"🧭 UV hit: {hit.textureCoord}");

            Renderer rend = hit.collider.GetComponent<Renderer>();
            if (rend == targetRenderer)
            {
                Debug.Log("✅ Співпадіння з цільовим Renderer — малюємо...");
                PaintAtUV(hit.textureCoord);
            }
            else
            {
                Debug.LogWarning("⚠️ Потрапили в інший Renderer");
            }
        }
        else
        {
            Debug.Log("🔭 Raycast не потрапив ні в що");
        }
    }

    // 🖌 Малювання по UV
    void PaintAtUV(Vector2 uv)
    {
        int x = Mathf.RoundToInt(uv.x * textureToPaint.width);
        int y = Mathf.RoundToInt(uv.y * textureToPaint.height);

        Debug.Log($"🎨 Малюємо в UV: ({uv.x:0.000}, {uv.y:0.000}) → Pixel: ({x}, {y})");

        float sqrRadius = brushSize * brushSize;

        for (int i = -brushSize; i <= brushSize; i++)
        {
            for (int j = -brushSize; j <= brushSize; j++)
            {
                if (i * i + j * j <= sqrRadius)
                {
                    int px = x + i;
                    int py = y + j;

                    if (px >= 0 && py >= 0 && px < textureToPaint.width && py < textureToPaint.height)
                    {
                        textureToPaint.SetPixel(px, py, brushColor);
                    }
                }
            }
        }

        textureToPaint.Apply();
        Debug.Log("🖌 Застосовано зміни до текстури");
    }

    // 🧪 Закрасити верхній лівий кут — опціонально
    void DrawTestArea()
    {
        for (int x = 0; x < 200; x++)
        {
            for (int y = 0; y < 200; y++)
            {
                textureToPaint.SetPixel(x, y, Color.magenta);
            }
        }
        textureToPaint.Apply();
        Debug.Log("🟪 Закрасили верхній лівий кут у magenta — тест");
    }
}
