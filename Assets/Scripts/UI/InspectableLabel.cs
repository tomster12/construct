
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;

[Serializable]
public enum LabelState { IPartN, TITLE, INFO }


public class InspectableLabel : MonoBehaviour
{
    // Declare references, variables
    [Header("References")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private CanvasGroup canvasGroupMain;
    [SerializeField] private CanvasGroup canvasGroupPrompt;
    [SerializeField] private Image promptInput;
    [SerializeField] private Image promptIcon;
    [SerializeField] private RectTransform mask;
    [SerializeField] private RectTransform background;
    [SerializeField] private Image icon;
    [SerializeField] private VerticalLayoutGroup layout;

    [Header("Prefabs")]
    [SerializeField] private Sprite promptIconAvailable;
    [SerializeField] private Sprite promptIconInProgress;
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
    
    public bool isNearby { get; private set; }
    public bool isHighlighted { get; private set; }

    private IInspectable inspectable;
    private Object inspectedObject;
    private InspectedData inspectedData;
    private LabelState currentState;
    private float offset;
    private float distanceScale = 1.0f;
    private float hoverTimer = 0.0f;


    private void Awake()
    {
        // Initialize variables
        ResetDynamics();
    }

    private void ResetDynamics()
    {
        // Intialize with default values
        currentState = LabelState.IPartN;
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
                currentState = LabelState.INFO;
                hoverTimer = expandHoverOffset + expandHoverTime;
            }
        }

        // Update UI layer based on state
        if (currentState == LabelState.IPartN)
        {
            if (canvas.gameObject.layer != LayerMask.NameToLayer("UI")) Util.SetLayer(canvas.transform, LayerMask.NameToLayer("UI"));
        }
        else if (canvas.gameObject.layer != LayerMask.NameToLayer("UI Ontop")) Util.SetLayer(canvas.transform, LayerMask.NameToLayer("UI Ontop"));
    }

    private void UpdateDynamics(bool setPos = false, bool setSize = false, bool setAlpha = false)
    {
        if (inspectable == null) return;
        inspectedData = inspectable.Inspect();

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
        float horizontalOffset = (currentState == LabelState.INFO ? 1.0f : 0.0f) * distanceScale;
        Vector3 currentCentre = inspectable.GetPosition();
        float maskWorldWidth = mask.rect.width * canvas.transform.localScale.x;
        float offsetHeight = GetMaskSize(LabelState.TITLE).y * canvas.transform.localScale.y;
        Vector3 right = Vector3.Cross(Vector3.up, billboardDir).normalized;
        Vector3 targetPosition = currentCentre + offset * (Vector3.up + right * horizontalOffset) + offsetHeight * Vector3.up;
        if (currentState == LabelState.INFO) targetPosition += maskWorldWidth * 0.5f * right;
        float positionPct = setPos ? 1.0f : posLerp * Time.deltaTime;
        Vector3 lerpedPosition = Vector3.Lerp(canvas.transform.position, targetPosition, positionPct);
        canvas.transform.position = lerpedPosition;

        // Lerp canvas scale
        Vector3 targetScale = Vector3.one * (currentState == LabelState.INFO ? infoCanvasScale : baseCanvasScale) * distanceScale;
        float scalePct = setSize ? 1.0f : sizeUpLerp * Time.deltaTime;
        canvas.transform.localScale = Vector3.Lerp(canvas.transform.localScale, targetScale, scalePct);

        // Lerp mask size to background
        Vector2 targetSize = GetMaskSize();
        float sizePct = setSize ? 1.0f : (targetSize.x > mask.rect.size.x ? sizeUpLerp : sizeDownLerp) * Time.deltaTime;
        Vector2 lerpedSize = Vector2.Lerp(mask.rect.size, targetSize, sizePct);
        mask.sizeDelta = lerpedSize;

        // Lerp icon size
        float iconSize = currentState == LabelState.IPartN ? iconBaseSize : iconSmallSize;
        float iconSizePct = setSize ? 1.0f : (iconSize > icon.rectTransform.rect.size.x ? sizeUpLerp : sizeDownLerp) * Time.deltaTime;
        icon.rectTransform.sizeDelta = Vector2.Lerp(icon.rectTransform.rect.size, Vector2.one * iconSize, iconSizePct);

        // Lerp main bar sized
        Vector2 mainBarTargetSize = contentMainBar.rectTransform.sizeDelta;
        if (currentState == LabelState.IPartN) mainBarTargetSize.x = ((icon.rectTransform.localPosition.x - layout.padding.left) - contentMainBar.rectTransform.anchoredPosition.x) * 2;
        else mainBarTargetSize.x = contentBarParent.sizeDelta.x + (contentMainBar.rectTransform.anchoredPosition.x * -2);
        contentMainBar.rectTransform.sizeDelta = Vector2.Lerp(contentMainBar.rectTransform.sizeDelta, mainBarTargetSize, sizePct);

        // Lerp progress bar
        float pct = Util.Easing.EaseOutSine(Mathf.Clamp01((hoverTimer - expandHoverOffset) / expandHoverTime));
        contentProgressBar.rectTransform.sizeDelta = new Vector2(pct * contentMainBar.rectTransform.sizeDelta.x, contentProgressBar.rectTransform.sizeDelta.y);

        // Lerp alpha values
        float targetAlpha = GetCanvasAlpha();
        canvasGroupMain.interactable = isNearby;
        canvasGroupMain.blocksRaycasts = isNearby;
        float alphaPct = setAlpha ? 1.0f : alphaLerp * Time.deltaTime;
        canvasGroupMain.alpha = Mathf.Lerp(canvasGroupMain.alpha, targetAlpha, alphaPct);

        // Enable / Disable prompt
        InteractionState promptState = PlayerController.instance.GetAttachmentInteractionState();
        bool promptEnabled =
            (currentState == LabelState.TITLE || currentState == LabelState.INFO)
            && isNearby && (promptState != InteractionState.CLOSED);

        promptInput.transform.localPosition = new Vector3(-lerpedSize.x * 0.5f - 15.0f, -30.0f, 0.0f);
        if (promptState == InteractionState.BLOCKED)
        {
            promptInput.color = Color.gray;
            promptIcon.color = Color.gray;
            promptIcon.sprite = promptIconInProgress;
        }
        else if (promptState == InteractionState.OPEN)
        {
            promptInput.color = Color.white;
            promptIcon.color = Color.white;
            promptIcon.sprite = promptIconAvailable;
        }

        float promptTargetAlpha = promptEnabled ? 1.0f : 0.0f;
        float promptAlphaPct = setAlpha ? 1.0f : alphaLerp * Time.deltaTime;
        canvasGroupPrompt.interactable = isNearby;
        canvasGroupPrompt.blocksRaycasts = isNearby;
        canvasGroupPrompt.alpha = Mathf.Lerp(canvasGroupPrompt.alpha, promptTargetAlpha, promptAlphaPct);
    }


    private Vector2 GetMaskSize(LabelState? state = null)
    {
        state = (state == null) ? currentState : state;
        return state switch
        {
            LabelState.IPartN => new Vector2(
                                layout.padding.left + layout.spacing + contentName.rectTransform.rect.height,
                                layout.padding.top + layout.spacing * 2 + contentName.rectTransform.rect.height),
            LabelState.TITLE => new Vector2(
                                background.rect.width,
                                layout.padding.top + layout.spacing * 2 + contentName.rectTransform.rect.height),
            LabelState.INFO => new Vector2(background.rect.width, background.rect.height),
            _ => Vector2.zero,
        };
    }

    private float GetCanvasAlpha(LabelState? state = null)
    {
        if (!isNearby && !isHighlighted) return 0.0f;
        state = (state == null) ? currentState : state;
        return state switch
        {
            LabelState.IPartN => iconAlpha,
            _ => standardAlpha,
        };
    }

    public void SetObject(IInspectable inspectable_, float offset_)
    {
        // Set variables
        inspectable = inspectable_;
        inspectedObject = inspectable.GetObject();
        inspectedData = inspectable.Inspect();
        offset = offset_;
        currentState = LabelState.IPartN;
        UpdateDynamics(true, true, false);

        // Update all content values
        icon.sprite = inspectedData.icon;
        contentName.text = inspectedData.name;
        contentDescription.text = inspectedData.description;
        SetAttributeStrings(inspectable.GetAttributes());
        contentElementImage.texture = inspectedData.element.sprite.texture;
        contentElementName.text = inspectedData.element.name;
        contentElementName.color = inspectedData.element.color;
        if (inspectedObject.rb.mass != 0.0f)
            contentWeight.text = inspectedObject.rb.mass + "kg";
        else contentWeight.text = "";
        SetModifierStrings(inspectable.GetModifiers());
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

    public void SetIsNearby(bool isNearby_)
    {
        // Handle updating state and set isNearby
        if (isNearby == isNearby_) return;
        isNearby = isNearby_;

        if (isNearby)
        {
            currentState = LabelState.IPartN;
            hoverTimer = 0.0f;
            UpdateDynamics(true, true, false);
        }
    }

    public void SetIsHighlighted(bool isHighlighted_)
    {
        // Handle updating state and set isHighlighted
        if (isHighlighted == isHighlighted_) return;
        isHighlighted = isHighlighted_;

        if (isHighlighted)
        {
            currentState = LabelState.TITLE;
            hoverTimer = 0.0f;
        }
        else if (isNearby)
        {
            currentState = LabelState.IPartN;
            hoverTimer = 0.0f;
        }
    }
}
