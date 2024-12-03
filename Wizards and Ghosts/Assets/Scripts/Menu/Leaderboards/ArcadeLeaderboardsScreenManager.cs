using System.Collections.Generic;
using Save_Data;
using UnityEngine;
using Utils;

public class ArcadeLeaderboardsScreenManager : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private List<Result> leaderboard;
    private JsonSaving _jsonSaving;

    private SaveData _saveData;

//Inputs
    private MyInputManager _inputManager;

    void Start()
    {
        _jsonSaving = FindObjectOfType<JsonSaving>();
        _saveData = _jsonSaving._saveData;
        _inputManager = FindObjectOfType<MyInputManager>();

        ArcadeResults[] savedResults = _saveData.GetLeaderBoard();
        for (int i = 0; i < leaderboard.Count; i++)
        {
            string name = savedResults[i].GetName();
            int points = savedResults[i].GetPoints();
            leaderboard[i].SetNameAndPoints(name, points);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_inputManager.NavigationSelect())
        {
            MySceneLoader.LoadArcade();
        }
        else if (_inputManager.NavigationReturn())
        {
            MySceneLoader.LoadMainMenu();
        }
    }
}