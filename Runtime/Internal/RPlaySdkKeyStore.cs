using System;
using System.Security.Cryptography;
using System.Text;

namespace RPlay.Games.Internal
{
    /// <summary>
    /// SDK 키를 설정 에셋에 그대로 적어 두지 않도록 가려서 보관합니다.
    ///
    /// 암호화가 아니라 눈에 띄지 않게 하는 수준입니다. 빌드를 뜯어보면 키는 나옵니다.
    /// 목적은 에셋 덤프 도구나 실수로 올린 저장소에서 키가 바로 읽히지 않게 하는 것입니다.
    /// </summary>
    public static class RPlaySdkKeyStore
    {
        private const string MaskSeed = "rplay-sdk-key-mask-v1";

        /// <summary>키를 가려서 저장 가능한 문자열로 바꿉니다. (에디터에서 사용)</summary>
        public static string Mask(string sdkKey)
        {
            if (string.IsNullOrWhiteSpace(sdkKey))
            {
                return string.Empty;
            }

            var raw = Encoding.UTF8.GetBytes(sdkKey.Trim());
            return Convert.ToBase64String(ApplyKeystream(raw));
        }

        /// <summary>가려 둔 문자열에서 원래 키를 되돌립니다.</summary>
        public static string Unmask(string maskedValue)
        {
            if (string.IsNullOrWhiteSpace(maskedValue))
            {
                return string.Empty;
            }

            try
            {
                var masked = Convert.FromBase64String(maskedValue.Trim());
                return Encoding.UTF8.GetString(ApplyKeystream(masked));
            }
            catch (FormatException)
            {
                return string.Empty;
            }
        }

        private static byte[] ApplyKeystream(byte[] input)
        {
            var output = new byte[input.Length];
            using (var sha256 = SHA256.Create())
            {
                var block = sha256.ComputeHash(Encoding.UTF8.GetBytes(MaskSeed));
                for (var i = 0; i < input.Length; i++)
                {
                    if (i > 0 && i % block.Length == 0)
                    {
                        block = sha256.ComputeHash(block);
                    }

                    output[i] = (byte)(input[i] ^ block[i % block.Length]);
                }
            }

            return output;
        }
    }
}
