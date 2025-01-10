using System;

using Save_Data;
using UnityEngine;

[Serializable]
public class SaveData
{
    [SerializeField] private int levelIdx = -1;
    [SerializeField] private int currentVersion;
    [SerializeField] private ArcadeResults[] leaderBoard;
    [SerializeField] private int currentArcadePoints = 10;

    private const int Version = 1;

    //general settings

    public SaveData()
    {
        levelIdx = -1;
        currentVersion = Version;
        leaderBoard = new ArcadeResults[7];
        currentArcadePoints = 10;
        InitLeaderBoard();
    }

    #region Methods

    public int GetCurrentLevel()
    {
        return levelIdx;
    }

    public void SetCurrentLevel(int idx)
    {
        levelIdx = idx;
    }

    public int GetCurrentArcadePoints()
    {
        return currentArcadePoints;
    }

    public void SetCurrentArcadePoints(int points)
    {
        currentArcadePoints = points;
    }

    public int GetVersion()
    {
        return currentVersion;
    }

    public static bool CheckVersionObsolet(int old)
    {
        return old < Version;
    }

    #region Leaderboards

    private void InitLeaderBoard()
    {
        for (int i = 0; i < leaderBoard.Length; i++)
        {
            leaderBoard[i] = new ArcadeResults("---", 0);
        }
    }

    public void AddScoreToSortedLeaderboard(ArcadeResults score)
    {
        ArcadeResults[] aux = new ArcadeResults[leaderBoard.Length + 1];
        Array.Copy(leaderBoard, aux, leaderBoard.Length);
        aux[leaderBoard.Length] = score;
        Array.Sort(aux);
        Array.Copy(aux, leaderBoard, leaderBoard.Length);
    }

    public ArcadeResults[] GetLeaderBoard()
    {
        return leaderBoard;
    }

    #endregion

    #endregion
}