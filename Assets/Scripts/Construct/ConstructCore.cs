
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class ConstructCore : ConstructPart, IConstructCore
{
    private ConstructCoreData coreData => (ConstructCoreData)partData;
    private ConstructCoreMovement inherentCoreMovement => (ConstructCoreMovement)inherentMovement;
    private CoreAttachmentShape attachmentShape => inherentCoreMovement.attachmentShape;
    private CoreAttachmentState state = CoreAttachmentState.DETACHED;
    private IConstructPart attachedIPart;
    protected override int movementPriority => 0;

    public bool CanTransition() => IsConstructed() && !IConstruct.IsBlocking() && !inherentCoreMovement.IsBlocking();
    public bool CanAttach(IConstructPart checkIPart)
    {
        return checkIPart != null && !checkIPart.IsConstructed()
            && CanTransition() && IsDetached()
            && inherentCoreMovement.CanAttach(checkIPart);
    }
    public bool CanDetach() => CanTransition() && IsAttached();
    public bool IsAttached() => state == CoreAttachmentState.ATTACHED;
    public bool IsDetached() => state == CoreAttachmentState.DETACHED;
    public bool IsTransitioning() => state == CoreAttachmentState.ATTACHING || state == CoreAttachmentState.DETACHING;

    public override bool IsBlocking() => IsTransitioning();
    public override bool IsAttachable() => false;


    public void Attach(IConstructPart targetIPart)
    {
        StartCoroutine(IEAttach(targetIPart));
    }

    public void Detach() => StartCoroutine(IEDetach());

    public override bool ContainsObject(Object checkObject) => state == CoreAttachmentState.ATTACHED ? attachedIPart.ContainsObject(checkObject) : baseObject == checkObject;

    public override bool ContainsIPart(IConstructPart checkIPart) => state == CoreAttachmentState.ATTACHED ? attachedIPart.ContainsIPart(checkIPart) : checkIPart == (IConstructCore)this;

    public override IConstructPart GetCentreIPart() => state == CoreAttachmentState.ATTACHED ? attachedIPart.GetCentreIPart() : this;

    public override List<string> GetAttributes() => new List<string>()
    {
        "Health: " + coreData.health,
        "Energy: " + coreData.energy + " (" + coreData.energyRegen + "/s)",
        "Max Construct Size: " + coreData.maxConstructSize
    };

    public override List<string> GetModifiers() => new List<string>();

    public CoreAttachmentShape GetAttachmentShape() => attachmentShape;

    private IEnumerator IEAttach(IConstructPart targetIPart)
    {
        if (!CanAttach(targetIPart)) yield break;

        // Tell movement to attach and update state
        state = CoreAttachmentState.ATTACHING;
        yield return inherentCoreMovement.IEAttach(targetIPart);
        state = CoreAttachmentState.ATTACHED;

        // Add object to construct and set movement
        attachedIPart = targetIPart;
        attachedIPart.GetTransform().parent = IConstruct.GetTransform();
        IConstruct.AddIPart(attachedIPart);
    }

    private IEnumerator IEDetach()
    {
        if (!CanDetach()) yield break;

        // Remove object from construct and parent to world
        attachedIPart.GetTransform().parent = IConstruct.GetTransform().parent;
        IConstruct.RemoveIPart(attachedIPart);
        attachedIPart = null;

        // Tell movement to detach and update state
        state = CoreAttachmentState.DETACHING;
        yield return inherentCoreMovement.IEDetach();
        state = CoreAttachmentState.DETACHED;
    }
}
