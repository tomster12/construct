
using UnityEngine;
using UnityEngine.UI;


public class MouseCursor : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Sprite idleSprite;
    [SerializeField] private Sprite interactSprite;
    [SerializeField] private Image cursorImage;


    private void Update()
    {
        if (playerController.couldInteract)
        {
            cursorImage.sprite = interactSprite;
            cursorImage.rectTransform.sizeDelta = Vector2.one * 8.0f;
        }
        else
        {
            cursorImage.sprite = idleSprite;
            cursorImage.rectTransform.sizeDelta = Vector2.one * 5.0f;
        }
    }
}
