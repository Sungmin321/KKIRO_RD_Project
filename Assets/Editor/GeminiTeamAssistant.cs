using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using System.Collections;
using System.IO;
using System.Text;

public class GeminiTeamAssistant : EditorWindow
{
    private string apiKey = "";
    private string promptText = "운빨존많겜 스타일 2D 몬스터 이동 경로(Waypoint) 스크립트를 작성해줘.";
    private string outputResult = "";
    private Vector2 scrollPos;
    private bool isLoading = false;
    private static readonly string ConfigPath = "GeminiConfig.json";

    [System.Serializable]
    private class ConfigData { public string savedApiKey; }

    [MenuItem("Tools/Gemini Team Assistant")]
    public static void OpenWindow()
    {
        GetWindow<GeminiTeamAssistant>("Gemini Assistant");
    }

    private void OnEnable()
    {
        // 로컬에 저장된 키 로드 (.gitignore 처리됨)
        if (File.Exists(ConfigPath))
        {
            string json = File.ReadAllText(ConfigPath);
            ConfigData config = JsonUtility.FromJson<ConfigData>(json);
            if (config != null) apiKey = config.savedApiKey;
        }
    }

    private void OnGUI()
    {
        GUILayout.Label("Gemini Unity Developer (Team Sync)", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUI.BeginChangeCheck();
        apiKey = EditorGUILayout.PasswordField("Gemini API Key", apiKey);
        if (EditorGUI.EndChangeCheck())
        {
            // API 키가 수정되면 로컬에 자동 저장
            ConfigData config = new ConfigData { savedApiKey = apiKey };
            File.WriteAllText(ConfigPath, JsonUtility.ToJson(config));
        }

        EditorGUILayout.HelpBox("입력한 API 키는 로컬 파일(GeminiConfig.json)에만 저장되며 Git에 공유되지 않습니다.", MessageType.Info);
        EditorGUILayout.Space();

        GUILayout.Label("요청할 작업 (프롬프트):");
        promptText = EditorGUILayout.TextArea(promptText, GUILayout.Height(60));

        EditorGUILayout.Space();
        if (GUILayout.Button(isLoading ? "생성 진행 중..." : "Gemini에게 코드 요청", GUILayout.Height(30)) && !isLoading)
        {
            if (string.IsNullOrEmpty(apiKey.Trim()))
            {
                EditorUtility.DisplayDialog("경고", "Google AI Studio에서 발급받은 API 키를 입력해주세요.", "확인");
                return;
            }
            EditorApplication.update += RunCoroutine;
        }

        EditorGUILayout.Space();
        GUILayout.Label("생성된 코드 결과:");
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(280));
        EditorGUILayout.TextArea(outputResult, GUILayout.ExpandHeight(true));
        EditorGUILayout.EndScrollView();

        if (GUILayout.Button("클립보드로 복사") && !string.IsNullOrEmpty(outputResult))
        {
            GUIUtility.systemCopyBuffer = outputResult;
            EditorUtility.DisplayDialog("완료", "코드가 클립보드에 복사되었습니다.", "확인");
        }
    }

    private IEnumerator currentCoroutine;
    private void RunCoroutine()
    {
        if (currentCoroutine == null)
            currentCoroutine = SendGeminiRequest(apiKey, promptText);

        if (!currentCoroutine.MoveNext())
        {
            EditorApplication.update -= RunCoroutine;
            currentCoroutine = null;
        }
    }

    private IEnumerator SendGeminiRequest(string key, string prompt)
    {
        isLoading = true;
        outputResult = "Gemini 응답 대기 중...";
        Repaint();

        string endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={key}";
        string escapedPrompt = prompt.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "");
        string jsonPayload = "{\"contents\":[{\"parts\":[{\"text\":\"" + escapedPrompt + "\"}]}]}";

        using (UnityWebRequest req = new UnityWebRequest(endpoint, "POST"))
        {
            byte[] body = Encoding.UTF8.GetBytes(jsonPayload);
            req.uploadHandler = new UploadHandlerRaw(body);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");

            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                outputResult = ParseResponse(req.downloadHandler.text);
            }
            else
            {
                outputResult = $"[오류]: {req.error}\n{req.downloadHandler.text}";
            }
        }

        isLoading = false;
        Repaint();
    }

    private string ParseResponse(string json)
    {
        try
        {
            int textKey = json.IndexOf("\"text\": \"");
            if (textKey == -1) return json;
            textKey += 9;
            int endKey = json.IndexOf("\"", textKey);
            string raw = json.Substring(textKey, endKey - textKey);
            return raw.Replace("\\n", "\n").Replace("\\\"", "\"").Replace("\\\\", "\\");
        }
        catch { return json; }
    }
}