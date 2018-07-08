using UnityEngine;

[ExecuteInEditMode]
public class UI_GraphSizeFitter : MonoBehaviour
{
    public RectTransform ThisObject;
    public RectTransform Target;
    public float Offset;

    public void Update()
    {
        var sd = ThisObject.sizeDelta;
        sd.x = Target.rect.height + Offset;
        ThisObject.sizeDelta = sd;
    }
}