using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmerginPointsPool : MonoBehaviour
{
    private Queue<EmerginPoints> usedPoints;
    [SerializeField] private GameObject emergingPointTemplate,container;

    private void Awake()
    {
        usedPoints = new Queue<EmerginPoints>();
    }


    public EmerginPoints GetText()
    {
        EmerginPoints point = FindPoints();
        if (point != null)
            return point;
        GameObject newPoint = Instantiate(emergingPointTemplate, container.transform);
        EmerginPoints textComp = newPoint.GetComponent<EmerginPoints>();
        InsertNewPoint(textComp);
        return textComp;
    }

    private EmerginPoints FindPoints()
    {
        int pointsCount = usedPoints.Count;
        for (int i = 0; i < pointsCount; i++)
        {
            EmerginPoints point = usedPoints.Dequeue();
            usedPoints.Enqueue(point);
            if (point.GetIsReadyToUse())
            {
                point.SetIsReadyToUse(false);
                return point;
            }
        }

        return null;
    }

    private void InsertNewPoint(EmerginPoints newPoint) =>
        usedPoints.Enqueue(newPoint);
}
