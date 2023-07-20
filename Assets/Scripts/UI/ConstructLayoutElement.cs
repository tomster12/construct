
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConstructLayoutElement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image imageIcon;
    [SerializeField] private TextMeshProUGUI textLevel;
    [SerializeField] private RectTransform hpBarFill;
    [SerializeField] private RectTransform xpBarFill;

    private ConstructObject targetCO;


    public void SetConstructObject(ConstructObject targetCO_)
    {
        targetCO = targetCO_;
        imageIcon.sprite = targetCO.IIGetIconSprite();
        UpdateContents();
    }


    private void Update()
    {
        UpdateContents();
    }

    private void UpdateContents()
    {
        float hpProgress = 1.0f;
        hpBarFill.localScale =  new Vector3(
            hpProgress * hpBarFill.parent.localScale.x,
            1.0f * hpBarFill.parent.localScale.y,
            1.0f * hpBarFill.parent.localScale.z);

        float xpProgress = 0.2f;
        xpBarFill.localScale =  new Vector3(
            xpProgress * hpBarFill.parent.localScale.x,
            1.0f * hpBarFill.parent.localScale.y,
            1.0f * hpBarFill.parent.localScale.z);

        int level = 1;
        textLevel.text = level.ToString();
    }
}
