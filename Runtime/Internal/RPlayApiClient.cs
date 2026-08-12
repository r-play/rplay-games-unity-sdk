using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine.Networking;

namespace RPlay.Games.Internal
{
    internal sealed class RPlayApiClient
    {
        private static readonly JsonSerializerSettings SerializerSettings =
            new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                DateParseHandling = DateParseHandling.DateTimeOffset,
            };

        private readonly RPlayGamesSettings settings;

        internal RPlayApiClient(RPlayGamesSettings settings)
        {
            this.settings = settings;
        }

        internal string GameToken { get; private set; }

        internal string ConnectAccessToken { get; private set; }

        internal string PlatformType { get; private set; }

        internal bool HasGameToken => !string.IsNullOrWhiteSpace(GameToken);

        internal bool HasConnectSession => !string.IsNullOrWhiteSpace(ConnectAccessToken);

        internal void SetSession(
            string gameToken,
            string connectAccessToken = null,
            string platformType = null
        )
        {
            GameToken = gameToken;
            ConnectAccessToken = connectAccessToken;
            PlatformType = string.IsNullOrWhiteSpace(platformType)
                ? ReadPlatformType(gameToken)
                : platformType;
        }

        internal void ClearSession()
        {
            GameToken = null;
            ConnectAccessToken = null;
            PlatformType = null;
        }

        private static string ReadPlatformType(string gameToken)
        {
            if (string.IsNullOrWhiteSpace(gameToken)) return null;

            var parts = gameToken.Split('.');
            if (parts.Length != 3) return null;

            try
            {
                var payload = parts[1].Replace('-', '+').Replace('_', '/');
                payload = payload.PadRight(
                    payload.Length + (4 - payload.Length % 4) % 4,
                    '='
                );
                var json = System.Text.Encoding.UTF8.GetString(
                    Convert.FromBase64String(payload)
                );
                var value = JObject.Parse(json).Value<string>("platformType");
                return value == "storyengine" || value == "rplay" ? value : null;
            }
            catch (Exception)
            {
                // 플랫폼 표시는 부가 정보이므로 토큰 검증은 실제 API 서버에 맡기고 초기화를 막지 않습니다.
                return null;
            }
        }

        internal Task<TResponse> SendGameApiAsync<TResponse>(
            string method,
            string path,
            object body,
            CancellationToken cancellationToken,
            bool useConnectAccessToken = false,
            int? timeoutSeconds = null
        )
            where TResponse : RPlayResponse
        {
            if (!HasGameToken)
            {
                throw new InvalidOperationException(
                    "RPlay 게임 토큰이 없습니다. 먼저 RPlayGames.LoginAsync를 호출하세요."
                );
            }

            return SendAsync<TResponse>(
                settings.ApiOrigin + "/game-api" + path,
                method,
                body,
                GameToken,
                useConnectAccessToken ? ConnectAccessToken : null,
                timeoutSeconds ?? settings.RequestTimeoutSeconds,
                cancellationToken
            );
        }

        internal Task<TResponse> SendConnectAuthAsync<TResponse>(
            string path,
            object body,
            CancellationToken cancellationToken,
            string connectAccessToken = null
        )
            where TResponse : RPlayResponse
        {
            return SendAsync<TResponse>(
                settings.ApiOrigin + "/game/connect-auth" + path,
                UnityWebRequest.kHttpVerbPOST,
                body,
                null,
                connectAccessToken,
                settings.RequestTimeoutSeconds,
                cancellationToken
            );
        }

        internal static string BuildQuery(params (string Key, object Value)[] values)
        {
            var parts = new List<string>();
            foreach (var pair in values)
            {
                if (pair.Value == null)
                {
                    continue;
                }

                var value = Convert.ToString(pair.Value, CultureInfo.InvariantCulture);
                parts.Add(
                    UnityWebRequest.EscapeURL(pair.Key)
                        + "="
                        + UnityWebRequest.EscapeURL(value)
                );
            }

            return string.Join("&", parts);
        }

        private static async Task<TResponse> SendAsync<TResponse>(
            string url,
            string method,
            object body,
            string gameToken,
            string connectAccessToken,
            int timeoutSeconds,
            CancellationToken cancellationToken
        )
            where TResponse : RPlayResponse
        {
            var headers = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(gameToken))
            {
                headers["X-Rplay-Game-Token"] = gameToken;
            }

            if (!string.IsNullOrWhiteSpace(connectAccessToken))
            {
                headers["Authorization"] = "Bearer " + connectAccessToken;
            }

            var jsonBody = body == null
                ? null
                : JsonConvert.SerializeObject(body, SerializerSettings);
            var httpResponse = await RPlayRuntimeHost.Instance.SendAsync(
                url,
                method,
                jsonBody,
                headers,
                timeoutSeconds,
                cancellationToken
            );

            if (string.IsNullOrWhiteSpace(httpResponse.Body))
            {
                throw new RPlayApiException(
                    "RPlay Games API가 빈 응답을 반환했습니다.",
                    "EMPTY_RESPONSE",
                    httpResponse.StatusCode
                );
            }

            TResponse response;
            try
            {
                response = JsonConvert.DeserializeObject<TResponse>(
                    httpResponse.Body,
                    SerializerSettings
                );
            }
            catch (JsonException exception)
            {
                throw new RPlayApiException(
                    "RPlay Games API가 올바르지 않은 JSON을 반환했습니다.",
                    "INVALID_RESPONSE",
                    httpResponse.StatusCode,
                    httpResponse.Body,
                    exception
                );
            }

            if (response == null)
            {
                throw new RPlayApiException(
                    "RPlay Games API 응답을 읽을 수 없습니다.",
                    "INVALID_RESPONSE",
                    httpResponse.StatusCode,
                    httpResponse.Body
                );
            }

            response.HttpStatusCode = httpResponse.StatusCode;
            response.RawJson = httpResponse.Body;
            return response;
        }
    }

    internal sealed class RPlayConnectTokenResponse : RPlayResponse
    {
        [JsonProperty("gameToken")]
        public string GameToken { get; set; }

        [JsonProperty("connectAccessToken")]
        public string ConnectAccessToken { get; set; }

        [JsonProperty("refreshToken")]
        public string RefreshToken { get; set; }

        [JsonProperty("expiresIn")]
        public int ExpiresIn { get; set; }

        [JsonProperty("platformType")]
        public string PlatformType { get; set; }
    }

    internal sealed class RPlayJsonResponse : RPlayResponse
    {
        [JsonExtensionData]
        public IDictionary<string, JToken> AdditionalData { get; set; }
    }
}
