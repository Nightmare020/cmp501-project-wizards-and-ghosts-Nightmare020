using UnityEngine;

public class PlayersSelectionInputs : MonoBehaviour
{
    // Reference to the SelectionWizardGhost component
    private SelectionWizardGhost _selectionPanel;

    // Reference to the MyInputManager component
    private MyInputManager _input;

    private void Awake()
    {
        // Find and assign the SelectionWizardGhost component in the scene
        _selectionPanel = FindObjectOfType<SelectionWizardGhost>();

        // Find and assign the MyInputManager component in the scene
        _input = FindObjectOfType<MyInputManager>();
    }

    private void Start()
    {
        // Initialize the selection panel with the input manager if it exists
        if (_selectionPanel != null)
        {
            _selectionPanel.Initialize(_input);
        }
    }

    void Update()
    {
        if (_selectionPanel)
        {
            // Confirm selection
            if (_input.NavigationSelect())
            {
                _selectionPanel.PlayerAccept();
            }

            // Select Wizard role
            if (_input.NavigationLeft())
            {
                _selectionPanel.SelectLeft();
            }

            // Select Ghost role
            if (_input.NavigationRight())
            {
                _selectionPanel.SelectRight();
            }
        }
    }
}