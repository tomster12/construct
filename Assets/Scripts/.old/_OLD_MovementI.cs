
using UnityEngine;


public interface MovementI
{
    void moveInDirection(Vector3 dir, float strength);
    void aimAtPosition(Vector3 pos, float strength);
    void attack(Object targetWJ, Vector3 aimedPos);

    bool canAttack(Object targetWJ, Vector3 aimedPos);
    void setActive(bool active);
    bool getActive();
    StatList getStats();
}
