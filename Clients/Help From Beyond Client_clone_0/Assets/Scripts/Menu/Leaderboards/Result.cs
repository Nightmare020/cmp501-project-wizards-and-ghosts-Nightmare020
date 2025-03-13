
using TMPro;
using UnityEngine;

public class Result : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText, pointsText;

    public void SetPoints(int points)
    {
        pointsText.text = "" + points;
    }

    public void SetName(string name)
    {
        nameText.text = name.Substring(0, 3);
    }

    public void SetNameAndPoints(string name, int points)
    {
        pointsText.text = "" + points;
        nameText.text = name; //.Substring(0, 3);
    }

    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }
}