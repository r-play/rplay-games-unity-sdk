using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using RPlay.Games.Internal;
using UnityEngine.Networking;

namespace RPlay.Games
{
    public static class RPlayGames
    {
        private static SemaphoreSlim DataMutationLock = new SemaphoreSlim(1, 1);
        private static SemaphoreSlim ConsumeLock = new SemaphoreSlim(1, 1);
        private static SemaphoreSlim SessionRefreshLock = new SemaphoreSlim(1, 1);

        private static RPlayGamesSettings settings;
        private static RPlayApiClient apiClient;
        private static RPlayConnectAuthenticator connectAuthenticator;
        private static RPlayConnectSession connectSession;

        public static bool IsInitialized => apiClient != null;

        public static bool IsAuthenticated => apiClient?.HasGameToken == true;

        public static RPlayPlatform? ConnectedPlatform => apiClient?.PlatformType == null
            ? (RPlayPlatform?)null
            : apiClient.PlatformType == "storyengine"
                ? RPlayPlatform.StoryEngine
                : RPlayPlatform.RPlay;

        public static RPlayGamesSettings Settings => settings;

        [UnityEngine.RuntimeInitializeOnLoadMethod(
            UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration
        )]
        private static void ResetRuntimeState()
        {
            // 도메인 재로드를 끈 Editor에서도 이전 플레이 세션 토큰을 남기지 않는다.
            apiClient?.ClearSession();
            settings = null;
            apiClient = null;
            connectAuthenticator = null;
            connectSession = null;
            DataMutationLock = new SemaphoreSlim(1, 1);
            ConsumeLock = new SemaphoreSlim(1, 1);
            SessionRefreshLock = new SemaphoreSlim(1, 1);
        }

        public static async Task InitializeAsync(
            RPlayGamesSettings gameSettings,
            CancellationToken cancellationToken = default
        )
        {
            if (gameSettings == null)
            {
                throw new ArgumentNullException(nameof(gameSettings));
            }

            gameSettings.Validate();
            if (apiClient != null)
            {
                if (ReferenceEquals(settings, gameSettings))
                {
                    return;
                }

                throw new InvalidOperationException(
                    "RPlayGames는 이미 다른 설정으로 초기화되었습니다. 앱 실행 중에는 설정을 변경할 수 없습니다."
                );
            }

            settings = gameSettings;
            apiClient = new RPlayApiClient(settings);
            connectAuthenticator = new RPlayConnectAuthenticator(settings, apiClient);
            _ = RPlayRuntimeHost.Instance;

            if (RPlayWebGlBridge.IsInjectedSdkRuntime)
            {
                var token = await RPlayRuntimeHost.Instance.WaitForInjectedWebGlTokenAsync(
                    settings.RequestTimeoutSeconds,
                    cancellationToken
                );
                apiClient.SetSession(token);
            }
        }

        public static async Task LoginAsync(CancellationToken cancellationToken = default)
        {
            EnsureInitialized();

            if (RPlayWebGlBridge.IsInjectedSdkRuntime)
            {
                if (!apiClient.HasGameToken)
                {
                    var token = await RPlayRuntimeHost.Instance.WaitForInjectedWebGlTokenAsync(
                        settings.RequestTimeoutSeconds,
                        cancellationToken
                    );
                    apiClient.SetSession(token);
                }

                return;
            }

            connectSession = await connectAuthenticator.LoginAsync(cancellationToken);
            apiClient.SetSession(
                connectSession.GameToken,
                connectSession.ConnectAccessToken,
                connectSession.PlatformType
            );
        }

        public static async Task LogoutAsync(CancellationToken cancellationToken = default)
        {
            EnsureInitialized();

            if (RPlayWebGlBridge.IsInjectedSdkRuntime)
            {
                apiClient.ClearSession();
                return;
            }

            var connectAccessToken = connectSession?.ConnectAccessToken;
            var refreshToken = connectSession?.RefreshToken;
            try
            {
                if (
                    !string.IsNullOrWhiteSpace(connectAccessToken)
                    || !string.IsNullOrWhiteSpace(refreshToken)
                )
                {
                    await connectAuthenticator.LogoutAsync(
                        connectAccessToken,
                        refreshToken,
                        cancellationToken
                    );
                }
            }
            finally
            {
                connectSession = null;
                apiClient.ClearSession();
            }
        }

        public static Task<RPlayResponse> VerifyUserAsync(
            CancellationToken cancellationToken = default
        )
        {
            EnsureReady();
            return apiClient.SendGameApiAsync<RPlayResponse>(
                UnityWebRequest.kHttpVerbPOST,
                "/verify-user",
                null,
                cancellationToken
            );
        }

        public static Task<RPlayUserInfo> GetUserInfoAsync(
            CancellationToken cancellationToken = default
        )
        {
            EnsureReady();
            return apiClient.SendGameApiAsync<RPlayUserInfo>(
                UnityWebRequest.kHttpVerbPOST,
                "/user-info",
                null,
                cancellationToken
            );
        }

        public static async Task<RPlayResponse> RequestChargeAsync(
            CancellationToken cancellationToken = default
        )
        {
            EnsureReady();
            var response = await apiClient.SendGameApiAsync<RPlayResponse>(
                UnityWebRequest.kHttpVerbPOST,
                "/request-charge",
                null,
                cancellationToken
            );

            if (
                response.Success
                && !RPlayWebGlBridge.IsInjectedSdkRuntime
            )
            {
                UnityEngine.Application.OpenURL(
                    settings.GetWebOrigin(connectSession.PlatformType)
                        + "/game/"
                        + settings.GameOid
                        + "/charge"
                );
            }

            return response;
        }

        public static async Task<RPlayConsumeResult> ConsumeAsync(
            double amount,
            string itemName,
            RPlayConsumeOptions options = null,
            CancellationToken cancellationToken = default
        )
        {
            EnsureReady();
            ValidateFinitePositive(amount, nameof(amount));
            if (string.IsNullOrWhiteSpace(itemName))
            {
                throw new ArgumentException("아이템 이름이 필요합니다.", nameof(itemName));
            }
            itemName = itemName.Trim();
            if (itemName.Length > 200)
            {
                throw new ArgumentException(
                    "아이템 이름은 200자 이하여야 합니다.",
                    nameof(itemName)
                );
            }

            options = options ?? new RPlayConsumeOptions();
            if (
                options.ItemDescription != null
                && options.ItemDescription.Length > 1000
            )
            {
                throw new ArgumentException(
                    "아이템 설명은 1,000자 이하여야 합니다.",
                    nameof(options)
                );
            }
            if (options.Metadata != null)
            {
                var metadataToken = JToken.FromObject(options.Metadata);
                if (
                    metadataToken.Type != JTokenType.Object
                    || metadataToken.ToString(Newtonsoft.Json.Formatting.None).Length > 10000
                )
                {
                    throw new ArgumentException(
                        "메타데이터는 10,000자 이하의 JSON 객체여야 합니다.",
                        nameof(options)
                    );
                }
            }
            await ConsumeLock.WaitAsync(cancellationToken);
            try
            {
                var requestId = "unity_" + Guid.NewGuid().ToString("N");
                var requestBody = new
                {
                    amount,
                    itemName,
                    itemDescription = options.ItemDescription,
                    metadata = options.Metadata,
                    requestId,
                    skipConfirmPopup = options.SkipConfirmation,
                };

                if (RPlayWebGlBridge.IsInjectedSdkRuntime)
                {
                    return await apiClient.SendGameApiAsync<RPlayConsumeResult>(
                        UnityWebRequest.kHttpVerbPOST,
                        "/consume",
                        requestBody,
                        cancellationToken,
                        false,
                        settings.ConsumeTimeoutSeconds
                    );
                }

                await EnsureConnectSessionAsync(cancellationToken);
                using (var consumeCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
                {
                    var consumeTask = apiClient.SendGameApiAsync<RPlayConsumeResult>(
                        UnityWebRequest.kHttpVerbPOST,
                        "/consume",
                        requestBody,
                        consumeCancellation.Token,
                        false,
                        settings.ConsumeTimeoutSeconds
                    );

                    try
                    {
                        bool confirmed;
                        if (options.SkipConfirmation)
                        {
                            confirmed = true;
                        }
                        else
                        {
                            var confirmationTask = RPlayConfirmationOverlay.ShowAsync(
                                new RPlayConsumePrompt
                                {
                                    Amount = amount,
                                    ItemName = itemName,
                                    ItemDescription = options.ItemDescription,
                                    IsStoryEngine =
                                        connectSession.PlatformType == "storyengine",
                                },
                                consumeCancellation.Token
                            );
                            var firstCompletedTask = await Task.WhenAny(
                                consumeTask,
                                confirmationTask
                            );
                            if (firstCompletedTask == consumeTask)
                            {
                                var earlyResult = await consumeTask;
                                consumeCancellation.Cancel();
                                try
                                {
                                    await confirmationTask;
                                }
                                catch (OperationCanceledException)
                                {
                                }
                                return earlyResult;
                            }
                            confirmed = await confirmationTask;
                        }

                        var confirmationResult = await apiClient.SendGameApiAsync<RPlayConsumeResult>(
                            UnityWebRequest.kHttpVerbPOST,
                            "/confirm-consume",
                            new
                            {
                                requestId,
                                confirmed,
                                amount,
                                itemName,
                                itemDescription = options.ItemDescription,
                                metadata = options.Metadata,
                            },
                            cancellationToken,
                            true
                        );

                        // 확인 요청 자체가 거부되면 긴 폴링을 끝까지 기다리지 않는다.
                        if (
                            !confirmationResult.Success
                            && string.IsNullOrWhiteSpace(confirmationResult.Status)
                        )
                        {
                            consumeCancellation.Cancel();
                            try
                            {
                                await consumeTask;
                            }
                            catch
                            {
                            }
                            return confirmationResult;
                        }

                        return await consumeTask;
                    }
                    catch
                    {
                        consumeCancellation.Cancel();
                        try
                        {
                            await consumeTask;
                        }
                        catch
                        {
                        }

                        throw;
                    }
                }
            }
            finally
            {
                ConsumeLock.Release();
            }
        }

        public static Task<RPlayGameData> LoadDataAsync(
            CancellationToken cancellationToken = default
        )
        {
            EnsureReady();
            return apiClient.SendGameApiAsync<RPlayGameData>(
                UnityWebRequest.kHttpVerbPOST,
                "/load-data",
                null,
                cancellationToken
            );
        }

        public static Task<RPlayGameData<T>> LoadDataAsync<T>(
            CancellationToken cancellationToken = default
        )
        {
            EnsureReady();
            return apiClient.SendGameApiAsync<RPlayGameData<T>>(
                UnityWebRequest.kHttpVerbPOST,
                "/load-data",
                null,
                cancellationToken
            );
        }

        public static async Task<RPlayDataWriteResult> SetDataAsync(
            string key,
            object value,
            CancellationToken cancellationToken = default
        )
        {
            EnsureReady();
            ValidateDataKey(key);
            return await ExecuteDataMutationAsync(
                UnityWebRequest.kHttpVerbPOST,
                "/data",
                new { key, value },
                cancellationToken
            );
        }

        public static async Task<RPlayDataWriteResult> SetDataAsync(
            object data,
            CancellationToken cancellationToken = default
        )
        {
            EnsureReady();
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            var token = JToken.FromObject(data);
            if (token.Type != JTokenType.Object)
            {
                throw new ArgumentException("벌크 저장 데이터는 JSON 객체여야 합니다.", nameof(data));
            }
            if (token["_id"] != null)
            {
                throw new ArgumentException(
                    "_id는 수정할 수 없는 서버 보호 키입니다.",
                    nameof(data)
                );
            }

            return await ExecuteDataMutationAsync(
                UnityWebRequest.kHttpVerbPOST,
                "/data",
                new { data },
                cancellationToken
            );
        }

        public static async Task<RPlayDataWriteResult> DeleteDataAsync(
            string key,
            CancellationToken cancellationToken = default
        )
        {
            EnsureReady();
            ValidateDataKey(key);
            return await ExecuteDataMutationAsync(
                UnityWebRequest.kHttpVerbDELETE,
                "/data/" + Uri.EscapeDataString(key),
                null,
                cancellationToken
            );
        }

        public static async Task<RPlayResponse> DeleteAllDataAsync(
            CancellationToken cancellationToken = default
        )
        {
            EnsureReady();
            await DataMutationLock.WaitAsync(cancellationToken);
            try
            {
                return await apiClient.SendGameApiAsync<RPlayResponse>(
                    UnityWebRequest.kHttpVerbDELETE,
                    "/data",
                    null,
                    cancellationToken
                );
            }
            finally
            {
                DataMutationLock.Release();
            }
        }

        public static Task<RPlayLeaderboardUpdateResult> SetScoreAsync(
            double score,
            CancellationToken cancellationToken = default
        )
        {
            return UpdateScoreAsync("set", score, cancellationToken);
        }

        public static Task<RPlayLeaderboardUpdateResult> IncrementScoreAsync(
            double amount,
            CancellationToken cancellationToken = default
        )
        {
            return UpdateScoreAsync("increment", amount, cancellationToken);
        }

        public static Task<RPlayLeaderboardMeResult> GetMyRankAsync(
            CancellationToken cancellationToken = default
        )
        {
            EnsureReady();
            return apiClient.SendGameApiAsync<RPlayLeaderboardMeResult>(
                UnityWebRequest.kHttpVerbGET,
                "/leaderboard/me",
                null,
                cancellationToken
            );
        }

        public static Task<RPlayLeaderboardPage> GetTopRanksAsync(
            int limit = 50,
            int offset = 0,
            CancellationToken cancellationToken = default
        )
        {
            EnsureReady();
            if (limit < 1 || limit > 50)
            {
                throw new ArgumentOutOfRangeException(nameof(limit), "limit은 1 이상 50 이하여야 합니다.");
            }

            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offset), "offset은 0 이상이어야 합니다.");
            }

            var query = RPlayApiClient.BuildQuery(("limit", limit), ("offset", offset));
            return apiClient.SendGameApiAsync<RPlayLeaderboardPage>(
                UnityWebRequest.kHttpVerbGET,
                "/leaderboard/top?" + query,
                null,
                cancellationToken
            );
        }

        public static Task<RPlayLeaderboardAroundResult> GetRanksAroundMeAsync(
            int range = 5,
            CancellationToken cancellationToken = default
        )
        {
            EnsureReady();
            if (range < 0 || range > 50)
            {
                throw new ArgumentOutOfRangeException(nameof(range), "range는 0 이상 50 이하여야 합니다.");
            }

            var query = RPlayApiClient.BuildQuery(("range", range));
            return apiClient.SendGameApiAsync<RPlayLeaderboardAroundResult>(
                UnityWebRequest.kHttpVerbGET,
                "/leaderboard/around?" + query,
                null,
                cancellationToken
            );
        }

        private static async Task<RPlayDataWriteResult> ExecuteDataMutationAsync(
            string method,
            string path,
            object body,
            CancellationToken cancellationToken
        )
        {
            await DataMutationLock.WaitAsync(cancellationToken);
            try
            {
                return await apiClient.SendGameApiAsync<RPlayDataWriteResult>(
                    method,
                    path,
                    body,
                    cancellationToken
                );
            }
            finally
            {
                DataMutationLock.Release();
            }
        }

        private static Task<RPlayLeaderboardUpdateResult> UpdateScoreAsync(
            string operation,
            double value,
            CancellationToken cancellationToken
        )
        {
            EnsureReady();
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException(nameof(value), "점수는 유한한 숫자여야 합니다.");
            }

            return apiClient.SendGameApiAsync<RPlayLeaderboardUpdateResult>(
                UnityWebRequest.kHttpVerbPOST,
                "/leaderboard/score",
                new { operation, value },
                cancellationToken
            );
        }

        private static async Task EnsureConnectSessionAsync(
            CancellationToken cancellationToken
        )
        {
            if (RPlayWebGlBridge.IsInjectedSdkRuntime)
            {
                return;
            }

            if (connectSession == null)
            {
                throw new InvalidOperationException(
                    "데스크톱 소비 요청에는 로그인이 필요합니다. 먼저 RPlayGames.LoginAsync를 호출하세요."
                );
            }

            if (connectSession.ExpiresAt > DateTimeOffset.UtcNow.AddSeconds(30))
            {
                return;
            }

            await SessionRefreshLock.WaitAsync(cancellationToken);
            try
            {
                if (connectSession.ExpiresAt > DateTimeOffset.UtcNow.AddSeconds(30))
                {
                    return;
                }

                if (string.IsNullOrWhiteSpace(connectSession.RefreshToken))
                {
                    throw new RPlayApiException(
                        "로그인 세션이 만료되었습니다. 다시 로그인하세요.",
                        "SESSION_EXPIRED"
                    );
                }

                connectSession = await connectAuthenticator.RefreshAsync(
                    connectSession.RefreshToken,
                    cancellationToken
                );
                apiClient.SetSession(
                    connectSession.GameToken,
                    connectSession.ConnectAccessToken,
                    connectSession.PlatformType
                );
            }
            finally
            {
                SessionRefreshLock.Release();
            }
        }

        private static void EnsureInitialized()
        {
            if (!IsInitialized)
            {
                throw new InvalidOperationException(
                    "먼저 RPlayGames.InitializeAsync를 호출하세요."
                );
            }
        }

        private static void EnsureReady()
        {
            EnsureInitialized();
            if (!IsAuthenticated)
            {
                throw new InvalidOperationException(
                    "로그인이 필요합니다. 먼저 RPlayGames.LoginAsync를 호출하세요."
                );
            }
        }

        private static void ValidateFinitePositive(double value, string parameterName)
        {
            if (double.IsNaN(value) || double.IsInfinity(value) || value <= 0d)
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    "값은 0보다 큰 유한한 숫자여야 합니다."
                );
            }
        }

        private static void ValidateDataKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("저장 데이터 키가 필요합니다.", nameof(key));
            }

            if (string.Equals(key, "_id", StringComparison.Ordinal))
            {
                throw new ArgumentException(
                    "_id는 수정하거나 삭제할 수 없는 서버 보호 키입니다.",
                    nameof(key)
                );
            }
        }
    }
}
