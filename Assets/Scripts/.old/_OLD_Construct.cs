
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Construct : MonoBehaviour
{
    // Declare variables
    public List<WorldObject> orbWJs { get; private set; } = new List<WorldObject>();
    public WorldObject coreWJ { get; private set; }
    public CoreMovementI coreMovement { get; private set; }
    public WorldObject mainOrbWJ { get; private set; }
    public MovementI mainOrbMovement { get; private set; }


    public void initConstruct(WorldObject coreWJ_)
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
        foreach (WorldObject wj in orbWJs) wj.transform.parent = transform;
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


    public void interact(WorldObject targetWJ, Vector3 aimedPos)
    {
        // Try attach core (*1)
        if (targetWJ != null
          && getCoreState() == CoreState.Detached
          && !getContainsWJ(targetWJ)) attachCore(targetWJ, aimedPos);

        // Attack in direction
        else if (getCoreState() == CoreState.Attached) mainOrbMovement.attack(targetWJ, aimedPos);
    }

    public bool canInteract(WorldObject targetWJ)
    {
        // Can attach core (*1)
        bool canAttach = (targetWJ != null
          && getCoreState() == CoreState.Detached
          && !getContainsWJ(targetWJ));

        return canAttach;
    }


    public void setActive(bool active_) => mainOrbMovement.setActive(active_);


    public void setKinematic(bool kinematic_)
    {
        if (getCoreState() == CoreState.Attached)
        {
            // Set isKinematic on all world objects
            foreach (WorldObject orbWJ in orbWJs)
                orbWJ.rb.isKinematic = kinematic_;

        }
        else if (getCoreState() == CoreState.Detached)
        {
            coreWJ.rb.isKinematic = kinematic_;
        }
    }


    private void setControlled(WorldObject wj)
    {
        // Update current controlled world object
        if (mainOrbMovement != null) mainOrbMovement.setActive(false);
        mainOrbWJ = wj;
        mainOrbMovement = mainOrbWJ.GetComponent<MovementI>();
        if (mainOrbMovement != null) mainOrbMovement.setActive(true);
    }


    public bool getContainsWJ(WorldObject wj)
    {
        // Check whether WJ is within the construct
        return coreWJ == wj || orbWJs.Contains(wj);
    }

    // #endregion


    #region Core

    public void attachCore(WorldObject targetWJ, Vector3 aimedPos) { StartCoroutine(attachCoreIE(targetWJ)); }

    public IEnumerator attachCoreIE(WorldObject targetWJ)
    {
        if (getCoreState() == CoreState.Detached)
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
        if (getCoreState() == CoreState.Attached)
        {

            // Handle detaching from current world
            setControlled(coreWJ);
            foreach (WorldObject orbWJ in orbWJs)
                orbWJ.transform.parent = transform;
            orbWJs.Clear();

            // Update core parent
            coreWJ.transform.parent = transform;

            // Tell core to detach - Also setActive(true) on core
            yield return coreMovement.detachCoreIE();
        }
    }


    public CoreState getCoreState() => coreMovement.getCoreState();

    #endregion
}
