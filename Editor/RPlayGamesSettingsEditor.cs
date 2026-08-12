using UnityEditor;
using UnityEngine;

namespace RPlay.Games.Editor
{
    [CustomEditor(typeof(RPlayGamesSettings))]
    internal sealed class RPlayGamesSettingsEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("gameOid"));
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
        }
    }
}
