using ExitGames.Client.Photon;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Photon.Pun.MicroGames
{
    public class RoomManager : MonoBehaviourPunCallbacks
    {

        [Header("Selection Panel")]
        public GameObject SelectionPanel;

        [Header("Lobby Panel")]
        public GameObject LobbyPanel;
        public Image PlayerOneImage;
        public Sprite PlayerOneSprite;
        public Text PlayerOneText;
        public Image PlayerTwoImage;
        public Sprite PlayerTwoSprite;
        public Text PlayerTwoText;
        public GameObject SearchingText;
        public GameObject BackButton;
        public Button StartGameButton;

        [Header("Games")]
        public string[] MiniGames;

        private Dictionary<int, GameObject> _playerListEntries;
        private string _playerName;
        private string _playerTeam = "red";
        private string _specificGame = "";

        #region UNITY

        public void SetPlayerName(string name)
        {
            Debug.Log("set name to " + name);

            _playerName = name;
        }
        public void SetPlayerTeam(string team)
        {
            Debug.Log("set team to " + team);

            _playerTeam = team;
        }
        private bool CheckForPlayers()
        {
            return PhotonNetwork.CurrentRoom.PlayerCount == 2;
        }

        private void SetActivePanel(string activePanel)
        {
            SelectionPanel.SetActive(activePanel.Equals(SelectionPanel.name));
            LobbyPanel.SetActive(activePanel.Equals(LobbyPanel.name));
        }

        void Start()
        {
            StartGameButton.interactable = false;
            SetActivePanel(SelectionPanel.name);
        }

        void Awake()
        {
            _playerName = "Player" + Random.Range(1000, 10000);

            PhotonNetwork.AutomaticallySyncScene = true;
            PhotonNetwork.LocalPlayer.NickName = _playerName;
            PhotonNetwork.ConnectUsingSettings();
        }

        #endregion

        #region PUN CALLBACKS

        public override void OnConnectedToMaster()
        {
            Debug.Log("connected to master");
            this.SetActivePanel(SelectionPanel.name);
        }

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            Debug.Log("failed to join random");
            if (_specificGame == "")
                CreateRandomRoom();
            else
                CreateSpecificRoom(_specificGame);
        }

        public override void OnJoinedRoom()
        {
            Debug.Log("joined room: " +PhotonNetwork.CurrentRoom.Name);
            SetActivePanel(LobbyPanel.name);

            if (CheckForPlayers())
            {
                SearchingText.SetActive(false);

                PlayerOneText.text = PhotonNetwork.PlayerList[0].NickName;
                PlayerOneImage.sprite = PlayerOneSprite;
                PlayerTwoText.text = _playerName;
                PlayerTwoImage.sprite = PlayerTwoSprite;

                PlayerTwoImage.gameObject.SetActive(true);
                StartGameButton.interactable = true;
            }
            else
            {
                PlayerOneText.text = _playerName;
                PlayerOneImage.sprite = PlayerOneSprite;
            }

            /*
            Hashtable props = new Hashtable
            {
                {AsteroidsGame.PLAYER_LOADED_LEVEL, false}
            };
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);
            */
        }

        public override void OnLeftRoom()
        {
            Debug.Log("left room");

            SetActivePanel(SelectionPanel.name);
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            SearchingText.SetActive(false);
            BackButton.SetActive(false);

            PlayerTwoText.text = PhotonNetwork.PlayerList[1].NickName;
            PlayerTwoImage.sprite = PlayerTwoSprite;

            PlayerTwoImage.gameObject.SetActive(true);
            StartGameButton.interactable = true;
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            SearchingText.SetActive(true);

            PlayerTwoImage.gameObject.SetActive(false);
            StartGameButton.interactable = false;
        }

        #endregion

        #region UI CALLBACKS

        public void JoinSpecificRoom(string game_name)
        {
            Debug.Log("attempting to join " +game_name +" room");

            string other_team = _playerTeam.Equals("red") ? "blue" : "red";
            Hashtable expectedRoomProperties = new Hashtable() { { "game_name", game_name }, { "host_team", other_team } };

            PhotonNetwork.JoinRandomRoom(expectedRoomProperties, 2);
        }

        public void JoinRandomRoom()
        {
            Debug.Log("attempting to join random room");

            string other_team = _playerTeam.Equals("red") ? "blue" : "red";
            Hashtable expectedRoomProperties = new Hashtable() { { "host_team", other_team } };

            PhotonNetwork.JoinRandomRoom(expectedRoomProperties, 2);
        }

        public void CreateSpecificRoom(string game_name)
        {
            Debug.Log("creating " + game_name + " room");

            string roomName = game_name + " " + _playerName +Random.Range(1000, 10000);
            RoomOptions roomOptions = new RoomOptions();
            string[] roomProperties = { "game_name", "host_team" };
            roomOptions.CustomRoomPropertiesForLobby = roomProperties;
            roomOptions.CustomRoomProperties = new Hashtable() { { "game_name", game_name }, { "host_team", _playerTeam } };
            roomOptions.MaxPlayers = 2;

            _specificGame = game_name;
            PhotonNetwork.CreateRoom(roomName, roomOptions, null);
        }

        public void CreateRandomRoom()
        {
            Debug.Log("creating random room");

            string game_name = MiniGames[Random.Range(0, MiniGames.Length)];
            string room_name = game_name + " " + _playerName + Random.Range(1000, 10000);
            RoomOptions roomOptions = new RoomOptions();
            string[] roomProperties = { "game_name", "host_team" };
            roomOptions.CustomRoomPropertiesForLobby = roomProperties;
            roomOptions.CustomRoomProperties = new Hashtable() { { "game_name", game_name }, { "host_team", _playerTeam } };
            roomOptions.MaxPlayers = 2;

            _specificGame = "";
            PhotonNetwork.CreateRoom(room_name, roomOptions, null);
        }

        public void BacktoSelection()
        {
            Debug.Log("back to selection");
            if (PhotonNetwork.InRoom)
            {
                PhotonNetwork.LeaveRoom();
                _specificGame = "";
            }

            SetActivePanel(SelectionPanel.name);
        }

        public void StartGame()
        {
            Debug.Log("Starting Game");

            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.CurrentRoom.IsVisible = false;

            string miniGame = PhotonNetwork.CurrentRoom.Name.Split(new char[] { ' ' })[0];
            Debug.Log(miniGame);
            PhotonNetwork.LoadLevel(miniGame);
        }

        #endregion
    }
}