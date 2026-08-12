# RPlay Games Unity SDK

RPlay Games와 StoryEngine의 게임 API를 Unity에서 일관된 C# API로 사용하는 UPM 패키지입니다.

## 지원 환경

- Unity 2022.3 LTS 이상
- WebGL
- Windows, macOS, Linux 데스크톱 빌드
- Unity Editor
- RPlay 및 StoryEngine 운영 환경

이 SDK에는 Sandbox나 공개 Mock 환경이 없습니다. API 호출로 생성된 저장 데이터, 리더보드, 코인 및 크레딧 소비는 운영 데이터에 반영됩니다.

## 설치

개발 중인 현재 프로젝트에서는 `Packages/com.rplay.games-sdk` 임베디드 패키지로 포함되어 있습니다. Git 배포 후에는 Unity Package Manager의 Git URL 설치 방식을 사용합니다.

```text
https://github.com/r-play/rplay-games-unity-sdk.git#v0.1.2
```

## 초기 설정

1. Project 창에서 `Create > RPlay > Games Settings`를 선택합니다.
2. RPlay 게임 관리 화면에서 확인한 `GameOid`를 입력합니다.
3. 게임 시작 코드에서 설정 에셋으로 SDK를 초기화합니다.

```csharp
using RPlay.Games;
using UnityEngine;

public sealed class GameBootstrap : MonoBehaviour
{
    [SerializeField] private RPlayGamesSettings settings;

    private async void Start()
    {
        await RPlayGames.InitializeAsync(settings);
        await RPlayGames.LoginAsync();
    }
}
```

WebGL에서는 게시 과정에서 자동 주입된 `window.RplayGameSDK`의 게임 토큰을 사용합니다. 별도의 로그인 창이나 Unity 결제 확인 UI를 만들지 않고 현재 RPlay 게임 페이지의 인증 및 팝업 흐름을 그대로 사용합니다.

Editor와 Standalone에서는 RPlay Games 연결 화면이 열립니다. 사용자가 RPlay 또는 StoryEngine 로그인을 선택하면 해당 플랫폼에서 PKCE 로그인을 진행하고, 게임 실행 세션 동안만 토큰을 메모리에 보관합니다. 웹사이트 전체 권한을 가진 로그인 토큰은 게임에 저장하지 않습니다.

게임 연결 JWT 자체에는 기존 WebGL 게임 JWT와 동일하게 만료 시각을 넣지 않습니다. 결제 승인용 `connectAccessToken`은 별도로 짧게 유지되며, 로그아웃하거나 서버의 실행 세션이 종료되면 더 이상 사용할 수 없습니다.

## 사용자 정보

```csharp
var access = await RPlayGames.VerifyUserAsync();
if (!access.Success)
{
    Debug.LogWarning($"플레이 권한 확인 실패: {access.ErrorCode}");
    return;
}

var user = await RPlayGames.GetUserInfoAsync();
user.EnsureSuccess();
Debug.Log($"{user.Nickname}: {user.CoinBalance}");
```

HTTP 200 응답이어도 `Success`가 `false`일 수 있으므로 항상 응답 본문을 확인해야 합니다. `EnsureSuccess()`를 호출하면 실패 응답을 `RPlayApiException`으로 변환할 수 있습니다.

## 저장 데이터

```csharp
await RPlayGames.SetDataAsync("chapter", 3);
await RPlayGames.SetDataAsync(new { chapter = 3, hp = 80 });

var save = await RPlayGames.LoadDataAsync<PlayerSave>();
save.EnsureSuccess();
var playerSave = save.Data;

await RPlayGames.DeleteDataAsync("chapter");
await RPlayGames.DeleteAllDataAsync();
```

벌크 저장은 기존 데이터와 얕게 병합됩니다. SDK는 같은 실행 세션의 저장 변경 요청을 직렬화하지만, 서로 다른 기기에서 동시에 저장한 값의 충돌까지 해결하지는 않습니다. `_id`는 서버 보호 키이며 SDK에서 수정할 수 없습니다. `DeleteAllDataAsync`는 저장 데이터만 비우고 리더보드 기록은 보존합니다.

## 리더보드

```csharp
await RPlayGames.SetScoreAsync(1200);
await RPlayGames.IncrementScoreAsync(50);

var mine = await RPlayGames.GetMyRankAsync();
var top = await RPlayGames.GetTopRanksAsync(limit: 20);
var around = await RPlayGames.GetRanksAroundMeAsync(range: 5);
```

## 코인 및 크레딧 소비

```csharp
var result = await RPlayGames.ConsumeAsync(
    amount: 10,
    itemName: "부활권",
    options: new RPlayConsumeOptions
    {
        ItemDescription = "현재 위치에서 이어서 플레이합니다.",
        SkipConfirmation = false,
        Metadata = new { stage = 7 }
    }
);
```

- 기본값에서는 Editor와 Standalone에 RPlay 공용 소비 확인 UI가 표시되며, 잔액이 부족하면 코인 또는 크레딧 부족 안내가 표시됩니다.
- `SkipConfirmation = true`이면 SDK의 소비 관련 팝업이 전혀 표시되지 않으며 운영 코인 또는 크레딧이 즉시 차감될 수 있습니다.
- StoryEngine의 `amount` 단위는 게임 코인이며 실제 크레딧 소비량은 현재 서버 계약에 따라 `amount × 14`입니다.
- WebGL에서는 기존 웹 팝업이 소비 확인을 처리합니다.

`RequestChargeAsync()`는 WebGL에서 기존 게임 페이지의 충전 팝업을 열고, Editor와 Standalone에서는 로그인할 때 선택한 플랫폼의 웹 충전 화면을 시스템 브라우저로 엽니다.

## 오류 처리

네트워크 실패, 잘못된 JSON 등 응답 자체를 처리할 수 없는 경우 `RPlayApiException`이 발생합니다. 서버가 정상적으로 반환한 비즈니스 실패는 응답의 `Success`, `Status`, `ErrorCode`, `Message`로 확인합니다.

## 라이선스

Copyright (c) 2026 PLAX INC. 이 SDK는 [MIT License](LICENSE.md)로 제공됩니다.
