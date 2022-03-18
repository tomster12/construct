
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface MovementI {

  void moveInDirection(Vector3 dir, float strength);

  void aimAtPosition(Vector3 pos, float strength);

  void attack(WorldObject targetWJ, Vector3 aimedPos);


  bool canAttack(WorldObject targetWJ, Vector3 aimedPos);

  void setActive(bool active);

  bool getActive();

  StatList getStats();
}
