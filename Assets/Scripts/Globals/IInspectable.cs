
using UnityEngine;
using System.Collections.Generic;

public interface IInspectable
{
    InspectedData Inspect();
    List<string> GetAttributes();
    List<string> GetModifiers();
    Vector3 GetPosition();
    Object GetObject();
}
