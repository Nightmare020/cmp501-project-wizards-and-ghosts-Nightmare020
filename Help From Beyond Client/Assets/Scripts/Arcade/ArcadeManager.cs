using System;
using System.Collections.Generic;
using Enemies;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using Utils;
using Random = UnityEngine.Random;

[DefaultExecutionOrder(1)]
public class ArcadeManager : MonoBehaviour
{
    //spawns
    [SerializeField] private GameObject wizardCoinsContainer, ghostCoinsContainer;

    [SerializeField] private Transform wizardCoinsSpawnPoints, ghostCoinsSpawnPoints;
    private List<Transform> wizardCoinsSpawns, ghostCoinsSpawns;

    //coins
    private List<ArcadePoint> wizardCoins, ghostCoins;
    [SerializeField] private int maxWizardCoins = 2, maxGhostCoins = 2;
    [SerializeField] private GameObject wizardCoinTemplate, ghostCoinTemplate;

    //feedback
    [SerializeField] private TMP_Text wizardCoinCountText,
        wizardCoinCountTextHud,
        ghostCoinCountText,
        ghostCoinCountTextHud,
        totalPointsText;

    private ArcadePoint currentWizardTarget, currentGhostTarget;
    [SerializeField] private SpriteRenderer ghostCoinClue, wizardCoinClue;

    private WizardValues _wizardValues;
    private GhostValues _ghostValues;

    //enemies
    private EnemyManager _enemyManager;

    //points
    private int wizardPoints = 0, ghostPoints = 0;

    //save data
    private JsonSaving _jsonSaving;
    private SaveData _saveData;

    //timer
    private MyStopwatch timer;
    [SerializeField] private float maxTime = 60f;
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private EmerginPointsPool _emerginPointsPool;
    [SerializeField] private Vector3 pointsOffset;

    private void Awake()
    {
        timer = gameObject.AddComponent<MyStopwatch>();
        _enemyManager = FindObjectOfType<EnemyManager>();
        _jsonSaving = FindObjectOfType<JsonSaving>();
        _saveData = _jsonSaving._saveData;
    }

    void Start()
    {
        //get spawns wizard
        wizardCoinsSpawns = GetSpawns(wizardCoinsSpawnPoints);

        //get spawns Ghost
        ghostCoinsSpawns = GetSpawns(ghostCoinsSpawnPoints);

        //instantiate all wizard Points
        wizardCoins = InitializeCoins(maxWizardCoins, wizardCoinTemplate, wizardCoinsContainer.transform);
        LocatePoints(wizardCoinsSpawns, wizardCoins);

        //instantiate allGhostCoins
        ghostCoins = InitializeCoins(maxGhostCoins, ghostCoinTemplate, ghostCoinsContainer.transform);
        LocatePoints(ghostCoinsSpawns, ghostCoins);

        //set signs puntuation
        PrintCoins();
    }

    private void Update()
    {
        if (_wizardValues && _ghostValues)
        {
            if (!timer.IsRunning())
            {
                timer.StartStopwatch();
            }

            float remeaningTime = maxTime * 1000f - timer.GetElapsedMiliseconds();

            if (Time.frameCount % 2 == 0)
            {
                //show timer
                timeText.text = MyUtils.GetCountdownTimeString(remeaningTime);
            }

            if (remeaningTime <= 0f)
            {
                GameOver();
            }
        }
    }

    private void LateUpdate()
    {
        //indicator wizard
        _wizardValues = GetWizard();
        currentWizardTarget = GetValidCoin(currentWizardTarget, wizardCoins);

        if (_wizardValues && _ghostValues && currentWizardTarget)
        {
            Ray ray = new Ray(_wizardValues.transform.position,
                currentWizardTarget.transform.position - _wizardValues.transform.position);
            wizardCoinClue.transform.position = ray.GetPoint(2);
            float dist = Vector2.Distance(_wizardValues.transform.position, currentWizardTarget.transform.position);
            float alpha = Mathf.Clamp01(MyUtils.Normalice(dist, 2, 10));
            wizardCoinClue.color = new Color(1, 1, 1, alpha);
        }

        //indicator ghost
        _ghostValues = GetGhost();
        currentGhostTarget = GetValidCoin(currentGhostTarget, ghostCoins);

        if (_wizardValues && _ghostValues && currentGhostTarget)
        {
            Ray ray = new Ray(_ghostValues.transform.position,
                currentGhostTarget.transform.position - _ghostValues.transform.position);
            ghostCoinClue.transform.position = ray.GetPoint(3);
            float dist = Vector2.Distance(_ghostValues.transform.position, currentGhostTarget.transform.position);
            float alpha = Mathf.Clamp01(MyUtils.Normalice(dist, 2, 10));
            ghostCoinClue.color = new Color(1, 1, 1, alpha);
        }
    }

    #region Initialization

    List<Transform> GetSpawns(Transform spawnPointsObject)
    {
        List<Transform> spawns = new List<Transform>();
        spawns.AddRange(spawnPointsObject.GetComponentsInChildren<Transform>());
        spawns.Remove(spawnPointsObject.transform);

        return spawns;
    }

    List<ArcadePoint> InitializeCoins(int maxCoins, GameObject coinTemplate, Transform container)
    {
        List<ArcadePoint> coins = new List<ArcadePoint>();
        for (int i = 0; i < maxCoins; i++)
        {
            GameObject coin = Instantiate(coinTemplate, container.transform);
            ArcadePoint coinComp = coin.GetComponent<ArcadePoint>();
            coins.Add(coinComp);
        }

        return coins;
    }

    #endregion


    private void LocatePoints(List<Transform> spawns, List<ArcadePoint> coins)
    {
        List<Transform> spawnsNotUsed = new List<Transform>(spawns.ToArray());
        int index = 0;
        while (index < coins.Count)
        {
            int randIdx = Random.Range(0, spawnsNotUsed.Count);
            coins[index].EnablePoint(spawnsNotUsed[randIdx].position);
            spawnsNotUsed.Remove(spawnsNotUsed[randIdx]);
            index++;
        }
    }

    private ArcadePoint GetValidCoin(ArcadePoint currentPoint, List<ArcadePoint> arcadePoints)
    {
        if (!currentPoint || (currentPoint && currentPoint.collected))
        {
            for (int i = 0; i < arcadePoints.Count; i++)
            {
                if (!arcadePoints[i].collected)
                {
                    return arcadePoints[i];
                }
            }

            return null;
        }

        return currentPoint;
    }

    bool AllCoinsCollected(List<ArcadePoint> coins)
    {
        for (int i = 0; i < coins.Count; i++)
        {
            if (!coins[i].collected)
            {
                return false;
            }
        }

        return true;
    }


    public void GameOver()
    {
        _saveData.SetCurrentArcadePoints(ghostPoints + wizardPoints);
        _jsonSaving.SaveTheData();
        MySceneLoader.LoadArcadeEndScreen();
    }

    public void AddWizardPoints(int val, ArcadePoint arcadePoint)
    {
        if (currentWizardTarget == arcadePoint)
        {
            wizardPoints += val * 2;
            _emerginPointsPool.GetText()
                .StartPoints(wizardCoinCountTextHud.transform.position + pointsOffset, "+" + val * 2);
        }
        else
        {
            wizardPoints += val;
            _emerginPointsPool.GetText()
                .StartPoints(wizardCoinCountTextHud.transform.position + pointsOffset, "+" + val);
        }

        _emerginPointsPool.GetText().StartPoints(timeText.transform.position, "+" + 5);
        timer.SubtractTime(5);


        if (wizardPoints % 10 == 0)
        {
            _enemyManager.IncreaseDifficultyGhost();
        }

        if (AllCoinsCollected(wizardCoins))
        {
            LocatePoints(wizardCoinsSpawns, wizardCoins);
            _enemyManager.IncreaseDifficulty();
        }

        PrintCoins();
    }


    public void AddGhostPoints(int val, ArcadePoint arcadePoint)
    {
        if (currentGhostTarget == arcadePoint)
        {
            ghostPoints += val * 2;
            _emerginPointsPool.GetText()
                .StartPoints(ghostCoinCountTextHud.transform.position + pointsOffset, "+" + val * 2);
        }
        else
        {
            ghostPoints += val;
            _emerginPointsPool.GetText()
                .StartPoints(ghostCoinCountTextHud.transform.position + pointsOffset, "+" + val);
        }

        _emerginPointsPool.GetText().StartPoints(timeText.transform.position, "+" + 5);
        timer.SubtractTime(5);

        if (ghostPoints % 10 == 0)
        {
            _enemyManager.IncreaseDifficultyWizard();
        }

        if (AllCoinsCollected(ghostCoins))
        {
            LocatePoints(ghostCoinsSpawns, ghostCoins);
            _enemyManager.IncreaseDifficulty();
        }

        PrintCoins();
    }


    private void PrintCoins()
    {
        string ghostText = ghostPoints % (maxGhostCoins) + "/" + maxGhostCoins;
        ghostCoinCountText.text = ghostText;
        ghostCoinCountTextHud.text = ghostText;
        string wizardText = wizardPoints % (maxWizardCoins) + "/" + maxWizardCoins;
        wizardCoinCountText.text = wizardText;
        wizardCoinCountTextHud.text = wizardText;
        totalPointsText.text = wizardPoints + ghostPoints + " pts";
    }

    GhostValues GetGhost()
    {
        if (!_ghostValues || _ghostValues && !_ghostValues.transform.parent.CompareTag("ActiveGhost"))
        {
            GameObject ghostObj = GameObject.FindWithTag("ActiveGhost");
            if (ghostObj)
            {
                return ghostObj.GetComponentInChildren<GhostValues>();
            }

            return null;
        }

        return _ghostValues;
    }

    WizardValues GetWizard()
    {
        if (!_wizardValues || _wizardValues && !_wizardValues.transform.parent.CompareTag("ActiveWizard"))
        {
            GameObject wizardObj = GameObject.FindWithTag("ActiveWizard");
            if (wizardObj)
            {
                return wizardObj.GetComponentInChildren<WizardValues>();
            }

            return null;
        }

        return _wizardValues;
    }
}