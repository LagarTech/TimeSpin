using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class AchievementItemUI : MonoBehaviour
{
    [SerializeField] private TMP_Text titleText; // Referencia al texto del nombre
    [SerializeField] private TMP_Text statusText; // Referencia al texto del estado
    [SerializeField] private Image statusIcon; // Icono del estado
    [SerializeField] private Slider progressSlider;//Slider

    public void SetAchievementData(string title, bool unlocked)
    {
        if (titleText != null)
            titleText.text = title;

        if (statusText != null)
        {
            statusText.text = unlocked ? "1/1" : "0/1";
            statusText.color = unlocked ? Color.gray : Color.gray;
        }

        if (statusIcon != null)
        {
            statusIcon.color = unlocked ? Color.green : Color.gray;
        }

        if (progressSlider != null)
        {
            progressSlider.value = unlocked ? progressSlider.maxValue : 0f; // Completar o reiniciar slider
        }
    }

}
