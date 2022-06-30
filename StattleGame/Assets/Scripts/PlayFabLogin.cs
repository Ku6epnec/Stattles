using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;

public class PlayFabLogin : MonoBehaviour
{
    [SerializeField] private TMP_InputField Result;
        
    public void TryToLogin()
    {
        if (string.IsNullOrEmpty(PlayFabSettings.staticSettings.TitleId))
            PlayFabSettings.staticSettings.TitleId = "1B50D";

        var request = new LoginWithCustomIDRequest
        {
            CustomId = "Player 1",
            CreateAccount = true
        };

        PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnLoginError);
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
