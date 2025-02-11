
using System.Collections.Generic;
using Save_Data;
using TMPro;
using UnityEngine;
using Utils;

public class EndScreenManager : MonoBehaviour
{
    string[] letters =
    {
        "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V",
        "W", "X", "Y", "Z"
    };

    [SerializeField] private List<TMP_Text> lettersTexts;
    [SerializeField] private TMP_Text pointsText;
    private int[] letterIndex;

    private int selectedTextIndex = 0;

    //inputs
    private MyInputManager _inputManager;

    //save data
    private JsonSaving _jsonSaving;
    private SaveData _saveData;

    void Start()
    {
        _inputManager = FindObjectOfType<MyInputManager>();
        _jsonSaving = FindObjectOfType<JsonSaving>();
        _saveData = _jsonSaving._saveData;

        letterIndex = new int [lettersTexts.Count];
        for (int i = 0; i < letterIndex.Length; i++)
        {
            letterIndex[i] = 0;
        }

        for (int i = 0; i < lettersTexts.Count; i++)
        {
            lettersTexts[i].text = letters[letterIndex[i]];
        }

        pointsText.text = _saveData.GetCurrentArcadePoints() + " pts";

        HighlightText();
    }

    private void Update()
    {
        if (_inputManager.NavigationRight())
        {
            SelectNextText();
        }
        else if (_inputManager.NavigationLeft())
        {
            SelectPrevText();
        }
        else if (_inputManager.NavigationUp())
        {
            SelectPrevLetter();
        }
        else if (_inputManager.NavigationDown())
        {
            SelectNextLetter();
        }
        else if (_inputManager.NavigationSelect())
        {
            CheckButton();
        }
    }


    public void SelectNextText()
    {
        selectedTextIndex = (selectedTextIndex + 1) % lettersTexts.Count;
        HighlightText();
    }

    public void SelectPrevText()
    {
        selectedTextIndex = selectedTextIndex - 1 < 0 ? lettersTexts.Count - 1 : selectedTextIndex - 1;
        HighlightText();
    }

    public void SelectNextLetter()
    {
        int index = letterIndex[selectedTextIndex];
        letterIndex[selectedTextIndex] = (index + 1) % letters.Length;
        lettersTexts[selectedTextIndex].text = letters[letterIndex[selectedTextIndex]];
    }

    public void SelectPrevLetter()
    {
        int index = letterIndex[selectedTextIndex];
        letterIndex[selectedTextIndex] = index - 1 < 0 ? letters.Length - 1 : index - 1;
        lettersTexts[selectedTextIndex].text = letters[letterIndex[selectedTextIndex]];
    }

    private void HighlightText()
    {
        for (var i = 0; i < lettersTexts.Count; i++)
        {
            if (i == selectedTextIndex)
            {
                lettersTexts[i].alpha = 1;
            }
            else
            {
                lettersTexts[i].alpha = 0.3f;
            }
        }
    }

    private string GetName()
    {
        string text = "";
        for (int i = 0; i < lettersTexts.Count; i++)
        {
            text += lettersTexts[i].text;
        }

        return text;
    }


    public void CheckButton()
    {
        _saveData.AddScoreToSortedLeaderboard(new ArcadeResults(GetName(), _saveData.GetCurrentArcadePoints()));
        _jsonSaving.SaveTheData();
        MySceneLoader.LoadLeaderBoards();
    }
}