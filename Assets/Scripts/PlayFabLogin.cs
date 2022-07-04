using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using System;

public class PlayFabLogin : MonoBehaviour
{
    private const string AuthGuidKey = "auth_guid";

    [SerializeField] private TMP_InputField Result;
        
    public void TryToLogin()
    {
        if (string.IsNullOrEmpty(PlayFabSettings.staticSettings.TitleId))
            PlayFabSettings.staticSettings.TitleId = "1B50D";

        var needCreation = PlayerPrefs.HasKey(AuthGuidKey);
        var id = PlayerPrefs.GetString(AuthGuidKey, Guid.NewGuid().ToString());

        var request = new LoginWithCustomIDRequest
        {
            CustomId = id,
            CreateAccount = !needCreation
        };

        PlayFabClientAPI.LoginWithCustomID(request, success =>
        {
            PlayerPrefs.SetString(AuthGuidKey, id);
            OnLoginSuccess(success);
        },
            OnLoginError);
    }

    private void OnLoginSuccess(LoginResult result)
    {
        Result.image.color = Color.green;
        Debug.Log("Complete!");
        Result.text = "Your connection Success";
    }

    private void OnLoginError(PlayFabError error)
    {
        Result.image.color = Color.red;
        var errorMessage = error.GenerateErrorReport();
        Debug.Log("Error: " + errorMessage);
        Result.text = "Your connection has Error: " + errorMessage;
    }

}
