using RPlay.Games.Internal;
using UnityEditor;
using UnityEngine;

namespace RPlay.Games.Editor
{
    [CustomEditor(typeof(RPlayGamesSettings))]
    internal sealed class RPlayGamesSettingsEditor : UnityEditor.Editor
    {
        // 에셋에는 가려서 저장하므로 인스펙터에서 보여 줄 원본은 따로 들고 있습니다.
        private string sdkKeyInput;
        private bool showSdkKey;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("gameOid"));
            DrawSdkKeyField();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("requestTimeoutSeconds"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("consumeTimeoutSeconds"));
            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();
            var useLocalDevelopmentEnvironment = EditorPrefs.GetBool(
                "RPlay.Games.UseLocalDevelopmentEnvironment:"
                    + Application.dataPath,
                false
            );
            var settings = (RPlayGamesSettings)target;
            if (useLocalDevelopmentEnvironment)
            {
                EditorGUILayout.HelpBox(
                    "실험실 로컬 환경을 사용 중입니다.\n\nRPlay 웹: http://localhost:8080\nStoryEngine 웹: http://localhost:8081\nAPI: http://localhost:2053",
                    MessageType.Info
                );
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "API 호출은 운영 환경에 연결됩니다. 저장 데이터, 리더보드, 코인 및 크레딧 소비가 실제 계정에 반영됩니다.",
                    MessageType.Warning
                );
            }

            if (string.IsNullOrWhiteSpace(settings.GameOid))
            {
                EditorGUILayout.HelpBox(
                    "RPlay 게임 관리 화면에서 확인한 GameOid를 입력하세요.",
                    MessageType.Error
                );
            }

            DrawSdkKeyHelp(settings);
        }

        private void DrawSdkKeyField()
        {
            var sdkKeyData = serializedObject.FindProperty("sdkKeyData");
            if (sdkKeyInput == null)
            {
                sdkKeyInput = RPlaySdkKeyStore.Unmask(sdkKeyData.stringValue);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUI.BeginChangeCheck();
                var label = new GUIContent(
                    "Sdk Key",
                    "RPlay 스튜디오의 게임 설정 화면에서 발급받은 SDK 키입니다."
                );
                var nextValue = showSdkKey
                    ? EditorGUILayout.TextField(label, sdkKeyInput)
                    : EditorGUILayout.PasswordField(label, sdkKeyInput);
                if (EditorGUI.EndChangeCheck())
                {
                    sdkKeyInput = (nextValue ?? string.Empty).Trim();
                    sdkKeyData.stringValue = RPlaySdkKeyStore.Mask(sdkKeyInput);
                }

                showSdkKey = GUILayout.Toggle(
                    showSdkKey,
                    "표시",
                    EditorStyles.miniButton,
                    GUILayout.Width(40)
                );
            }
        }

        private void DrawSdkKeyHelp(RPlayGamesSettings settings)
        {
            if (string.IsNullOrWhiteSpace(settings.SdkKey))
            {
                EditorGUILayout.HelpBox(
                    "SDK 키가 없습니다. RPlay 스튜디오의 게임 설정 화면에서 SDK 키를 복사해 입력하세요.\nWebGL 빌드에는 필요 없지만, 데스크톱·모바일 빌드는 이 키가 없으면 로그인에 실패합니다.",
                    MessageType.Error
                );
                return;
            }

            if (!settings.SdkKey.StartsWith("rpsk_"))
            {
                EditorGUILayout.HelpBox(
                    "SDK 키 형식이 올바르지 않습니다. rpsk_ 로 시작하는 값을 그대로 붙여넣으세요.",
                    MessageType.Warning
                );
            }

            if (
                EditorUserBuildSettings.activeBuildTarget == BuildTarget.WebGL
            )
            {
                EditorGUILayout.HelpBox(
                    "WebGL 빌드에서는 SDK 키를 사용하지 않습니다. 플랫폼이 토큰을 직접 넣어 주기 때문입니다.\n다만 설정 에셋에 담긴 값은 빌드 결과물에 포함되므로, WebGL 전용 프로젝트라면 키를 비워 두는 편이 안전합니다.",
                    MessageType.Info
                );
            }

            EditorGUILayout.HelpBox(
                "SDK 키는 게임 빌드 안에 들어갑니다. 빌드를 분석하면 추출될 수 있으므로 이 키만으로 모든 부정 사용을 막을 수는 없습니다.\n저장소나 화면 공유로 키가 노출되지 않도록만 주의하세요.",
                MessageType.None
            );
        }
    }
}
