using UnityEngine.InputSystem;

public class FirstPersonPlayer : UnityEngine.MonoBehaviour
{
    private MaroonInputActions _maroonInputActions = null;

    private void Start()
    {
        UnityEngine.Debug.Log("First person player start");

        // Get input action definitions
        _maroonInputActions = Maroon.GlobalInputManager.Instance.MaroonInputActions;

        // Link input actions to functions
        _maroonInputActions.FirstPersonPlayer.Jump.performed += context => InputJump();
    }

    void Movement()
    {

    }

    void InputJump()
    {
        UnityEngine.Debug.Log("INPUT SYS JUMP");
    }
}