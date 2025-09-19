using UnityEngine;

public class FishController : MonoBehaviour
{
    [SerializeField] private Renderer targetRenderer;

    // Дамо доступ до Renderer для FishLogic → TextureApplier
    public Renderer TargetRenderer => targetRenderer;

    public void Activate()
    {
        //if (targetRenderer != null) targetRenderer.enabled = true;
        //else gameObject.SetActive(true);
        gameObject.SetActive(true);
    }

    public void Deactivate()
    {
        // if (targetRenderer != null) targetRenderer.enabled = false;
        // else gameObject.SetActive(false);
        gameObject.SetActive(false);
    }
}