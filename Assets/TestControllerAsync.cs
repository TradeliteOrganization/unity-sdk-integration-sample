using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Tradelite.SDK.Model.ConfigScope;
using Tradelite.SDK.Model.KnowledgeScope;
using Tradelite.SDK.Model.MatchingScope;
using Tradelite.SDK.Model.UserScope;
using Tradelite.SDK.Service.ConfigScope;
using Tradelite.SDK.Service.KnowledgeScope;
using Tradelite.SDK.Service.MatchingScope;
using Tradelite.SDK.Service.UserScope;
using UnityEngine;
using UnityEngine.UI;

public class TestControllerAsync : MonoBehaviour {

    private GameConfiguration gameConfig;
    private string authToken;
    private bool forceReload = true;

    [ContextMenu("Force Singleton Reload")]
    public void _Z_ToggleForceReload() {
        forceReload = !forceReload;
        Debug.Log(forceReload ? "Services will be now forced to reload" : "Services will be now always using the initial instance");
    }

    [SerializeField] TMP_Text feedbackTextField;
    [SerializeField] TMP_InputField gameIdInputField;
    [SerializeField] TMP_InputField usernameInputField;
    [SerializeField] TMP_InputField passwordInputField;
    [SerializeField] TMP_InputField usernameSuffixInputField;
    [SerializeField] TMP_InputField runIdInputField;
    [SerializeField] TMP_InputField questionIdInputField;
    [SerializeField] TMP_InputField guestIdInputField;
    [SerializeField] TMP_InputField stockCardIdInputField;
    [SerializeField] TMP_InputField stockCardIdsInputField;
    [SerializeField] TMP_Text questionTextField;
    [SerializeField] TMP_Text answerTextField;
    [SerializeField] Button choice1Button;
    [SerializeField] Button choice2Button;
    [SerializeField] Button choice3Button;
    [SerializeField] Button choice4Button;
    [SerializeField] Button choice5Button;

    void Start() {
        feedbackTextField.text = "Set the Game ID with something like 'StockTiles_Local' and load the configuration.";
        /**/
        gameIdInputField.text = "StockTiles_Local";
        usernameInputField.text = "ddd";
        passwordInputField.text = "ddd";
        runIdInputField.text = "r-bbb";
        /**/
    }

    [ContextMenu("Load Config (async)")]
    public async void _A_LoadConfig() {
        string id = gameIdInputField.text;
        if (string.IsNullOrEmpty(id)) {
            feedbackTextField.text = "Missing Game ID!\nSet the Game ID with something like 'StockTiles_Local' and load the configuration.";
        }
        else {
            try {
                GameConfigurationService service = GameConfigurationService.GetInstance(id, forceReload);
                gameConfig = await service.Get();
                feedbackTextField.text = "Configuration retrieved.\nTemporarily, the SDK need administrative credentials, so trigger the admin authentication.";
            }
            catch (Exception) {
                feedbackTextField.text = "Invalid Game ID!\nSet the Game ID with something like 'StockTiles_Local' and load the configuration.";
            }

        }
    }

    [ContextMenu("Authenticate Admin (async)")]
    public async void _B_AuthenticateAdmin() {
        string username = gameConfig.GetCredential("adminName");
        string password = gameConfig.GetCredential("adminPsw");
        string message = await _Authenticate("Admin", username, password);
        feedbackTextField.text = message;
    }

    [ContextMenu("Authenticate Player (async)")]
    public async void _B_AuthenticatePlayer() {
        string username = usernameInputField.text;
        string password = passwordInputField.text;
        string message = await _Authenticate("Player", username, password);
        feedbackTextField.text = message;
    }

    protected async Task<string> _Authenticate(string title, string username, string password) {
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password)) {
            return $"Missing {title} Credentials!\nSet the {title} credentials and authorize her..";
        }

        AuthenticationService service = AuthenticationService.GetInstance(gameConfig, forceReload);
        try {
            authToken = await service.Authenticate(username, password);
        }
        catch (Exception) {
            return $"{title} authentication failed!\nSet the {title} credentials and authorize her.";
        }

        return $"{title} successfully authorized.\nYou can create a new player or get data for a matching game.";
    }

    [ContextMenu("Create Random User (async)")]
    public async void _C_CreateRandomUser() {
        UserService service = UserService.GetInstance(gameConfig, authToken, forceReload);

        User user = new User();
        user.id = null;
        string suffix = usernameSuffixInputField.text;
        if (string.IsNullOrEmpty(suffix)) {
            suffix = "" + UnityEngine.Random.Range(1, 10000);
            usernameSuffixInputField.text = suffix;
        }
        user.firstname = "Fable";
        user.lastname = "Vader";
        user.email = user.firstname + "_" + user.lastname + "_" + suffix + "@example.com";
        user.password = "Password" + suffix;
        user.dateAcceptConditions = "2000-01-01T00:00:00.0Z";
        user.acceptConditionsVersion = "v1.0";
        user.ageVerification = true;
        user.mobile = true;

        try {
            string id = await service.Create(user);
            User copy = await service.Get(id);
            feedbackTextField.text = $"Player created with id {id} and credentials {{ {copy.email} / {user.password } }}.";
            Debug.Log($"Created Player: {user}");
        }
        catch (Exception) {
            feedbackTextField.text = "User creation failed...";
        }
    }

    [ContextMenu("Get Logged User Record (async)")]
    public async void _D_GetLoggedUserRecord() {
        AuthenticationService service = AuthenticationService.GetInstance(gameConfig, forceReload);

        User user = await service.GetLoggedUser();
        Debug.Log($"Logged Player: {user}");
    }

    public async void _E_GetMatchingRun() {
        MatchingRunService service = MatchingRunService.GetInstance(gameConfig, authToken, forceReload);

        string runId = runIdInputField.text;
        if (string.IsNullOrEmpty(runId)) {
            feedbackTextField.text = $"Run identifier is missing...";
            return;
        }

        try {
            MatchingRun run = await service.Get(runId);
            feedbackTextField.text = run.ToString();
            Debug.Log($"MatchingRun: {run}");
        }
        catch (Exception) {
            feedbackTextField.text = "Matching run retrieval failed...";
        }
    }

    public async void _E_GetQuestion() {
        QuestionService service = QuestionService.GetInstance(gameConfig, authToken, forceReload);

        string questionId = questionIdInputField.text;
        if (string.IsNullOrEmpty(questionId)) {
            feedbackTextField.text = $"Question identifier is missing...";
            return;
        }

        try {
            Question question = await service.Get(questionId);
            feedbackTextField.text = question.ToString();
            Debug.Log($"Question: {question}");
        }
        catch (Exception) {
            feedbackTextField.text = "Question retrieval failed...";
        }
    }

    public async void _F_GetStockCard() {
        StockCardService service = StockCardService.GetInstance(gameConfig, authToken, forceReload);

        string stockCardId = stockCardIdInputField.text;
        if (string.IsNullOrEmpty(stockCardId)) {
            feedbackTextField.text = $"StockCard identifier is missing...";
            return;
        }

        try {
            StockCard stockCard = await service.Get(stockCardId);
            feedbackTextField.text = $"Stock with symbol {stockCardId} has the name: {stockCard.name}";
            Debug.Log($"StockCard: {stockCard}");
        }
        catch (Exception) {
            feedbackTextField.text = "StockCard retrieval failed...";
        }
    }

    public async void _F_GetStockCards() {
        StockCardService service = StockCardService.GetInstance(gameConfig, authToken, forceReload);

        string stockCardIds = stockCardIdsInputField.text;
        if (string.IsNullOrEmpty(stockCardIds)) {
            feedbackTextField.text = $"StockCard identifier list is missing...";
            return;
        }

        try {
            Hashtable parameters = new Hashtable();
            parameters.Add("id", stockCardIds.Split(','));
            StockCard[] stockCards = await service.Select(parameters);
            feedbackTextField.text = stockCards.ToString();
        }
        catch (Exception) {
            feedbackTextField.text = "StockCard retrieval failed...";
        }
    }

    protected int questionIdx = 0;
    protected MatchingRun activeRun;
    protected int runScore;

    public async void _G_StartMatchingRun() {
        MatchingRunService service = MatchingRunService.GetInstance(gameConfig, authToken, forceReload);

        string runId = runIdInputField.text;
        if (string.IsNullOrEmpty(runId)) {
            feedbackTextField.text = $"Run identifier is missing...";
            return;
        }

        try {
            activeRun = await service.Get(runId);
            answerTextField.text = "A:";
            questionIdx = 0;
            runScore = 0;
            showQuestion();
        }
        catch (Exception) {
            feedbackTextField.text = "Matching run retrieval failed...";
        }
    }

    protected List<int> choiceDistribution;

    protected async void showQuestion() {
        // TODO:
        // - Support infinite run when run.questionNb == -1
        // - Ask for a random question when it's infinite and the questionIdx is greater than the questionIds array size
        // - Implement lookups in localized dictionaries

        if (activeRun.questionNb <= questionIdx) {
            // No more questions
            feedbackTextField.text = $"No more question in run `{activeRun.id}`.";
            questionTextField.text = "No more questions...";
            choice1Button.GetComponentInChildren<Text>().text = "1:";
            choice2Button.GetComponentInChildren<Text>().text = "2:";
            choice3Button.GetComponentInChildren<Text>().text = "3:";
            choice4Button.GetComponentInChildren<Text>().text = "4:";
            choice5Button.GetComponentInChildren<Text>().text = "5:";
            choice1Button.interactable = false;
            choice2Button.interactable = false;
            choice3Button.interactable = false;
            choice4Button.interactable = false;
            choice5Button.interactable = false;
            return;
        }

        QuestionService service = QuestionService.GetInstance(gameConfig, authToken, forceReload);

        try {
            Question question = await service.Get(activeRun.questionIds[questionIdx]);
            questionTextField.text = "Q: " + (question.useShort ? question.shortQuestionId : question.longQuestionId);
            choiceDistribution = distributeRandomly(question.choiceNb);
            updateChoiceButton(choice1Button, 0, question);
            updateChoiceButton(choice2Button, 1, question);
            updateChoiceButton(choice3Button, 2, question);
            updateChoiceButton(choice4Button, 3, question);
            updateChoiceButton(choice5Button, 4, question);
            feedbackTextField.text = $"Question {questionIdx + 1} of run `{activeRun.id}` is ready.";
        }
        catch (Exception) {
            feedbackTextField.text = $"Processing question {questionIdx} of run `{activeRun.id}` failed...";
        }
    }

    protected System.Random randomGenerator = new System.Random((int)DateTime.Now.Ticks);

    protected List<int> distributeRandomly(int limit) {
        List<int> temp = new List<int>(limit);
        for (int j = 0; j < limit; j++) {
            temp.Add(j);
        }
        List<int> output = new List<int>(limit);
        for (int i = 0; i < limit; i++) {
            int maxPosition = limit - i;
            int randomPosition = randomGenerator.Next(maxPosition);
            int pick = temp[randomPosition];
            temp.RemoveAt(randomPosition);
            output.Add(pick);
        }
        return output;
    }

    protected void updateChoiceButton(Button button, int buttonIdx, Question question) {
        int choiceIdx = choiceDistribution.Count <= buttonIdx ? 1000 : choiceDistribution[buttonIdx];

        if (choiceIdx < question.choiceNb) {
            button.GetComponentInChildren<Text>().text = (buttonIdx + 1) + ": " + (question.useShort ? question.shortChoiceIds[choiceIdx] : question.longChoiceIds[choiceIdx]);
            button.interactable = true;
        }
        else {
            button.GetComponentInChildren<Text>().text = (buttonIdx + 1) + ":";
            button.interactable = false;
        }
    }

    public async void _G_ProcessChoice(int buttonIdx) {
        AnswerService answerService = AnswerService.GetInstance(gameConfig, authToken, forceReload);
        QuestionService questionService = QuestionService.GetInstance(gameConfig, authToken, forceReload);

        try {
            Answer answer = await answerService.Get(activeRun.questionIds[questionIdx]);
            Question question = await questionService.Get(activeRun.questionIds[questionIdx]);
            string initialQuestionText = question.useShort ? question.shortQuestionId : question.longQuestionId;
            string rightAnswerText = question.useShort ? question.shortChoiceIds[answer.validIdx] : question.longChoiceIds[answer.validIdx];
            int translatedValidIdx = choiceDistribution.FindIndex(idx => idx == answer.validIdx); // Find the index of the button which displays the valid answer
            bool success = buttonIdx == translatedValidIdx;
            runScore += success ? answer.scoreWeight : 0;
            answerTextField.text = $"Q: {initialQuestionText}\nA: {(success ? "Youpi!" : "Nope!")} The right anwswer is: {rightAnswerText}.\nYour score for this run is: {runScore} points!";
        }
        catch (Exception) {
            feedbackTextField.text = $"Processing question {questionIdx} of run `{activeRun.id}` failed...";
        }

        questionIdx += 1;
        showQuestion();
    }

    public async void _H_CreateGuestAccount() {
        GuestService service = GuestService.GetInstance(gameConfig, authToken, forceReload);

        try {
            string guestId = await service.Create(null);
            Guest guest = await service.Get(guestId);

            guestIdInputField.text = guest.id;
        }
        catch (Exception) {
            feedbackTextField.text = $"Creating guest account failed...";
        }
    }

    public void _H_ReleaseGuestAccount() {
        GuestService service = GuestService.GetInstance(gameConfig, authToken, forceReload);


        try {
            service.Delete(null);

            guestIdInputField.text = "";
        }
        catch (Exception) {
            feedbackTextField.text = $"Deleting guest account failed...";
        }
    }

}