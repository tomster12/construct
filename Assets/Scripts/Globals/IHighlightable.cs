
using UnityEngine;


public interface IHighlightable
{
    Vector3 IHGetPosition();
    bool IHGetHighlighted();
    ObjectType IHGetObjectType();
    void IHSetNearby(bool isNearby);
    void IHSetHighlighted(bool isHighlighted);
}
