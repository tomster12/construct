
using UnityEngine;

public interface IConstructCore : IConstructPart
{
    bool CanTransition();   
    bool CanAttach(IConstructPart part);
    bool CanDetach();
    bool IsAttached();
    bool IsDetached();
    bool IsTransitioning();

    void Attach(IConstructPart part);
    void Detach();

    CoreAttachmentShape GetAttachmentShape();
};
