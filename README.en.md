<div align="center">
  <h1>RPlay Games Unity SDK</h1>
  <p><a href="README.md">한국어</a> · <strong>English</strong> · <a href="README.ja.md">日本語</a></p>
  <p>
    <a href="#requirements"><img src="https://img.shields.io/badge/Unity-2022.3%2B-000000?logo=unity&logoColor=white" alt="Unity 2022.3 or later"></a>
    <a href="https://github.com/r-play/rplay-games-unity-sdk/tree/v0.1.3"><img src="https://img.shields.io/badge/version-0.1.3-2596be" alt="Version 0.1.3"></a>
    <a href="#license"><img src="https://img.shields.io/badge/license-MIT-2596be" alt="MIT License"></a>
  </p>
  <img src="Documentation~/images/rplay-games-banner.png" alt="RPlay Games" width="100%">
  <p>A Unity SDK for authentication, game data, leaderboards, and platform coin or credit APIs provided by <a href="https://rplay.live/p/game">RPlay Games</a> and <a href="https://storyengine.live/p/game">StoryEngine</a>.</p>
</div>

## Table of Contents

- [Requirements](#requirements)
- [Installation](#installation)
- [Run the Sample](#run-the-sample)
- [Getting Started](#getting-started)
  - [Create a Settings Asset](#1-create-a-settings-asset)
  - [Connect a Login Button](#2-connect-a-login-button)
- [SDK State and Authentication](#sdk-state-and-authentication)
- [User Information](#user-information)
- [Save and Load Game Data](#save-and-load-game-data)
- [Leaderboard](#leaderboard)
- [RPlay Coins and StoryEngine Credits](#rplay-coins-and-storyengine-credits)
- [Responses and Error Handling](#responses-and-error-handling)
- [License](#license)

## Requirements

- Unity 2022.3 LTS or later
- WebGL
- Windows, macOS, or Linux desktop build

## Installation

1. Open `Window > Package Manager` from the Unity menu.
2. Select the `+` button in the top-left corner, then choose `Install package from git URL...`.
3. Enter the URL below and select `Install`.

```text
https://github.com/r-play/rplay-games-unity-sdk.git#v0.1.3
```

## Run the Sample

Use the included API Playground to try all APIs immediately.

1. Select `RPlay Games SDK` in Package Manager.
2. Select `Import` for `API Playground` under `Samples`.
3. Open `Samples/RPlay Games SDK/0.1.3/API Playground` in the Project window.
4. Open the `RPlayGamesApiPlayground` scene and enter Play mode.

The sample includes a test `GameOid`. Replace it with your game's `GameOid` when integrating the SDK into an actual game.

## Getting Started

### 1. Create a Settings Asset

1. Create a game on RPlay and find its `GameOid` on the game management page.
2. Right-click in Unity's Project window and select `Create > RPlay > Games Settings`.
3. Enter the `GameOid` in the generated `RPlayGamesSettings` asset's `Game Oid` field.

### 2. Connect a Login Button

Add the script below to a GameObject and assign the settings asset to `Settings`. Then register `Login` in a UI Button's `On Click()` event.

```csharp
using System;
using RPlay.Games;
using UnityEngine;

public sealed class RPlayLoginButton : MonoBehaviour
{
    [SerializeField] private RPlayGamesSettings settings;

    public async void Login()
    {
        try
        {
            if (!RPlayGames.IsInitialized)
            {
                await RPlayGames.InitializeAsync(settings);
            }

            if (!RPlayGames.IsAuthenticated)
            {
                await RPlayGames.LoginAsync();
            }
            Debug.Log("RPlay Games login complete");
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
    }

    public async void Logout()
    {
        try
        {
            await RPlayGames.LogoutAsync();
            Debug.Log("RPlay Games logout complete");
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
    }
}
```

In the Editor and desktop builds, `LoginAsync()` opens the browser. After the user signs in with RPlay or StoryEngine, the SDK keeps the selected platform for the current runtime session.

In WebGL, the SDK uses the authentication information from the RPlay game page and does not open a separate browser login window.

## SDK State and Authentication

The SDK must be initialized and authenticated before calling game APIs.

| Property | Type | Description |
| --- | --- | --- |
| `RPlayGames.IsInitialized` | `bool` | Indicates whether SDK initialization has completed. |
| `RPlayGames.IsAuthenticated` | `bool` | Indicates whether the SDK has authentication information for game API calls. |
| `RPlayGames.ConnectedPlatform` | `RPlayPlatform?` | The currently connected platform. It is `null` before login and `RPlay` or `StoryEngine` after login. |
| `RPlayGames.Settings` | `RPlayGamesSettings` | The settings asset passed during initialization. |

| Method | Return Type | Description |
| --- | --- | --- |
| `InitializeAsync(settings)` | `Task` | Reads `RPlayGamesSettings` and prepares the SDK. Calling it again with the same settings is safe. |
| `LoginAsync()` | `Task` | Starts game API authentication. It opens browser login in the Editor and desktop builds, and uses the game page's authentication in WebGL. |
| `LogoutAsync()` | `Task` | Ends login for the current runtime session and clears authentication information held by the SDK. |

Every asynchronous method accepts an optional `CancellationToken` as its final argument.

## User Information

| Method | Return Type | Description |
| --- | --- | --- |
| `VerifyUserAsync()` | `Task<RPlayResponse>` | Checks whether the current account can play this game. |
| `GetUserInfoAsync()` | `Task<RPlayUserInfo>` | Loads the user identifier, platform, nickname, and coin or credit balance. |

```csharp
var user = await RPlayGames.GetUserInfoAsync();
user.EnsureSuccess();

Debug.Log($"Nickname: {user.Nickname}");
```

The main `RPlayUserInfo` properties are:

| Property | Type | Description |
| --- | --- | --- |
| `UserOid` | `string` | The platform's user identifier. |
| `PlatformType` | `string` | The platform that provided the response. |
| `Nickname` | `string` | The current user's nickname. |
| `MultiLangNick` | `IReadOnlyDictionary<string, string>` | Nicknames by language. |
| `CoinBalance` | `double` | The RPlay coin balance. |
| `CreditBalance` | `double?` | The StoryEngine credit balance. It is `null` for an RPlay account. |

Use `RPlayGames.ConnectedPlatform` to check the currently connected platform.

Use `VerifyUserAsync()` when you only need to check game access.

```csharp
var access = await RPlayGames.VerifyUserAsync();
if (!access.Success)
{
    Debug.LogWarning($"Game access check failed: {access.ErrorCode}");
}
```

## Save and Load Game Data

Data saved with `SetDataAsync()` is stored on the RPlay server for each game and signed-in platform account, not on the local device. Signing in to the same game with the same platform account lets you continue loading the data with `LoadDataAsync()` in later runtime sessions.

| Method | Return Type | Description |
| --- | --- | --- |
| `LoadDataAsync()` | `Task<RPlayGameData>` | Loads all saved data as a `JObject`. Use `ToObject<T>()` to convert it to a desired type. |
| `LoadDataAsync<T>()` | `Task<RPlayGameData<T>>` | Loads all saved data as the specified type `T`. |
| `RPlayGameData.ToObject<T>()` | `T` | Converts data loaded as a `JObject` to the specified type `T`. |
| `SetDataAsync(key, value)` | `Task<RPlayDataWriteResult>` | Adds or overwrites a single key. |
| `SetDataAsync(data)` | `Task<RPlayDataWriteResult>` | Adds or overwrites multiple fields from an object at once. |
| `DeleteDataAsync(key)` | `Task<RPlayDataWriteResult>` | Deletes the specified key and value. |
| `DeleteAllDataAsync()` | `Task<RPlayResponse>` | Deletes all game data for the current user. Leaderboard records are preserved. |

```csharp
using Newtonsoft.Json;

public sealed class PlayerSave
{
    [JsonProperty("chapter")]
    public int Chapter { get; set; }

    [JsonProperty("hp")]
    public int Hp { get; set; }
}
```

```csharp
await RPlayGames.SetDataAsync("chapter", 3);
await RPlayGames.SetDataAsync(new { chapter = 3, hp = 80 });

var save = await RPlayGames.LoadDataAsync<PlayerSave>();
save.EnsureSuccess();

Debug.Log($"Chapter: {save.Data.Chapter}, HP: {save.Data.Hp}");

await RPlayGames.DeleteDataAsync("chapter");
await RPlayGames.DeleteAllDataAsync();
```

`RPlayGameData<T>.Data` contains the loaded data, while `DataSize` contains the current saved-data size. You can also read the size after a write or delete from `RPlayDataWriteResult.DataSize`.

`SetDataAsync(object)` adds or overwrites only the fields supplied in the object.

## Leaderboard

| Method | Return Type | Description |
| --- | --- | --- |
| `SetScoreAsync(score)` | `Task<RPlayLeaderboardUpdateResult>` | Sets your leaderboard score to the specified value. |
| `IncrementScoreAsync(amount)` | `Task<RPlayLeaderboardUpdateResult>` | Adds the specified value to your current score. |
| `GetMyRankAsync()` | `Task<RPlayLeaderboardMeResult>` | Loads your score and current rank. |
| `GetTopRanksAsync(limit = 50, offset = 0)` | `Task<RPlayLeaderboardPage>` | Loads top ranks one page at a time. |
| `GetRanksAroundMeAsync(range = 5)` | `Task<RPlayLeaderboardAroundResult>` | Loads users ranked before and after your current rank. |

```csharp
await RPlayGames.SetScoreAsync(1200);
await RPlayGames.IncrementScoreAsync(50);

var mine = await RPlayGames.GetMyRankAsync();
var top = await RPlayGames.GetTopRanksAsync(limit: 20);
var around = await RPlayGames.GetRanksAroundMeAsync(range: 5);

mine.EnsureSuccess();
top.EnsureSuccess();
around.EnsureSuccess();
```

The maximum value for `GetTopRanksAsync()`'s `limit` and `GetRanksAroundMeAsync()`'s `range` is 50.

Score update results are available in `RPlayLeaderboardUpdateResult.Entry`. Your rank is available in `RPlayLeaderboardMeResult.Rank` and `Entry`. Each item in a list response's `Entries` includes the user's `Rank`, `Score`, `UpdatedAt`, nickname, and profile information.

## RPlay Coins and StoryEngine Credits

RPlay uses **coins**, while StoryEngine uses **credits**. The SDK automatically selects coins or credits based on the platform connected at login, so the game does not need to specify a platform.

| Method | Return Type | Description |
| --- | --- | --- |
| `RequestChargeAsync()` | `Task<RPlayResponse>` | Opens the charge page for the connected platform. The response indicates whether the page request succeeded. |
| `ConsumeAsync(amount, itemName, options)` | `Task<RPlayConsumeResult>` | Requests a coin or credit consumption for an item and returns the transaction result and remaining balance. |

Call the following API to open the charge page:

```csharp
var charge = await RPlayGames.RequestChargeAsync();
charge.EnsureSuccess();
```

`RequestChargeAsync()` opens the coin or credit charge page that matches the connected platform.

To consume coins or credits, supply an amount and an item name to display. Enter `amount` in RPlay coin units for both platforms. RPlay consumes the specified number of coins, while StoryEngine converts it to credits according to the platform conversion rate. The SDK and server calculate the credit amount, so the game must not convert it directly.

```csharp
var result = await RPlayGames.ConsumeAsync(
    amount: 10,
    itemName: "Revive",
    options: new RPlayConsumeOptions
    {
        ItemDescription = "Continue playing from the current position.",
        Metadata = new { stage = 7 }
    }
);

result.EnsureSuccess();
```

Use `RPlayConsumeOptions` to configure information passed with the consumption UI.

| Property | Type | Description |
| --- | --- | --- |
| `SkipConfirmation` | `bool` | When `true`, skips every SDK popup, including consumption confirmation and insufficient-balance notices. The default is `false`. |
| `ItemDescription` | `string` | The item description shown on the consumption confirmation screen. |
| `Metadata` | `object` | Additional game-specific information sent with the consumption request. Use a value that can be converted to a JSON object. |

After a successful consumption, `RPlayConsumeResult.TransactionId` contains the transaction identifier. Read the post-consumption balance from `RemainingCoins` for RPlay or `RemainingCredits` for StoryEngine.

In the Editor and desktop builds, the SDK displays consumption confirmation and insufficient-balance notices. WebGL uses the popup supplied by the game page.

## Responses and Error Handling

Use an API response's `Success` property to check whether the request succeeded. Calling `EnsureSuccess()` on a failed response throws an `RPlayApiException`.

Every API response inherits from `RPlayResponse` and provides these common properties:

| Property | Type | Description |
| --- | --- | --- |
| `Success` | `bool` | Indicates whether the request succeeded. |
| `Status` | `string` | The request processing status. |
| `ErrorCode` | `string` | An error code that identifies the failure. |
| `Message` | `string` | An error or informational message returned by the server. |
| `HttpStatusCode` | `long` | The HTTP status code. |
| `RawJson` | `string` | The original JSON string returned by the server. |
| `EnsureSuccess()` | `void` | Throws an `RPlayApiException` when `Success` is `false`. |

```csharp
try
{
    var user = await RPlayGames.GetUserInfoAsync();
    user.EnsureSuccess();
}
catch (RPlayApiException exception)
{
    Debug.LogError($"RPlay Games API error: {exception.ErrorCode}");
}
```

To handle a response directly, use `Success`, `Status`, `ErrorCode`, and `Message`. An `RPlayApiException` exposes failure details through `ErrorCode`, `HttpStatusCode`, and `ResponseBody`.

## License

Copyright (c) 2026 PLAX INC. This SDK is provided under the [MIT License](LICENSE.md).
