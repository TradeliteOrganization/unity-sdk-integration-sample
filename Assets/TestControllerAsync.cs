using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using TradeliteSDK;
using TradeliteSDK.DAO;
using TradeliteSDK.Model.ConfigScope;
using TradeliteSDK.Model.KbScope;
using TradeliteSDK.Model.UserScope;
using TradeliteSDK.Service.ConfigScope;
using TradeliteSDK.Service.KbScope;
using TradeliteSDK.Service.UserScope;
using UnityEngine;
using UnityEngine.UI;

public class TestControllerAsync : MonoBehaviour {

    private bool forceReload = true;

    [ContextMenu("Force Singleton Reload")]
    public void _Z_ToggleForceReload() {
        forceReload = !forceReload;
        Debug.Log(forceReload ? "Services will be now forced to reload" : "Services will be now always using the initial instance");
    }

    [SerializeField] TMP_Text feedbackTextField;

    [SerializeField] TMP_InputField jwtTokenInputField;
    [SerializeField] TMP_Text userIdTextField;
    [SerializeField] TMP_Text nicknameTextField;

    [SerializeField] TMP_Text categoryIdsTextField;

    [SerializeField] TMP_InputField questionIdField;
    [SerializeField] TMP_Text questionTextField;

    [SerializeField] TMP_Text matchInfoTextField;

    void Start() {
        feedbackTextField.text = "Provide a valid JWT Token to get the user info.";
    }

    [ContextMenu("Get User (async)")]
    public async void _A_GetUser() {
        string jwt = jwtTokenInputField.text;
        if (string.IsNullOrEmpty(jwt)) {
            feedbackTextField.text = "Missing JWT Token.";
            return;
        }
        try {
            feedbackTextField.text = "Loading...";
            DataSource.SetActiveJWT(jwt);

            UserService service = UserService.GetInstance();
            User user = await service.Get("me");
            Debug.Log($"Player: {user}");

            feedbackTextField.text = "User data retrieved.";
            userIdTextField.text = "Id: " + user.id.Substring(0, 20) + "...";
            nicknameTextField.text = "Nickname: " + user.nickname;
        }
        catch (Exception)
        {
            feedbackTextField.text = "Invalid JWT token or invalid connection parameters. The information for the user cannot be retrieved.";
        }
    }

    [ContextMenu("Get Categories (async)")]
    public async void _A_GetCategories()
    {
        feedbackTextField.text = "Loading...";
        categoryIdsTextField.text = "";

        CategoryService service = CategoryService.GetInstance();
        string[] categoryIds = await service.GetIds();
        feedbackTextField.text = $"{categoryIds.Length} category ids retrieved.";

        Category[] categories = await service.GetByIds(categoryIds);
        for(int i=0; i<categories.Length; i++)
        {
            categoryIdsTextField.text += categories[i].title + "\n";
        }
    }

    [ContextMenu("Get Quiz Progression (async)")]
    public async void _A_GetQuizProgressions()
    {
        feedbackTextField.text = "Loading...";
        categoryIdsTextField.text = "";

        QuizProgressionService service = QuizProgressionService.GetInstance();
        QuizProgression progression = await service.Get("economics");
        categoryIdsTextField.text = $"Progression for 'economics': {progression}";
        feedbackTextField.text = $"Progression for 'economics' retrieved";
    }

    [ContextMenu("Get App Metrics (async)")]
    public async void _A_GetAppMetrics()
    {
        feedbackTextField.text = "Loading...";
        categoryIdsTextField.text = "";

        AppMetricsService service = AppMetricsService.GetInstance();
        AppMetrics metrics = await service.Get("whatever");
        categoryIdsTextField.text = metrics.ToString();
        UnityEngine.Debug.Log(metrics.ToString());
        feedbackTextField.text = $"Metrics retrieved";
    }

    private readonly string sceneName = "testScene";
    private readonly int levelId = 123;

    [ContextMenu("Save Map (async)")]
    public async void _A_SaveMap()
    {
        Tile tile = new Tile()
        {
            SortingOrder = 1,
            GridPos = new Vector3Int(1, 2, 3),
            WorldPos = new Vector3(1f, 2f, 2f),
            SavedSprite = null,
            Type = TileType.Tile,
        };
        List<Tile> tiles = new () { tile };
        TileLayer layer = new TileLayer() { tiles = tiles };
        WorldLevel level = new WorldLevel() { sceneName = sceneName, levelId = levelId, layers = new() { layer } };

        feedbackTextField.text = "Saving...";
        WorldLevelService service = WorldLevelService.GetInstance();
        string id = await service.Create(level);
        feedbackTextField.text = $"World level created with id: {id}";
    }

    [ContextMenu("Load Map (async)")]
    public async void _A_LoadMap()
    {
        feedbackTextField.text = "Loading...";
        WorldLevelService service = WorldLevelService.GetInstance();
        WorldLevel level = await service.Get(sceneName, levelId);
        feedbackTextField.text = $"World level read: {level}";
    }

    private string currentQuizMatchId;
    private int tileCoordinates;

    [ContextMenu("Create match (async)")]
    public async void _A_CreateMatch()
    {
        feedbackTextField.text = "Creating...";

        string categoryId = "economics";
        string progressionMapId = "...";
        QuizMatchService quizMatchService = QuizMatchService.GetInstance();
        currentQuizMatchId = await quizMatchService.Create(categoryId, progressionMapId);
        feedbackTextField.text = "New Quiz Match ID: " + currentQuizMatchId;
        tileCoordinates = 0;
    }

    private string tileId;

    [ContextMenu("Get Next Question Id (async)")]
    public async void _A_GetNextQuestionId()
    {
        feedbackTextField.text = "Loading...";

        QuizMatchService quizMatchService = QuizMatchService.GetInstance();
        QuizMatch info = await quizMatchService.Get(currentQuizMatchId);

        if (info.matchStatus != QuizMatch.STATUS_IN_PROGRESS)
        {
            Boolean completed = info.matchStatus == QuizMatch.STATUS_COMPLETED;
            feedbackTextField.text = $"Match {(completed ? "completed" : "canceled")}. You need to create a new one.";
            return;
        }
        if (0 < tileCoordinates && info.playerHealth.HP <= 0)
        {
            feedbackTextField.text = "No more HP... You need to revive your character or to cancel the match...";
            return;
        }
        if (info.enemyHealth.HP <= 0)
        {
            // Move to a new Tile b/c the enemy is dead on this one
            tileCoordinates += 1;
            int random = new System.Random().Next(3);
            tileId = (random == 0 ? "Easy" : random == 1 ? "Medium" : "Hard") + ";" + tileCoordinates;
        }
        questionIdField.text = "Loading for: " + tileId;

        try
        {
            string questionId = await quizMatchService.GetNextQuestionId(currentQuizMatchId, tileId);
            feedbackTextField.text = "New Question ID ready";
            questionIdField.text = questionId;
        }
        catch (Exception ex)
        {
            feedbackTextField.text = "Cannot get a next question id: " + ex.Message;
        }

    }

    private Question currentQuestion;

    [ContextMenu("Get Question (async)")]
    public async void _A_GetQuestion()
    {
        string questionId = questionIdField.text;
        if (string.IsNullOrEmpty(questionId))
        {
            feedbackTextField.text = "Missing Question ID.";
            return;
        }
        QuestionService questionService = QuestionService.GetInstance();
        AnswerService answerService = AnswerService.GetInstance();
        try
        {
            feedbackTextField.text = "Loading...";

            currentQuestion = await questionService.Get(questionId);
            Answer[] answers = await answerService.GetByIds(currentQuestion.answerIds);

            questionTextField.text = "\ndF: " + currentQuestion.difficultyLevel +
                "\nCat.: [" + String.Join(", ", currentQuestion.categoryIds) + "]" +
                "\nQ: " + currentQuestion.shortText;
            for (int i = 0; i < answers.Length; i++)
            {
                questionTextField.text += $"\n- A{i + 1}: " + answers[i].shortText;
            }
            feedbackTextField.text = "Question ready";
        }
        catch (Exception)
        {
            feedbackTextField.text = "Cannot get a next question";
        }
    }

    [ContextMenu("Validate Answer (async)")]
    public async void _A_ValidateAnswer(int position)
    {
        if (currentQuestion == null)
        {
            feedbackTextField.text = "No question loaded yet!";
            return;
        }
        feedbackTextField.text = "Checking...";

        string answerId = currentQuestion.answerIds[position];
        AnswerService answerService = AnswerService.GetInstance();
        Answer selectedAnswer = await answerService.Get(answerId);
        questionTextField.text = "\ndF: " + currentQuestion.difficultyLevel +
                "\nCat.: [" + String.Join(", ", currentQuestion.categoryIds) + "]" +
                "\nQ: " + currentQuestion.shortText +
                "\n- A: " + selectedAnswer.shortText;

        try
        {
            QuizMatchService quizMatchService = QuizMatchService.GetInstance();
            Boolean isCorrect = await quizMatchService.CheckAnswer(currentQuizMatchId, answerId);
            //SolutionService solutionService = SolutionService.GetInstance();
            //Boolean isCorrect = await solutionService.IsCorrectAnswer(currentQuestion.id, answerId);
            feedbackTextField.text = isCorrect ? "Congratulations" : "Sorry, you selected a wrong answer";
            questionTextField.text += "\n\nF: " + (isCorrect ? "Good choice" : "Wrong choice") +
                "\nD: " + selectedAnswer.longText;
        }
        catch (Exception)
        {
            feedbackTextField.text = "Could not verify the answer :(";
        }
    }

    [ContextMenu("Get Quiz Match Info (async)")]
    public async void _A_GetMatchInfo()
    {
        feedbackTextField.text = "Loading...";

        QuizMatchService quizMatchService = QuizMatchService.GetInstance();
        QuizMatch info = await quizMatchService.Get(currentQuizMatchId);
        matchInfoTextField.text = "";
        matchInfoTextField.text += "matchStatus: " + info.matchStatus + "\n";
        matchInfoTextField.text += "eloRatingStart: " + info.eloRatingStart + "\n";
        matchInfoTextField.text += "eloRatingEnd: " + info.eloRatingEnd + "\n";
        matchInfoTextField.text += "playerHealth: " + info.playerHealth.HP + " HP\n";
        matchInfoTextField.text += "enemyHealth: " + info.enemyHealth.HP + " HP\n";
        matchInfoTextField.text += "resurrectionNb: " + info.resurrectionNb + "\n";
        matchInfoTextField.text += "chestPoints: " + info.chestPoints + "\n";
        matchInfoTextField.text += "activeTile.id: " + info.activeTile.id + "\n";
        feedbackTextField.text = "Match info loaded";
    }

    [ContextMenu("Swap Question (async)")]
    public async void _A_SwapQuestion()
    {
        feedbackTextField.text = "Loading...";
        questionIdField.text = "Loading for: " + tileId;

        QuizMatchService quizMatchService = QuizMatchService.GetInstance();
        string questionId = await quizMatchService.SwapQuestion(currentQuizMatchId);

        feedbackTextField.text = "New Question ID ready";
        questionIdField.text = questionId;
    }

    [ContextMenu("Remove one Answer (async)")]
    public async void _A_RemoveOneAnswer()
    {
        feedbackTextField.text = "Loading...";

        QuizMatchService quizMatchService = QuizMatchService.GetInstance();
        AnswerService answerService = AnswerService.GetInstance();
        try
        {
            feedbackTextField.text = "Loading...";

            string removedAnswerId = await quizMatchService.RemoveOneAnswer(currentQuizMatchId);

            Answer[] answers = await answerService.GetByIds(currentQuestion.answerIds);

            questionTextField.text = "\ndF: " + currentQuestion.difficultyLevel +
                "\nCat.: [" + String.Join(", ", currentQuestion.categoryIds) + "]" +
                "\nQ: " + currentQuestion.shortText;
            for (int i = 0; i < answers.Length; i++)
            {
                if (answers[i].id != removedAnswerId)
                {
                    questionTextField.text += $"\n- A{i + 1}: " + answers[i].shortText;
                }
            }
            feedbackTextField.text = "Question ready";
        }
        catch (Exception)
        {
            feedbackTextField.text = "Cannot get a next question";
        }
    }

    [ContextMenu("Heal w/ Video (async)")]
    public async void _A_HealWithVideo()
    {
        feedbackTextField.text = "Loading...";

        QuizMatchService quizMatchService = QuizMatchService.GetInstance();
        int hpIncrease = await quizMatchService.HealWithVideo(currentQuizMatchId, "382948324321");

        feedbackTextField.text = $"{hpIncrease} HP added to your health record.";
    }

    [ContextMenu("Revive with Money (async)")]
    public async void _A_ReviveWithMoney()
    {
        feedbackTextField.text = "Loading...";

        QuizMatchService quizMatchService = QuizMatchService.GetInstance();
        int newHP = await quizMatchService.ResurrectWithMoney(currentQuizMatchId);

        feedbackTextField.text = $"Your HP has been restored to: {newHP}";
    }

    [ContextMenu("Cancel Match (async)")]
    public async void _A_CancelMatch()
    {
        feedbackTextField.text = "Loading...";

        QuizMatchService quizMatchService = QuizMatchService.GetInstance();
        Boolean success = await quizMatchService.CancelMatch(currentQuizMatchId);

        feedbackTextField.text = success ? "Match canceled" : "Could not cancel the match...";
    }

    [ContextMenu("Timer Ran Out (async)")]
    public async void _A_TimerRanOut()
    {
        feedbackTextField.text = "Loading...";

        QuizMatchService quizMatchService = QuizMatchService.GetInstance();
        int newHP = await quizMatchService.TimerRanOut(currentQuizMatchId);

        feedbackTextField.text = newHP <= 0 ? "You don't have HP anymore!" : "Get next question id.";
    }

    [ContextMenu("Get Game Stats (async)")]
    public async void _A_GetGameStats()
    {
        feedbackTextField.text = "Loading...";

        GameStatsService gameStatsService = GameStatsService.GetInstance();
        GameStats gameStats = await gameStatsService.Get();
        matchInfoTextField.text = "";
        matchInfoTextField.text += "experienceLevel: " + gameStats.experienceLevel + "\n";
        matchInfoTextField.text += "playerHealth: " + gameStats.playerHealth + "\n";
        matchInfoTextField.text += "quizGameStats: " + gameStats.quizGameStats + "\n";
        
        feedbackTextField.text = "Game stats info loaded";
    }

    [ContextMenu("Get Wallet (async)")]
    public async void _A_GetWallet()
    {
        feedbackTextField.text = "Loading...";

        WalletService walletService = WalletService.GetInstance();
        Wallet wallet = await walletService.Get();
        matchInfoTextField.text = "";
        matchInfoTextField.text += "assets: {" + wallet.assets + "}\n";
        
        feedbackTextField.text = "Wallet info loaded";
    }
}