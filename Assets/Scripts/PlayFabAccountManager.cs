using UnityEngine.SceneManagement;
using UnityEngine;
using TMPro;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class PlayFabAccountManager : MonoBehaviour
{
    [SerializeField] private TMP_Text _titleLabel;
    [SerializeField] private TMP_Text _catalogLabel = null;
    [SerializeField] private Button _changeAccountButton;
    [SerializeField] private Slider _sliderLoadingProcess;
    [SerializeField] private TMP_Text _loadingValue;
    [SerializeField] private Image _imageEndLoading;

    [SerializeField] private GameObject _newCharacterCreatePanel;
    [SerializeField] private Button _createCharacterButton;
    [SerializeField] TMP_InputField _inputField;
    [SerializeField] private List<SlotCharacterWidget> _slots;

    private string _characterName;

    private int _endTimer = 100;
    private bool _timerStatus = true;

    private void Start()
    {
        LoadingTimer();
        PlayFabClientAPI.GetAccountInfo(new GetAccountInfoRequest(), OnGetAccountSuccess, OnError);
        _changeAccountButton.onClick.AddListener(ChangeAccount);
        PlayFabClientAPI.GetCatalogItems(new GetCatalogItemsRequest(), OnGetCatalogSuccess, OnError);

        foreach (var slot in _slots)
        {
            slot.SlotButton.onClick.AddListener(OpenPanerCreateCharacter);
        }

        _createCharacterButton.onClick.AddListener(CreateCharacterWithItem);
        _inputField.onValueChanged.AddListener(OnNameChange);
    }

    private void OnNameChange(string characterName)
    {
        _characterName = characterName;
    }

    private void CreateCharacterWithItem()
    {
        PlayFabClientAPI.GrantCharacterToUser(new GrantCharacterToUserRequest
        {
            CharacterName = _characterName,
            ItemId = "character_item"
        },
        result =>
        {
            UpdateCharacterStatistics(result.CharacterId);
        }, OnError);
    }

    private void UpdateCharacterStatistics(string characterId)
    {
        PlayFabClientAPI.UpdateCharacterStatistics(new UpdateCharacterStatisticsRequest
        {
            CharacterId = characterId,
            CharacterStatistics = new Dictionary<string, int>
            {
                {"Level", 1},
                {"Gold", 0}
            }
        }, result =>
        {
            Debug.Log("Complete character!");
            ClosePanelCreateCharacter();
            GetCharacter(); 
        }, OnError);
    }

    private void OpenPanerCreateCharacter()
    {
        _newCharacterCreatePanel.SetActive(true);
    }

    private void ClosePanelCreateCharacter()
    {
        _newCharacterCreatePanel.SetActive(false);
    }

    private void GetCharacter()
    {
        PlayFabClientAPI.GetAllUsersCharacters(new ListUsersCharactersRequest(),
            result =>
            {
                Debug.Log("Character count: " + result.Characters.Count);
                ShowInfoCharacter(result.Characters);
            }, OnError);
    }

    private void ShowInfoCharacter(List<CharacterResult> characters)
    {
        if (characters.Count == 0)
        {
            foreach(var slot in _slots)
            {
                slot.ShowEmptySlot();
            }
        }
        else if(characters.Count > 0 && characters.Count <= _slots.Count)
        {
            PlayFabClientAPI.GetCharacterStatistics(new GetCharacterStatisticsRequest
            {
                CharacterId = characters.First().CharacterId
            },
            result =>
            {
                var level = result.CharacterStatistics["Level"].ToString();
                var gold = result.CharacterStatistics["Gold"].ToString();

                _slots.First().ShowInfoCharacterSlot(characters.First().CharacterName, level, gold);
            }, OnError);
        }
        else
        {
            Debug.Log("Add a slots for characters");
        }
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
