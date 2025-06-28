using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
#if TMP_PRESENT
using TMPro;
#endif

public class LobbyManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private Button joinRoomButton;
    [SerializeField] private GameObject statusText; // Reference to GameObject with Text or TextMeshProUGUI

    void Start()
    {
        if (joinRoomButton != null)
        {
            joinRoomButton.onClick.AddListener(JoinRoom);
        }
        UpdateStatus("Connecting to Photon...");
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        UpdateStatus("Connected to Photon. Click to join room.");
    }

    private void JoinRoom()
    {
        UpdateStatus("Joining room...");
        RoomOptions roomOptions = new RoomOptions { MaxPlayers = 4, IsVisible = true, IsOpen = true };
        PhotonNetwork.JoinOrCreateRoom("PixelDrawRoom", roomOptions, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        UpdateStatus($"Joined room: {PhotonNetwork.CurrentRoom.Name}");
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        UpdateStatus($"Failed to join room: {message}");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        UpdateStatus($"Disconnected: {cause}");
    }

    private void UpdateStatus(string message)
    {
        if (statusText != null)
        {
            Text text = statusText.GetComponent<Text>();
            #if TMP_PRESENT
            TextMeshProUGUI tmpText = statusText.GetComponent<TextMeshProUGUI>();
            if (tmpText != null)
            {
                tmpText.text = message;
            }
            else
            #endif
            if (text != null)
            {
                text.text = message;
            }
            else
            {
                Debug.LogWarning($"StatusText GameObject lacks Text or TextMeshProUGUI component: {statusText.name}");
            }
        }
        Debug.Log(message);
    }
}