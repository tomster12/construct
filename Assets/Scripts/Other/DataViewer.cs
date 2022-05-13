
using UnityEngine;
using UnityEngine.UI;
using TMPro;


class DataViewer : MonoBehaviour
{
    // Declare variables
    [SerializeField] private Image back;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private float alphaLerp = 10.0f;

    private bool isActive;
    private WorldObject targetWJ;
    private Vector3 targetPos;
    private float currentAlpha;
    private float targetAlpha;


    private void Awake()
    {
        // Initialize game objects
        back.gameObject.SetActive(true);
        SetAlpha(0.0f);
    }


    private void Update() => UpdateUI();


    private void UpdateUI()
    {
        // Update UI position
        if (targetWJ != null)
        {
            targetPos = Camera.main.WorldToScreenPoint(targetWJ.transform.position);
            back.transform.position = targetPos;
        }

        // Lerp alpha towards target
        float newAlpha = Mathf.Lerp(currentAlpha, targetAlpha, Time.deltaTime * alphaLerp);
        SetAlpha(newAlpha);
    }


    private void SetAlpha(float alpha)
    {
        // Update color of all UI
        currentAlpha = alpha;
        back.color = new Color(back.color.r, back.color.g, back.color.b, currentAlpha);
        nameText.color = new Color(nameText.color.r, nameText.color.g, nameText.color.b, currentAlpha);
        descriptionText.color = new Color(descriptionText.color.r, descriptionText.color.g, descriptionText.color.b, currentAlpha);
    }


    public void SetWorldObject(WorldObject targetWJ_)
    {
        // Update variables
        targetWJ = targetWJ_;
        nameText.text = targetWJ.objectData.name;
        UpdateUI();
    }


    public void SetActive(bool isActive_)
    {
        // Update variables
        if (isActive) 
        isActive = isActive_;
        targetAlpha = isActive ? 0.95f : 0.0f;
    }
}
