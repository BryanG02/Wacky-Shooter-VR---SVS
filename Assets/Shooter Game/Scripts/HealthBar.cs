using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Slider slider;

    /// <summary>Initialize the bar's range.</summary>
    public void SetMaxHealth(float max)
    {
        slider.maxValue = max;
        slider.value    = max;
    }

    /// <summary>Update the bar's current value.</summary>
    public void SetHealth(float current)
    {
        slider.value = current;
    }
}
