using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    void Start()
    {
        // Connect to Photon Cloud
        PhotonNetwork.ConnectUsingSettings();
        Debug.Log("Connecting to Photon Cloud...");
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Photon Master Server");
        // Join or create a room
        RoomOptions roomOptions = new RoomOptions { MaxPlayers = 4, IsVisible = true, IsOpen = true };
        PhotonNetwork.JoinOrCreateRoom("PixelDrawRoom", roomOptions, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"Joined room: {PhotonNetwork.CurrentRoom.Name}");
        // Load the game scene (ensure it's added in Build Settings)
        PhotonNetwork.LoadLevel("GameScene");
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"Failed to join room: {message} (Code: {returnCode})");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning($"Disconnected from Photon: {cause}");
    }
}