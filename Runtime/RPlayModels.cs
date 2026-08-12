using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RPlay.Games
{
    public class RPlayResponse
    {
        [JsonProperty("success")]
        public bool Success { get; internal set; }

        [JsonProperty("status")]
        public string Status { get; internal set; }

        [JsonProperty("errorCode")]
        public string ErrorCode { get; internal set; }

        [JsonProperty("error")]
        internal string Error
        {
            set
            {
                if (string.IsNullOrWhiteSpace(ErrorCode))
                {
                    ErrorCode = value;
                }
            }
        }

        [JsonProperty("message")]
        public string Message { get; internal set; }

        [JsonIgnore]
        public long HttpStatusCode { get; internal set; }

        [JsonIgnore]
        public string RawJson { get; internal set; }

        public void EnsureSuccess()
        {
            if (Success)
            {
                return;
            }

            throw new RPlayApiException(
                string.IsNullOrWhiteSpace(Message) ? "RPlay Games API 요청에 실패했습니다." : Message,
                ErrorCode,
                HttpStatusCode,
                RawJson
            );
        }
    }

    public sealed class RPlayUserInfo : RPlayResponse
    {
        [JsonProperty("userOid")]
        public string UserOid { get; internal set; }

        [JsonProperty("platformType")]
        public string PlatformType { get; internal set; }

        [JsonProperty("nickname")]
        public string Nickname { get; internal set; }

        [JsonProperty("multiLangNick")]
        public IReadOnlyDictionary<string, string> MultiLangNick { get; internal set; }

        [JsonProperty("coinBalance")]
        public double CoinBalance { get; internal set; }

        [JsonProperty("creditBalance")]
        public double? CreditBalance { get; internal set; }
    }

    public sealed class RPlayGameData : RPlayResponse
    {
        [JsonProperty("data")]
        public JObject Data { get; internal set; }

        [JsonProperty("dataSize")]
        public long DataSize { get; internal set; }

        public T ToObject<T>()
        {
            return Data == null ? default : Data.ToObject<T>();
        }
    }

    public sealed class RPlayGameData<T> : RPlayResponse
    {
        [JsonProperty("data")]
        public T Data { get; internal set; }

        [JsonProperty("dataSize")]
        public long DataSize { get; internal set; }
    }

    public sealed class RPlayDataWriteResult : RPlayResponse
    {
        [JsonProperty("dataSize")]
        public long DataSize { get; internal set; }

        [JsonProperty("currentSize")]
        public long? CurrentSize { get; internal set; }

        [JsonProperty("limit")]
        public long? Limit { get; internal set; }
    }

    public sealed class RPlayConsumeOptions
    {
        public bool SkipConfirmation { get; set; }

        public string ItemDescription { get; set; }

        public object Metadata { get; set; }
    }

    public sealed class RPlayConsumeResult : RPlayResponse
    {
        [JsonProperty("transactionId")]
        public string TransactionId { get; internal set; }

        [JsonProperty("remainingCoins")]
        public double? RemainingCoins { get; internal set; }

        [JsonProperty("remainingCredits")]
        public double? RemainingCredits { get; internal set; }

        [JsonProperty("consumedCredits")]
        public JObject ConsumedCredits { get; internal set; }

        [JsonProperty("requiredCredits")]
        public double? RequiredCredits { get; internal set; }

        [JsonProperty("currentBalance")]
        public double? CurrentBalance { get; internal set; }

        [JsonProperty("required")]
        public double? Required { get; internal set; }
    }

    public sealed class RPlayLeaderboardUser
    {
        [JsonProperty("nickname")]
        public string Nickname { get; internal set; }

        [JsonProperty("multiLangNick")]
        public IReadOnlyDictionary<string, string> MultiLangNick { get; internal set; }

        [JsonProperty("profileImage")]
        public string ProfileImage { get; internal set; }
    }

    public sealed class RPlayLeaderboardEntry
    {
        [JsonProperty("userOid")]
        public string UserOid { get; internal set; }

        [JsonProperty("rank")]
        public long? Rank { get; internal set; }

        [JsonProperty("score")]
        public double Score { get; internal set; }

        [JsonProperty("updatedAt")]
        public DateTimeOffset? UpdatedAt { get; internal set; }

        [JsonProperty("user")]
        public RPlayLeaderboardUser User { get; internal set; }
    }

    public sealed class RPlayLeaderboardUpdateResult : RPlayResponse
    {
        [JsonProperty("entry")]
        public RPlayLeaderboardEntry Entry { get; internal set; }
    }

    public sealed class RPlayLeaderboardMeResult : RPlayResponse
    {
        [JsonProperty("rank")]
        public long? Rank { get; internal set; }

        [JsonProperty("entry")]
        public RPlayLeaderboardEntry Entry { get; internal set; }
    }

    public sealed class RPlayLeaderboardPage : RPlayResponse
    {
        [JsonProperty("limit")]
        public int Limit { get; internal set; }

        [JsonProperty("offset")]
        public int Offset { get; internal set; }

        [JsonProperty("entries")]
        public IReadOnlyList<RPlayLeaderboardEntry> Entries { get; internal set; }
    }

    public sealed class RPlayLeaderboardAroundResult : RPlayResponse
    {
        [JsonProperty("range")]
        public int Range { get; internal set; }

        [JsonProperty("rank")]
        public long? Rank { get; internal set; }

        [JsonProperty("entry")]
        public RPlayLeaderboardEntry Entry { get; internal set; }

        [JsonProperty("entries")]
        public IReadOnlyList<RPlayLeaderboardEntry> Entries { get; internal set; }
    }
}
