
using UnityEngine;

public interface IHighlightable
{
    ObjectType GetObjectType();
    Vector3 GetPosition();
    bool GetIsNearby();
    bool GetIsHighlighted();
    void SetIsNearby(bool isNearby);
    void SetIsHighlighted(bool isHighlighted);
}
