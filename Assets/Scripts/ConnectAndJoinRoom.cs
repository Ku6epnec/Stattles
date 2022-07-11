using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System.Linq;
using ExitGames.Client.Photon;

public class ConnectAndJoinRoom : MonoBehaviour, IConnectionCallbacks, IMatchmakingCallbacks, ILobbyCallbacks
{
    [SerializeField] private ServerSettings _serverSettings;
    [SerializeField] private TMP_Text _stateUiText;

    private const string GAME_MOD_KEY = "gm";
    private const string AI_MOD_KEY = "ai";

    private const string MONEY_PROPERTY_KEY = "C0";
    private const string MAP_PROPERTY_KEY = "C1";

    private TypedLobby _sqlLobby = new TypedLobby("sqlLobby", LobbyType.SqlLobby);
    private TypedLobby _asyncRandomLobby = new TypedLobby("asyncRandomLobby", LobbyType.AsyncRandomLobby);

    private LoadBalancingClient _loadBalancingClient;

    private void Start()
    {
        _loadBalancingClient = new LoadBalancingClient();
        _loadBalancingClient.AddCallbackTarget(this);

        if (!_loadBalancingClient.ConnectUsingSettings(_serverSettings.AppSettings))
        {
            Debug.LogError("Error connections");
        }
    }

    private void OnDestroy()
    {
        _loadBalancingClient.RemoveCallbackTarget(this);
    }

    private void Update()
    {
        if (_loadBalancingClient == null)
            return;

        _loadBalancingClient.Service();

        var state = _loadBalancingClient.State.ToString();
        _stateUiText.text = "State: " + state + ", userId" + _loadBalancingClient.UserId; 
    }

    public void OnConnected()
    {

    }

    public void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master");
        //_loadBalancingClient.OpJoinRandomRoom();
        var roomOptions = new RoomOptions
        {
            MaxPlayers = 12,
            CustomRoomProperties = new Hashtable { { MONEY_PROPERTY_KEY, 400 }, { MAP_PROPERTY_KEY, "Map_3" } },
            CustomRoomPropertiesForLobby = new[] { MONEY_PROPERTY_KEY, MAP_PROPERTY_KEY },
            IsVisible = false,
            PublishUserId = true
        };
        var enterRoomParams = new EnterRoomParams 
        { 
            RoomName = "Game Room", 
            RoomOptions = roomOptions, 
            Lobby = _sqlLobby, 
            ExpectedUsers = new[] { "111", "222", "333", "444" } 
        };
        _loadBalancingClient.OpCreateRoom(enterRoomParams);

        _loadBalancingClient.OpFindFriends(new[] { _loadBalancingClient.UserId });
        //_loadBalancingClient.CurrentRoom.Players[0].UserId
    }

    public void OnCreatedRoom()
    {
        Debug.Log("Created room");
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

    }

    public void OnFriendListUpdate(List<FriendInfo> friendList)
    {
    }

    public void OnJoinedLobby()
    {
        var sqlLobbyFilter = MONEY_PROPERTY_KEY + " BETWEEN 300 AND 500 AND " + MAP_PROPERTY_KEY + "= 'Map_3'";
        var opJoinRandomRoomParams = new OpJoinRandomRoomParams
        {
            SqlLobbyFilter = sqlLobbyFilter
        };
        _loadBalancingClient.OpJoinRandomRoom(opJoinRandomRoomParams);
    }

    public void OnJoinedRoom()
    {
        Debug.Log("Joined room");
        Debug.Log(_loadBalancingClient.CurrentRoom);
    }

    public void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("Join random room Failed");
        _loadBalancingClient.OpCreateRoom(new EnterRoomParams());
    }

    public void OnJoinRoomFailed(short returnCode, string message)
    {
    }

    public void OnLeftLobby()
    {
    }

    public void OnLeftRoom()
    {
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
    }
}
