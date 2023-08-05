
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class ConstructLayout : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Construct construct;
    [SerializeField] private GameObject layoutElementPfb;
    [SerializeField] private RectTransform layoutElements;
    [SerializeField] private Canvas canvas;


    private void Awake()
    {
        construct.onLayoutChanged += UpdateLayout;
        UpdateLayout();
    }


    private void UpdateLayout()
    {
        foreach (Transform child in layoutElements.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (ConstructObject targetCO in construct.trackedObjects)
        {
            GameObject layoutElement = Instantiate(layoutElementPfb);
            layoutElement.transform.SetParent(layoutElements);
            layoutElement.GetComponent<ConstructLayoutElement>().SetConstructObject(targetCO);
        }
        Canvas.ForceUpdateCanvases();
    }
}
