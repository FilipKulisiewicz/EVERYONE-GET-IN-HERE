using UnityEngine;

public class ColorHolder : MonoBehaviour
{
    [SerializeField] private Color color = Color.white;

    public Color Color
    {
        get => color;
        set => color = value;
    }

    public static ColorHolder GetColorHolderFromObj(GameObject obj)
    {
        if (obj == null) return null;
        return obj.GetComponent<ColorHolder>();
    }
}
