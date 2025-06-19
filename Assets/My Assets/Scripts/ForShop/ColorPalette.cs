using UnityEngine;

[CreateAssetMenu(fileName = "NewColorPalette", menuName = "Shop/Color Palette")]
public class ColorPalette : ScriptableObject
{
    [System.Serializable]
    public struct ColorOption
    {
        public string name;        // e.g. “White”, “Red”, “Gold”
        public Color color;
        public float multiplier;   // pricing modifier
    }

    public ColorOption[] options;
}