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

    private void Start()
    {
        _selectionPanel.Initialize(_input);
    }

    // Update is called once per frame
    void Update()
    {
        if (_selectionPanel)
        {
            //confirm
            if (_input.NavigationSelect())
            {
                _selectionPanel.PlayerAccept();
            }

            //left 
            if (_input.NavigationLeft())
            {
                _selectionPanel.SelectLeft();
            }

            //right
            if (_input.NavigationRight())
            {
                _selectionPanel.SelectRight();
            }
            //exit
        }
    }
}