
using UnityEngine;


public class UIController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject ingameUI;
    [SerializeField] private GameObject forgingUI;


    public void SetIngame(bool ingame) => ingameUI.SetActive(ingame);
    public void SetForging(bool forging) => forgingUI.SetActive(forging);
}
