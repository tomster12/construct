
using UnityEngine;
using System.Collections.Generic;

public interface IConstruct
{
    bool CanMove();
    bool CanUseSkill();
    bool IsBlocking();

    void AimAtPosition(Vector3 pos);
    void MoveInDirection(Vector3 dir);
    
    void AddIPart(IConstructPart part);
    void RemoveIPart(IConstructPart part);
    void SubscribeSkill(Skill skill, string binding=null);
    void UnsubscribeSkill(Skill skill);
    void SubscribeMovement(ConstructPartMovement movement, int priority);
    void UnsubscribeMovement(ConstructPartMovement movement);
    
    bool ContainsObject(Object checkObject);
    bool ContainsIPart(IConstructPart checkIPart);
    IConstructPart GetCentreIPart();
    HashSet<IConstructPart> GetContainedIParts();
    IConstructCore GetICore();
    Vector3 GetPosition();
    Transform GetTransform();
    
    ConstructState GetState();
    bool SetState(ConstructState state);
    bool GetStateAccessible(ConstructState state);

    void OnMovementUpdate();
    void SubscribeOnLayoutChanged(System.Action action);
    void UnsubscribeOnLayoutChanged(System.Action action);
    void SubscribeOnStateChanged(System.Action<ConstructState> action);
    void UnsubscribeOnStateChanged(System.Action<ConstructState> action);
};
