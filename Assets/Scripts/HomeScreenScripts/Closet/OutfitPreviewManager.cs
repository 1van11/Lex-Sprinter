using UnityEngine;
using UnityEngine.UI;

public class OutfitPreviewManager : MonoBehaviour
{
    public Image previewImage;

    public void SetOutfit(Sprite outfitSprite)
    {
        previewImage.sprite = outfitSprite;
        previewImage.color = Color.white; // ensure visible
    }
}
