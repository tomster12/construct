
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerController : MonoBehaviour
{

    // Declare enums, references, variables
    public enum GameState { Playing, Forging };

    private PlayerConstructController constructController;
    private PlayerForgingController forgingController;

    [SerializeField] private GameState gameState;


    private void Awake()
    {
        // Initialize references
        constructController = GetComponent<PlayerConstructController>();
        forgingController = GetComponent<PlayerForgingController>();
    }


    private void Start()
    {
        // Initialize as playing
        setState(GameState.Playing, true);
    }


    private void Update()
    {
        // [Toggle Forging]: on tab
        if (Input.GetKeyDown("tab"))
        {

            if (gameState == GameState.Playing
            && constructController.controlledConstruct.getCoreAttachmentState() != CoreAttachmentState.Attaching
            && constructController.controlledConstruct.getCoreAttachmentState() != CoreAttachmentState.Detaching)
            {
                setState(GameState.Forging);

            }
            else if (gameState == GameState.Forging)
            {
                setState(GameState.Playing);
            }
        }
    }


    private void setState(GameState newState, bool force = false)
    {
        // Ensure different state
        if (newState == gameState && !force) return;

        // Handle controllers / cameras
        if (newState == GameState.Playing)
        {
            if (forgingController.enabled) forgingController.setActive(false);
            if (!constructController.enabled) constructController.setActive(true);

        }
        else if (newState == GameState.Forging)
        {
            if (constructController.enabled) constructController.setActive(false);
            if (!forgingController.enabled) forgingController.setActive(true);
        }

        // Update gameState
        gameState = newState;
    }
}
