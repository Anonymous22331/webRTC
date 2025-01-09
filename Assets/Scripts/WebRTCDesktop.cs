using System.Text;
using UnityEngine;
using UnityEngine.UI;

#if !UNITY_WEBGL
using Unity.WebRTC;
using WebSocketSharp;
#endif

public class WebRTCDesktop : MonoBehaviour
{
#if !UNITY_WEBGL
    [SerializeField] private RawImage ReceivedImage;
    private RTCPeerConnection _peerConnection;
    private RTCDataChannel _dataChannel;
    private WebSocket _webSocket;
    
    private static System.Threading.SynchronizationContext mainThreadContext;

    private void Awake()
    {
        mainThreadContext = System.Threading.SynchronizationContext.Current;
    }

    void Start()
    {
        //Подключение к серверу
        _webSocket = new WebSocket("ws://localhost:8080");
        
        _webSocket.OnMessage += (sender, e) =>
        {
            var requestArray = e.Data.Split("!");
            if (requestArray.Length < 2)
            {
                Debug.LogError("Invalid message format received: " + e.Data);
                return;
            }

            var requestType = requestArray[0];
            var requestData = requestArray[1];

            switch (requestType)
            {
                case "IMAGE":
                    Debug.Log("Received image data.");
                    var imageData = System.Convert.FromBase64String(requestData);

                    MessageToImage(imageData);
                    break;

                case "CANDIDATE":
                    var candidateInit = JsonUtility.FromJson<RTCIceCandidateInit>(requestData);
                    if (candidateInit != null && _peerConnection != null)
                    {
                        var candidate = new RTCIceCandidate(candidateInit);
                        _peerConnection.AddIceCandidate(candidate);
                    }
                    break;

                default:
                    Debug.Log("Unhandled WebSocket message: " + e.Data);
                    break;
            }
        };

        _webSocket.OnOpen += (sender, e) =>
        {
            Debug.Log("WebSocket connection opened.");
        };

        _webSocket.OnError += (sender, e) =>
        {
            Debug.LogError($"WebSocket error: {e.Message}");
        };

        _webSocket.OnClose += (sender, e) =>
        {
            Debug.Log("WebSocket connection closed.");
        };

        _webSocket.Connect();
        
        InitializePeerConnection();
    }

    private void InitializePeerConnection()
    {
        //Подключение WebRTC
        _peerConnection = new RTCPeerConnection();

        _peerConnection.OnIceCandidate = candidate =>
        {
            var candidateInit = new RTCIceCandidateInit
            {
                sdpMid = candidate.SdpMid,
                sdpMLineIndex = candidate.SdpMLineIndex ?? 0,
                candidate = candidate.Candidate
            };
            if (_webSocket.ReadyState == WebSocketState.Open)
            {
                _webSocket.Send("CANDIDATE!" + JsonUtility.ToJson(candidateInit));
                Debug.Log("Sent ICE candidate: " + JsonUtility.ToJson(candidateInit));
            }
            else
            {
                Debug.LogError("WebSocket is not open. Failed to send ICE candidate.");
            }
        };
        _dataChannel = _peerConnection.CreateDataChannel("dataChannel");
        _peerConnection.OnDataChannel = channel =>
        {
            
            _dataChannel = channel;
            _dataChannel.OnMessage = bytes =>
            {
                var message = System.Text.Encoding.UTF8.GetString(bytes);
                Debug.Log("Received via DataChannel: " + message);
            };
        };
    }

    private void MessageToImage(byte[] data)
    {
        mainThreadContext.Post(_ =>
        {
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(data);
            ReceivedImage.texture = texture;
            Debug.Log("Image successfully displayed.");
        }, null);
    }

    private void OnDestroy()
    {
            _webSocket.Close();
            _peerConnection.Close();
            _dataChannel.Close();
    }
    
#endif
}
