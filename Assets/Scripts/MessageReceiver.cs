using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FlutterUnityIntegration;

[DisallowMultipleComponent]
public class MessageBridge : MonoBehaviour
{
    [Header("UI (TextMeshPro)")]
    public TMP_InputField input;          // assign your TMP Input Field
    public Button sendBtn;                // assign your Button
    public TextMeshProUGUI output;        // assign your TMP Text used as the log/display

    [Header("Behavior")]
    public bool clearInputAfterSend = true;
    public bool alsoSendToFlutter = true; // toggle if you want local-only appending

    void Awake()
    {
        if (sendBtn != null) sendBtn.onClick.AddListener(OnSendClicked);
    }

    void OnDestroy()
    {
        if (sendBtn != null) sendBtn.onClick.RemoveListener(OnSendClicked);
    }

    // Button click: append to output (new line) and optionally send to Flutter
    void OnSendClicked()
    {
        var text = input != null ? input.text : "";

        // append to output (each entry on its own line)
        if (output != null && !string.IsNullOrEmpty(text))
        {
            if (!string.IsNullOrEmpty(output.text)) output.text += "\n";
            output.text += text;
        }

        if (alsoSendToFlutter) SendToFlutter(text);

        if (clearInputAfterSend && input != null) input.text = "";

        // keep focus in the field (nice for rapid testing)
        if (input != null)
        {
            input.Select();
            input.ActivateInputField();
        }
    }

    // Unity -> Flutter
    public void SendToFlutter(string value)
    {
        var mgr = UnityMessageManager.Instance;
        if (mgr != null)
        {
            var payload = $"{{\"event\":\"submitted\",\"value\":\"{Escape(value)}\"}}";
            mgr.SendMessageToFlutter(payload);
        }
        else
        {
            Debug.LogWarning("UnityMessageManager not found in the scene. Add the Flutter prefab.");
        }
    }

    // Flutter -> Unity
    // Ensure the GameObject holding this script is named "MessageReceiver"
    // Flutter should call: gameObject: "MessageReceiver", method: "HandleCommand", payload: JSON string
    public void HandleCommand(string json)
    {
        try
        {
            var cmd = JsonUtility.FromJson<Cmd>(json);
            if (cmd == null) return;

            switch (cmd.cmd)
            {
                case "setText":     // sets the input field text
                    if (input != null) input.text = cmd.value ?? "";
                    break;

                case "appendOutput": // appends a line to the output text
                    AppendOutput(cmd.value);
                    break;

                case "clearOutput": // clears the output text
                    if (output != null) output.text = "";
                    break;

                case "submit":      // sets text (optional) and simulates button click
                    if (input != null && !string.IsNullOrEmpty(cmd.value)) input.text = cmd.value;
                    OnSendClicked();
                    break;

                default:
                    Debug.Log($"[MessageBridge] Unknown cmd: {cmd.cmd}");
                    break;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"HandleCommand parse error: {e.Message}\nPayload: {json}");
        }
    }

    void AppendOutput(string value)
    {
        if (output != null && !string.IsNullOrEmpty(value))
        {
            if (!string.IsNullOrEmpty(output.text)) output.text += "\n";
            output.text += value;
        }
    }

    [System.Serializable]
    class Cmd { public string cmd; public string value; }

    string Escape(string s) => s?.Replace("\\", "\\\\").Replace("\"", "\\\"") ?? "";
}
