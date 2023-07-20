
public class ShapeHoverAttachment : ShapeCoreAttachment
{
    public override bool SetActive(bool isActive_)
    {
        if (!base.SetActive(isActive_)) return false;
        
        // Update attaching CC
        if (isActive)
        {
            attachingCC.baseWO.isLoose = false;
            attachingCC.baseWO.isFloating = true;
            attachingCC.baseWO.isColliding = false;
            attachingCC.transform.parent = attachedCO.transform;
        }
        else
        {
            attachingCC.transform.parent = attachingCC.construct.transform;
        }

        return true;
    }
}
