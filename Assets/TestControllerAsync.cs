using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Tradelite.SDK.DAO;
using Tradelite.SDK.Model.KbScope;
using Tradelite.SDK.Model.UserScope;
using Tradelite.SDK.Service.KbScope;
using Tradelite.SDK.Service.UserScope;
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
            
                UserService userService = UserService.GetInstance();
                User user = await userService.Get("me");
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
        CategoryService categoryService = CategoryService.GetInstance();
        string[] categoryIds = await categoryService.GetIds();
        feedbackTextField.text = $"{categoryIds.Length} category ids retrieved.";
        Category[] categories = await categoryService.GetByIds(categoryIds);
        feedbackTextField.text = "Categories retrieved. Example: " + categories[0].title;
    }

}