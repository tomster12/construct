
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;


public enum LabelState { ICON, TITLE, INFO }


public class InspectableLabel : MonoBehaviour
{
    // Declare references, variables
    [Header("References")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform mask;
    [SerializeField] private RectTransform background;
    [SerializeField] private Image icon;
    [SerializeField] private VerticalLayoutGroup layout;

    [Header("Prefabs")]
    [SerializeField] private GameObject attributePfb;
    [SerializeField] private GameObject modifierPfb;

    [Header("Content Elements")]
    [SerializeField] private TextMeshProUGUI contentName;
    [SerializeField] private TextMeshProUGUI contentDescription;
    [SerializeField] private VerticalLayoutGroup contentAttributeLayout;
    [SerializeField] private RawImage contentElementImage;
    [SerializeField] private TextMeshProUGUI contentElementName;
    [SerializeField] private TextMeshProUGUI contentWeight;
    [SerializeField] private VerticalLayoutGroup contentModifierLayout;

    [Header("Config")]
    [SerializeField] private float baseCanvasScale = 0.005f;
    [SerializeField] private float infoCanvasScale = 0.008f;
    [SerializeField] private float iconBaseSize = 40f;
    [SerializeField] private float iconSmallSize = 25f;
    [SerializeField] private float sizeUpLerp = 8f;
    [SerializeField] private float sizeDownLerp = 14.5f;
    [SerializeField] private float posLerp = 7f;
    [SerializeField] private float alphaLerp = 10f;
    [SerializeField] private float expandTime = 1.0f;
    [SerializeField] private Vector2 iconOffset = new Vector2(0.0f, 1.0f);
    [SerializeField] private Vector2 titleOffset = new Vector2(0.0f, 1.0f);
    [SerializeField] private Vector2 infoOffset = new Vector2(1.0f, 1.0f);
    [SerializeField] private float iconAlpha = 0.4f;
    [SerializeField] private float standardAlpha = 1.0f;

    private IInspectable inspectedObject;
    private float offset;
    [SerializeField] private bool isNearby;
    [SerializeField] private bool isHighlighted;
    [SerializeField] private LabelState state;
    private float expandTimer = 0.0f;


    private void Awake()
    {
        // Initialize variables
        ResetDynamics();
    }


    private void ResetDynamics()
    {
        // Intialize with default values
        state = LabelState.ICON;
        isNearby = false;
        isHighlighted = false;
        UpdateDynamics(true, true, true);
    }


    private void Update()
    {
        // Run update functions
        UpdateExpansion();
        UpdateDynamics();
    }

    private void UpdateExpansion()
    {
        // Update expansion timer
        if (isNearby && isHighlighted && expandTimer < expandTime)
        {
            expandTimer += Time.deltaTime;
            if (expandTimer >= expandTime)
            {
                state = LabelState.INFO;
                expandTimer = expandTime;
            }
        }

        // Update UI layer based on state
        if (state == LabelState.ICON)
        {
            if (canvas.gameObject.layer != LayerMask.NameToLayer("UI")) Util.SetLayer(canvas.transform, LayerMask.NameToLayer("UI"));
        }
        else if (canvas.gameObject.layer != LayerMask.NameToLayer("UI Ontop")) Util.SetLayer(canvas.transform, LayerMask.NameToLayer("UI Ontop"));
    }

    private void UpdateDynamics(bool setPos = false, bool setSize = false, bool setAlpha = false)
    {
        if (inspectedObject == null) return;

        // Aim canvas same direction as player camera
        Camera playerCam = PlayerController.instance.cam;
        transform.up = Vector3.up;
        transform.forward = transform.position - playerCam.transform.position;

        // Lerp canvas position to set offset
        Vector2 targetOffset = GetCurrentStateOffset();
        Vector3 currentCentre = inspectedObject.GetIIPosition();
        Vector3 playerCentre = PlayerController.instance.GetBillboardTarget();
        float maskWorldWidth = mask.rect.width * canvas.transform.localScale.x;
        float offsetHeight = GetCurrentStateSize(LabelState.TITLE).y * canvas.transform.localScale.y;
        Vector3 up = Vector3.up;
        Vector3 right = Vector3.Cross(up, currentCentre - playerCentre).normalized;
        Vector3 targetPosition = currentCentre + offset * (right * targetOffset.x + up * targetOffset.y);
        targetPosition += offsetHeight * up;
        if (state == LabelState.INFO) targetPosition += maskWorldWidth * 0.5f * right;
        float positionPct = setPos ? 1.0f : posLerp * Time.deltaTime;
        Vector3 lerpedPosition = Vector3.Lerp(canvas.transform.position, targetPosition, positionPct);
        canvas.transform.position = lerpedPosition;

        // Lerp mask size to background
        Vector2 targetSize = GetCurrentStateSize();
        float sizePct = setSize ? 1.0f : (targetSize.x > mask.rect.size.x ? sizeUpLerp : sizeDownLerp) * Time.deltaTime;
        Vector2 lerpedSize = Vector2.Lerp(mask.rect.size, targetSize, sizePct);
        mask.sizeDelta = lerpedSize;

        // Lerp icon size
        float iconSize = state == LabelState.ICON ? iconBaseSize : iconSmallSize;
        float iconSizePct = setSize ? 1.0f : (iconSize > icon.rectTransform.rect.size.x ? sizeUpLerp : sizeDownLerp) * Time.deltaTime;
        Vector2 iconLerpedSize = Vector2.Lerp(icon.rectTransform.rect.size, Vector2.one * iconSize, iconSizePct);
        icon.rectTransform.sizeDelta = iconLerpedSize;

        // Lerp canvas scale
        Vector3 targetScale = Vector3.one * (state == LabelState.INFO ? infoCanvasScale : baseCanvasScale);
        float scalePct = setSize ? 1.0f : sizeUpLerp * Time.deltaTime;
        canvas.transform.localScale = Vector3.Lerp(canvas.transform.localScale, targetScale, scalePct);

        // Lerp alpha values
        float targetAlpha = GetCurrentStateAlpha();
        canvasGroup.interactable = isNearby;
        canvasGroup.blocksRaycasts = isNearby;
        float alphaPct = setAlpha ? 1.0f : alphaLerp * Time.deltaTime;
        canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, alphaPct);
    }


    private Vector2 GetCurrentStateOffset()
    {
        switch (state)
        {
            case LabelState.ICON: return iconOffset;
            case LabelState.TITLE: return titleOffset;
            case LabelState.INFO: return infoOffset;
        }
        return Vector2.zero;
    }

    private Vector2 GetCurrentStateSize(LabelState? overrideState = null)
    {
        switch (overrideState == null ? state : overrideState.Value)
        {
            case LabelState.ICON:
                return new Vector2(
                    layout.padding.left + layout.spacing + contentName.rectTransform.rect.height,
                    layout.padding.top + layout.spacing + contentName.rectTransform.rect.height);

            case LabelState.TITLE:
                return new Vector2(
                    background.rect.width,
                    layout.padding.top + layout.spacing + contentName.rectTransform.rect.height);

            case LabelState.INFO:
                return new Vector2(background.rect.width, background.rect.height);

            default: return Vector2.zero;
        }
    }

    private float GetCurrentStateAlpha()
    {
        if (!isNearby && !isHighlighted) return 0.0f;
        else if (state == LabelState.ICON) return iconAlpha;
        else return standardAlpha;
    }


    public void SetObject(IInspectable inspectedObject_, float offset_)
    {
        // Set variables
        inspectedObject = inspectedObject_;
        offset = offset_;
        state = LabelState.ICON;
        UpdateDynamics(true, true, false);

        // Update all content values
        icon.sprite = inspectedObject.GetIIIconSprite();
        contentName.text = inspectedObject.GetIIName();
        contentDescription.text = inspectedObject.GetIIDescription();
        SetAttributeStrings(inspectedObject.GetIIAttributes());
        contentElementImage.texture = inspectedObject.GetIIElement().sprite.texture;
        contentElementName.text = inspectedObject.GetIIElement().name;
        contentElementName.color = inspectedObject.GetIIElement().color;
        if (inspectedObject.GetIIMass() != 0.0f)
            contentWeight.text = inspectedObject.GetIIMass() + "kg";
        else contentWeight.text = "";
        SetModifierStrings(inspectedObject.GetIIModifiers());
    }

    public void SetAttributeStrings(List<string> attributeStrings)
    {
        // Empty current attributes
        foreach (Transform child in contentAttributeLayout.transform) Destroy(child.gameObject);

        // Create new attribute for each string
        foreach (string attribute in attributeStrings)
        {
            GameObject attributeObject = Instantiate(attributePfb);
            TextMeshProUGUI text = attributeObject.GetComponent<TextMeshProUGUI>();
            attributeObject.transform.SetParent(contentAttributeLayout.transform, false);
            text.text = attribute;
        }
    }

    public void SetModifierStrings(List<string> modifierStrings)
    {
        // Empty current modifiers
        foreach (Transform child in contentModifierLayout.transform) Destroy(child.gameObject);

        // Create new modifier for each string
        foreach (string modifier in modifierStrings)
        {
            GameObject modifierObject = Instantiate(modifierPfb);
            TextMeshProUGUI text = modifierObject.GetComponent<TextMeshProUGUI>();
            modifierObject.transform.SetParent(contentModifierLayout.transform, false);
            text.text = "- " + modifier;
        }

        // Handle no modifiers
        if (modifierStrings.Count == 0)
        {
            GameObject modifierObject = Instantiate(modifierPfb);
            TextMeshProUGUI text = modifierObject.GetComponent<TextMeshProUGUI>();
            modifierObject.transform.SetParent(contentModifierLayout.transform, false);
            text.text = "<i>No modifiers</i>";
        }
    }

    public void SetNearby(bool isNearby_)
    {
        // Handle updating state and set isNearby
        if (isNearby == isNearby_) return;
        isNearby = isNearby_;

        if (isNearby)
        {
            state = LabelState.ICON;
            expandTimer = 0.0f;
            UpdateDynamics(true, true, false);
        }
    }

    public void SetHighlighted(bool isHighlighted_)
    {
        // Handle updating state and set isHighlighted
        if (isHighlighted == isHighlighted_) return;
        isHighlighted = isHighlighted_;

        if (isHighlighted)
        {
            state = LabelState.TITLE;
            expandTimer = 0.0f;
        }
        else if (isNearby)
        {
            state = LabelState.ICON;
            expandTimer = 0.0f;
        }
    }
}
