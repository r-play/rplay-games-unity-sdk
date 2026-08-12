using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace RPlay.Games.Internal
{
    internal sealed class RPlayHttpResponse
    {
        public RPlayHttpResponse(long statusCode, string body)
        {
            StatusCode = statusCode;
            Body = body ?? string.Empty;
        }

        public long StatusCode { get; }

        public string Body { get; }
    }

    [DefaultExecutionOrder(-10000)]
    internal sealed class RPlayRuntimeHost : MonoBehaviour
    {
        private static RPlayRuntimeHost instance;
        private readonly CancellationTokenSource lifetimeCancellation =
            new CancellationTokenSource();

        internal CancellationToken LifetimeToken => lifetimeCancellation.Token;

        internal static RPlayRuntimeHost Instance
        {
            get
            {
                if (instance != null)
                {
                    return instance;
                }

                var gameObject = new GameObject("[RPlay Games SDK]");
                DontDestroyOnLoad(gameObject);
                instance = gameObject.AddComponent<RPlayRuntimeHost>();
                return instance;
            }
        }

        internal Task<RPlayHttpResponse> SendAsync(
            string url,
            string method,
            string jsonBody,
            IReadOnlyDictionary<string, string> headers,
            int timeoutSeconds,
            CancellationToken cancellationToken
        )
        {
            var completion = new TaskCompletionSource<RPlayHttpResponse>();
            StartCoroutine(
                SendCoroutine(
                    url,
                    method,
                    jsonBody,
                    headers,
                    timeoutSeconds,
                    cancellationToken,
                    completion
                )
            );
            return completion.Task;
        }

        internal Task<string> WaitForInjectedWebGlTokenAsync(
            int timeoutSeconds,
            CancellationToken cancellationToken
        )
        {
            var completion = new TaskCompletionSource<string>();
            StartCoroutine(
                WaitForInjectedWebGlTokenCoroutine(
                    timeoutSeconds,
                    cancellationToken,
                    completion
                )
            );
            return completion.Task;
        }

        private void OnDestroy()
        {
            // 로그인 콜백처럼 제한 없이 대기하는 작업도 게임 종료 시 함께 정리합니다.
            lifetimeCancellation.Cancel();
            lifetimeCancellation.Dispose();
            if (instance == this)
            {
                instance = null;
            }
        }

        private static IEnumerator SendCoroutine(
            string url,
            string method,
            string jsonBody,
            IReadOnlyDictionary<string, string> headers,
            int timeoutSeconds,
            CancellationToken cancellationToken,
            TaskCompletionSource<RPlayHttpResponse> completion
        )
        {
            using (var request = new UnityWebRequest(url, method))
            {
                request.downloadHandler = new DownloadHandlerBuffer();
                request.timeout = timeoutSeconds;

                if (jsonBody != null)
                {
                    request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonBody));
                    request.SetRequestHeader("Content-Type", "application/json");
                }

                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        if (!string.IsNullOrWhiteSpace(header.Value))
                        {
                            request.SetRequestHeader(header.Key, header.Value);
                        }
                    }
                }

                var operation = request.SendWebRequest();
                while (!operation.isDone)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        request.Abort();
                        completion.TrySetCanceled();
                        yield break;
                    }

                    yield return null;
                }

                if (request.result == UnityWebRequest.Result.ConnectionError)
                {
                    completion.TrySetException(
                        new RPlayApiException(
                            string.IsNullOrWhiteSpace(request.error)
                                ? "RPlay Games API에 연결할 수 없습니다."
                                : request.error,
                            "NETWORK_ERROR",
                            request.responseCode,
                            request.downloadHandler?.text
                        )
                    );
                    yield break;
                }

                completion.TrySetResult(
                    new RPlayHttpResponse(
                        request.responseCode,
                        request.downloadHandler?.text ?? string.Empty
                    )
                );
            }
        }

        private static IEnumerator WaitForInjectedWebGlTokenCoroutine(
            int timeoutSeconds,
            CancellationToken cancellationToken,
            TaskCompletionSource<string> completion
        )
        {
            var deadline = Time.realtimeSinceStartup + timeoutSeconds;
            while (Time.realtimeSinceStartup < deadline)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    completion.TrySetCanceled();
                    yield break;
                }

                var token = RPlayWebGlBridge.GetToken();
                if (!string.IsNullOrWhiteSpace(token))
                {
                    completion.TrySetResult(token);
                    yield break;
                }

                yield return null;
            }

            completion.TrySetException(
                new RPlayApiException(
                    "자동 주입된 RplayGameSDK에서 제한 시간 안에 게임 토큰을 받지 못했습니다.",
                    "GAME_TOKEN_TIMEOUT"
                )
            );
        }
    }

    internal static class RPlayWebGlBridge
    {
        internal static bool IsInjectedSdkRuntime
        {
            get
            {
#if UNITY_WEBGL && !UNITY_EDITOR
                return true;
#else
                return false;
#endif
            }
        }

        internal static string GetToken()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            var pointer = RPlayGames_GetInjectedToken();
            if (pointer == IntPtr.Zero)
            {
                return null;
            }

            try
            {
                return Marshal.PtrToStringUTF8(pointer);
            }
            finally
            {
                RPlayGames_Free(pointer);
            }
#else
            return null;
#endif
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern IntPtr RPlayGames_GetInjectedToken();

        [DllImport("__Internal")]
        private static extern void RPlayGames_Free(IntPtr pointer);
#endif
    }
}
