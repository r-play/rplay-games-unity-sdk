<div align="center">
  <h1>RPlay Games Unity SDK</h1>
  <img src="Documentation~/images/rplay-games-banner.png" alt="RPlay Games" width="100%">
  <p><a href="https://rplay.live/p/game">RPlay Games</a>와 <a href="https://storyengine.live/p/game">StoryEngine</a>의 로그인, 게임 데이터, 리더보드 및 플랫폼 코인·크레딧 API를 Unity에서 사용할 수 있는 SDK입니다.</p>
</div>

## 목차

- [요구 사항](#요구-사항)
- [설치](#설치)
- [샘플 실행](#샘플-실행)
- [시작하기](#시작하기)
  - [설정 에셋 만들기](#1-설정-에셋-만들기)
  - [로그인 버튼 연결하기](#2-로그인-버튼-연결하기)
- [SDK 상태 및 로그인](#sdk-상태-및-로그인)
- [사용자 정보](#사용자-정보)
- [게임 데이터 저장 및 불러오기](#게임-데이터-저장-및-불러오기)
- [리더보드](#리더보드)
- [RPlay 코인 및 StoryEngine 크레딧](#rplay-코인-및-storyengine-크레딧)
- [응답 및 오류 처리](#응답-및-오류-처리)
- [라이선스](#라이선스)

## 요구 사항

- Unity 2022.3 LTS 이상
- WebGL
- Windows, macOS, Linux 데스크톱 빌드

## 설치

1. Unity 메뉴에서 `Window > Package Manager`를 엽니다.
2. 왼쪽 위의 `+` 버튼을 누르고 `Install package from git URL...`을 선택합니다.
3. 아래 URL을 입력하고 `Install`을 누릅니다.

```text
https://github.com/r-play/rplay-games-unity-sdk.git#v0.1.3
```

## 샘플 실행

전체 API를 바로 확인하려면 패키지에 포함된 API Playground를 사용하세요.

1. Package Manager에서 `RPlay Games SDK`를 선택합니다.
2. `Samples`의 `API Playground`에서 `Import`를 누릅니다.
3. Project 창에서 `Samples/RPlay Games SDK/0.1.3/API Playground` 폴더를 엽니다.
4. `RPlayGamesApiPlayground` 씬을 열고 Play 버튼을 누릅니다.

샘플에는 테스트용 `GameOid`가 설정되어 있습니다. 실제 게임에서는 해당 게임의 `GameOid`로 교체하세요.

## 시작하기

### 1. 설정 에셋 만들기

1. RPlay에서 게임을 만든 뒤 게임 관리 화면에서 `GameOid`를 확인합니다.
2. Unity의 Project 창에서 마우스 오른쪽 버튼을 누르고 `Create > RPlay > Games Settings`를 선택합니다.
3. 생성된 `RPlayGamesSettings` 에셋의 `Game Oid`에 확인한 값을 입력합니다.

### 2. 로그인 버튼 연결하기

아래 스크립트를 GameObject에 추가하고 `Settings`에 생성한 설정 에셋을 연결합니다. 그다음 UI Button의 `On Click()`에 `Login`을 등록하세요.

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
            Debug.Log("RPlay Games 로그인 완료");
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
            Debug.Log("RPlay Games 로그아웃 완료");
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
    }
}
```

Editor와 데스크톱 빌드에서는 `LoginAsync()`를 호출하면 브라우저가 열립니다. 사용자가 RPlay 또는 StoryEngine으로 로그인하면 SDK가 선택한 플랫폼을 실행 세션 동안 유지합니다.

WebGL에서는 RPlay 게임 페이지의 로그인 정보를 사용하므로 별도의 브라우저 로그인 창을 열지 않습니다.

## SDK 상태 및 로그인

게임 API를 호출하기 전에 SDK 초기화와 인증이 완료되어야 합니다.

| 속성 | 타입 | 설명 |
| --- | --- | --- |
| `RPlayGames.IsInitialized` | `bool` | SDK 초기화가 완료되었는지 나타냅니다. |
| `RPlayGames.IsAuthenticated` | `bool` | 게임 API를 호출할 수 있는 인증 상태인지 나타냅니다. |
| `RPlayGames.ConnectedPlatform` | `RPlayPlatform?` | 현재 연결된 플랫폼입니다. 로그인 전에는 `null`이며, 로그인 후 `RPlay` 또는 `StoryEngine`입니다. |
| `RPlayGames.Settings` | `RPlayGamesSettings` | 초기화할 때 전달한 설정 에셋입니다. |

| 함수 | 반환형 | 설명 |
| --- | --- | --- |
| `InitializeAsync(settings)` | `Task` | `RPlayGamesSettings`를 읽고 SDK를 준비합니다. 같은 설정으로 다시 호출해도 안전합니다. |
| `LoginAsync()` | `Task` | 게임 API 인증을 시작합니다. Editor와 데스크톱에서는 브라우저 로그인을 열고, WebGL에서는 게임 페이지의 인증을 사용합니다. |
| `LogoutAsync()` | `Task` | 현재 실행 세션의 로그인을 종료하고 SDK가 보관한 인증 정보를 지웁니다. |

모든 비동기 함수는 마지막 인자로 선택적인 `CancellationToken`을 받을 수 있습니다.

## 사용자 정보

| 함수 | 반환형 | 설명 |
| --- | --- | --- |
| `VerifyUserAsync()` | `Task<RPlayResponse>` | 현재 계정이 이 게임을 플레이할 수 있는지 확인합니다. |
| `GetUserInfoAsync()` | `Task<RPlayUserInfo>` | 사용자 식별자, 플랫폼, 닉네임 및 코인·크레딧 잔액을 불러옵니다. |

```csharp
var user = await RPlayGames.GetUserInfoAsync();
user.EnsureSuccess();

Debug.Log($"닉네임: {user.Nickname}");
```

`RPlayUserInfo`의 주요 속성은 다음과 같습니다.

| 속성 | 타입 | 설명 |
| --- | --- | --- |
| `UserOid` | `string` | 플랫폼의 사용자 식별자입니다. |
| `PlatformType` | `string` | 응답을 제공한 플랫폼입니다. |
| `Nickname` | `string` | 현재 사용자의 닉네임입니다. |
| `MultiLangNick` | `IReadOnlyDictionary<string, string>` | 언어별 닉네임입니다. |
| `CoinBalance` | `double` | RPlay 코인 잔액입니다. |
| `CreditBalance` | `double?` | StoryEngine 크레딧 잔액입니다. RPlay 계정에서는 `null`입니다. |

현재 연결된 플랫폼은 `RPlayGames.ConnectedPlatform`으로 확인할 수 있습니다.

게임의 플레이 권한만 확인하려면 `VerifyUserAsync()`를 사용합니다.

```csharp
var access = await RPlayGames.VerifyUserAsync();
if (!access.Success)
{
    Debug.LogWarning($"플레이 권한 확인 실패: {access.ErrorCode}");
}
```

## 게임 데이터 저장 및 불러오기

`SetDataAsync()`로 저장한 데이터는 기기 로컬이 아니라 RPlay 서버에 게임과 로그인한 플랫폼 계정별로 저장됩니다. 같은 게임에서 같은 플랫폼 계정으로 로그인하면 다른 실행 세션에서도 `LoadDataAsync()`로 이어서 불러올 수 있습니다.

| 함수 | 반환형 | 설명 |
| --- | --- | --- |
| `LoadDataAsync()` | `Task<RPlayGameData>` | 저장된 전체 데이터를 `JObject`로 불러옵니다. `ToObject<T>()`로 원하는 타입으로 변환할 수 있습니다. |
| `LoadDataAsync<T>()` | `Task<RPlayGameData<T>>` | 저장된 전체 데이터를 지정한 타입 `T`로 불러옵니다. |
| `RPlayGameData.ToObject<T>()` | `T` | `JObject`로 불러온 데이터를 지정한 타입 `T`로 변환합니다. |
| `SetDataAsync(key, value)` | `Task<RPlayDataWriteResult>` | 지정한 키 하나를 추가하거나 덮어씁니다. |
| `SetDataAsync(data)` | `Task<RPlayDataWriteResult>` | 객체에 들어 있는 여러 필드를 한 번에 추가하거나 덮어씁니다. |
| `DeleteDataAsync(key)` | `Task<RPlayDataWriteResult>` | 지정한 키와 값을 삭제합니다. |
| `DeleteAllDataAsync()` | `Task<RPlayResponse>` | 현재 사용자의 게임 데이터를 모두 삭제합니다. 리더보드 기록은 유지됩니다. |

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

Debug.Log($"챕터: {save.Data.Chapter}, HP: {save.Data.Hp}");

await RPlayGames.DeleteDataAsync("chapter");
await RPlayGames.DeleteAllDataAsync();
```

`RPlayGameData<T>.Data`에는 불러온 데이터가, `DataSize`에는 현재 저장 데이터 크기가 들어 있습니다. 저장 및 삭제 결과의 `RPlayDataWriteResult.DataSize`에서도 변경 후 크기를 확인할 수 있습니다.

`SetDataAsync(object)`는 전달한 필드만 기존 데이터에 추가하거나 덮어씁니다.

## 리더보드

| 함수 | 반환형 | 설명 |
| --- | --- | --- |
| `SetScoreAsync(score)` | `Task<RPlayLeaderboardUpdateResult>` | 내 리더보드 점수를 지정한 값으로 설정합니다. |
| `IncrementScoreAsync(amount)` | `Task<RPlayLeaderboardUpdateResult>` | 현재 점수에 지정한 값을 더합니다. |
| `GetMyRankAsync()` | `Task<RPlayLeaderboardMeResult>` | 내 점수와 현재 순위를 불러옵니다. |
| `GetTopRanksAsync(limit = 50, offset = 0)` | `Task<RPlayLeaderboardPage>` | 상위 순위를 페이지 단위로 불러옵니다. |
| `GetRanksAroundMeAsync(range = 5)` | `Task<RPlayLeaderboardAroundResult>` | 내 순위를 중심으로 앞뒤 사용자의 순위를 불러옵니다. |

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

`GetTopRanksAsync()`의 `limit`과 `GetRanksAroundMeAsync()`의 `range`는 최대 50입니다.

점수 변경 결과는 `RPlayLeaderboardUpdateResult.Entry`, 내 순위 결과는 `RPlayLeaderboardMeResult.Rank`와 `Entry`에서 확인합니다. 목록 조회 결과의 `Entries`에는 각 사용자의 `Rank`, `Score`, `UpdatedAt` 및 닉네임과 프로필 정보가 들어 있습니다.

## RPlay 코인 및 StoryEngine 크레딧

RPlay에서는 **코인**, StoryEngine에서는 **크레딧**을 사용합니다. SDK는 로그인할 때 연결된 플랫폼을 기준으로 코인 또는 크레딧을 자동으로 선택하므로 게임에서 플랫폼을 따로 지정할 필요가 없습니다.

| 함수 | 반환형 | 설명 |
| --- | --- | --- |
| `RequestChargeAsync()` | `Task<RPlayResponse>` | 연결된 플랫폼의 충전 화면을 엽니다. 반환값으로 화면을 정상적으로 요청했는지 확인할 수 있습니다. |
| `ConsumeAsync(amount, itemName, options)` | `Task<RPlayConsumeResult>` | 지정한 아이템의 코인 또는 크레딧 소비를 요청하고 거래 결과와 남은 잔액을 반환합니다. |

충전 화면을 열려면 다음 API를 호출합니다.

```csharp
var charge = await RPlayGames.RequestChargeAsync();
charge.EnsureSuccess();
```

`RequestChargeAsync()`는 연결된 플랫폼에 맞는 코인 또는 크레딧 충전 화면을 엽니다.

코인 또는 크레딧을 소비할 때는 금액과 표시할 아이템 이름을 전달합니다. `amount`는 두 플랫폼 모두 RPlay 코인 단위로 입력합니다. RPlay에서는 해당 금액의 코인이 소비되고, StoryEngine에서는 플랫폼의 환산 기준에 따라 크레딧으로 변환되어 소비됩니다. 크레딧 금액은 SDK와 서버가 계산하므로 게임에서 직접 환산하지 않습니다.

```csharp
var result = await RPlayGames.ConsumeAsync(
    amount: 10,
    itemName: "부활권",
    options: new RPlayConsumeOptions
    {
        ItemDescription = "현재 위치에서 이어서 플레이합니다.",
        Metadata = new { stage = 7 }
    }
);

result.EnsureSuccess();
```

`RPlayConsumeOptions`로 소비 화면과 함께 전달할 정보를 설정할 수 있습니다.

| 속성 | 타입 | 설명 |
| --- | --- | --- |
| `SkipConfirmation` | `bool` | `true`이면 소비 확인과 잔액 부족 안내를 포함한 SDK 팝업을 모두 생략합니다. 기본값은 `false`입니다. |
| `ItemDescription` | `string` | 소비 확인 화면에 표시할 아이템 설명입니다. |
| `Metadata` | `object` | 소비 요청에 함께 전달할 게임별 추가 정보입니다. JSON 객체로 변환 가능한 값을 사용합니다. |

소비가 성공하면 `RPlayConsumeResult.TransactionId`에 거래 식별자가 반환됩니다. 소비 후 잔액은 RPlay 응답의 `RemainingCoins` 또는 StoryEngine 응답의 `RemainingCredits`에서 확인할 수 있습니다.

Editor와 데스크톱 빌드에서는 SDK가 소비 확인과 잔액 부족 안내를 표시합니다. WebGL에서는 게임 페이지의 팝업을 사용합니다.

## 응답 및 오류 처리

API 응답의 `Success`로 요청 성공 여부를 확인할 수 있습니다. 실패한 응답에서 `EnsureSuccess()`를 호출하면 `RPlayApiException`이 발생합니다.

모든 API 응답은 `RPlayResponse`를 상속하며 다음 공통 속성을 제공합니다.

| 속성 | 타입 | 설명 |
| --- | --- | --- |
| `Success` | `bool` | 요청이 성공했는지 나타냅니다. |
| `Status` | `string` | 요청의 처리 상태입니다. |
| `ErrorCode` | `string` | 실패 원인을 구분할 수 있는 오류 코드입니다. |
| `Message` | `string` | 서버가 반환한 오류 또는 안내 메시지입니다. |
| `HttpStatusCode` | `long` | HTTP 상태 코드입니다. |
| `RawJson` | `string` | 서버가 반환한 원본 JSON 문자열입니다. |
| `EnsureSuccess()` | `void` | `Success`가 `false`이면 `RPlayApiException`을 발생시킵니다. |

```csharp
try
{
    var user = await RPlayGames.GetUserInfoAsync();
    user.EnsureSuccess();
}
catch (RPlayApiException exception)
{
    Debug.LogError($"RPlay Games API 오류: {exception.ErrorCode}");
}
```

응답을 직접 처리하려면 `Success`, `Status`, `ErrorCode`, `Message`를 사용하세요. `RPlayApiException`에서는 `ErrorCode`, `HttpStatusCode`, `ResponseBody`로 실패 정보를 확인할 수 있습니다.

## 라이선스

Copyright (c) 2026 PLAX INC. 이 SDK는 [MIT License](LICENSE.md)로 제공됩니다.
