using UnityEngine;
using UnityEngine.UI;

public class WebRTCImageManager : MonoBehaviour
{
    [SerializeField] private Button chooseImageButton;
    [SerializeField] private Button sendButton;
    [SerializeField] private RawImage previewImage;

    private byte[] imageBytes;

    
    //Работа с JS. Все подключение происходить в Index.html
#if UNITY_WEBGL
    [System.Runtime.InteropServices.DllImport("__Internal")]
    private static extern void sendImageToDesktop(string base64Image);
#endif
    
    private void Start()
    {
        chooseImageButton.onClick.AddListener(OpenFilePicker);
        sendButton.onClick.AddListener(() => SendImage());
        
        previewImage.gameObject.SetActive(false);
        sendButton.interactable = false;
    }

    public void OpenFilePicker()
    {
#if UNITY_WEBGL
        Application.ExternalEval("createFileInput();");
#else
        Debug.LogError("File picker is only supported in WebGL builds.");
#endif
    }

    public void PreviewImage(string base64Image)
    {
        imageBytes = System.Convert.FromBase64String(base64Image);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(imageBytes);
        previewImage.texture = texture;
        previewImage.gameObject.SetActive(true);
        sendButton.interactable = true;

        Debug.Log("Image preview loaded.");
    }

    public void SendImage()
    {
        if (imageBytes != null)
        {
#if UNITY_WEBGL
            string base64Image = System.Convert.ToBase64String(imageBytes);
            Application.ExternalCall("sendImageToDesktop", base64Image);
#else
            Debug.LogError("WebRTC dont support WebGL");
#endif
        }
        else
        {
            Debug.LogError("No image selected to send.");
        }
    }
    
}