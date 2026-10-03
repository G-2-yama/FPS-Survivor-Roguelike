using UnityEngine.InputSystem;

public class GameplayInputController
{
    private readonly PlayerInput playerInput;

    public GameplayInputController(PlayerInput playerInput)
    {
        this.playerInput = playerInput;
    }

    public void UpdateInput(GameState currentState)
    {
        if (currentState == null || playerInput == null)
        {
            return;
        }

        bool shouldBeActive = currentState.AcceptsPlayerInput;
        if (playerInput.inputIsActive == shouldBeActive)
        {
            return;
        }

        if (shouldBeActive)
        {
            playerInput.ActivateInput();
        }
        else
        {
            playerInput.DeactivateInput();
        }
    }
}
