using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

public class GeminiTeamAssistant : EditorWindow
{
    private string apiKey = "";
    private string promptText = "Unity C#으로 운빨존많겜 몬스터 이동 경로(Waypoint) 스크립트를 작성해줘.";
    private string outputResult = "";
    private Vector2 scrollPos;
    private bool isLoading = false;
    private static readonly string ConfigPath = "GeminiConfig.json";

    // 기본 모델 목록
    private List<string> modelList = new List<string>
    {
        "gemini-1.5-flash",
        "gemini-1.5-pro",
        "gemini-pro"
    };
    private int selectedModelIndex = 0;

    private static readonly HttpClient client = new HttpClient();

    [System.Serializable]
    private class ConfigData { public string savedApiKey; }

    [MenuItem("Tools/Gemini Team Assistant")]
    public static void OpenWindow()
    {
        GetWindow<GeminiTeamAssistant>("Gemini Assistant");
    }

    private void OnEnable()
    {
        if (File.Exists(ConfigPath))
        {
            try
            {
                string json = File.ReadAllText(ConfigPath);
                ConfigData config = JsonUtility.FromJson<ConfigData>(json);
                if (config != null) apiKey = config.savedApiKey;
            }
            catch { }
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
            ConfigData config = new ConfigData { savedApiKey = apiKey.Trim() };
            File.WriteAllText(ConfigPath, JsonUtility.ToJson(config));
        }

        EditorGUILayout.BeginHorizontal();
        selectedModelIndex = EditorGUILayout.Popup("사용 모델", selectedModelIndex, modelList.ToArray());
        if (GUILayout.Button("사용 가능한 모델 새로고침", GUILayout.Width(160)))
        {
            FetchAvailableModels();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.HelpBox("Google AI Studio(aistudio.google.com)에서 발급받은 API 키를 입력하세요.", MessageType.Info);
        EditorGUILayout.Space();

        GUILayout.Label("요청할 작업 (프롬프트):");
        promptText = EditorGUILayout.TextArea(promptText, GUILayout.Height(60));

        EditorGUILayout.Space();
        if (GUILayout.Button(isLoading ? "요청 처리 중..." : "Gemini에게 코드 요청", GUILayout.Height(30)) && !isLoading)
        {
            if (string.IsNullOrEmpty(apiKey.Trim()))
            {
                EditorUtility.DisplayDialog("경고", "API Key를 먼저 입력해주세요.", "확인");
                return;
            }
            RequestGeminiAsync();
        }

        EditorGUILayout.Space();
        GUILayout.Label("생성된 코드 결과 / 응답 로그:");
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(280));
        EditorGUILayout.TextArea(outputResult, GUILayout.ExpandHeight(true));
        EditorGUILayout.EndScrollView();

        if (GUILayout.Button("클립보드로 복사") && !string.IsNullOrEmpty(outputResult))
        {
            GUIUtility.systemCopyBuffer = outputResult;
            EditorUtility.DisplayDialog("완료", "결과가 클립보드에 복사되었습니다.", "확인");
        }
    }

    // 본인 API 키로 사용 가능한 모델 목록 자동 조회
    private async void FetchAvailableModels()
    {
        if (string.IsNullOrEmpty(apiKey.Trim()))
        {
            EditorUtility.DisplayDialog("경고", "API Key를 먼저 입력해주세요.", "확인");
            return;
        }

        isLoading = true;
        outputResult = "사용 가능한 Gemini 모델 목록 조회 중...";
        Repaint();

        try
        {
            string url = "https://generativelanguage.googleapis.com/v1beta/models";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("x-goog-api-key", apiKey.Trim());

            var response = await client.SendAsync(request);
            string body = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                List<string> foundModels = new List<string>();
                string[] lines = body.Split('\n');
                foreach (var line in lines)
                {
                    if (line.Contains("\"name\": \"models/"))
                    {
                        int start = line.IndexOf("models/") + 7;
                        int end = line.IndexOf("\"", start);
                        if (start != -1 && end != -1)
                        {
                            string modelName = line.Substring(start, end - start);
                            if (modelName.Contains("gemini") && !foundModels.Contains(modelName))
                            {
                                foundModels.Add(modelName);
                            }
                        }
                    }
                }

                if (foundModels.Count > 0)
                {
                    modelList = foundModels;
                    selectedModelIndex = 0;
                    outputResult = $"[조회 성공] 사용 가능한 {foundModels.Count}개의 모델을 불러왔습니다:\n" + string.Join("\n", foundModels);
                }
                else
                {
                    outputResult = $"[조회 완료] 응답 본문:\n{body}";
                }
            }
            else
            {
                outputResult = $"[모델 목록 조회 실패]: {response.StatusCode}\n{body}";
            }
        }
        catch (Exception ex)
        {
            outputResult = $"[목록 조회 예외]: {ex.Message}";
        }
        finally
        {
            isLoading = false;
            Repaint();
        }
    }

    private async void RequestGeminiAsync()
    {
        isLoading = true;
        outputResult = "Google Gemini 서버와 통신 중...";
        Repaint();

        try
        {
            string currentModel = modelList[selectedModelIndex];

            // v1beta 및 v1 공용 표준 엔드포인트 + x-goog-api-key 헤더 인증 방식
            string url = $"https://generativelanguage.googleapis.com/v1beta/models/{currentModel}:generateContent";

            string escapedPrompt = EscapeJsonString(promptText);
            string jsonBody = "{\"contents\":[{\"parts\":[{\"text\":\"" + escapedPrompt + "\"}]}]}";

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("x-goog-api-key", apiKey.Trim());
            request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.SendAsync(request);
            string responseBody = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                outputResult = ParseResponse(responseBody);
            }
            else
            {
                outputResult = $"[서버 응답 오류]\n상태 코드: {response.StatusCode} ({(int)response.StatusCode})\n\n[상세 내용]:\n{responseBody}";
            }
        }
        catch (Exception ex)
        {
            outputResult = $"[클라이언트 통신 예외]: {ex.Message}\n{ex.StackTrace}";
        }
        finally
        {
            isLoading = false;
            Repaint();
        }
    }

    private string EscapeJsonString(string text)
    {
        if (string.IsNullOrEmpty(text)) return "";
        return text.Replace("\\", "\\\\")
                   .Replace("\"", "\\\"")
                   .Replace("\n", "\\n")
                   .Replace("\r", "\\r")
                   .Replace("\t", "\\t");
    }

    private string ParseResponse(string json)
    {
        try
        {
            int textKey = json.IndexOf("\"text\": \"");
            if (textKey == -1) return json;

            textKey += 9;
            int endKey = -1;

            for (int i = textKey; i < json.Length; i++)
            {
                if (json[i] == '"' && json[i - 1] != '\\')
                {
                    endKey = i;
                    break;
                }
            }

            if (endKey == -1) return json;

            string raw = json.Substring(textKey, endKey - textKey);
            return raw.Replace("\\n", "\n")
                      .Replace("\\\"", "\"")
                      .Replace("\\\\", "\\")
                      .Replace("\\r", "");
        }
        catch (Exception ex)
        {
            return $"파싱 오류: {ex.Message}\n\n원본:\n{json}";
        }
    }
}