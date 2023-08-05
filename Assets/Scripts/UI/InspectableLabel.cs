
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;

[Serializable]
public enum LabelState { ICON, TITLE, INFO }


public class InspectableLabel : MonoBehaviour
{
    // Declare references, variables
    [Header("References")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private CanvasGroup canvasGroupMain;
    [SerializeField] private CanvasGroup canvasGroupPrompt;
    [SerializeField] private Image promptAttachablePrompt;
    [SerializeField] private Image promptAttachableIcon;
    [SerializeField] private RectTransform mask;
    [SerializeField] private RectTransform background;
    [SerializeField] private Image icon;
    [SerializeField] private VerticalLayoutGroup layout;

    [Header("Prefabs")]
    [SerializeField] private Sprite promptAttachableIconAvailable;
    [SerializeField] private Sprite promptAttachableIconInProgress;
    [SerializeField] private GameObject attributePfb;
    [SerializeField] private GameObject modifierPfb;

    [Header("Content Elements")]
    [SerializeField] private RectTransform contentIcon;
    [SerializeField] private TextMeshProUGUI contentName;
    [SerializeField] private TextMeshProUGUI contentDescription;
    [SerializeField] private RectTransform contentBarParent;
    [SerializeField] private Image contentProgressBar;
    [SerializeField] private Image contentMainBar;
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
    [SerializeField] private float expandHoverOffset = 0.8f;
    [SerializeField] private float expandHoverTime = 0.55f;
    [SerializeField] private float iconAlpha = 0.4f;
    [SerializeField] private float standardAlpha = 1.0f;

    [SerializeField] float[] DISTANCE_MAP = new float[] { 0.5f, 2.0f, 0.5f, 1.0f };
    
    private IInspectable inspectedObject;
    private LabelState state;
    private float offset;
    private float distanceScale = 1.0f;
    private bool isNearby;
    private bool isHighlighted;
    private float hoverTimer = 0.0f;


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
        UpdateState();
        UpdateDynamics();
    }

    private void UpdateState()
    {
        // Update expansion timer
        if (isNearby && isHighlighted && (hoverTimer - expandHoverOffset) < expandHoverTime)
        {
            hoverTimer += Time.deltaTime;
            if ((hoverTimer - expandHoverOffset) >= expandHoverTime)
            {
                state = LabelState.INFO;
                hoverTimer = expandHoverOffset + expandHoverTime;
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

        // Calculate distanceScale
        Vector3 billboardPos = PlayerController.instance.GetBillboardTarget();
        float dist = Vector3.Distance(billboardPos, transform.position);
        distanceScale = Util.ConstrainMap(dist, DISTANCE_MAP[0], DISTANCE_MAP[1], DISTANCE_MAP[2], DISTANCE_MAP[3]);

        // Aim canvas same direction as player camera
        Camera playerCam = PlayerController.instance.cam;
        Vector3 billboardDir = PlayerController.instance.GetBillboardDirection();
        transform.up = Vector3.up;
        transform.forward = billboardDir;
        
        // Lerp canvas position to set offsets
        float horizontalOffset = (state == LabelState.INFO ? 1.0f : 0.0f) * distanceScale;
        Vector3 currentCentre = inspectedObject.IIGetPosition();
        float maskWorldWidth = mask.rect.width * canvas.transform.localScale.x;
        float offsetHeight = GetCurrentStateSize(LabelState.TITLE).y * canvas.transform.localScale.y;
        Vector3 right = Vector3.Cross(Vector3.up, billboardDir).normalized;
        Vector3 targetPosition = currentCentre + offset * (Vector3.up + right * horizontalOffset) + offsetHeight * Vector3.up;
        if (state == LabelState.INFO) targetPosition += maskWorldWidth * 0.5f * right;
        float positionPct = setPos ? 1.0f : posLerp * Time.deltaTime;
        Vector3 lerpedPosition = Vector3.Lerp(canvas.transform.position, targetPosition, positionPct);
        canvas.transform.position = lerpedPosition;

        // Lerp canvas scale
        Vector3 targetScale = Vector3.one * (state == LabelState.INFO ? infoCanvasScale : baseCanvasScale) * distanceScale;
        float scalePct = setSize ? 1.0f : sizeUpLerp * Time.deltaTime;
        canvas.transform.localScale = Vector3.Lerp(canvas.transform.localScale, targetScale, scalePct);

        // Lerp mask size to background
        Vector2 targetSize = GetCurrentStateSize();
        float sizePct = setSize ? 1.0f : (targetSize.x > mask.rect.size.x ? sizeUpLerp : sizeDownLerp) * Time.deltaTime;
        Vector2 lerpedSize = Vector2.Lerp(mask.rect.size, targetSize, sizePct);
        mask.sizeDelta = lerpedSize;

        // Lerp icon size
        float iconSize = state == LabelState.ICON ? iconBaseSize : iconSmallSize;
        float iconSizePct = setSize ? 1.0f : (iconSize > icon.rectTransform.rect.size.x ? sizeUpLerp : sizeDownLerp) * Time.deltaTime;
        icon.rectTransform.sizeDelta = Vector2.Lerp(icon.rectTransform.rect.size, Vector2.one * iconSize, iconSizePct);

        // Lerp main bar sized
        Vector2 mainBarTargetSize = contentMainBar.rectTransform.sizeDelta;
        if (state == LabelState.ICON) mainBarTargetSize.x = ((icon.rectTransform.localPosition.x - layout.padding.left) - contentMainBar.rectTransform.anchoredPosition.x) * 2;
        else mainBarTargetSize.x = contentBarParent.sizeDelta.x + (contentMainBar.rectTransform.anchoredPosition.x * -2);
        contentMainBar.rectTransform.sizeDelta = Vector2.Lerp(contentMainBar.rectTransform.sizeDelta, mainBarTargetSize, sizePct);

        // Lerp progress bar
        float pct = Util.Easing.EaseOutSine(Mathf.Clamp01((hoverTimer - expandHoverOffset) / expandHoverTime));
        contentProgressBar.rectTransform.sizeDelta = new Vector2(pct * contentMainBar.rectTransform.sizeDelta.x, contentProgressBar.rectTransform.sizeDelta.y);

        // Lerp alpha values
        float targetAlpha = GetCurrentStateAlpha();
        canvasGroupMain.interactable = isNearby;
        canvasGroupMain.blocksRaycasts = isNearby;
        float alphaPct = setAlpha ? 1.0f : alphaLerp * Time.deltaTime;
        canvasGroupMain.alpha = Mathf.Lerp(canvasGroupMain.alpha, targetAlpha, alphaPct);

        // Enable / Disable prompt
        PromptState promptAttachableState = PlayerController.instance.GetPromptStateAttachable();
        bool promptAttachableEnabled =
            (state == LabelState.TITLE || state == LabelState.INFO)
            && isNearby && promptAttachableState != PromptState.UNAVAILABLE;

        promptAttachablePrompt.transform.localPosition = new Vector3(-lerpedSize.x * 0.5f - 15.0f, -30.0f, 0.0f);
        if (promptAttachableState == PromptState.IN_PROGRESS)
        {
            promptAttachablePrompt.color = Color.gray;
            promptAttachableIcon.color = Color.gray;
            promptAttachableIcon.sprite = promptAttachableIconInProgress;
        }
        else if (promptAttachableState == PromptState.AVAILABLE)
        {
            promptAttachablePrompt.color = Color.white;
            promptAttachableIcon.color = Color.white;
            promptAttachableIcon.sprite = promptAttachableIconAvailable;
        }

        float promptTargetAlpha = promptAttachableEnabled ? 1.0f : 0.0f;
        float promptAlphaPct = setAlpha ? 1.0f : alphaLerp * Time.deltaTime;
        canvasGroupPrompt.interactable = isNearby;
        canvasGroupPrompt.blocksRaycasts = isNearby;
        canvasGroupPrompt.alpha = Mathf.Lerp(canvasGroupPrompt.alpha, promptTargetAlpha, promptAlphaPct);
    }


    private Vector2 GetCurrentStateSize(LabelState? overrideState = null)
    {
        switch (overrideState == null ? state : overrideState.Value)
        {
            case LabelState.ICON:
                return new Vector2(
                    layout.padding.left + layout.spacing + contentName.rectTransform.rect.height,
                    layout.padding.top + layout.spacing * 2 + contentName.rectTransform.rect.height);

            case LabelState.TITLE:
                return new Vector2(
                    background.rect.width,
                    layout.padding.top + layout.spacing * 2 + contentName.rectTransform.rect.height);

            case LabelState.INFO:
                return new Vector2(background.rect.width, background.rect.height);

            default:
                return Vector2.zero;
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
        icon.sprite = inspectedObject.IIGetIconSprite();
        contentName.text = inspectedObject.IIGetName();
        contentDescription.text = inspectedObject.IIGetDescription();
        SetAttributeStrings(inspectedObject.IIGetAttributes());
        contentElementImage.texture = inspectedObject.IIGetElement().sprite.texture;
        contentElementName.text = inspectedObject.IIGetElement().name;
        contentElementName.color = inspectedObject.IIGetElement().color;
        if (inspectedObject.IIGetMass() != 0.0f)
            contentWeight.text = inspectedObject.IIGetMass() + "kg";
        else contentWeight.text = "";
        SetModifierStrings(inspectedObject.IIGetModifiers());
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
            hoverTimer = 0.0f;
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
            hoverTimer = 0.0f;
        }
        else if (isNearby)
        {
            state = LabelState.ICON;
            hoverTimer = 0.0f;
        }
    }
}
