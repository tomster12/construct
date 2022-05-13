
using UnityEngine;


public interface IMovable
{
    void MoveInDirection(Vector3 dir);
    void AimAtPosition(Vector3 pos);

    bool GetControlled();
    void SetControlled(bool isControlled_);
}
