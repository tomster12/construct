
using System.Collections;
using UnityEngine;


public abstract class ConstructCoreMovement : ConstructObjectMovement
{
    [Header("Core References")]
    [SerializeField] protected ConstructCore controlledCC;

    public ShapeCoreAttachment shapeCoreAttachment { get; protected set; }
    public bool isTransitioning { get; private set; } = false;
    public override bool isBlocking => isTransitioning;


    public IEnumerator IE_Attach(ConstructObject targetCO)
    {
        if (isBlocking) yield break;

        // Update state and run main attach bits
        SetTransitioning(true);
        yield return StartCoroutine(IE_RunAttach(targetCO));
        SetTransitioning(false);
        SetCanActivate(false);
        SetActive(false);
        shapeCoreAttachment.SetActive(true);
    }

    public IEnumerator IE_Detach()
    {
        if (isBlocking) yield break;

        // Update state and run main detach bits
        shapeCoreAttachment.SetActive(false);
        SetTransitioning(true);
        yield return StartCoroutine(IE_RunDetach());
        SetTransitioning(false);
        SetCanActivate(true);
        shapeCoreAttachment.Clear();
    }

    protected abstract IEnumerator IE_RunAttach(ConstructObject targetCO);

    protected abstract IEnumerator IE_RunDetach();


    public virtual bool GetCanAttach(ConstructObject targetCO) => targetCO != null && !isBlocking && targetCO.isAttachable;

    protected virtual void SetTransitioning(bool isTransitioning_)
    {
        if (isPaused) throw new System.Exception("Cannot SetTransitioning() if isPaused");
        isTransitioning = isTransitioning_;
        if (isTransitioning) controlledCC.SetControlledBy(this);
        else if (isActive) controlledCC.SetControlledBy(this);
        else controlledCC.SetControlledBy(null);
    }
}
