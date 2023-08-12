
public class CoreAttachmentShapeHover : CoreAttachmentShape
{
    public override bool SetActive(bool isActive_)
    {
        if (!base.SetActive(isActive_)) return false;
        
        // Update attaching Core
        if (isActive)
        {
            attachingICore.GetObject().isLoose = false;
            attachingICore.GetObject().isFloating = true;
            attachingICore.GetObject().isColliding = false;
            attachingICore.GetTransform().parent = attachedIPart.GetTransform();
        }
        else
        {
            attachingICore.GetTransform().parent = attachingICore.GetIConstruct().GetTransform();
        }

        return true;
    }
}
