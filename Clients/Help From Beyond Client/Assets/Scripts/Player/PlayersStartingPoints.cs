using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayersStartingPoints : MonoBehaviour
{
    [SerializeField] private List<Transform> startingPoints; // List of starting points for players

    // Method to get the list of starting points
    public List<Transform> GetStartingPoints()
    {
        return startingPoints;
    }
}