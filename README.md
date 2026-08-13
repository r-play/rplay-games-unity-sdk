# RPlay Games Unity SDK

RPlay Games와 StoryEngine의 로그인, 게임 데이터, 리더보드 및 재화 API를 Unity에서 사용할 수 있는 SDK입니다.

## 요구 사항

- Unity 2022.3 LTS 이상
- WebGL
- Windows, macOS, Linux 데스크톱 빌드

## 설치

1. Unity 메뉴에서 `Window > Package Manager`를 엽니다.
2. 왼쪽 위의 `+` 버튼을 누르고 `Install package from git URL...`을 선택합니다.
3. 아래 URL을 입력하고 `Install`을 누릅니다.

```text
https://github.com/r-play/rplay-games-unity-sdk.git#v0.1.2
```

## 샘플 실행

전체 API를 바로 확인하려면 패키지에 포함된 API Playground를 사용하세요.

1. Package Manager에서 `RPlay Games SDK`를 선택합니다.
2. `Samples`의 `API Playground`에서 `Import`를 누릅니다.
3. Project 창에서 `Samples/RPlay Games SDK/0.1.2/API Playground` 폴더를 엽니다.
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

## 주요 API

| 기능 | API |
| --- | --- |
| 초기화 및 로그인 | `InitializeAsync`, `LoginAsync`, `LogoutAsync` |
| 사용자 | `VerifyUserAsync`, `GetUserInfoAsync` |
| 충전 및 소비 | `RequestChargeAsync`, `ConsumeAsync` |
| 게임 데이터 | `LoadDataAsync`, `SetDataAsync`, `DeleteDataAsync`, `DeleteAllDataAsync` |
| 리더보드 | `SetScoreAsync`, `IncrementScoreAsync`, `GetMyRankAsync`, `GetTopRanksAsync`, `GetRanksAroundMeAsync` |

게임 API를 호출하기 전에 SDK 초기화와 인증이 완료되어야 합니다. 현재 상태는 `RPlayGames.IsInitialized`와 `RPlayGames.IsAuthenticated`로 확인할 수 있습니다.

## 사용자 정보

```csharp
var user = await RPlayGames.GetUserInfoAsync();
user.EnsureSuccess();

Debug.Log($"닉네임: {user.Nickname}");
```

RPlay 계정의 잔액은 `CoinBalance`, StoryEngine 계정의 잔액은 `CreditBalance`에서 확인할 수 있습니다. 현재 연결된 플랫폼은 `RPlayGames.ConnectedPlatform`으로 확인하세요.

게임의 플레이 권한만 확인하려면 `VerifyUserAsync()`를 사용합니다.

```csharp
var access = await RPlayGames.VerifyUserAsync();
if (!access.Success)
{
    Debug.LogWarning($"플레이 권한 확인 실패: {access.ErrorCode}");
}
```

## 게임 데이터

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

`SetDataAsync(object)`는 전달한 필드를 기존 데이터에 추가하거나 덮어씁니다. `DeleteAllDataAsync()`는 게임 데이터만 삭제하며 리더보드 기록은 유지합니다.

## 리더보드

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

## 재화 충전 및 소비

충전 화면을 열려면 다음 API를 호출합니다.

```csharp
var charge = await RPlayGames.RequestChargeAsync();
charge.EnsureSuccess();
```

재화를 소비할 때는 금액과 표시할 아이템 이름을 전달합니다. `amount`는 RPlay와 StoryEngine 모두 게임 코인 단위입니다.

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

Editor와 데스크톱 빌드에서는 SDK가 소비 확인과 잔액 부족 안내를 표시합니다. WebGL에서는 게임 페이지의 팝업을 사용합니다.

`SkipConfirmation = true`로 설정하면 SDK의 소비 관련 팝업을 모두 생략합니다.

## 응답 및 오류 처리

API 응답의 `Success`로 요청 성공 여부를 확인할 수 있습니다. 실패한 응답에서 `EnsureSuccess()`를 호출하면 `RPlayApiException`이 발생합니다.

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

응답을 직접 처리하려면 `Success`, `Status`, `ErrorCode`, `Message`를 사용하세요.

## 라이선스

Copyright (c) 2026 PLAX INC. 이 SDK는 [MIT License](LICENSE.md)로 제공됩니다.
