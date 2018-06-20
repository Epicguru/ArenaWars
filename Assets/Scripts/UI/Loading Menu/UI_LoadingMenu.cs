using UnityEngine;
using UnityEngine.UI;

public class UI_LoadingMenu : MonoBehaviour
{
    public float Percentage
    {
        get
        {
            return StepBar.fillAmount;
        }
        set
        {
            StepBar.fillAmount = value;
            StepText.text = (Percentage * 100f).ToString("n1") + "%";
        }
    }

    public string Title
    {
        get
        {
            return TitleText.text;
        }
        set
        {
            TitleText.text = value;
        }
    }

    [Header("References")]
    public Text StepText;
    public Text TitleText;
    public Image StepBar;
}