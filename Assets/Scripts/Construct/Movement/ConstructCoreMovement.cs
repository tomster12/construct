
using System.Collections;
using UnityEngine;


public abstract class  ConstructCoreMovement : ConstructPartMovement
{
    [Header("Core References")]
    [SerializeField] protected ConstructCore _controlledCore;

    public CoreAttachmentShape attachmentShape { get; protected set; }
    public bool isTransitioning { get; private set; } = false;
    public override bool IsBlocking() => isTransitioning;

    protected IConstructCore controlledICore => _controlledCore;


    public virtual bool CanAttach(IConstructPart targetIPart) => targetIPart != null && !IsBlocking() && targetIPart.IsAttachable();

    public IEnumerator IEAttach(IConstructPart targetIPart)
    {
        if (IsBlocking()) yield break;

        // Update state and run main attachment
        SetTransitioning(true);
        yield return StartCoroutine(IEAttachImpl(targetIPart));
        SetTransitioning(false);
        SetCanActivate(false);
        SetActive(false);
        attachmentShape.SetActive(true);
    }

    public IEnumerator IEDetach()
    {
        if (IsBlocking()) yield break;

        // Update state and run main detachment
        attachmentShape.SetActive(false);
        SetTransitioning(true);
        yield return StartCoroutine(IEDetachImpl());
        SetTransitioning(false);
        SetCanActivate(true);
        attachmentShape.Clear();
    }


    protected abstract IEnumerator IEAttachImpl(IConstructPart targetIPart);

    protected abstract IEnumerator IEDetachImpl();

    protected virtual void SetTransitioning(bool isTransitioning_)
    {
        if (isPaused) throw new System.Exception("Cannot SetTransitioning() if isPaused");
        isTransitioning = isTransitioning_;
        if (isTransitioning) controlledICore.SetControlledBy(this);
        else if (isActive) controlledICore.SetControlledBy(this);
        else controlledICore.SetControlledBy(null);
    }
}
