using System;
using RPlay.Games.Internal;
using UnityEngine;

namespace RPlay.Games
{
    [CreateAssetMenu(fileName = "RPlayGamesSettings", menuName = "RPlay/Games Settings")]
    public sealed class RPlayGamesSettings : ScriptableObject
    {
        [SerializeField]
        private string gameOid = string.Empty;

        // 키를 그대로 적어 두지 않고 가려서 보관합니다. 입력은 인스펙터에서 처리합니다.
        [SerializeField]
        [HideInInspector]
        private string sdkKeyData = string.Empty;

        [SerializeField]
        [Range(5, 120)]
        private int requestTimeoutSeconds = 30;

        [SerializeField]
        [Range(185, 300)]
        private int consumeTimeoutSeconds = 190;

        public string GameOid => gameOid.Trim();

        /// <summary>
        /// RPlay 스튜디오에서 발급한 SDK 키입니다.
        /// 데스크톱·모바일 빌드의 로그인에만 쓰이며 WebGL 빌드에서는 사용하지 않습니다.
        /// </summary>
        public string SdkKey => RPlaySdkKeyStore.Unmask(sdkKeyData);

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

#if UNITY_EDITOR
        /// <summary>
        /// 로컬 개발 환경 사용 여부가 저장되는 EditorPrefs 키입니다.
        ///
        /// 프로젝트 폴더를 옮기거나 이름을 바꿔도 설정이 유지되도록 경로 대신
        /// 프로젝트 GUID로 구분합니다.
        /// </summary>
        public static string LocalDevelopmentPrefKey =>
            "RPlay.Games.UseLocalDevelopmentEnvironment:"
            + UnityEditor.PlayerSettings.productGUID;

        /// <summary>
        /// 에디터에서 RPlay 로컬 서버에 연결할지 여부입니다. RPlay 서버를 직접 띄워
        /// 개발할 때만 사용합니다.
        ///
        /// 이 설정은 에셋이나 빌드에 포함되지 않고 현재 기기의 현재 프로젝트에만
        /// 보관되므로 저장소나 팀원에게 전달되지 않습니다.
        /// </summary>
        public static bool IsLocalDevelopmentEnvironment
        {
            get => UnityEditor.EditorPrefs.GetBool(LocalDevelopmentPrefKey, false);
            set => UnityEditor.EditorPrefs.SetBool(LocalDevelopmentPrefKey, value);
        }
#endif

        private static bool UseLocalDevelopmentEnvironment
        {
            get
            {
#if UNITY_EDITOR
                return IsLocalDevelopmentEnvironment;
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

#if !UNITY_WEBGL || UNITY_EDITOR
            // WebGL은 플랫폼이 토큰을 넣어 주므로 키가 필요 없습니다.
            if (string.IsNullOrWhiteSpace(SdkKey))
            {
                throw new InvalidOperationException(
                    "RPlayGamesSettings에 SDK 키가 필요합니다. RPlay 스튜디오의 게임 설정 화면에서 SDK 키를 복사해 입력하세요."
                );
            }
#endif
        }
    }
}
