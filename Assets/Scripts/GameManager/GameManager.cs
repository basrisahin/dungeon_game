using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class GameManager : SingletonMonobehaviour<GameManager>
{
    // Level gecislerinde gosterilecek
    [SerializeField] private TextMeshProUGUI messageTextTMP;
    // Level gecislerinde goruntulenecek UI component'in fade'i buradan ayarlanacak.
    [SerializeField] private CanvasGroup canvasGroup;

    #region Header DUNGEON LEVELS

    [Space(10)]
    [Header("DUNGEON LEVELS")]

    #endregion Header DUNGEON LEVELS

    #region Tooltip

    [Tooltip("Populate with the dungeon level scriptable objects")]

    #endregion Tooltip

    [SerializeField] private List<DungeonLevelSO> dungeonLevelList;

    #region Tooltip

    [Tooltip("Populate with the starting dungeon level for testing , first level = 0")]

    #endregion Tooltip

    [SerializeField] private int currentDungeonLevelListIndex = 0;
    private Room currentRoom;
    private Room previousRoom;
    private PlayerDetailsSO playerDetails;
    private Player player;
    private long gameScore;
    private int scoreMultiplier;
    private InstantiatedRoom bossRoom;

    [HideInInspector] public GameState gameState;
    [HideInInspector] public GameState previousGameState;

    protected override void Awake()
    {
        // Call base class
        base.Awake();

        // Set player details - saved in current player scriptable object from the main menu
        playerDetails = GameResources.Instance.currentPlayer.playerDetails;

        // Instantiate player
        InstantiatePlayer();

    }

    /// <summary>
    /// Create player in scene at position
    /// </summary>
    private void InstantiatePlayer()
    {
        // Instantiate player
        GameObject playerGameObject = Instantiate(playerDetails.playerPrefab);

        // Initialize Player
        player = playerGameObject.GetComponent<Player>();

        player.Initialize(playerDetails);

    }

    private void OnEnable()
    {
        // Subscribe to room changed event.
        StaticEventHandler.OnRoomChanged += StaticEventHandler_OnRoomChanged;
        StaticEventHandler.OnPointsScored += StaticEventHandler_OnPointScoredEvent;
        StaticEventHandler.OnMultiplier += StaticEventHandler_OnMultiplier;
        // bu odada enemy kalmadi eventi
        StaticEventHandler.OnRoomEnemiesDefeated += StaticEventHandler_OnRoomEnemiesDefeated;
        // player oldu eventi
        player.destroyedEvent.OnDestroyed += Player_OnDestroyed;
    }

    private void OnDisable()
    {
        // Unsubscribe from room changed event
        StaticEventHandler.OnRoomChanged -= StaticEventHandler_OnRoomChanged;
        StaticEventHandler.OnPointsScored -= StaticEventHandler_OnPointScoredEvent;
        StaticEventHandler.OnMultiplier -= StaticEventHandler_OnMultiplier;

        StaticEventHandler.OnRoomEnemiesDefeated -= StaticEventHandler_OnRoomEnemiesDefeated;
        player.destroyedEvent.OnDestroyed -= Player_OnDestroyed;
    }

    //
    private void StaticEventHandler_OnRoomEnemiesDefeated(RoomEnemiesDefeatedArgs roomEnemiesDefeatedArgs)
    {
        RoomEnemiesDefeated();
    }

    private void RoomEnemiesDefeated()
    {
        bool isDungeonClearOfRegularEnemies = true;
        bossRoom = null;

        foreach(KeyValuePair<string, Room> keyValuePair in DungeonBuilder.Instance.dungeonBuilderRoomDictionary)
        {
            Room room = keyValuePair.Value;
            if (room.roomNodeType.isBossRoom)
            {
                bossRoom = room.instantiatedRoom;
                continue;
            }

            if (!room.isClearedOfEnemies)
            {
                isDungeonClearOfRegularEnemies = false;
                break;
            }
        }

        // Set the gameState
        if((isDungeonClearOfRegularEnemies && bossRoom == null) || (isDungeonClearOfRegularEnemies && bossRoom.room.isClearedOfEnemies))
        {
            // if there are more dungeon level
            if (currentDungeonLevelListIndex < dungeonLevelList.Count - 1)
            {
                gameState = GameState.levelCompleted;
            }
            else
            {
                gameState = GameState.gameWon;
            }

        }
        else if (isDungeonClearOfRegularEnemies)
        {
            gameState = GameState.bossStage;
            StartCoroutine(BossStage());
        }
        
    }

    /// <summary>
    /// Handle room changed event
    /// </summary>
    private void StaticEventHandler_OnRoomChanged(RoomChangedEventArgs roomChangedEventArgs)
    {
        SetCurrentRoom(roomChangedEventArgs.room);
    }

    private void StaticEventHandler_OnPointScoredEvent(PointsScoredArgs pointsScoredArgs)
    {
        gameScore += pointsScoredArgs.points * scoreMultiplier;
        // publish Score Changed Event.
        StaticEventHandler.CallScoreChangedEvent(gameScore, scoreMultiplier);
    }

    private void StaticEventHandler_OnMultiplier(MultiplierArgs multiplierArgs)
    {
        if(multiplierArgs.multiplier)
            scoreMultiplier++;
        else
            scoreMultiplier--;
        // Eger bi degeri iki deger arasinda sikistirmak istersek Mathf.Clamp kullaniriz.
        scoreMultiplier = Mathf.Clamp(scoreMultiplier, 1, 30);

        StaticEventHandler.CallScoreChangedEvent(gameScore, scoreMultiplier);
    }

    private void Player_OnDestroyed(DestroyedEvent destroyedEvent, DestroyedEventArgs destroyedEventArgs)
    {
        previousGameState = gameState;
        gameState = GameState.gameLost; 
    }

    // Start is called before the first frame update
    private void Start()
    {
        previousGameState = GameState.gameStarted;
        gameState = GameState.gameStarted;
        gameScore = 0;
        scoreMultiplier = 1;
        // start alpha, target alpha, seconds for transition
        // Alpha 0'dan 1'e renk siyah olacak sekilde aninda ekran karasin.
        StartCoroutine(Fade(0f,1f,0f, Color.black));

    }

    private IEnumerator Fade(float startFadeAlpha, float targetFadeAlpha, float fadeSeconds, Color backgrounColor)
    {
        
        Image image = canvasGroup.GetComponent<Image>();
        image.color = backgrounColor;

        float time = 0;

        while (time <= fadeSeconds)
        {
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startFadeAlpha, targetFadeAlpha, time/fadeSeconds);
            yield return null;
        }
    }

    // Update is called once per frame
    private void Update()
    {
        HandleGameState();

    }

    private void HandleGameState()
    {
        // Handle game state
        switch (gameState)
        {
            case GameState.gameStarted:

                // Play first level
                PlayDungeonLevel(currentDungeonLevelListIndex);
                gameState = GameState.playingLevel;
                // if the level only have boss room scenario. not likely to happen.
                RoomEnemiesDefeated();
                break;

            case GameState.levelCompleted:
                StartCoroutine(LevelCompleted());
                break;

            case GameState.gameWon:
                if(previousGameState != GameState.gameWon)
                    StartCoroutine(GameWon());
                break;
            
            case GameState.gameLost:
                if(previousGameState != GameState.gameLost)
                {
                    StopAllCoroutines();
                    StartCoroutine(GameLost());
                }
                break;
            
            case GameState.restartGame:
                RestartGame();

                break;  
        }

    }
    // level tamamlanginda ne olacak?
    private IEnumerator LevelCompleted()
    {
        gameState = GameState.playingLevel;
        yield return new WaitForSeconds(2f);

        yield return StartCoroutine(Fade(0f,1f,2f, new Color(0f,0f,0f,0.4f)));
        string levelCompletedText = $"WELL DONE {GameResources.Instance.currentPlayer.playerName} YOU SURVIVED! PRESS RETURN TO CONTINUE!";
        yield return StartCoroutine(DisplayMessageRoutine(levelCompletedText,Color.white, 5f));
        yield return StartCoroutine(Fade(1f,0f,2f, new Color(0f,0f,0f,0.4f)));

        currentDungeonLevelListIndex++;

        PlayDungeonLevel(currentDungeonLevelListIndex);
    }

    private IEnumerator GameWon()
    {
       previousGameState = GameState.gameWon;
        yield return StartCoroutine(Fade(0f,1f,2f, Color.black));
        string gameWonText = $"CONGRULATIONS!! {GameResources.Instance.currentPlayer.playerName} YOU DEFEATED THE DUNGEON!";
        string gameWonScoreText = $"YOU SCORED {gameScore.ToString("###,###0")}";
        yield return StartCoroutine(DisplayMessageRoutine(gameWonScoreText,Color.white, 4f));
        yield return StartCoroutine(DisplayMessageRoutine("PRESS RETURN TO RESTART GAME",Color.white, 0f));
       gameState = GameState.restartGame;
    }

    private IEnumerator GameLost()
    {
        previousGameState = GameState.gameLost;
        GetPlayer().playerControl.DisablePlayer();
        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(Fade(0f,1f,2f, Color.black));
        // resourcefull operation.
        // buna alternatif olarak Pool Manager'a yeni bir method ekleyip butun enemy'leri kill etmek denenebilir.

        Enemy[] enemyArray = GameObject.FindObjectsOfType<Enemy>();
        foreach (Enemy enemy in enemyArray)
        {
            enemy.gameObject.SetActive(false);
        }

        string gameLostText = $"BAD LUCK!! {GameResources.Instance.currentPlayer.playerName} YOU HAVE SUCCUMBED THE DUNGEON";
        string gameLostScoreText = $"YOU SCORED {gameScore.ToString("###,###0")}";
        yield return StartCoroutine(DisplayMessageRoutine(gameLostText,Color.white, 2f));
        yield return StartCoroutine(DisplayMessageRoutine(gameLostScoreText,Color.white, 4f));
        yield return StartCoroutine(DisplayMessageRoutine("PRESS RETURN TO RESTART GAME",Color.white, 0f));

        
        gameState = GameState.restartGame;

    }

    private void RestartGame()
    {
        SceneManager.LoadScene("MainGameScene");

    }


    /// <summary>
    /// Set the current room the player in in
    /// </summary>
    public void SetCurrentRoom(Room room)
    {
        previousRoom = currentRoom;
        currentRoom = room;
    }


    private void PlayDungeonLevel(int dungeonLevelListIndex)
    {
        // Build dungeon for level
        bool dungeonBuiltSucessfully = DungeonBuilder.Instance.GenerateDungeon(dungeonLevelList[dungeonLevelListIndex]);

        if (!dungeonBuiltSucessfully)
        {
            Debug.LogError("Couldn't build dungeon from specified rooms and node graphs");
        }

        // Call static event that room has changed.
        StaticEventHandler.CallRoomChangedEvent(currentRoom);

        // Set player roughly mid-room
        player.gameObject.transform.position = new Vector3((currentRoom.lowerBounds.x + currentRoom.upperBounds.x) / 2f, (currentRoom.lowerBounds.y + currentRoom.upperBounds.y) / 2f, 0f);

        // Get nearest spawn point in room nearest to player
        player.gameObject.transform.position = HelperUtilities.GetSpawnPositionNearestToPlayer(player.gameObject.transform.position);

        //Display Dungeon Level Text
        StartCoroutine(DisplayDungeonLevelText());

        // // Demo Code
        // RoomEnemiesDefeated();

    }

    private IEnumerator DisplayDungeonLevelText()
    {
        StartCoroutine(Fade(0f,1f,0f, Color.black));
        GetPlayer().playerControl.DisablePlayer();
        
        string levelNumber = $"LEVEL {currentDungeonLevelListIndex+1}";
        string levelName = $"{dungeonLevelList[currentDungeonLevelListIndex].levelName.ToUpper()}";
        string messageText = $"{levelNumber} \n\n {levelName}";
        // DisplayMessageRoutine bitmeden sonraki satirlara ilerlemez. yani 2f. 
        yield return StartCoroutine(DisplayMessageRoutine(messageText, Color.white, 2f));

        GetPlayer().playerControl.EnablePlayer();
        //StartCoroutine(Fade(1f, 0f, 2f, Color.black));
    }

    private IEnumerator DisplayMessageRoutine(string text, Color textColor, float displaySeconds)
    {
        messageTextTMP.SetText(text);
        messageTextTMP.color = textColor;

        if(displaySeconds > 0f)
        {
            float timer = displaySeconds;
            while (timer > 0f && !Input.GetKeyDown(KeyCode.Return))
            {
                timer -= Time.deltaTime;
                yield return null;
            }
        }
        else
        {   // return'e basana kadar bekle.
            while (!Input.GetKeyDown(KeyCode.Return))
            {
                yield return null;
            }
        }

        yield return null;

        messageTextTMP.SetText("");
    }


    private IEnumerator BossStage()
    {
        // it can be out of minicamera range so lets make it active first.
        bossRoom.gameObject.SetActive(true);
        bossRoom.UnlockDoors(0f);
        yield return new WaitForSeconds(2f);
        // Fade in yapalim
        yield return StartCoroutine(Fade(0f,1f,2f,new Color(0f,0f,0f,0.4f)));
        //Display Boss Message
        string bossStageText = $"WELL DONE {GameResources.Instance.currentPlayer.playerName} YOU SURVIVED.. SO FAR \n\n NOW FIND AND DEFEAT THE BOSS";
        yield return StartCoroutine(DisplayMessageRoutine(bossStageText,Color.white,5f));
        yield return StartCoroutine(Fade(1f,0f,2f, new Color(0f,0f,0f,0.4f)));
    }

    /// <summary>
    /// Get the player
    /// </summary>
    public Player GetPlayer()
    {
        return player;
    }


    /// <summary>
    /// Get the player minimap icon
    /// </summary>
    public Sprite GetPlayerMiniMapIcon()
    {
        return playerDetails.playerMiniMapIcon;
    }


    /// <summary>
    /// Get the current room the player is in
    /// </summary>
    public Room GetCurrentRoom()
    {
        return currentRoom;
    }

    /// <summary>
    /// Get the current dungeon level
    /// </summary>
    public DungeonLevelSO GetCurrentDungeonLevel()
    {
        return dungeonLevelList[currentDungeonLevelListIndex];
    }


    #region Validation

#if UNITY_EDITOR

    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(dungeonLevelList), dungeonLevelList);
    }

#endif

    #endregion Validation

}

