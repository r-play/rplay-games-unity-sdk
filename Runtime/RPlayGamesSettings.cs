using System;
using UnityEngine;

namespace RPlay.Games
{
    [CreateAssetMenu(fileName = "RPlayGamesSettings", menuName = "RPlay/Games Settings")]
    public sealed class RPlayGamesSettings : ScriptableObject
    {
        [SerializeField]
        private string gameOid = string.Empty;

        [SerializeField]
        [Range(5, 120)]
        private int requestTimeoutSeconds = 30;

        [SerializeField]
        [Range(185, 300)]
        private int consumeTimeoutSeconds = 190;

        public string GameOid => gameOid.Trim();

        public int RequestTimeoutSeconds => requestTimeoutSeconds;

        public int ConsumeTimeoutSeconds => consumeTimeoutSeconds;

        internal string ApiOrigin
        {
            get
            {
                if (UseLocalDevelopmentEnvironment)
                {
                    return "http://localhost:2053";
                }

                return "https://api.rplay.live";
            }
        }

        internal string WebOrigin
        {
            get
            {
                if (UseLocalDevelopmentEnvironment)
                {
                    return "http://localhost:8080";
                }

                return "https://rplay.live";
            }
        }

        internal string StoryEngineWebOrigin => UseLocalDevelopmentEnvironment
            ? "http://localhost:8081"
            : "https://storyengine.live";

        internal string GetWebOrigin(string platformType)
        {
            return string.Equals(
                platformType,
                "storyengine",
                StringComparison.OrdinalIgnoreCase
            )
                ? StoryEngineWebOrigin
                : WebOrigin;
        }

        private static bool UseLocalDevelopmentEnvironment
        {
            get
            {
#if UNITY_EDITOR
                // 실험실 설정은 에셋이나 빌드에 포함하지 않고 현재 기기와 프로젝트에만 보관합니다.
                return UnityEditor.EditorPrefs.GetBool(
                    "RPlay.Games.UseLocalDevelopmentEnvironment:"
                        + Application.dataPath,
                    false
                );
#else
                return false;
#endif
            }
        }

        internal void Validate()
        {
            if (string.IsNullOrWhiteSpace(GameOid))
            {
                throw new InvalidOperationException(
                    "RPlayGamesSettings에 GameOid가 필요합니다. RPlay에서 게임을 만든 뒤 설정 에셋에 OID를 입력하세요."
                );
            }

            if (GameOid.Length != 24)
            {
                throw new InvalidOperationException(
                    "GameOid는 RPlay 게임 관리 화면에서 확인한 24자리 식별자여야 합니다."
                );
            }
            foreach (var character in GameOid)
            {
                if (!Uri.IsHexDigit(character))
                {
                    throw new InvalidOperationException(
                        "GameOid에는 16진수 문자만 사용할 수 있습니다."
                    );
                }
            }
        }
    }
}
