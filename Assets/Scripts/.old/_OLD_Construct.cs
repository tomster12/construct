
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Construct : MonoBehaviour
{
    // Declare variables
    public List<Object> orbWJs { get; private set; } = new List<Object>();
    public Object coreWJ { get; private set; }
    public CoreMovementI coreMovement { get; private set; }
    public Object mainOrbWJ { get; private set; }
    public MovementI mainOrbMovement { get; private set; }


    public void initConstruct(Object coreWJ_)
    {
        // Setup core variables
        coreWJ = coreWJ_;
        coreWJ.transform.parent = transform;
        coreMovement = coreWJ.GetComponent<CoreMovementI>();

        // Control core by default
        mainOrbWJ = coreWJ;
        mainOrbMovement = coreMovement;
    }

    public void terminateConstruct()
    {
        // Stop completely being a construct
        mainOrbMovement.setActive(false);
        foreach (Object wj in orbWJs) wj.transform.parent = transform;
        Destroy(gameObject);
    }


    public void moveInDirection(Vector3 dir, float force)
    {
        // Pass movement onto correct MovementI
        if (mainOrbMovement != null) mainOrbMovement.moveInDirection(dir, force);
    }

    public void aimAtPosition(Vector3 pos, float force)
    {
        // Pass aim onto correct MovementI
        if (mainOrbMovement != null) mainOrbMovement.aimAtPosition(pos, force);
    }


    public void interact(Object targetWJ, Vector3 aimedPos)
    {
        // Try attach core (*1)
        if (targetWJ != null
          && getCoreAttachmentState() == CoreAttachmentState.Detached
          && !getContainsWJ(targetWJ)) attachCore(targetWJ, aimedPos);

        // Attack in direction
        else if (getCoreAttachmentState() == CoreAttachmentState.Attached) mainOrbMovement.attack(targetWJ, aimedPos);
    }

    public bool canInteract(Object targetWJ)
    {
        // Can attach core (*1)
        bool canAttach = (targetWJ != null
          && getCoreAttachmentState() == CoreAttachmentState.Detached
          && !getContainsWJ(targetWJ));

        return canAttach;
    }


    public void setActive(bool active_) => mainOrbMovement.setActive(active_);


    public void setKinematic(bool kinematic_)
    {
        if (getCoreAttachmentState() == CoreAttachmentState.Attached)
        {
            // Set isKinematic on all world objects
            foreach (Object orbWJ in orbWJs)
                orbWJ.rb.isKinematic = kinematic_;

        }
        else if (getCoreAttachmentState() == CoreAttachmentState.Detached)
        {
            coreWJ.rb.isKinematic = kinematic_;
        }
    }


    private void setControlled(Object wj)
    {
        // Update current controlled world object
        if (mainOrbMovement != null) mainOrbMovement.setActive(false);
        mainOrbWJ = wj;
        mainOrbMovement = mainOrbWJ.GetComponent<MovementI>();
        if (mainOrbMovement != null) mainOrbMovement.setActive(true);
    }


    public bool getContainsWJ(Object wj)
    {
        // Check whether WJ is within the construct
        return coreWJ == wj || orbWJs.Contains(wj);
    }

    // #endregion


    #region Core

    public void attachCore(Object targetWJ, Vector3 aimedPos) { StartCoroutine(attachCoreIE(targetWJ)); }

    public IEnumerator attachCoreIE(Object targetWJ)
    {
        if (getCoreAttachmentState() == CoreAttachmentState.Detached)
        {
            // Wait for core to attach - Also setActive(false) on core
            yield return coreMovement.attachCoreIE(targetWJ);

            // Handle attaching to world object
            setControlled(targetWJ);
            orbWJs.Add(targetWJ);
            mainOrbWJ.transform.parent = transform;
        }
    }


    public void detachCore() { StartCoroutine(detachCoreIE()); }

    public IEnumerator detachCoreIE()
    {
        if (getCoreAttachmentState() == CoreAttachmentState.Attached)
        {

            // Handle detaching from current world
            setControlled(coreWJ);
            foreach (Object orbWJ in orbWJs)
                orbWJ.transform.parent = transform;
            orbWJs.Clear();

            // Update core parent
            coreWJ.transform.parent = transform;

            // Tell core to detach - Also setActive(true) on core
            yield return coreMovement.detachCoreIE();
        }
    }


    public CoreAttachmentState getCoreAttachmentState() => coreMovement.getCoreAttachmentState();

    #endregion
}
