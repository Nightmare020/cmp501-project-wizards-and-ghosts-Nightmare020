
using UnityEngine;
using UnityEngine.InputSystem;


public enum CurrentInputState
{
    Wizard,
    Ghost,
    UiNavigation
}

public class MyInputManager : MonoBehaviour
{
    [SerializeField] private CurrentInputState _inputMap;
    [SerializeField] private CanvasGroup reconectCanvas;
    private PlayerInput _playerInput;
    private InputActionAsset inputAsset;
    private InputActionMap wizardMap;
    private InputActionMap ghostMap;
    private InputActionMap uiNavigationMap;

    //input actions

    //wizard
    private InputAction wizard_Move;
    private InputAction wizard_Jump;
    private InputAction wizard_Dash;
    private InputAction wizard_Shoot;
    private InputAction wizard_Aim;
    private InputAction wizard_Pause;
    private InputAction wizard_Interact;

    //Ghost
    private InputAction ghost_Move;
    private InputAction ghost_Interact;
    private InputAction ghost_SetTrampoline;
    private InputAction ghost_Aim;
    private InputAction ghost_Pause;

    //ui navigation
    private InputAction navigation_Up;
    private InputAction navigation_Down;
    private InputAction navigation_Left;
    private InputAction navigation_Right;
    private InputAction navigation_Select;
    private InputAction navigation_Return;
    private InputAction navigation_Pause;

    public bool isMenues;


    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        _playerInput.onDeviceLost += DeviceLost;
        _playerInput.onDeviceRegained += DeviceRegained;
        inputAsset = _playerInput.actions;

        // Maps setup
        wizardMap = inputAsset.FindActionMap("Player");
        ghostMap = inputAsset.FindActionMap("Ghost");

        // Wizard inputs set up
        wizard_Move = wizardMap.FindAction("Movement");
        wizard_Jump = wizardMap.FindAction("Jump");
        wizard_Dash = wizardMap.FindAction("Dash");
        wizard_Shoot = wizardMap.FindAction("Shoot");
        wizard_Aim = wizardMap.FindAction("Aim");
        wizard_Pause = wizardMap.FindAction("Pause");
        wizard_Interact = wizardMap.FindAction("Interact");

        // Ghost input set up

        ghost_Move = ghostMap.FindAction("Movement");
        ghost_Aim = ghostMap.FindAction("Aim");
        ghost_Interact = ghostMap.FindAction("Interact");
        ghost_SetTrampoline = ghostMap.FindAction("Set Trampoline");
        ghost_Pause = ghostMap.FindAction("Pause");

        // UI navigation set up
        uiNavigationMap = inputAsset.FindActionMap("InterfaceNavigation");
        navigation_Up = uiNavigationMap.FindAction("Up");
        navigation_Down = uiNavigationMap.FindAction("Down");
        navigation_Left = uiNavigationMap.FindAction("Left");
        navigation_Right = uiNavigationMap.FindAction("Right");
        navigation_Select = uiNavigationMap.FindAction("Select");
        navigation_Return = uiNavigationMap.FindAction("Return");
        navigation_Pause = uiNavigationMap.FindAction("Pause");

        SetInputMap(_inputMap);
    }

    private void Start()
    {
        reconectCanvas.alpha = 0;
    }

    private void DeviceLost(PlayerInput playerInput)
    {
        Time.timeScale = 0;
        reconectCanvas.alpha = 1f;
    }
    private void DeviceRegained(PlayerInput playerInput)
    {
        Time.timeScale = 1;
        reconectCanvas.alpha = 0f;

    }

    public void SetInputMap(CurrentInputState map)
    {
        _inputMap = map;
        if (map == CurrentInputState.UiNavigation)
        {
            uiNavigationMap.Enable();
            wizardMap.Disable();
            ghostMap.Disable();
        }
        else
        {
            wizardMap.Enable();
            ghostMap.Enable();
            uiNavigationMap.Disable();
        }
    }

    #region UI Navigation

    public bool NavigationSelect()
    {
        return navigation_Select.WasPerformedThisFrame();
    }

    public bool NavigationReturn()
    {
        return navigation_Return.WasPerformedThisFrame();
    }

    public bool NavigationUp()
    {
        return navigation_Up.WasPerformedThisFrame();
    }

    public bool NavigationDown()
    {
        return navigation_Down.WasPerformedThisFrame();
    }

    public bool NavigationLeft()
    {
        return navigation_Left.WasPerformedThisFrame();
    }

    public bool NavigationRight()
    {
        return navigation_Right.WasPerformedThisFrame();
    }

    public bool NavigationPause()
    {
        return navigation_Pause.WasPerformedThisFrame();
    }

    #endregion

    #region Wizard

    public Vector2 WizardMovement()
    {
        return wizard_Move.ReadValue<Vector2>();
    }

    public Vector2 WizardAim()
    {
        return wizard_Aim.ReadValue<Vector2>();
    }

    public bool WizardJumpPerformedThisFrame()
    {
        return wizard_Jump.WasPerformedThisFrame();
    }

    public bool WizardJumpPressed()
    {
        return wizard_Jump.IsPressed();
    }

    public bool WizardDashPerformedThisFrame()
    {
        return wizard_Dash.WasPerformedThisFrame();
    }

    public bool WizardShootPerformedThisFrame()
    {
        return wizard_Shoot.WasPerformedThisFrame();
    }

    public bool WizardInteractPerformedThisFrame()
    {
        return wizard_Interact.WasPerformedThisFrame();
    }

    public bool WizardPausePerformed()
    {
        return wizard_Pause.WasPerformedThisFrame();
    }

    #endregion

    #region Ghost

    public Vector2 GhostMovement()
    {
        return ghost_Move.ReadValue<Vector2>();
    }

    public Vector2 GhostAim()
    {
        return ghost_Aim.ReadValue<Vector2>();
    }

    public bool GhostInteractPerformedThisFrame()
    {
        return ghost_Interact.WasPerformedThisFrame();
    }

    public bool GhostSetTrampolinePerformedThisFrame()
    {
        return ghost_SetTrampoline.WasPerformedThisFrame();
    }

    public bool GhostPausePerformed()
    {
        return ghost_Pause.WasPerformedThisFrame();
    }

    #endregion
}