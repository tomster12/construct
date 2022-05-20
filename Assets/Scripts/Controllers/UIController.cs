
using UnityEngine;


public class UIController : MonoBehaviour
{
    // Declare references
    [Header("References")]
    [SerializeField] private DataViewer dataViewer;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private GameObject ingameUI;
    [SerializeField] private GameObject forgingUI;


    public void SetIngame(bool ingame) => ingameUI.SetActive(ingame);
    public void SetForging(bool forging) => forgingUI.SetActive(forging);
}
