using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AppendInputToText : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TextMeshProUGUI outputText;
    [SerializeField] private Button addButton;
    [SerializeField] private ScrollRect scrollRect; // optional (for auto-scroll)

    [Header("Behavior")]
    [SerializeField] private bool clearAfterAdd = true;
    [Tooltip("0 = unlimited")]
    [SerializeField] private int maxLines = 0;

    private void Reset()
    {
        inputField = GetComponentInChildren<TMP_InputField>();
        outputText = GetComponentInChildren<TextMeshProUGUI>();
        addButton = GetComponentInChildren<Button>();
        scrollRect = GetComponentInChildren<ScrollRect>();
    }

    private void Awake()
    {
        if (addButton != null) addButton.onClick.AddListener(AppendFromInput);
        if (inputField != null) inputField.onSubmit.AddListener(OnSubmit);
    }

    private void OnDestroy()
    {
        if (addButton != null) addButton.onClick.RemoveListener(AppendFromInput);
        if (inputField != null) inputField.onSubmit.RemoveListener(OnSubmit);
    }

    private void OnSubmit(string _)
    {
        // Fired by keyboard "Done/Enter" on mobile when LineType = Single Line
        AppendFromInput();
    }

    public void AppendFromInput()
    {
        if (!inputField || !outputText) return;

        var text = inputField.text;
        if (string.IsNullOrWhiteSpace(text)) return;

        string line = text.Trim();

        if (!string.IsNullOrEmpty(outputText.text))
            outputText.text += "\n";
        outputText.text += line;

        if (maxLines > 0)
        {
            var lines = outputText.text.Split('\n');
            if (lines.Length > maxLines)
            {
                int start = Mathf.Max(0, lines.Length - maxLines);
                outputText.text = string.Join("\n", lines, start, lines.Length - start);
            }
        }

        if (clearAfterAdd)
        {
            inputField.text = string.Empty;
            inputField.ActivateInputField();
            inputField.Select();
        }

        if (scrollRect != null)
        {
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f; // scroll to bottom
        }
    }
}
