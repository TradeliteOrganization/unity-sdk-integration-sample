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
    [SerializeField] Button getUserButton;
    [SerializeField] TMP_Text categoryIdsTextField;
    [SerializeField] Button saveMapButton;
    [SerializeField] Button loadMapButton;
    [SerializeField] TMP_InputField questionIdField;
    [SerializeField] Button loadQuestionButton;
    [SerializeField] TMP_Text questionTextField;
    [SerializeField] Button answer1Button;
    [SerializeField] Button answer2Button;
    [SerializeField] Button answer3Button;
    [SerializeField] Button answer4Button;
    [SerializeField] Button createMatchButton;
    [SerializeField] Button getNextQuestionIdButton;

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
            DataSource.SetActiveJWT(jwt);

            UserService service = UserService.GetInstance();
            User user = await service.Get("me");
            Debug.Log($"Player: {user}");

            feedbackTextField.text = "User data retrieved.";
            userIdTextField.text = user.id;
            nicknameTextField.text = user.nickname;
        }
        catch (Exception)
        {
            feedbackTextField.text = "Invalid JWT token or invalid connection parameters. The information for the user cannot be retrieved.";
        }
    }

    [ContextMenu("Get Categories (async)")]
    public async void _A_GetCategories()
    {
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

    private string sceneName = "testScene";
    private int levelId = 123;

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

        WorldLevelService service = WorldLevelService.GetInstance();
        string id = await service.Create(level);
        feedbackTextField.text = $"World level created with id: {id}";
    }

    [ContextMenu("Load Map (async)")]
    public async void _A_LoadMap()
    {
        WorldLevelService service = WorldLevelService.GetInstance();
        WorldLevel level = await service.Get(sceneName, levelId);
        feedbackTextField.text = $"World level read: {level}";
    }

    private Question currentQuestion;

    [ContextMenu("Get Quest (async)")]
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
            currentQuestion = await questionService.Get(questionId);
            Answer[] answers = await answerService.GetByIds(currentQuestion.answerIds);

            questionTextField.text = "\ndF: " + currentQuestion.difficultyLevel +
                "\nCat.: [" + String.Join(", ", currentQuestion.categoryIds) + "]" +
                "\nQ: " + currentQuestion.shortText;
            for(int i=0; i<answers.Length; i++)
            {
                questionTextField.text += $"\n- A{i+1}: " + answers[i].shortText;
            }
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
        string answerId = currentQuestion.answerIds[position];
        AnswerService answerService = AnswerService.GetInstance();
        Answer selectedAnswer = await answerService.Get(answerId);
        questionTextField.text = "\ndF: " + currentQuestion.difficultyLevel +
                "\nCat.: [" + String.Join(", ", currentQuestion.categoryIds) + "]" +
                "\nQ: " + currentQuestion.shortText +
                "\n- A: " + selectedAnswer.shortText;

        SolutionService solutionService = SolutionService.GetInstance();
        Boolean isCorrect = await solutionService.IsCorrectAnswer(currentQuestion.id, answerId);
        feedbackTextField.text = isCorrect ? "Congratulations" : "Sorry, you selected a wrong answer";
        questionTextField.text += "\n\nF: " + (isCorrect ? "Good choice" : "Wrong choice") +
            "\nD: " + selectedAnswer.longText;
    }

    private string currentQuizMatchId;

    [ContextMenu("Validate Answer (async)")]
    public async void _A_CreateMatch()
    {
        string categoryId = "finance";
        string progressionMapId = "...";
        QuizMatchService quizMatchService = QuizMatchService.GetInstance();
        currentQuizMatchId = await quizMatchService.Create(categoryId, progressionMapId);
        feedbackTextField.text = "New Quiz Match ID: " + currentQuizMatchId;
    }

    [ContextMenu("Validate Answer (async)")]
    public async void _A_GetNextQuestionId()
    {
        //string currentTileId = "Easy";
        string currentTileId = "Medium";
        //string currentTileId = "Hard";
        questionIdField.text = "Loading...";
        QuizMatchService quizMatchService = QuizMatchService.GetInstance();
        string questionId = await quizMatchService.GetNextQuestionId(currentQuizMatchId, currentTileId);
        feedbackTextField.text = "New Question ID ready";
        questionIdField.text = questionId;
    }
}