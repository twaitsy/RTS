using UnityEngine;

public class ColorPaletteRegistry : DefinitionRegistry<ColorPaletteDefinition>
{
    public static ColorPaletteRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple ColorPaletteRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }
}