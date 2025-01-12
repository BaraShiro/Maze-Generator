using UnityEngine;

/// <summary>
/// A visual representation of a maze tile.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class MazeTileVisual : MonoBehaviour
{
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

    public void Hide()
    {
        spriteRenderer.sprite = MazePainter.Instance.EmptySprite;
    }

    public void Paint()
    {
        spriteRenderer.color = MazePainter.Instance.PaintedColor;
    }

    /// <summary>
    /// Sets the colour of the sprite to <see cref="markedColour"/>.
    /// </summary>
    public void Mark()
    {
        spriteRenderer.color = MazePainter.Instance.MarkedColor;
    }

    /// <summary>
    /// Sets the colour of the sprite to <see cref="unmarkedColour"/>.
    /// </summary>
    public void Unpaint()
    {
        spriteRenderer.color = MazePainter.Instance.UnpaintedColor;
    }
}
