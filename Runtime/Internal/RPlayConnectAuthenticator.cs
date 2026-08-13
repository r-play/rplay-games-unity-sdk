using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace RPlay.Games.Internal
{
    internal sealed class RPlayConnectSession
    {
        internal string GameToken { get; set; }

        internal string ConnectAccessToken { get; set; }

        internal string RefreshToken { get; set; }

        internal string PlatformType { get; set; }

        internal DateTimeOffset ExpiresAt { get; set; }
    }

    internal sealed class RPlayConnectAuthenticator
    {
        private readonly RPlayGamesSettings settings;
        private readonly RPlayApiClient apiClient;
        private Task<RPlayConnectSession> activeLoginTask;
        private string activeLoginUrl;

        internal RPlayConnectAuthenticator(
            RPlayGamesSettings settings,
            RPlayApiClient apiClient
        )
        {
            this.settings = settings;
            this.apiClient = apiClient;
        }

        internal Task<RPlayConnectSession> LoginAsync(
            CancellationToken cancellationToken
        )
        {
            if (activeLoginTask != null && !activeLoginTask.IsCompleted)
            {
                Application.OpenURL(activeLoginUrl);
                return activeLoginTask;
            }

            activeLoginTask = StartLoginAsync(cancellationToken);
            return activeLoginTask;
        }

        private async Task<RPlayConnectSession> StartLoginAsync(
            CancellationToken cancellationToken
        )
        {
            var verifier = CreateUrlSafeRandom(32);
            var challenge = CreateCodeChallenge(verifier);
            var state = CreateUrlSafeRandom(24);
            var runtimeLifetimeToken = RPlayRuntimeHost.Instance.LifetimeToken;

            using (
                var loginCancellation = CancellationTokenSource.CreateLinkedTokenSource(
                    cancellationToken,
                    runtimeLifetimeToken
                )
            )
            using (
                var callback = new RPlayLoopbackCallback()
            )
            {
                callback.Start();
                var query = RPlayApiClient.BuildQuery(
                    ("callback_port", callback.Port),
                    ("code_challenge", challenge),
                    ("state", state)
                );

                activeLoginUrl =
                    settings.WebOrigin
                    + "/game/"
                    + settings.GameOid
                    + "/connect?"
                    + query;
                Application.OpenURL(activeLoginUrl);
                // 브라우저가 닫혀도 시간 제한으로 실패시키지 않고 게임이 실행되는 동안 계속 기다립니다.
                var result = await callback.WaitAsync(loginCancellation.Token);

                if (!string.IsNullOrWhiteSpace(result.Error))
                {
                    throw new RPlayApiException(
                        string.IsNullOrWhiteSpace(result.ErrorDescription)
                            ? "브라우저 로그인이 취소되었거나 실패했습니다."
                            : result.ErrorDescription,
                        result.Error
                    );
                }

                if (!string.Equals(result.State, state, StringComparison.Ordinal))
                {
                    throw new RPlayApiException(
                        "로그인 응답의 state 값이 일치하지 않습니다.",
                        "INVALID_OAUTH_STATE"
                    );
                }

                if (string.IsNullOrWhiteSpace(result.Code))
                {
                    throw new RPlayApiException(
                        "브라우저 로그인에서 인증 코드를 받지 못했습니다.",
                        "MISSING_AUTHORIZATION_CODE"
                    );
                }

                var token = await apiClient.SendConnectAuthAsync<RPlayConnectTokenResponse>(
                    "/token",
                    new
                    {
                        code = result.Code,
                        codeVerifier = verifier,
                    },
                    loginCancellation.Token
                );
                token.EnsureSuccess();

                if (
                    string.IsNullOrWhiteSpace(token.GameToken)
                    || string.IsNullOrWhiteSpace(token.ConnectAccessToken)
                    || string.IsNullOrWhiteSpace(token.RefreshToken)
                    || (
                        token.PlatformType != "rplay"
                        && token.PlatformType != "storyengine"
                    )
                )
                {
                    throw new RPlayApiException(
                        "로그인 서버가 필요한 게임 전용 액세스 토큰을 반환하지 않았습니다.",
                        "INVALID_TOKEN_RESPONSE",
                        token.HttpStatusCode,
                        token.RawJson
                    );
                }

                return ToSession(token);
            }
        }

        internal async Task<RPlayConnectSession> RefreshAsync(
            RPlayConnectSession session,
            CancellationToken cancellationToken
        )
        {
            var token = await apiClient.SendConnectAuthAsync<RPlayConnectRefreshResponse>(
                "/refresh",
                new { refreshToken = session.RefreshToken },
                cancellationToken
            );
            token.EnsureSuccess();
            if (
                string.IsNullOrWhiteSpace(token.ConnectAccessToken)
                || string.IsNullOrWhiteSpace(token.RefreshToken)
            )
            {
                throw new RPlayApiException(
                    "로그인 서버가 필요한 연결 토큰을 반환하지 않았습니다.",
                    "INVALID_TOKEN_RESPONSE",
                    token.HttpStatusCode,
                    token.RawJson
                );
            }

            // 갱신 응답에는 연결 토큰만 포함되므로 게임 토큰과 플랫폼은 기존 세션 값을 유지합니다.
            session.ConnectAccessToken = token.ConnectAccessToken;
            session.RefreshToken = token.RefreshToken;
            session.ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(
                Math.Max(60, token.ExpiresIn)
            );
            return session;
        }

        internal Task<RPlayResponse> LogoutAsync(
            string refreshToken,
            CancellationToken cancellationToken
        )
        {
            return apiClient.SendConnectAuthAsync<RPlayResponse>(
                "/logout",
                new { refreshToken },
                cancellationToken
            );
        }

        private static RPlayConnectSession ToSession(RPlayConnectTokenResponse token)
        {
            return new RPlayConnectSession
            {
                GameToken = token.GameToken,
                ConnectAccessToken = token.ConnectAccessToken,
                RefreshToken = token.RefreshToken,
                PlatformType = token.PlatformType,
                ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(Math.Max(60, token.ExpiresIn)),
            };
        }

        private static string CreateUrlSafeRandom(int byteCount)
        {
            var bytes = new byte[byteCount];
            using (var random = RandomNumberGenerator.Create())
            {
                random.GetBytes(bytes);
            }

            return ToBase64Url(bytes);
        }

        private static string CreateCodeChallenge(string verifier)
        {
            using (var sha256 = SHA256.Create())
            {
                return ToBase64Url(sha256.ComputeHash(Encoding.ASCII.GetBytes(verifier)));
            }
        }

        private static string ToBase64Url(byte[] bytes)
        {
            return Convert.ToBase64String(bytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }
    }

    internal sealed class RPlayLoopbackResult
    {
        internal string Code { get; set; }

        internal string State { get; set; }

        internal string Error { get; set; }

        internal string ErrorDescription { get; set; }
    }

    internal sealed class RPlayLoopbackCallback : IDisposable
    {
        private TcpListener listener;

        internal int Port { get; private set; }

        internal void Start()
        {
            listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start(1);
            Port = ((IPEndPoint)listener.LocalEndpoint).Port;
        }

        internal async Task<RPlayLoopbackResult> WaitAsync(
            CancellationToken cancellationToken
        )
        {
            using (cancellationToken.Register(Stop))
            {
                while (true)
                {
                    TcpClient client;
                    try
                    {
                        client = await listener.AcceptTcpClientAsync();
                    }
                    catch (ObjectDisposedException) when (cancellationToken.IsCancellationRequested)
                    {
                        throw new OperationCanceledException(cancellationToken);
                    }
                    catch (SocketException) when (cancellationToken.IsCancellationRequested)
                    {
                        throw new OperationCanceledException(cancellationToken);
                    }

                    using (client)
                    using (cancellationToken.Register(client.Close))
                    using (var stream = client.GetStream())
                    using (var reader = new StreamReader(stream, Encoding.ASCII, false, 1024, true))
                    {
                        string requestLine;
                        try
                        {
                            requestLine = await reader.ReadLineAsync();
                            await ReadHeadersAsync(reader);
                        }
                        catch (IOException) when (cancellationToken.IsCancellationRequested)
                        {
                            throw new OperationCanceledException(cancellationToken);
                        }
                        catch (ObjectDisposedException) when (cancellationToken.IsCancellationRequested)
                        {
                            throw new OperationCanceledException(cancellationToken);
                        }

                        var method = GetRequestMethod(requestLine);
                        if (!string.Equals(method, "GET", StringComparison.Ordinal))
                        {
                            await WriteEmptyResponseAsync(stream, "405 Method Not Allowed");
                            continue;
                        }

                        var target = GetRequestTarget(requestLine);
                        var query = ParseQuery(target);
                        await WriteBrowserResponseAsync(stream);

                        return new RPlayLoopbackResult
                        {
                            Code = GetValue(query, "code"),
                            State = GetValue(query, "state"),
                            Error = GetValue(query, "error"),
                            ErrorDescription = GetValue(query, "error_description"),
                        };
                    }
                }
            }
        }

        public void Dispose()
        {
            Stop();
        }

        private void Stop()
        {
            try
            {
                listener?.Stop();
            }
            catch (SocketException)
            {
            }
        }

        private static string GetRequestTarget(string requestLine)
        {
            var parts = (requestLine ?? string.Empty).Split(' ');
            var target = parts.Length >= 2 ? parts[1] : string.Empty;
            var queryStart = target.IndexOf('?');
            var path = queryStart < 0 ? target : target.Substring(0, queryStart);
            if (!string.Equals(path, "/callback", StringComparison.Ordinal))
            {
                throw new RPlayApiException(
                    "로그인 콜백 요청을 해석할 수 없습니다.",
                    "INVALID_LOOPBACK_CALLBACK"
                );
            }

            return target;
        }

        private static string GetRequestMethod(string requestLine)
        {
            var parts = (requestLine ?? string.Empty).Split(' ');
            return parts.Length >= 1 ? parts[0] : string.Empty;
        }

        private static async Task ReadHeadersAsync(StreamReader reader)
        {
            while (true)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrEmpty(line))
                {
                    return;
                }
            }
        }

        private static Dictionary<string, string> ParseQuery(string target)
        {
            var values = new Dictionary<string, string>(StringComparer.Ordinal);
            var queryStart = target.IndexOf('?');
            if (queryStart < 0 || queryStart == target.Length - 1)
            {
                return values;
            }

            var pairs = target.Substring(queryStart + 1).Split('&');
            foreach (var pair in pairs)
            {
                var separator = pair.IndexOf('=');
                var rawKey = separator < 0 ? pair : pair.Substring(0, separator);
                var rawValue = separator < 0 ? string.Empty : pair.Substring(separator + 1);
                values[Uri.UnescapeDataString(rawKey.Replace('+', ' '))] =
                    Uri.UnescapeDataString(rawValue.Replace('+', ' '));
            }

            return values;
        }

        private static string GetValue(
            IReadOnlyDictionary<string, string> values,
            string key
        )
        {
            return values.TryGetValue(key, out var value) ? value : null;
        }

        private static async Task WriteEmptyResponseAsync(
            NetworkStream stream,
            string status
        )
        {
            var header = Encoding.ASCII.GetBytes(
                "HTTP/1.1 "
                    + status
                    + "\r\nContent-Length: 0\r\n"
                    + "Connection: close\r\n\r\n"
            );
            await stream.WriteAsync(header, 0, header.Length);
            await stream.FlushAsync();
        }

        private static async Task WriteBrowserResponseAsync(NetworkStream stream)
        {
            var title = WebUtility.HtmlEncode(
                RPlayLocalization.Get("login_completed_title")
            );
            var description = WebUtility.HtmlEncode(
                RPlayLocalization.Get("login_completed_description")
            );
            // 결과 전달이 끝났으므로 브라우저가 허용하는 경우 연결 탭을 닫습니다.
            var html =
                "<!doctype html><html><head><meta charset=\"utf-8\">"
                + "<meta name=\"viewport\" content=\"width=device-width,initial-scale=1\">"
                + "<title>"
                + title
                + "</title><script>window.close();</script></head><body><h1>"
                + title
                + "</h1><p>"
                + description
                + "</p></body></html>";
            var body = Encoding.UTF8.GetBytes(html);
            var header = Encoding.ASCII.GetBytes(
                "HTTP/1.1 200 OK\r\nContent-Type: text/html; charset=utf-8\r\nContent-Length: "
                    + body.Length
                    + "\r\n"
                    + "Connection: close\r\n\r\n"
            );
            await stream.WriteAsync(header, 0, header.Length);
            await stream.WriteAsync(body, 0, body.Length);
            await stream.FlushAsync();
        }
    }
}
