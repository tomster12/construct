
using System.Collections;


public enum CoreAttachmentState { Detached, Attaching, Attached, Detaching };


public interface CoreMovementI : MovementI
{

    void attachCore(Object targetWJ);
    void detachCore();

    IEnumerator attachCoreIE(Object targetWJ);
    IEnumerator detachCoreIE();

    CoreAttachmentState getCoreAttachmentState();
}
