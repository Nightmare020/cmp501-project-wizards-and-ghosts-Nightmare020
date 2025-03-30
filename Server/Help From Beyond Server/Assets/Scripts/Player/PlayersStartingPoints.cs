using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayersStartingPoints : MonoBehaviour
{
    [SerializeField] private List<Transform> startingPoints;

    public List<Transform> GetStartingPoints()
    {
        return startingPoints;
    }
}
