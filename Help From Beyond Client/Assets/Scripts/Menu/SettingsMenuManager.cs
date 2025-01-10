using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Mathematics;
using UnityEngine.Audio;
using Utils;

[RequireComponent(typeof(NavegationSounds))]
public class SettingsMenuManager : MonoBehaviour
{
    //parameters
    // Botonoes del menu de configuraci�n
    [SerializeField] private GameObject resolutionObject, fullScreenObject, volumeObject;
    [SerializeField] TMP_Dropdown resolutionDropdown;
    [SerializeField] Toggle fullscreenToggle;
    [SerializeField] Slider volumeSlider;
    [SerializeField] Button DumpButton;
    [SerializeField] AudioMixer mixer;


    // Posiciones y rotaciones iniciales de los botones
    private Dictionary<GameObject, Vector3> initialPositions = new Dictionary<GameObject, Vector3>();

    // Propiedades para el efecto de levitaci�n
    [SerializeField] float levitationRadius = 5f;
    [SerializeField] float levitationSpeed = 1f;

    //components
    private NavegationSounds _navegationSounds;
    private MyInputManager _inputManager; //input manager


    //data
    private GameObject[] items;
    private int selectedItemIndex = 0;
    private bool isDropdownSelected = false; // Variable para controlar si el Dropdown est� seleccionado

    Resolution screenResolution;

    //Save data
    private JsonSaving _jsonSaving;

    private void Awake()
    {
        _inputManager = FindObjectOfType<MyInputManager>();
    }

    // Start is called before the first frame update
    void Start()
    {
        _navegationSounds = gameObject.GetComponent<NavegationSounds>();
        _jsonSaving = FindObjectOfType<JsonSaving>();
        DumpButton.onClick.AddListener(() =>
        {
            _jsonSaving.dumpData();
            DumpButton.interactable = false;
        });

        // Agregamos listeners para eventos de cambio en los controles
        resolutionDropdown.onValueChanged.AddListener(ChangeResolution);
        fullscreenToggle.onValueChanged.AddListener(ChangeFullScreen);
        volumeSlider.onValueChanged.AddListener(ChangeVolume);

        // Establecer el primer bot�n como seleccionado al inicio
        // EventSystem.current.SetSelectedGameObject(resolutionDropdown.gameObject);


        items = new[] { resolutionObject, fullScreenObject, volumeObject, DumpButton.gameObject };
        selectedItemIndex = 0;
        isDropdownSelected = true;

        // Almacenar las posiciones iniciales de los elementos
        for (int i = 0; i < items.Length; i++)
        {
            SaveInitialPosition(items[i]);

        }
    }

    void SaveInitialPosition(GameObject obj)
    {
        initialPositions[obj] = obj.transform.position;
    }

    private void Update()
    {
        if (_inputManager.NavigationDown())
        {
            if (isDropdownSelected && resolutionDropdown.IsExpanded)
            {
                SelectNextInDropDown();
            }
            else
            {
                SelectNext();
            }

            //play sound
            _navegationSounds.PlayScrollSound();
        }
        else if (_inputManager.NavigationUp())
        {
            if (isDropdownSelected && resolutionDropdown.IsExpanded)
            {
                SelectPrevInDropDown();
            }
            else
            {
                SelectPrev();
            }

            //play sound
            _navegationSounds.PlayScrollSound();
        }
        else if (_inputManager.NavigationRight())
        {
            if (selectedItemIndex == 2)
            {
                float value = Mathf.Clamp01(volumeSlider.value + 0.05f);
                ChangeVolume(value);
                volumeSlider.value = value;
                //play sound 
                _navegationSounds.PlaySliderSound();
            }
        }
        else if (_inputManager.NavigationLeft())
        {
            if (selectedItemIndex == 2)
            {
                float value = Mathf.Clamp01(volumeSlider.value - 0.05f);
                ChangeVolume(value);
                volumeSlider.value = value;
                //play sound 
                _navegationSounds.PlaySliderSound();
            }
        }
        else if (_inputManager.NavigationSelect())
        {
            if (isDropdownSelected && !resolutionDropdown.IsExpanded)
            {
                resolutionDropdown.Show();
            }
            else if (!isDropdownSelected)
            {
                if (selectedItemIndex == 1)
                {
                    ChangeFullScreen(!fullscreenToggle.isOn);
                }
                else if (selectedItemIndex == 3)
                {
                    DumpButton.onClick.Invoke();
                }
            }

            //play sound 
            _navegationSounds.PlaySelectSound();
        }
        else if (_inputManager.NavigationReturn())
        {
            if (isDropdownSelected && resolutionDropdown.IsExpanded)
            {
                resolutionDropdown.Hide();
                isDropdownSelected = false;
            }
            else
            {
                Return();
            }

            //play sound 
            _navegationSounds.PlaySelectSound();
        }

        // Llamar a LevitationEffect para aplicar el efecto de levitaci�n
        LevitationEffect();
    }

    void SelectNext()
    {
        selectedItemIndex = (selectedItemIndex + 1) % items.Length;
        if (selectedItemIndex == 0)
        {
            isDropdownSelected = true;
        }
        else
        {
            isDropdownSelected = false;
        }
    }

    void SelectNextInDropDown()
    {
        resolutionDropdown.value = math.min(resolutionDropdown.value + 1, resolutionDropdown.options.Count - 1);
        resolutionDropdown.RefreshShownValue();
    }

    void SelectPrevInDropDown()
    {
        resolutionDropdown.value = math.max(resolutionDropdown.value - 1, 0);
        resolutionDropdown.RefreshShownValue();
    }

    void SelectPrev()
    {
        selectedItemIndex = (selectedItemIndex - 1) < 0 ? items.Length - 1 : (selectedItemIndex - 1);
        if (selectedItemIndex == 0)
        {
            isDropdownSelected = true;
        }
        else
        {
            isDropdownSelected = false;
        }
    }

    public void OnDropdownSelect(bool selected)
    {
        isDropdownSelected = selected;
    }

    public void OnDropdownClose()
    {
        isDropdownSelected = false;
    }

    public void CloseDropdown()
    {
        resolutionDropdown.Hide();
        isDropdownSelected = false;
    }

    public void ChangeResolution(int dropDownIndex)
    {
        // Se obtienen las distintas configuraciones de resoluci�n
        TMP_Dropdown.OptionData resolutionOptions = resolutionDropdown.options[dropDownIndex];

        // Se parsean las dimensiones de la resoluci�n desde la etiqueta del Dropdown
        string[] resolutionParts = resolutionOptions.text.Split('x');
        int screenWidth = int.Parse(resolutionParts[0].Trim());
        int screenHeigth = int.Parse(resolutionParts[1].Trim());


        // Aplicamos la resoluci�n dependiendo si queremos en pantalla completa o ventana
        if (fullscreenToggle.isOn)
        {
            Screen.SetResolution(screenWidth, screenHeigth, true);
        }
        else
        {
            Screen.SetResolution(screenWidth, screenHeigth, false);
        }
    }

    public void ChangeFullScreen(bool isFullScreen)
    {
        Screen.fullScreen = isFullScreen;
        fullscreenToggle.isOn = isFullScreen;
    }

    public void ChangeVolume(float sliderValue)
    {
        mixer.SetFloat("AudioVolume", Mathf.Log10(sliderValue) * 20);
    }

    public void Return()
    {
        MySceneLoader.LoadMainMenu();
        // SceneManager.LoadScene("StartMenu");
    }

    private void LevitationEffect()
    {
        // Obtener el objeto seleccionado
        // GameObject selectedObject = EventSystem.current.currentSelectedGameObject;
        for (int i = 0; i < items.Length; i++)
        {
            if (i == selectedItemIndex)
            {
                Levitate(items[i], true);
            }
            else
            {
                Levitate(items[i], false);
            }
        }
    }

    private void Levitate(GameObject obj, bool shouldLevitate)
    {
        // Comprobar si el objeto est� seleccionado
        bool isSelected = shouldLevitate;

        // Aplicar levitaci�n vertical cuando est� seleccionado
        if (isSelected)
        {
            float angle = Time.time * levitationSpeed;
            float offsetX = Mathf.Sin(angle) * levitationRadius;
            float offsetY = Mathf.Cos(angle) * levitationRadius;

            obj.transform.position = initialPositions[obj] + new Vector3(offsetX, offsetY, 0f);
        }
        else
        {
            // Restablecer posici�n cuando no est� seleccionado
            obj.transform.position = initialPositions[obj];
        }
    }
}