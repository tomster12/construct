
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
        construct.onChanged += UpdateLayout;
    }


    private void UpdateLayout()
    {
        Debug.Log("Updating to " + construct.trackedObjects.Count);
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
