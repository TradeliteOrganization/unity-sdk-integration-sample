using System.Collections;
using System.Collections.Generic;
using Tradelite.SDK.Model;
using Tradelite.SDK.Model.ConfigScope;
using Tradelite.SDK.Model.UserScope;
using Tradelite.SDK.Service.ConfigScope;
using Tradelite.SDK.Service.UserScope;
using UnityEngine;


public class TestControllerWithCallbacks : MonoBehaviour {

    private GameConfiguration gameConfig;
    private string authToken;
    private bool forceReload = true;

    public void ToggleForceReload() {
        forceReload = !forceReload;
        Debug.Log(forceReload ? "Services will be now forced to reload" : "Services will be now always using the initial instance");
    }

    [ContextMenu("Test Configuration Service (CB)")]
    public void TestConfigServiceCB() {
        GameConfigurationService service = GameConfigurationService.GetInstance("StockTiles_Integration", forceReload);
        service.Get(
            (GameConfiguration gC) => { gameConfig = gC; Debug.Log("Configuration: " + gameConfig); },
            (BaseError e) => { Debug.Log($"Error: {e.message}"); }
        );
    }

    [ContextMenu("Test Authentication Service (CB)")]
    public void TestAuthenticationServiceCB() {
        AuthenticationService service = AuthenticationService.GetInstance(gameConfig, forceReload);
        service.Authenticate(
            gameConfig.GetCredential("playerUser"),
            gameConfig.GetCredential("playerPsw"),
            (string aT) => { authToken = aT; Debug.Log("Token: " + authToken); },
            (BaseError e) => { Debug.Log($"Error: {e.message}"); }
        );
    }
}