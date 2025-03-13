using UnityEngine;

public class PlayersSelectionInputs : MonoBehaviour
{
    private SelectionWizardGhost _selectionPanel;
    private MyInputManager _input;

    private void Awake()
    {
        _selectionPanel = FindObjectOfType<SelectionWizardGhost>();
        _input = FindObjectOfType<MyInputManager>();
    }


    // Update is called once per frame
    void Update()
    {
        if (_selectionPanel && _selectionPanel.inputEnabled)
        {
            //confirm
            if (_input.NavigationSelect())
            {
                _selectionPanel.PlayerAccept();
            }

            //left 
            if (_input.NavigationLeft())
            {
                _selectionPanel.SelectLeft(_input);
            }

            //right
            if (_input.NavigationRight())
            {
                _selectionPanel.SelectRight(_input);
            }
            //exit
        }
    }
}