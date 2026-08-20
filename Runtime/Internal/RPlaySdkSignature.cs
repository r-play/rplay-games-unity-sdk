using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace RPlay.Games.Internal
{
    /// <summary>
    /// 게임 연결 로그인 요청에 붙이는 SDK 서명입니다.
    ///
    /// 키 자체는 네트워크로 보내지 않고 서명만 보냅니다. 서버는 같은 계산을 다시 해서
    /// 이 요청이 해당 게임의 빌드에서 나온 것인지 확인합니다.
    ///
    /// 서명 대상에 들어가는 codeChallenge가 로그인마다 새로 만들어지는 값이라
    /// 별도의 일회성 값 없이도 같은 서명이 다시 쓰이지 않습니다.
    /// </summary>
    internal static class RPlaySdkSignature
    {
        internal static RPlaySdkSignatureValues Create(
            string sdkKey,
            string gameOid,
            string codeChallenge
        )
        {
            if (string.IsNullOrWhiteSpace(sdkKey))
            {
                throw new RPlayApiException(
                    "SDK 키가 없습니다. RPlay 스튜디오의 게임 설정 화면에서 SDK 키를 복사해 RPlayGamesSettings에 입력하세요.",
                    "MISSING_SDK_KEY"
                );
            }

            var timestamp = DateTimeOffset.UtcNow
                .ToUnixTimeSeconds()
                .ToString(CultureInfo.InvariantCulture);
            // 서버와 글자 하나까지 같은 문자열을 만들어야 서명이 맞습니다.
            var payload = string.Join(
                "\n",
                new[] { gameOid, timestamp, codeChallenge }
            );

            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(sdkKey)))
            {
                var signature = ToBase64Url(
                    hmac.ComputeHash(Encoding.UTF8.GetBytes(payload))
                );
                return new RPlaySdkSignatureValues
                {
                    Timestamp = timestamp,
                    Signature = signature,
                };
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

    internal sealed class RPlaySdkSignatureValues
    {
        internal string Timestamp { get; set; }

        internal string Signature { get; set; }
    }
}
