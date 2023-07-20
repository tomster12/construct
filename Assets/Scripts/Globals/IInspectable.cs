
using UnityEngine;
using System.Collections.Generic;

public interface IInspectable
{
    Sprite IIGetIconSprite();
    string IIGetName();
    string IIGetDescription();
    Element IIGetElement();
    List<string> IIGetAttributes();
    List<string> IIGetModifiers();
    Vector3 IIGetPosition();
    float IIGetMass();
}
