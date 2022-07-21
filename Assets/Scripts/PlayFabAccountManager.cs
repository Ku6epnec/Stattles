using UnityEngine.SceneManagement;
using UnityEngine;
using TMPro;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;

public class PlayFabAccountManager : MonoBehaviour
{
    [SerializeField] private TMP_Text _titleLabel;
    [SerializeField] private TMP_Text _catalogLabel = null;
    [SerializeField] private Button _changeAccountButton;
    [SerializeField] private Slider _sliderLoadingProcess;
    [SerializeField] private TMP_Text _loadingValue;
    [SerializeField] private Image _imageEndLoading;

    private int _endTimer = 100;
    private bool _timerStatus = true;

    private void Start()
    {
        LoadingTimer();
        PlayFabClientAPI.GetAccountInfo(new GetAccountInfoRequest(), OnGetAccountSuccess, OnError);
        _changeAccountButton.onClick.AddListener(ChangeAccount);
        PlayFabClientAPI.GetCatalogItems(new GetCatalogItemsRequest(), OnGetCatalogSuccess, OnError);
    }

    private void OnGetCatalogSuccess(GetCatalogItemsResult result)
    {
        ShowCatalog(result.Catalog);
        //Debug.Log("Complete load catalog!");
    }

    private void ShowCatalog(List<CatalogItem> catalog)
    {
        foreach (var item in catalog)
        {
            if (item.Bundle == null)
            {
                //Debug.Log("item_id: " + item.ItemId);
                _catalogLabel.text += "item_id: " + item.ItemId + "\n";
            }
        }
    }

    private void Update()
    {
        LoadingTimer();
    }

    private void ChangeAccount()
    {
        SceneManager.LoadScene(0);
    }

    private void OnGetAccountSuccess(GetAccountInfoResult result)
    {
        var accountInfo = result.AccountInfo;
        _titleLabel.text = "Welcome " + accountInfo.Username + "\n" + accountInfo.PlayFabId;
        _sliderLoadingProcess.value = _endTimer; 
        _timerStatus = false;
        _imageEndLoading.color = Color.magenta;
        //_catalogLabel.text = "Catalog.data: " + File.ReadAllText("Assets/title-1B50D-FirstCatalog.json");
    }


    private void OnError(PlayFabError error)
    {
        var errorMessage = error.GenerateErrorReport();
        //Debug.LogError(errorMessage);
        _sliderLoadingProcess.value = _endTimer;
        _timerStatus = false;
        _imageEndLoading.color = Color.red;
    }

    private void LoadingTimer()
    {
        if (_timerStatus)
        {
            _sliderLoadingProcess.value += 0.3f;
            _loadingValue.text = "Loading " + Mathf.Round(_sliderLoadingProcess.value) + "%";
        }
        else
        {
            _loadingValue.text = "Loading " + Mathf.Round(_sliderLoadingProcess.value) + "%";
        }
    }
}
