
using UnityEngine;


public class CoreAttachmentShape : ConstructShape
{
    public override bool canActivate => base.canActivate && attachedIPart != null && attachingICore != null;
    public IConstructCore attachingICore {  get; private set; }
    public IConstructPart attachedIPart { get; private set; }


    public override bool SetActive(bool isActive_)
    {
        if (!base.SetActive(isActive_)) return false;
        if (isActive)
        {
            attachingICore.SetControlledBy(this);
            attachingICore.OnJoinShape(this);
            attachedIPart.OnJoinShape(this);
        }
        else
        {
            attachingICore.SetControlledBy(null);
            attachingICore.OnExitShape(this);
            attachedIPart.OnExitShape(this);
        }
        return true;
    }

    public void SetAttachingICore(IConstructCore attachingICore_)
    {
        if (attachingICore == attachingICore_) return;
        if (attachingICore != null && attachingICore_ != null) throw new System.Exception("Cannot SetAttachingICore() if already set.");
        attachingICore = attachingICore_;
    }
    
    public void SetAttachedIPart(IConstructPart attachedIPart_)
    {
        if (attachedIPart == attachedIPart_) return;
        if (attachedIPart != null && attachedIPart_ != null) throw new System.Exception("Cannot SetAttachedIPart() if already set.");
        attachedIPart = attachedIPart_;
    }


    public void Clear()
    {
        // Clear variables
        attachingICore = null;
        attachedIPart = null;
    }
}
