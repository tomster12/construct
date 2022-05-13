
using System.Collections;


public enum CoreState { Detached, Attaching, Attached, Detaching };


public interface CoreMovementI : MovementI
{

    void attachCore(WorldObject targetWJ);
    void detachCore();

    IEnumerator attachCoreIE(WorldObject targetWJ);
    IEnumerator detachCoreIE();

    CoreState getCoreState();
}
