using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
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

    void Start() {
        feedbackTextField.text = "Provide a valid JWT Token to get the user info.";
    }

    [ContextMenu("Get User (async)")]
    public async void _A_GetUser() {
        string jwtToken = jwtTokenInputField.text;
        if (string.IsNullOrEmpty(jwtToken)) {
            feedbackTextField.text = "Missing JWT Token.";
        }
        else {
            try {
                HttpDao<User>.jwtToken = jwtToken;

                UserService service = UserService.GetInstance();
                User user = await service.Get("me");
                Debug.Log($"Player: {user}");

                feedbackTextField.text = "User data retrieved.";
                userIdTextField.text = user.id;
                nicknameTextField.text = user.nickname;
            }
            catch (Exception) {
                feedbackTextField.text = "Invalid JWT token or invalid connection parameters. The information for the user cannot be retrieved.";
            }

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

}