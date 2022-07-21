using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System.Linq;
using ExitGames.Client.Photon;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine.SceneManagement;

public class ConnectAndJoinRoom : MonoBehaviour, IConnectionCallbacks, IMatchmakingCallbacks, ILobbyCallbacks
{
    [SerializeField] private ServerSettings _serverSettings;
    [SerializeField] private TMP_Text _stateUiText;
    [SerializeField] private TMP_Text _statusRoomText;
    [SerializeField] private Button _openCloseRoomButton;
    [SerializeField] private Button _startGameButton;
    [SerializeField] private Button _createRandomRoomButton;
    [SerializeField] private Button _connectRoomButton;
    [SerializeField] private Button _leaveRoomButton;
    [SerializeField] private TMP_InputField _openCloseRoomInputField;
    [SerializeField] private TMP_Dropdown _roomListDropdown;

    [SerializeField] private GameObject _canvasControl;

    [SerializeField] private GameObject _gameManager;

    [SerializeField] private GameObject _playerPrefab;

    private const string openText = "Open";
    private const string closeText = "Close";

    private const string GAME_MOD_KEY = "gm";
    private const string AI_MOD_KEY = "ai";

    private const string MONEY_PROPERTY_KEY = "C0";
    private const string MAP_PROPERTY_KEY = "C1";

    private TypedLobby _sqlLobby = new TypedLobby("sqlLobby", LobbyType.SqlLobby);
    private TypedLobby _asyncRandomLobby = new TypedLobby("asyncRandomLobby", LobbyType.AsyncRandomLobby);

    private Dictionary<string, RoomInfo> cachedRoomList = new Dictionary<string, RoomInfo>();
    private List<RoomInfo> _roomList = new List<RoomInfo>();
    [SerializeField] private List<TMP_Dropdown.OptionData> _options= new List<TMP_Dropdown.OptionData>();

    bool isConnecting;
    private LoadBalancingClient _loadBalancingClient;

    private RoomInfo _choosenRoom;

    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    private void Start()
    {
        _openCloseRoomButton.onClick.AddListener(CloseRoom);
        _startGameButton.onClick.AddListener(StartGame);
        _createRandomRoomButton.onClick.AddListener(OnCreatedRoom);
        _leaveRoomButton.onClick.AddListener(OnLeftRoom);
        _connectRoomButton.onClick.AddListener(OnConnectedTestRoom);
        _roomListDropdown.onValueChanged.AddListener(OnChooseRoom);

        _loadBalancingClient = new LoadBalancingClient();
        _loadBalancingClient.AddCallbackTarget(this);

        if (!_loadBalancingClient.ConnectUsingSettings(_serverSettings.AppSettings))
        {
            Debug.LogError("Error connections");
        }

    }

    private void OnChooseRoom(int roomId)
    {
        _choosenRoom = _roomList[roomId];
    }

    private void UpdateCachedRoomList(List<RoomInfo> roomList)
    {
        Debug.Log("Update cached RoomList with Room count: " + roomList.Count);
        for (int i = 0; i < roomList.Count; i++)
        {
            RoomInfo info = roomList[i];
            if (info.RemovedFromList)
            {
                Debug.Log("Room remove of RoomList : " + cachedRoomList[info.Name]);
                cachedRoomList.Remove(info.Name);
            }
            else
            {
                //Debug.Log("Add room, and RoomList: " + cachedRoomList.Count);
                cachedRoomList[info.Name] = info;
                Debug.Log("New Room name: " + cachedRoomList[info.Name]);
                //_options.AddRange(new);
                _roomListDropdown.AddOptions(_options);
                _roomListDropdown.itemText.text = info.Name;
            }
        }
    }


    private void CloseRoom()
    {
        _loadBalancingClient.CurrentRoom.IsOpen = !_loadBalancingClient.CurrentRoom.IsOpen;
        Debug.Log("Status room: " + _loadBalancingClient.CurrentRoom.IsOpen);
        CheckStatusRoom();
    }

    private void CheckStatusRoom()
    {
        if (_loadBalancingClient.CurrentRoom.IsOpen)
        {
            _openCloseRoomInputField.text = "Room " + openText;
            _openCloseRoomInputField.image.color = Color.green;
            _statusRoomText.text = closeText;
        }
        else
        {
            _openCloseRoomInputField.text = "Room " + closeText;
            _openCloseRoomInputField.image.color = Color.yellow;
            _statusRoomText.text = openText;
        }    
    }

    private void OnDestroy()
    {
        _loadBalancingClient.RemoveCallbackTarget(this);
        _openCloseRoomButton.onClick.RemoveAllListeners();
        _startGameButton.onClick.RemoveAllListeners();
        _createRandomRoomButton.onClick.RemoveAllListeners();
        _leaveRoomButton.onClick.RemoveAllListeners();
    }

    private void Update()
    {
        if (_loadBalancingClient == null)
            return;

        _loadBalancingClient.Service();

        var state = _loadBalancingClient.State.ToString();
        _stateUiText.text = "State: " + state + ", userId" + _loadBalancingClient.UserId;

        //Debug.Log("Client state: " + PhotonNetwork.NetworkClientState);
        //Debug.Log("Cached Room List: " + cachedRoomList.Count);
    }

    public void OnConnectedTestRoom()
    {
        isConnecting = true;
        _canvasControl.SetActive(false);

        Debug.Log("Try Connect");
        _loadBalancingClient.OpJoinRandomRoom();
    }

    public void OnConnected()
    {
        
    }

    public void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master");
        //_loadBalancingClient.OpJoinRandomRoom();
        //JoinTestRoom();
        //_loadBalancingClient.OpJoinLobby(_sqlLobby);
        //foreach (var room in )
        //_loadBalancingClient.OpJoinLobby(default);
        //_loadBalancingClient.OpJoinRandomRoom();
        //_loadBalancingClient.OpFindFriends(new[] { _loadBalancingClient.UserId });
        //_loadBalancingClient.CurrentRoom.Players[0].UserId
    }

    private void JoinTestRoom()
    {
        var roomOptions = new RoomOptions
        {
            MaxPlayers = 12,
            CustomRoomProperties = new Hashtable { { MONEY_PROPERTY_KEY, 400 }, { MAP_PROPERTY_KEY, "Map_3" } },
            CustomRoomPropertiesForLobby = new[] { MONEY_PROPERTY_KEY, MAP_PROPERTY_KEY },
            IsVisible = true,
            PublishUserId = true
        };
        var enterRoomParams = new EnterRoomParams
        {
            RoomName = "Game Room",
            RoomOptions = roomOptions,
            Lobby = _sqlLobby,
            ExpectedUsers = new[] { "111", "222", "333", "444" }
        };
        PhotonNetwork.ConnectUsingSettings();
        //PhotonNetwork.Instantiate(_playerPrefab.name, new Vector3(0f, 5f, 0f), Quaternion.identity, 0);
        _loadBalancingClient.OpCreateRoom(enterRoomParams);
        //SceneManager.LoadScene(3);
    }

    public void OnCreatedRoom()
    {
        Debug.Log("Created room");
        _roomList.Add(_loadBalancingClient.CurrentRoom);
        OnRoomListUpdate(_roomList);
    }

    public void OnCreateRoomFailed(short returnCode, string message)
    {
        
    }

    public void OnCustomAuthenticationFailed(string debugMessage)
    {
        
    }

    public void OnCustomAuthenticationResponse(Dictionary<string, object> data)
    {
        
    }

    public void OnDisconnected(DisconnectCause cause)
    {
        cachedRoomList.Clear();
        _canvasControl.SetActive(true);
    }

    public void OnFriendListUpdate(List<Photon.Realtime.FriendInfo> friendList)
    {

    }

    public void OnJoinedLobby()
    {
        cachedRoomList.Clear();
        var sqlLobbyFilter = MONEY_PROPERTY_KEY + " BETWEEN 300 AND 500 AND " + MAP_PROPERTY_KEY + "= 'Map_3'";
        var opJoinRandomRoomParams = new OpJoinRandomRoomParams
        {
            SqlLobbyFilter = sqlLobbyFilter
        };
        _loadBalancingClient.OpJoinRandomRoom(opJoinRandomRoomParams);
        Debug.Log("Joined Lobby");
    }

    public void OnJoinedRoom()
    {
        Debug.Log("Joined Room");
        Debug.Log(_loadBalancingClient.CurrentRoom);
        Debug.Log("Status room: " + _loadBalancingClient.CurrentRoom);
        CheckStatusRoom();
        Debug.Log("CurrentRoom status: " + PhotonNetwork.CurrentRoom);
        //PhotonNetwork.LoadLevel("PunBasics-Room for 1");

        Instantiate(_playerPrefab, new Vector3(0f, 5f, 0f), Quaternion.identity);
        //Instantiate(_gameManager);
        //PhotonNetwork.Instantiate(this.playerPrefab.name, new Vector3(0f, 5f, 0f), Quaternion.identity, 0);
        //PhotonNetwork.LoadLevel("PunBasics-Room for 1");
        //_loadBalancingClient.OpLeaveRoom(false, false);
    }

    public void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("Join random room Failed");
        JoinTestRoom();
        //_loadBalancingClient.OpCreateRoom(new EnterRoomParams());
    }

    public void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log("Попытка");
    }

    public void OnLeftLobby()
    {
        cachedRoomList.Clear();
    }
        
    public void OnLeftRoom()
    {
        Debug.Log("Left Room");
    }

    public void OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics)
    {

    }

    public void OnRegionListReceived(RegionHandler regionHandler)
    {

    }

    public void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        //roomList.First().
        UpdateCachedRoomList(roomList);
        Debug.Log("Room list Update");
    }

    private void StartGame()
    {
        if (_loadBalancingClient.CurrentRoom.IsOpen)
        {
            Debug.Log("You need Close Room before start");
        }
        else
        {
            Debug.Log("Game Begin!!!");
        }
    }
}
