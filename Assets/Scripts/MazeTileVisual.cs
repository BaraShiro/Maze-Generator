using UnityEngine;

/// <summary>
/// A visual representation of a maze tile.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class MazeTileVisual : MonoBehaviour
{
    private static Color markedColour = new Color(1f, 0.15f, 0.15f);
    private static Color unmarkedColour = new Color(1f, 0.95f, 0.8f);

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// Changes the sprite of the sprite renderer to <paramref name="newSprite"/>.
    /// </summary>
    /// <param name="newSprite">The sprite to change to.</param>
    public void ChangeSprite(Sprite newSprite)
    {
        spriteRenderer.sprite = newSprite;
    }

    /// <summary>
    /// Sets the colour of the sprite to <see cref="markedColour"/>.
    /// </summary>
    public void SetColourMarked()
    {
        spriteRenderer.color = markedColour;
    }

    /// <summary>
    /// Sets the colour of the sprite to <see cref="unmarkedColour"/>.
    /// </summary>
    public void SetColourUnmarked()
    {
        spriteRenderer.color = unmarkedColour;
    }
}
