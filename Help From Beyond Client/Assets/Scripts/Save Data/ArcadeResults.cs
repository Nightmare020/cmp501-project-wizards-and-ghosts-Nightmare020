using System;
using UnityEngine;

namespace Save_Data
{
    [Serializable]
    public class ArcadeResults : IComparable<ArcadeResults>
    {
        [SerializeField] private string name;
        [SerializeField] private int points;

        public ArcadeResults(string name, int points)
        {
            this.name = name;
            this.points = points;
        }

        public void SetPoints(int points)
        {
            this.points = points;
        }

        public int GetPoints()
        {
            return points;
        }

        public void SetName(string name)
        {
            this.name = name;
        }

        public string GetName()
        {
            return name;
        }

        public int CompareTo(ArcadeResults other)
        {
            if (points < other.points)
                return 1;

            if (points > other.points)
                return -1;
            return 0;
        }
    }
}