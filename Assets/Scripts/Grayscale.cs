using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Grayscale : MonoBehaviour
{
    public Volume volume;
    private ColorAdjustments colorAdjustments;

    void Awake()
    {
        volume.profile.TryGet(out colorAdjustments);
    }

    public void EnableGrayscale(bool enable)
    {
        if (colorAdjustments != null)
            colorAdjustments.saturation.value = enable ? -100f : 0f;
    }
}
