using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Utils;

[DefaultExecutionOrder(2)]
[RequireComponent(typeof(NavegationSounds))]
public class StartMenuManager : MonoBehaviour
{
    // Botones del menu de inicio
    [SerializeField] private Button newGameButton, continueButton, settingsButton, arcadeButton;

    private Button[] buttons; // Array para almacenar todos los botones
    private int currentButtonIndex = 0; // Índice del botón actualmente seleccionado

    // Posiciones y rotaciones iniciales de los botones
    private Dictionary<Button, Vector3> initialPositions = new Dictionary<Button, Vector3>();

    // Propiedades para el efecto de levitación
    [SerializeField] float levitationRadius = 5f;
    [SerializeField] float levitationSpeed = 1f;

    //Save Data Stuff
    private JsonSaving _jsonSaving;

    private SaveData _saveData;

    //inputs 
    private MyInputManager _inputManager;
    private NavegationSounds _navegationSounds;

    // Start is called before the first frame update
    private void Awake()
    {
        //inputs
        _inputManager = FindObjectOfType<MyInputManager>();
        _navegationSounds = GetComponent<NavegationSounds>();
    }

    void Start()
    {
        //save
        _jsonSaving = FindObjectOfType<JsonSaving>();
        _saveData = _jsonSaving._saveData;

        buttons = new[] { newGameButton, continueButton, arcadeButton, settingsButton };

        // Guardamos la posición y rotación inicial en la que se encuentran los botones
        foreach (Button button in buttons)
        {
            // Guardar las posiciones y rotaciones iniciales
            initialPositions.Add(button, button.transform.position);
        }

        newGameButton.onClick.AddListener(CreateNewGame);
        continueButton.onClick.AddListener(ContinueGame);
        settingsButton.onClick.AddListener(OpenSettings);
        arcadeButton.onClick.AddListener(Arcade);
        // El botón de continuar se desactiva al inicio, si no tenemos checkpoints ni hemos empezado el juego
        CheckPointExistance();

        currentButtonIndex = 0;

        // Establecer el primer botón como seleccionado al inicio
        // EventSystem.current.SetSelectedGameObject(buttons[currentButtonIndex].gameObject);
    }

    private void Update()
    {
        // Manejar la navegación entre botones usando el gamepad

        if (_inputManager.NavigationDown())
        {
            SelectNext();

            //play sound
            _navegationSounds.PlayScrollSound();
        }
        else if (_inputManager.NavigationUp())
        {
            SelectPrev();
            //play sound
            _navegationSounds.PlayScrollSound();
        }
        else if (_inputManager.NavigationSelect())
        {
            // Ejecutar la acción del botón seleccionado
            buttons[currentButtonIndex].onClick.Invoke();
            //play sound
            _navegationSounds.PlaySelectSound();
        }
        else if (_inputManager.NavigationReturn())
        {
            QuitGame();
            //play sound
            _navegationSounds.PlaySelectSound();
        }

        // Efecto de levitación
        LevitationEffect();
    }

    void SelectNext()
    {
        currentButtonIndex = (currentButtonIndex + 1) % buttons.Length;
    }

    void SelectPrev()
    {
        currentButtonIndex = (currentButtonIndex - 1) < 0 ? buttons.Length - 1 : (currentButtonIndex - 1);
    }

    // Método que comprueba si existe un checkpoint
    void CheckPointExistance()
    {
        // int lastCheckpoint = PlayerPrefs.GetInt("LastCheckpoint", -1);
        int lastCheckpoint = _saveData.GetCurrentLevel();
        continueButton.interactable = (lastCheckpoint != -1);
    }


    #region Button Actions

    public void CreateNewGame()
    {
        // PlayerPrefs.DeleteKey("LastCheckpoint");
        //set the level started and save it
        _saveData.SetCurrentLevel(0);
        _jsonSaving.SaveTheData();

        CheckPointExistance(); // Actualiza el estado del botón de continue
        MySceneLoader.LoadTutorial();
        // SceneManager.LoadScene("Level1");
    }

    public void ContinueGame()
    {
        // Se obtiene el último checkpoint almacenado
        //int lastCheckpoint = PlayerPrefs.GetInt("LastCheckpoint", -1);
        int lastCheckpoint = _saveData.GetCurrentLevel();

        if (lastCheckpoint != -1)
        {
            // Carga el nivel desde el checkpoint
            MySceneLoader.LoadLevel(5);
            // SceneManager.LoadScene("Level" + lastCheckpoint);
        }
    }

    public void OpenSettings()
    {
        MySceneLoader.LoadSettings();
        // SceneManager.LoadScene("SettingsMenu");
    }

    public void QuitGame()
    {
        // Debug.Log("Saliendo del juego");
        Application.Quit();
    }

    public void Arcade()
    {
        MySceneLoader.LoadLeaderBoards();
    }

    #endregion

    private void LevitationEffect()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            if (i == currentButtonIndex)
            {
                float angle = Time.time * levitationSpeed;
                float offsetX = Mathf.Sin(angle) * levitationRadius;
                float offsetY = Mathf.Cos(angle) * levitationRadius;
                buttons[i].transform.position = initialPositions[buttons[i]] + new Vector3(offsetX, offsetY, 0f);
            }
            else
            {
                // Restablecer posición cuando no está seleccionado
                buttons[i].transform.position = initialPositions[buttons[i]];
            }
        }
    }
}