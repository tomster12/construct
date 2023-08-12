
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class ConstructLayout : MonoBehaviour
{
    [Header("References")]
    [SerializeReference] private Transform IConstructTransform;
    [SerializeField] private GameObject layoutElementPfb;
    [SerializeField] private RectTransform layoutElements;
    [SerializeField] private Canvas canvas;

    private IConstruct IConstruct;


    private void Awake()
    {
        IConstruct = IConstructTransform.GetComponent<IConstruct>();
        IConstruct.SubscribeOnLayoutChanged(UpdateLayout);
        UpdateLayout();
    }


    private void UpdateLayout()
    {
        foreach (Transform child in layoutElements.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (IConstructPart ITargetPart in IConstruct.GetContainedIParts())
        {
            GameObject layoutElement = Instantiate(layoutElementPfb);
            layoutElement.transform.SetParent(layoutElements);
            layoutElement.GetComponent<ConstructLayoutElement>().SetIConstructPart(ITargetPart);
        }
        Canvas.ForceUpdateCanvases();
    }
}
