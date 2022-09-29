using System.Collections;
using System.Collections.Generic;
using TradeliteSDK.Model;
using TradeliteSDK.Model.UserScope;
using TradeliteSDK.Service.UserScope;
using UnityEngine;


public class TestControllerWithCallbacks : MonoBehaviour {

    private bool forceReload = true;

    public void ToggleForceReload() {
        forceReload = !forceReload;
        Debug.Log(forceReload ? "Services will be now forced to reload" : "Services will be now always using the initial instance");
    }

    [ContextMenu("Test Configuration Service (CB)")]
    public void TestConfigServiceCB() {
    }

    [ContextMenu("Test Authentication Service (CB)")]
    public void TestAuthenticationServiceCB() {
        UserService service = UserService.GetInstance();
        service.Get(
            "me",
            null,
            (User u) => { Debug.Log("User: " + u); },
            (BaseError e) => { Debug.Log($"Error: {e.message}"); }
        );
    }
}