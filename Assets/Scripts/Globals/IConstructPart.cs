
using UnityEngine;

public interface IConstructPart : IHighlightable, IInspectable
{
    bool IsConstructed();
    bool IsControlled();
    bool IsBlocking();
    bool IsAttachable();

    bool ContainsObject(Object checkObject);
    bool ContainsIPart(IConstructPart checkIPart);
    IConstructPart GetCentreIPart();
    IConstruct GetIConstruct();
    new Vector3 GetPosition();
    Transform GetTransform();
    new Object GetObject();
    IObjectController GetIController();
    Quaternion GetForwardRot();
    bool SetControlledBy(IObjectController controller);

    void OnJoinConstruct(Construct construct);
    void OnExitConstruct();
    void OnJoinShape(ConstructShape shape);
    void OnExitShape(ConstructShape shape);
};
