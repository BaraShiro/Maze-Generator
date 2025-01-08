using UnityEngine;

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

    public void ChangeSprite(Sprite newSprite)
    {
        spriteRenderer.sprite = newSprite;
    }

    public void SetColourMarked()
    {
        spriteRenderer.color = markedColour;
    }

    public void SetColourUnmarked()
    {
        spriteRenderer.color = unmarkedColour;
    }
}
