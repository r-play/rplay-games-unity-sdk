using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using RPlay.Games;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RPlay.Games.Samples
{
    public sealed class RPlayGamesApiPlayground : MonoBehaviour
    {
        private const string ContentPath = "Background/ControlsScroll/Viewport/Content/";

        private enum SampleLanguage
        {
            Korean,
            English,
            Japanese,
            Spanish,
            TraditionalChinese,
            SimplifiedChinese,
        }

        private enum SampleText
        {
            Header,
            Warning,
            SessionConnected,
            UserTitle,
            DataTitle,
            LeaderboardTitle,
            CurrencyTitle,
            LoginSubtitle,
            Login,
            Logout,
            VerifyUser,
            UserInfo,
            DataKey,
            DataValue,
            SaveBulk,
            LoadAll,
            SaveKey,
            DeleteKey,
            DeleteAll,
            SetScoreLabel,
            IncrementScoreLabel,
            TopLimitLabel,
            AroundRangeLabel,
            SetScore,
            IncrementScore,
            MyRank,
            TopRanks,
            AroundRanks,
            ConsumeAmount,
            ItemName,
            ItemDescription,
            SkipConfirmation,
            OpenCharge,
            Consume,
            LogTitle,
            EmptyLog,
            ClearLog,
            Status,
            Ready,
            Processing,
            Initialized,
            LoggedIn,
            Yes,
            No,
        }

        // 샘플 화면은 RPlay 웹에서 지원하는 여섯 언어를 같은 순서로 제공한다.
        private static readonly Dictionary<SampleText, string[]> LocalizedTexts =
            new Dictionary<SampleText, string[]>
            {
                { SampleText.Header, new[] { "RPlay Games Unity SDK API 샘플", "RPlay Games Unity SDK API Sample", "RPlay Games Unity SDK API サンプル", "Ejemplo de API de RPlay Games Unity SDK", "RPlay Games Unity SDK API 範例", "RPlay Games Unity SDK API 示例" } },
                { SampleText.Warning, new[] { "Production API를 사용합니다. 저장 데이터, 리더보드와 재화 요청이 실제 계정에 반영됩니다.", "This sample uses the Production API. Saved data, leaderboard scores, and currency requests affect the current account.", "Production APIを使用します。保存データ、ランキング、通貨リクエストは実際のアカウントに反映されます。", "Esta muestra usa la API de producción. Los datos, la clasificación y las solicitudes de moneda afectan a la cuenta real.", "此範例使用 Production API。儲存資料、排行榜與貨幣請求會套用至實際帳號。", "此示例使用 Production API。存档数据、排行榜与货币请求会应用到实际账号。" } },
                { SampleText.SessionConnected, new[] { "1. 로그인 및 세션 ({0} 연결됨)", "1. Login and session ({0} connected)", "1. ログインとセッション（{0} 接続済み）", "1. Inicio de sesión y sesión ({0} conectado)", "1. 登入與工作階段（已連接 {0}）", "1. 登录与会话（已连接 {0}）" } },
                { SampleText.UserTitle, new[] { "2. 사용자 API (검증 / 정보 조회)", "2. User API (verification / profile)", "2. ユーザー API（検証 / 情報取得）", "2. API de usuario (verificación / información)", "2. 使用者 API（驗證 / 資訊查詢）", "2. 用户 API（验证 / 信息查询）" } },
                { SampleText.DataTitle, new[] { "3. 저장 데이터 API (저장 / 조회 / 삭제)", "3. Saved data API (save / load / delete)", "3. 保存データ API（保存 / 取得 / 削除）", "3. API de datos guardados (guardar / cargar / eliminar)", "3. 儲存資料 API（儲存 / 查詢 / 刪除）", "3. 存档数据 API（保存 / 查询 / 删除）" } },
                { SampleText.LeaderboardTitle, new[] { "4. 리더보드 API (점수 / 순위)", "4. Leaderboard API (scores / ranks)", "4. ランキング API（スコア / 順位）", "4. API de clasificación (puntuaciones / puestos)", "4. 排行榜 API（分數 / 名次）", "4. 排行榜 API（分数 / 名次）" } },
                { SampleText.CurrencyTitle, new[] { "5. 재화 API (충전 / 소비)", "5. Currency API (charge / consume)", "5. 通貨 API（チャージ / 消費）", "5. API de moneda (recarga / consumo)", "5. 貨幣 API（儲值 / 消耗）", "5. 货币 API（充值 / 消耗）" } },
                { SampleText.LoginSubtitle, new[] { "Unity SDK API 테스트", "Unity SDK API Playground", "Unity SDK API テスト", "Pruebas de API con Unity SDK", "Unity SDK API 測試", "Unity SDK API 测试" } },
                { SampleText.Login, new[] { "브라우저로 로그인", "Log in with browser", "ブラウザーでログイン", "Iniciar sesión en el navegador", "在瀏覽器中登入", "在浏览器中登录" } },
                { SampleText.Logout, new[] { "로그아웃", "Log out", "ログアウト", "Cerrar sesión", "登出", "退出登录" } },
                { SampleText.VerifyUser, new[] { "사용자 검증", "Verify user", "ユーザー検証", "Verificar usuario", "驗證使用者", "验证用户" } },
                { SampleText.UserInfo, new[] { "사용자 정보", "User info", "ユーザー情報", "Información del usuario", "使用者資訊", "用户信息" } },
                { SampleText.DataKey, new[] { "키", "Key", "キー", "Clave", "鍵", "键" } },
                { SampleText.DataValue, new[] { "값", "Value", "値", "Valor", "值", "值" } },
                { SampleText.SaveBulk, new[] { "여러 값 저장", "Save multiple values", "複数の値を保存", "Guardar varios valores", "儲存多個值", "保存多个值" } },
                { SampleText.LoadAll, new[] { "전체 불러오기", "Load all", "すべて読み込む", "Cargar todo", "全部載入", "全部加载" } },
                { SampleText.SaveKey, new[] { "단일 키 저장", "Save one key", "単一キーを保存", "Guardar una clave", "儲存單一鍵", "保存单个键" } },
                { SampleText.DeleteKey, new[] { "입력한 키 삭제", "Delete entered key", "入力したキーを削除", "Eliminar la clave escrita", "刪除輸入的鍵", "删除输入的键" } },
                { SampleText.DeleteAll, new[] { "저장 데이터 전체 삭제", "Delete all saved data", "保存データをすべて削除", "Eliminar todos los datos guardados", "刪除所有儲存資料", "删除所有存档数据" } },
                { SampleText.SetScoreLabel, new[] { "설정할 점수", "Score to set", "設定するスコア", "Puntuación a establecer", "要設定的分數", "要设置的分数" } },
                { SampleText.IncrementScoreLabel, new[] { "증가할 점수", "Score to add", "加算するスコア", "Puntuación a sumar", "要增加的分數", "要增加的分数" } },
                { SampleText.TopLimitLabel, new[] { "상위 순위 개수", "Top rank count", "上位件数", "Cantidad de puestos superiores", "前幾名數量", "前几名数量" } },
                { SampleText.AroundRangeLabel, new[] { "내 주변 범위", "Range around me", "自分の周辺範囲", "Rango a mi alrededor", "我的周邊範圍", "我的周边范围" } },
                { SampleText.SetScore, new[] { "점수 설정", "Set score", "スコア設定", "Establecer puntuación", "設定分數", "设置分数" } },
                { SampleText.IncrementScore, new[] { "점수 증가", "Add score", "スコア加算", "Sumar puntuación", "增加分數", "增加分数" } },
                { SampleText.MyRank, new[] { "내 순위", "My rank", "自分の順位", "Mi puesto", "我的名次", "我的名次" } },
                { SampleText.TopRanks, new[] { "상위 순위", "Top ranks", "上位ランキング", "Mejores puestos", "排行榜前段", "排行榜前列" } },
                { SampleText.AroundRanks, new[] { "내 주변 순위", "Ranks around me", "自分の周辺順位", "Puestos a mi alrededor", "我的周邊名次", "我的周边名次" } },
                { SampleText.ConsumeAmount, new[] { "소비 금액", "Amount to consume", "消費額", "Cantidad a consumir", "消耗金額", "消耗金额" } },
                { SampleText.ItemName, new[] { "아이템 이름", "Item name", "アイテム名", "Nombre del objeto", "道具名稱", "道具名称" } },
                { SampleText.ItemDescription, new[] { "아이템 설명", "Item description", "アイテム説明", "Descripción del objeto", "道具說明", "道具说明" } },
                { SampleText.SkipConfirmation, new[] { "SDK 소비 팝업 생략", "Skip SDK consume popups", "SDK の消費ポップアップを省略", "Omitir ventanas de consumo del SDK", "略過 SDK 消耗彈窗", "跳过 SDK 消耗弹窗" } },
                { SampleText.OpenCharge, new[] { "충전 화면 열기", "Open charge screen", "チャージ画面を開く", "Abrir pantalla de recarga", "開啟儲值畫面", "打开充值页面" } },
                { SampleText.Consume, new[] { "재화 소비", "Consume currency", "通貨を消費", "Consumir moneda", "消耗貨幣", "消耗货币" } },
                { SampleText.LogTitle, new[] { "API 응답 로그", "API response log", "API レスポンスログ", "Registro de respuestas de API", "API 回應紀錄", "API 响应日志" } },
                { SampleText.EmptyLog, new[] { "아직 호출 결과가 없습니다.", "No API responses yet.", "API の呼び出し結果はまだありません。", "Aún no hay respuestas de la API.", "尚無 API 呼叫結果。", "暂无 API 调用结果。" } },
                { SampleText.ClearLog, new[] { "로그 지우기", "Clear log", "ログを消去", "Borrar registro", "清除紀錄", "清除日志" } },
                { SampleText.Status, new[] { "상태", "Status", "状態", "Estado", "狀態", "状态" } },
                { SampleText.Ready, new[] { "대기 중", "Ready", "待機中", "Listo", "待命中", "就绪" } },
                { SampleText.Processing, new[] { "처리 중", "Processing", "処理中", "Procesando", "處理中", "处理中" } },
                { SampleText.Initialized, new[] { "초기화", "Initialized", "初期化", "Inicializado", "已初始化", "已初始化" } },
                { SampleText.LoggedIn, new[] { "로그인", "Logged in", "ログイン", "Sesión iniciada", "已登入", "已登录" } },
                { SampleText.Yes, new[] { "예", "Yes", "はい", "Sí", "是", "是" } },
                { SampleText.No, new[] { "아니요", "No", "いいえ", "No", "否", "否" } },
            };

        // 이 샘플의 설정 에셋에는 RPlay 테스트 게임 OID가 들어 있습니다.
        // 실제 게임에 SDK를 적용할 때에는 반드시 개발자가 만든 게임의 OID로 교체하세요.
        [Header("RPlay Games SDK 설정")]
        [SerializeField]
        private RPlayGamesSettings settings;

        // API 요청 중에는 중복 호출을 막되, 로그인 버튼은 브라우저를 다시 열 수 있도록 유지한다.
        private readonly List<Button> actionButtons = new List<Button>();
        private readonly List<string> logs = new List<string>();

        // 상단 실행 상태와 오른쪽 응답 로그 영역이다.
        private Text statusText;
        private Text logText;
        private ScrollRect logScroll;
        private ScrollRect controlsScroll;
        private GameObject controlsScrollHint;
        private Sprite controlsScrollHintSprite;
        private Sprite loginCardSprite;
        private Dropdown languageDropdown;
        private SampleLanguage currentLanguage;
        private bool isAuthenticatedView;
        private string currentStatusAction = "대기 중";

        // 각 API 예제의 요청값을 입력받는 필드다.
        private InputField dataKeyInput;
        private InputField dataValueInput;
        private InputField scoreInput;
        private InputField incrementScoreInput;
        private InputField topLimitInput;
        private InputField aroundRangeInput;
        private InputField consumeAmountInput;
        private InputField itemNameInput;
        private InputField itemDescriptionInput;
        private Toggle skipConfirmationToggle;
        private Text skipConfirmationCheckmark;

        // 로그인 여부에 따라 세션 버튼, API 영역과 로그 패널의 노출을 전환한다.
        private Text sessionTitle;
        private Text sessionSubtitle;
        private Button loginButton;
        private Button logoutButton;
        private GameObject[] apiSections;
        private RectTransform controlsPanel;
        private Image backgroundImage;
        private Image controlsPanelImage;
        private Shadow controlsPanelShadow;
        private GameObject logPanel;

        // 샘플 씬을 열고 Play하면 저장된 UI 계층을 찾아 레이아웃과 모든 버튼 이벤트를 연결한다.
        private void Awake()
        {
            EnsureEventSystemInputModule();

            // 편집 중에는 Canvas를 숨겨 Game 뷰를 비워 두고, 실행할 때만 샘플 UI를 표시한다.
            GetComponent<Canvas>().enabled = true;
            ConfigureLayout();

            // 기본 Unity 폰트는 한글 글리프가 빠질 수 있어 운영체제의 한글 폰트를 우선 사용한다.
            var defaultFont = Font.CreateDynamicFontFromOSFont(
                new[]
                {
                    "Malgun Gothic",
                    "Apple SD Gothic Neo",
                    "Noto Sans CJK KR",
                    "Arial Unicode MS",
                    "Arial",
                },
                16
            );
            if (defaultFont == null)
            {
                defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }
            foreach (var label in GetComponentsInChildren<Text>(true))
            {
                if (defaultFont != null)
                {
                    label.font = defaultFont;
                }
            }
            ConfigureTheme();

            statusText = FindRequired<Text>("Background/Status");
            logText = FindRequired<Text>("Background/LogPanel/LogScroll/Viewport/Content/LogText");
            logScroll = FindRequired<ScrollRect>("Background/LogPanel/LogScroll");
            controlsScroll = FindRequired<ScrollRect>("Background/ControlsScroll");
            controlsPanel = FindRequired<RectTransform>("Background/ControlsScroll");
            backgroundImage = FindRequired<Image>("Background");
            controlsPanelImage = FindRequired<Image>("Background/ControlsScroll");
            logPanel = FindRequired<RectTransform>("Background/LogPanel").gameObject;
            ConfigureLoginCard();
            ConfigureLanguageDropdown(defaultFont);
            ConfigureScrollRect(
                controlsScroll,
                "Background/ControlsScroll/Viewport"
            );
            ConfigureScrollRect(
                logScroll,
                "Background/LogPanel/LogScroll/Viewport"
            );

            dataKeyInput = ConfigureInputField(
                ContentPath + "DataSection/DataKeyRow/Input",
                "sample_message"
            );
            dataValueInput = ConfigureInputField(
                ContentPath + "DataSection/DataValueRow/Input",
                "안녕하세요, RPlay!"
            );
            scoreInput = ConfigureInputField(
                ContentPath + "LeaderboardSection/ScoreRow/Input",
                "1000"
            );
            incrementScoreInput = ConfigureInputField(
                ContentPath + "LeaderboardSection/IncrementRow/Input",
                "100"
            );
            topLimitInput = ConfigureInputField(
                ContentPath + "LeaderboardSection/TopLimitRow/Input",
                "10"
            );
            aroundRangeInput = ConfigureInputField(
                ContentPath + "LeaderboardSection/AroundRangeRow/Input",
                "5"
            );
            // 충전 API는 금액을 지정하지 않고 플랫폼의 충전 화면만 열기 때문에 금액 입력은 표시하지 않는다.
            FindRequired<RectTransform>(
                ContentPath + "CurrencySection/ChargeAmountRow"
            ).gameObject.SetActive(false);
            consumeAmountInput = ConfigureInputField(
                ContentPath + "CurrencySection/ConsumeAmountRow/Input",
                "10"
            );
            itemNameInput = ConfigureInputField(
                ContentPath + "CurrencySection/ItemNameRow/Input",
                "샘플 부활권"
            );
            itemDescriptionInput = ConfigureInputField(
                ContentPath + "CurrencySection/ItemDescriptionRow/Input",
                "API 샘플에서 요청한 테스트 아이템입니다."
            );

            skipConfirmationToggle = FindRequired<Toggle>(
                ContentPath + "CurrencySection/SkipConfirmRow/SkipConfirmToggle"
            );
            if (!skipConfirmationToggle.TryGetComponent<Image>(out var toggleBackground))
            {
                throw new InvalidOperationException("소비 확인 토글의 배경 이미지가 없습니다.");
            }
            skipConfirmationToggle.targetGraphic = toggleBackground;
            skipConfirmationToggle.navigation = new Navigation
            {
                mode = Navigation.Mode.None,
            };
            toggleBackground.sprite = loginCardSprite;
            toggleBackground.type = Image.Type.Sliced;
            var toggleOutline = skipConfirmationToggle.gameObject.AddComponent<Outline>();
            toggleOutline.effectColor = new Color32(32, 37, 41, 48);
            toggleOutline.effectDistance = new Vector2(1f, -1f);
            toggleOutline.useGraphicAlpha = true;

            // 씬에 잘못 저장된 큰 Image 대신 중앙의 체크 문자만 켜고 끈다.
            // 이 보정이 없으면 토글을 클릭할 때 화면 위에 파란 사각형이 나타난다.
            var checkmarkObject = FindRequired<RectTransform>(
                ContentPath + "CurrencySection/SkipConfirmRow/SkipConfirmToggle/Checkmark"
            );
            Anchor(
                checkmarkObject,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                Vector2.zero,
                new Vector2(20f, 20f)
            );
            var serializedCheckmark = checkmarkObject.GetComponent<Image>();
            serializedCheckmark.enabled = false;
            serializedCheckmark.raycastTarget = false;
            var checkmarkLabelObject = new GameObject(
                "CheckmarkLabel",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Text)
            );
            checkmarkLabelObject.transform.SetParent(checkmarkObject, false);
            Stretch(
                (RectTransform)checkmarkLabelObject.transform,
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                Vector2.zero
            );
            skipConfirmationCheckmark = checkmarkLabelObject.GetComponent<Text>();
            skipConfirmationCheckmark.font = defaultFont;
            skipConfirmationCheckmark.fontSize = 18;
            skipConfirmationCheckmark.fontStyle = FontStyle.Bold;
            skipConfirmationCheckmark.alignment = TextAnchor.MiddleCenter;
            skipConfirmationCheckmark.color = RPlayGames.ConnectedPlatform == RPlayPlatform.StoryEngine
                ? new Color32(99, 46, 255, 255)
                : new Color32(37, 150, 190, 255);
            skipConfirmationCheckmark.raycastTarget = false;
            skipConfirmationCheckmark.text = "✓";
            skipConfirmationToggle.graphic = skipConfirmationCheckmark;

            // 샘플에서는 초기화와 로그인을 한 번에 처리하므로, 씬에 남아 있는 분리형 버튼은 숨긴다.
            FindRequired<Button>(
                ContentPath + "SessionSection/Buttons/InitializeButton"
            ).gameObject.SetActive(false);
            FindRequired<Button>(
                ContentPath + "SessionSection/Buttons/LoginButton"
            ).gameObject.SetActive(false);

            sessionTitle = FindRequired<Text>(ContentPath + "SessionSection/Title");
            // 긴 영문 제목 한 줄 대신 브랜드명과 샘플 용도를 나눠 로그인 카드의 위계를 만든다.
            var subtitleObject = new GameObject(
                "Subtitle",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Text),
                typeof(LayoutElement)
            );
            subtitleObject.transform.SetParent(sessionTitle.transform.parent, false);
            subtitleObject.transform.SetSiblingIndex(sessionTitle.transform.GetSiblingIndex() + 1);
            sessionSubtitle = subtitleObject.GetComponent<Text>();
            sessionSubtitle.font = defaultFont;
            sessionSubtitle.fontSize = 14;
            sessionSubtitle.alignment = TextAnchor.MiddleCenter;
            sessionSubtitle.color = new Color32(108, 117, 125, 255);
            sessionSubtitle.raycastTarget = false;
            var subtitleLayout = subtitleObject.GetComponent<LayoutElement>();
            subtitleLayout.minHeight = 20f;
            subtitleLayout.preferredHeight = 20f;
            sessionTitle.transform.parent.GetComponent<VerticalLayoutGroup>().spacing = 8f;
            ConfigureControlsScrollHint();
            loginButton = FindRequired<Button>(
                ContentPath + "SessionSection/Buttons/InitializeLoginButton"
            );
            logoutButton = FindRequired<Button>(
                ContentPath + "SessionSection/Buttons/LogoutButton"
            );
            ConfigureButton(loginButton);
            ConfigureButton(logoutButton);
            if (loginButton.TryGetComponent<LayoutElement>(out var loginLayout))
            {
                loginLayout.minHeight = 52f;
                loginLayout.preferredHeight = 52f;
            }
            // SDK를 초기화한 뒤 시스템 브라우저에서 PKCE 로그인을 진행한다.
            // 성공한 경우에만 사용자·저장 데이터·리더보드·재화 API 버튼을 표시한다.
            loginButton.onClick.AddListener(
                () => RunActionAsync("로그인", LoginAndRevealAsync)
            );
            // 서버 실행 세션과 메모리 토큰을 정리한 뒤 샘플을 로그인 전 화면으로 되돌린다.
            logoutButton.onClick.AddListener(
                () => RunActionAsync("로그아웃", LogoutAndResetAsync)
            );

            apiSections = new[]
            {
                FindRequired<RectTransform>(ContentPath + "UserSection").gameObject,
                FindRequired<RectTransform>(ContentPath + "DataSection").gameObject,
                FindRequired<RectTransform>(ContentPath + "LeaderboardSection").gameObject,
                FindRequired<RectTransform>(ContentPath + "CurrencySection").gameObject,
            };

            // 현재 사용자가 이 게임을 실행할 수 있는지 공개 상태와 구매 권한을 확인한다.
            BindResponseButton<RPlayResponse>(
                "UserSection/Buttons/VerifyUserButton",
                "사용자 검증",
                () => RPlayGames.VerifyUserAsync()
            );
            // 로그인 사용자의 OID, 닉네임과 RPlay 코인 또는 StoryEngine 크레딧 잔액을 조회한다.
            BindResponseButton<RPlayUserInfo>(
                "UserSection/Buttons/UserInfoButton",
                "사용자 정보",
                () => RPlayGames.GetUserInfoAsync()
            );

            // 입력한 키 하나를 입력한 값으로 저장한다. 기존의 다른 키는 유지된다.
            BindResponseButton<RPlayDataWriteResult>(
                "DataSection/ButtonsPrimary/SaveKeyButton",
                "단일 키 저장",
                () => RPlayGames.SetDataAsync(dataKeyInput.text, dataValueInput.text)
            );
            // 여러 필드를 한 요청으로 저장하는 예제다. 샘플 값과 저장 시각을 함께 기록한다.
            BindResponseButton<RPlayDataWriteResult>(
                "DataSection/ButtonsPrimary/SaveBulkButton",
                "여러 값 저장",
                SaveBulkDataAsync
            );
            // 현재 사용자와 게임 조합에 저장된 data 객체 전체를 불러온다.
            BindResponseButton<RPlayGameData>(
                "DataSection/ButtonsPrimary/LoadDataButton",
                "전체 불러오기",
                () => RPlayGames.LoadDataAsync()
            );
            // 입력한 키만 저장 데이터에서 제거하며 나머지 키는 유지한다.
            BindResponseButton<RPlayDataWriteResult>(
                "DataSection/ButtonsDelete/DeleteKeyButton",
                "입력한 키 삭제",
                () => RPlayGames.DeleteDataAsync(dataKeyInput.text)
            );
            // 현재 사용자의 저장 데이터 전체를 삭제한다. 리더보드 점수는 삭제되지 않는다.
            BindResponseButton<RPlayResponse>(
                "DataSection/ButtonsDelete/DeleteAllButton",
                "저장 데이터 전체 삭제",
                () => RPlayGames.DeleteAllDataAsync()
            );

            // 입력한 값을 현재 사용자의 리더보드 점수로 덮어쓴다.
            BindResponseButton<RPlayLeaderboardUpdateResult>(
                "LeaderboardSection/ButtonsPrimary/SetScoreButton",
                "점수 설정",
                SetScoreAsync
            );
            // 기존 리더보드 점수에 입력한 값을 더한다. 서버에서 최종 점수의 하한은 0이다.
            BindResponseButton<RPlayLeaderboardUpdateResult>(
                "LeaderboardSection/ButtonsPrimary/IncrementScoreButton",
                "점수 증가",
                IncrementScoreAsync
            );
            // 현재 사용자의 점수와 전체 리더보드 순위를 조회한다.
            BindResponseButton<RPlayLeaderboardMeResult>(
                "LeaderboardSection/ButtonsPrimary/MyRankButton",
                "내 순위",
                () => RPlayGames.GetMyRankAsync()
            );
            // 점수가 높은 순서대로 입력한 개수만큼 조회한다. 샘플 입력 범위는 1~50이다.
            BindResponseButton<RPlayLeaderboardPage>(
                "LeaderboardSection/ButtonsSecondary/TopRanksButton",
                "상위 순위",
                GetTopRanksAsync
            );
            // 내 순위를 중심으로 위아래의 사용자를 조회한다. 샘플 입력 범위는 각 방향 0~50이다.
            BindResponseButton<RPlayLeaderboardAroundResult>(
                "LeaderboardSection/ButtonsSecondary/AroundRanksButton",
                "내 주변 순위",
                GetRanksAroundMeAsync
            );

            // 결제 금액을 지정하지 않고 로그인한 플랫폼의 충전 화면을 연다.
            BindResponseButton<RPlayResponse>(
                "CurrencySection/Buttons/RequestChargeButton",
                "충전 화면 열기",
                RequestChargeAsync
            );
            // 운영 계정의 코인 또는 크레딧을 실제로 소비한다.
            // SDK 소비 팝업 생략이 켜져 있으면 확인 및 잔액 부족 안내 없이 결과만 반환한다.
            BindResponseButton<RPlayConsumeResult>(
                "CurrencySection/Buttons/ConsumeButton",
                "재화 소비",
                ConsumeAsync
            );

            // API나 서버 데이터에는 영향을 주지 않고 화면에 쌓인 샘플 로그만 지운다.
            var clearButton = FindRequired<Button>("Background/LogPanel/ClearLogButton");
            ConfigureButton(clearButton);
            clearButton.onClick.AddListener(ClearLogs);
            SetAuthenticatedState(RPlayGames.IsAuthenticated);
            UpdateStatus("대기 중");
        }

        // 설정 에셋을 검증하고 게임 API 클라이언트를 준비한다.
        private async Task InitializeAsync()
        {
            if (settings == null)
            {
                throw new InvalidOperationException(
                    "Inspector에서 RPlayGamesSettings 에셋을 연결하세요."
                );
            }

            await RPlayGames.InitializeAsync(settings);
        }

        // 시스템 브라우저에서 PKCE 로그인을 진행하고 발급된 토큰을 실행 중인 메모리에 보관한다.
        private async Task LoginAsync()
        {
            if (!RPlayGames.IsInitialized)
            {
                throw new InvalidOperationException("먼저 SDK를 초기화하세요.");
            }

            await RPlayGames.LoginAsync();
        }

        // 처음 누르는 로그인 버튼 하나로 초기화와 로그인을 반드시 이 순서대로 실행한다.
        private async Task InitializeAndLoginAsync()
        {
            await InitializeAsync();
            await LoginAsync();
        }

        // 로그인까지 성공한 경우에만 운영 API 테스트 영역을 화면에 표시한다.
        private async Task LoginAndRevealAsync()
        {
            await InitializeAndLoginAsync();
            ConfigureTheme();
            SetAuthenticatedState(true);
        }

        // 서버 세션과 로컬 토큰을 정리한 후 로그인 버튼만 보이는 초기 화면으로 복귀한다.
        private async Task LogoutAndResetAsync()
        {
            await RPlayGames.LogoutAsync();
            ConfigureTheme();
            SetAuthenticatedState(false);
        }

        // 객체 하나를 전달해 여러 저장 필드를 한 요청으로 갱신하는 사용 예제다.
        private Task<RPlayDataWriteResult> SaveBulkDataAsync()
        {
            return RPlayGames.SetDataAsync(
                new
                {
                    sample_message = dataValueInput.text,
                    sample_saved_at = DateTimeOffset.UtcNow.ToString("O"),
                }
            );
        }

        // 문자열 입력을 유한한 숫자로 검증한 뒤 절대 점수 설정 API를 호출한다.
        private Task<RPlayLeaderboardUpdateResult> SetScoreAsync()
        {
            return RPlayGames.SetScoreAsync(ParseDouble(scoreInput.text, "설정할 점수"));
        }

        // 문자열 입력을 유한한 숫자로 검증한 뒤 기존 점수에 더하는 API를 호출한다.
        private Task<RPlayLeaderboardUpdateResult> IncrementScoreAsync()
        {
            return RPlayGames.IncrementScoreAsync(
                ParseDouble(incrementScoreInput.text, "증가할 점수")
            );
        }

        // 서버 최대치에 맞춰 1~50 범위로 검증한 개수만큼 상위 순위를 요청한다.
        private Task<RPlayLeaderboardPage> GetTopRanksAsync()
        {
            return RPlayGames.GetTopRanksAsync(
                ParseInt(topLimitInput.text, "상위 순위 개수", 1, 50)
            );
        }

        // 내 순위를 중심으로 각 방향에서 조회할 사용자 수를 0~50 범위로 제한한다.
        private Task<RPlayLeaderboardAroundResult> GetRanksAroundMeAsync()
        {
            return RPlayGames.GetRanksAroundMeAsync(
                ParseInt(aroundRangeInput.text, "내 주변 범위", 0, 50)
            );
        }

        // 충전 상품과 결제 금액은 게임이 아니라 플랫폼의 충전 화면에서 사용자가 선택한다.
        private Task<RPlayResponse> RequestChargeAsync()
        {
            return RPlayGames.RequestChargeAsync();
        }

        // 아이템 정보와 샘플 메타데이터를 포함해 운영 재화 소비 요청을 보낸다.
        // SkipConfirmation은 소비 확인과 잔액 부족 안내를 포함한 SDK 팝업을 모두 생략한다.
        private Task<RPlayConsumeResult> ConsumeAsync()
        {
            return RPlayGames.ConsumeAsync(
                ParsePositiveDouble(consumeAmountInput.text, "소비 금액"),
                itemNameInput.text,
                new RPlayConsumeOptions
                {
                    ItemDescription = itemDescriptionInput.text,
                    SkipConfirmation = skipConfirmationToggle.isOn,
                    Metadata = new
                    {
                        source = "unity_sdk_api_playground",
                        requestedAt = DateTimeOffset.UtcNow.ToString("O"),
                    },
                }
            );
        }

        // 샘플이 특정 입력 패키지를 강제하지 않도록 현재 프로젝트에 맞는 UI 입력 모듈을 실행 시 연결한다.
        private static void EnsureEventSystemInputModule()
        {
            var eventSystem = EventSystem.current
                ?? UnityEngine.Object.FindObjectOfType<EventSystem>(true);
            if (eventSystem == null)
            {
                var eventSystemObject = new GameObject("EventSystem", typeof(EventSystem));
                eventSystem = eventSystemObject.GetComponent<EventSystem>();
            }

            if (eventSystem.GetComponent<BaseInputModule>() != null)
            {
                return;
            }

#if ENABLE_INPUT_SYSTEM
            var inputSystemModule = Type.GetType(
                "UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem"
            );
            if (inputSystemModule != null)
            {
                eventSystem.gameObject.AddComponent(inputSystemModule);
                return;
            }
#endif

            eventSystem.gameObject.AddComponent<StandaloneInputModule>();
        }

        // 씬의 InputField 자식 참조를 연결하고 API를 바로 시험할 수 있는 기본값을 채운다.
        private InputField ConfigureInputField(string path, string initialValue)
        {
            var input = FindRequired<InputField>(path);
            if (!input.TryGetComponent<Image>(out var inputBackground))
            {
                throw new InvalidOperationException("입력 필드의 배경 이미지가 없습니다: " + path);
            }
            input.targetGraphic = inputBackground;
            input.textComponent = FindRequired<Text>(path + "/Text");
            input.placeholder = FindRequired<Graphic>(path + "/Placeholder");
            Stretch(
                input.textComponent.rectTransform,
                Vector2.zero,
                Vector2.one,
                new Vector2(12f, 6f),
                new Vector2(-12f, -6f)
            );
            Stretch(
                input.placeholder.rectTransform,
                Vector2.zero,
                Vector2.one,
                new Vector2(12f, 6f),
                new Vector2(-12f, -6f)
            );
            input.text = initialValue;
            return input;
        }

        // 해상도와 Unity 버전에 상관없이 동일하게 보이도록 핵심 RectTransform을 런타임에 확정한다.
        private void ConfigureLayout()
        {
            Stretch(
                FindRequired<RectTransform>("Background"),
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                Vector2.zero
            );

            Anchor(
                FindRequired<RectTransform>("Background/Header"),
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -20f),
                new Vector2(-64f, 46f)
            );
            Anchor(
                FindRequired<RectTransform>("Background/Warning"),
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -70f),
                new Vector2(-64f, 42f)
            );
            Anchor(
                FindRequired<RectTransform>("Background/Status"),
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -116f),
                new Vector2(-64f, 34f)
            );

            Stretch(
                FindRequired<RectTransform>("Background/ControlsScroll"),
                new Vector2(0f, 0f),
                new Vector2(0.62f, 1f),
                new Vector2(24f, 24f),
                new Vector2(-12f, -164f)
            );
            Stretch(
                FindRequired<RectTransform>("Background/LogPanel"),
                new Vector2(0.62f, 0f),
                new Vector2(1f, 1f),
                new Vector2(12f, 24f),
                new Vector2(-24f, -164f)
            );

            Stretch(
                FindRequired<RectTransform>("Background/ControlsScroll/Viewport"),
                Vector2.zero,
                Vector2.one,
                new Vector2(8f, 10f),
                new Vector2(-8f, -10f)
            );
            Anchor(
                FindRequired<RectTransform>(
                    "Background/ControlsScroll/Viewport/Content"
                ),
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(0.5f, 1f),
                Vector2.zero,
                Vector2.zero
            );

            // 섹션의 중복 가로 패딩을 제거해 상단 제목과 같은 기준선에 맞춘다.
            foreach (
                var sectionName in new[]
                {
                    "SessionSection",
                    "UserSection",
                    "DataSection",
                    "LeaderboardSection",
                    "CurrencySection",
                }
            )
            {
                var sectionLayout = FindRequired<VerticalLayoutGroup>(
                    ContentPath + sectionName
                );
                sectionLayout.padding.left = 0;
                sectionLayout.padding.right = 0;
            }

            // 모든 입력 행은 같은 라벨 너비를 사용해 입력 필드의 시작점과 끝점을 맞춘다.
            foreach (
                var rowPath in new[]
                {
                    "DataSection/DataKeyRow",
                    "DataSection/DataValueRow",
                    "LeaderboardSection/ScoreRow",
                    "LeaderboardSection/IncrementRow",
                    "LeaderboardSection/TopLimitRow",
                    "LeaderboardSection/AroundRangeRow",
                    "CurrencySection/ConsumeAmountRow",
                    "CurrencySection/ItemNameRow",
                    "CurrencySection/ItemDescriptionRow",
                }
            )
            {
                var row = FindRequired<HorizontalLayoutGroup>(ContentPath + rowPath);
                row.childAlignment = TextAnchor.MiddleLeft;
                row.childForceExpandWidth = false;
                row.childForceExpandHeight = false;
                row.spacing = 12f;
                var rowLayout = row.GetComponent<LayoutElement>();
                if (rowLayout == null)
                {
                    rowLayout = row.gameObject.AddComponent<LayoutElement>();
                }
                rowLayout.minHeight = 44f;
                rowLayout.preferredHeight = 44f;

                var labelObject = FindRequired<RectTransform>(
                    ContentPath + rowPath + "/Label"
                ).gameObject;
                var labelLayout = labelObject.GetComponent<LayoutElement>();
                if (labelLayout == null)
                {
                    labelLayout = labelObject.AddComponent<LayoutElement>();
                }
                labelLayout.minWidth = 164f;
                labelLayout.preferredWidth = 164f;
                labelLayout.flexibleWidth = 0f;

                var inputObject = FindRequired<RectTransform>(
                    ContentPath + rowPath + "/Input"
                ).gameObject;
                var inputLayout = inputObject.GetComponent<LayoutElement>();
                if (inputLayout == null)
                {
                    inputLayout = inputObject.AddComponent<LayoutElement>();
                }
                inputLayout.minWidth = 0f;
                inputLayout.preferredWidth = 0f;
                inputLayout.flexibleWidth = 1f;
                inputLayout.minHeight = 44f;
                inputLayout.preferredHeight = 44f;
            }

            var skipConfirmRow = FindRequired<HorizontalLayoutGroup>(
                ContentPath + "CurrencySection/SkipConfirmRow"
            );
            skipConfirmRow.childAlignment = TextAnchor.MiddleLeft;
            skipConfirmRow.childForceExpandWidth = false;
            skipConfirmRow.childForceExpandHeight = false;
            skipConfirmRow.spacing = 12f;
            var skipConfirmLabelObject = FindRequired<RectTransform>(
                ContentPath + "CurrencySection/SkipConfirmRow/Label"
            ).gameObject;
            var skipConfirmLabel = skipConfirmLabelObject.GetComponent<LayoutElement>();
            if (skipConfirmLabel == null)
            {
                skipConfirmLabel = skipConfirmLabelObject.AddComponent<LayoutElement>();
            }
            skipConfirmLabel.minWidth = 164f;
            skipConfirmLabel.preferredWidth = 164f;
            skipConfirmLabel.flexibleWidth = 0f;
            var skipConfirmToggleLayout = FindRequired<LayoutElement>(
                ContentPath + "CurrencySection/SkipConfirmRow/SkipConfirmToggle"
            );
            skipConfirmToggleLayout.minWidth = 24f;
            skipConfirmToggleLayout.preferredWidth = 24f;
            skipConfirmToggleLayout.flexibleWidth = 0f;
            skipConfirmToggleLayout.minHeight = 24f;
            skipConfirmToggleLayout.preferredHeight = 24f;

            Anchor(
                FindRequired<RectTransform>("Background/LogPanel/Title"),
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(-55f, -18f),
                new Vector2(-150f, 38f)
            );
            Anchor(
                FindRequired<RectTransform>("Background/LogPanel/ClearLogButton"),
                Vector2.one,
                Vector2.one,
                Vector2.one,
                new Vector2(-16f, -14f),
                new Vector2(116f, 38f)
            );
            Stretch(
                FindRequired<RectTransform>("Background/LogPanel/LogScroll"),
                Vector2.zero,
                Vector2.one,
                new Vector2(16f, 16f),
                new Vector2(-16f, -66f)
            );
            Stretch(
                FindRequired<RectTransform>(
                    "Background/LogPanel/LogScroll/Viewport"
                ),
                Vector2.zero,
                Vector2.one,
                new Vector2(10f, 10f),
                new Vector2(-10f, -10f)
            );
            Anchor(
                FindRequired<RectTransform>(
                    "Background/LogPanel/LogScroll/Viewport/Content"
                ),
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(0.5f, 1f),
                Vector2.zero,
                Vector2.zero
            );
            Anchor(
                FindRequired<RectTransform>(
                    "Background/LogPanel/LogScroll/Viewport/Content/LogText"
                ),
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(0.5f, 1f),
                Vector2.zero,
                new Vector2(0f, 48f)
            );
        }

        // 흰색 배경과 검은색 본문은 유지하고 로그인한 플랫폼의 강조색을 사용한다.
        private void ConfigureTheme()
        {
            var isStoryEngine =
                RPlayGames.ConnectedPlatform == RPlayPlatform.StoryEngine;
            var accent = isStoryEngine
                ? new Color32(99, 46, 255, 255)  // StoryEngine #632EFF
                : new Color32(37, 150, 190, 255); // RPlay #2596BE
            var accentSoft = new Color32(
                (byte)(isStoryEngine ? 99 : 37),
                (byte)(isStoryEngine ? 46 : 150),
                (byte)(isStoryEngine ? 255 : 190),
                20
            );
            var textColor = new Color32(32, 37, 41, 255);
            var textMuted = new Color32(32, 37, 41, 135);

            foreach (var image in GetComponentsInChildren<Image>(true))
            {
                image.color = Color.white;
            }
            foreach (var label in GetComponentsInChildren<Text>(true))
            {
                label.color = textColor;
            }
            foreach (var button in GetComponentsInChildren<Button>(true))
            {
                if (button.TryGetComponent<Image>(out var buttonBackground))
                {
                    buttonBackground.color = accent;
                }
                var label = button.transform.Find("Label");
                if (label != null && label.TryGetComponent<Text>(out var buttonLabel))
                {
                    buttonLabel.color = Color.white;
                }
            }
            foreach (var input in GetComponentsInChildren<InputField>(true))
            {
                if (input.TryGetComponent<Image>(out var inputBackground))
                {
                    inputBackground.color = accentSoft;
                }
                var placeholder = input.transform.Find("Placeholder");
                if (
                    placeholder != null
                    && placeholder.TryGetComponent<Text>(out var placeholderLabel)
                )
                {
                    placeholderLabel.color = textMuted;
                }
            }

            FindRequired<Text>("Background/Header").color = accent;
            FindRequired<Text>("Background/Status").color = accent;
            FindRequired<Text>(ContentPath + "SessionSection/Title").color = accent;
            FindRequired<Text>(ContentPath + "UserSection/Title").color = accent;
            FindRequired<Text>(ContentPath + "DataSection/Title").color = accent;
            FindRequired<Text>(ContentPath + "LeaderboardSection/Title").color = accent;
            FindRequired<Text>(ContentPath + "CurrencySection/Title").color = accent;
            FindRequired<Text>("Background/LogPanel/Title").color = accent;

            // Viewport의 Mask는 이미지 알파를 스텐실 생성에 사용한다.
            // m_ShowMaskGraphic이 꺼져 있으므로 이미지는 불투명하게 유지해도 화면에는 표시되지 않는다.
            FindRequired<Image>(
                "Background/ControlsScroll/Viewport"
            ).color = Color.white;
            FindRequired<Image>(
                "Background/LogPanel/LogScroll/Viewport"
            ).color = Color.white;
            FindRequired<Image>(
                "Background/LogPanel/LogScroll"
            ).color = accentSoft;
            FindRequired<Image>(
                ContentPath + "CurrencySection/SkipConfirmRow/SkipConfirmToggle"
            ).color = accentSoft;
            FindRequired<Image>(
                ContentPath
                    + "CurrencySection/SkipConfirmRow/SkipConfirmToggle/Checkmark"
            ).color = accent;
            if (skipConfirmationCheckmark != null)
            {
                skipConfirmationCheckmark.color = accent;
            }
        }

        // 가져온 샘플에서도 직렬화 참조에 의존하지 않도록 Viewport와 Content를 명시적으로 연결한다.
        private void ConfigureScrollRect(ScrollRect scrollRect, string viewportPath)
        {
            scrollRect.viewport = FindRequired<RectTransform>(viewportPath);
            scrollRect.content = FindRequired<RectTransform>(viewportPath + "/Content");
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
        }

        // 아래에 더 많은 API 예제가 있을 때만 스크롤 영역 하단을 옅게 덮어 탐색 방향을 알려 준다.
        private void ConfigureControlsScrollHint()
        {
            var hintObject = new GameObject(
                "ScrollHint",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image)
            );
            hintObject.transform.SetParent(controlsScroll.transform, false);
            var hintRect = (RectTransform)hintObject.transform;
            Anchor(
                hintRect,
                new Vector2(0f, 0f),
                new Vector2(1f, 0f),
                new Vector2(0.5f, 0f),
                Vector2.zero,
                new Vector2(0f, 56f)
            );
            hintRect.SetAsLastSibling();

            const int textureHeight = 32;
            var texture = new Texture2D(1, textureHeight, TextureFormat.RGBA32, false)
            {
                name = "RPlay Games 샘플 스크롤 그라데이션",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave,
            };
            for (var y = 0; y < textureHeight; y++)
            {
                var alpha = Mathf.Lerp(0.92f, 0f, y / (textureHeight - 1f));
                texture.SetPixel(0, y, new Color(1f, 1f, 1f, alpha));
            }
            texture.Apply();

            controlsScrollHintSprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                100f,
                0,
                SpriteMeshType.FullRect
            );
            controlsScrollHintSprite.name = "RPlay Games 샘플 스크롤 그라데이션";

            var hintImage = hintObject.GetComponent<Image>();
            hintImage.sprite = controlsScrollHintSprite;
            hintImage.color = Color.white;
            hintImage.raycastTarget = false;
            controlsScrollHint = hintObject;
            controlsScroll.onValueChanged.AddListener(OnControlsScrollChanged);
        }

        // 로그인 전 화면의 흰색 카드를 다른 이미지 에셋 없이도 동일하게 보이도록 만든다.
        private void ConfigureLoginCard()
        {
            const int textureSize = 32;
            const float cornerRadius = 7f;
            var texture = new Texture2D(
                textureSize,
                textureSize,
                TextureFormat.RGBA32,
                false
            )
            {
                name = "RPlay Games 샘플 로그인 카드",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave,
            };

            for (var y = 0; y < textureSize; y++)
            {
                for (var x = 0; x < textureSize; x++)
                {
                    var pixelCenter = new Vector2(x + 0.5f, y + 0.5f);
                    var nearest = new Vector2(
                        Mathf.Clamp(pixelCenter.x, cornerRadius, textureSize - cornerRadius),
                        Mathf.Clamp(pixelCenter.y, cornerRadius, textureSize - cornerRadius)
                    );
                    var alpha = Mathf.Clamp01(
                        cornerRadius + 0.5f - Vector2.Distance(pixelCenter, nearest)
                    );
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
            texture.Apply();

            loginCardSprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, textureSize, textureSize),
                new Vector2(0.5f, 0.5f),
                100f,
                0,
                SpriteMeshType.FullRect,
                new Vector4(8f, 8f, 8f, 8f)
            );
            loginCardSprite.name = "RPlay Games 샘플 로그인 카드";
            controlsPanelImage.sprite = loginCardSprite;
            controlsPanelImage.type = Image.Type.Sliced;

            controlsPanelShadow = controlsPanel.gameObject.AddComponent<Shadow>();
            controlsPanelShadow.effectColor = new Color32(24, 31, 38, 24);
            controlsPanelShadow.effectDistance = new Vector2(0f, -4f);
            controlsPanelShadow.useGraphicAlpha = true;
        }

        // 현재 운영체제 언어를 기본값으로 삼고 우측 상단에 여섯 언어 선택기를 만든다.
        private void ConfigureLanguageDropdown(Font font)
        {
            switch (Application.systemLanguage)
            {
                case SystemLanguage.English:
                    currentLanguage = SampleLanguage.English;
                    break;
                case SystemLanguage.Japanese:
                    currentLanguage = SampleLanguage.Japanese;
                    break;
                case SystemLanguage.Spanish:
                    currentLanguage = SampleLanguage.Spanish;
                    break;
                case SystemLanguage.ChineseTraditional:
                    currentLanguage = SampleLanguage.TraditionalChinese;
                    break;
                case SystemLanguage.ChineseSimplified:
                    currentLanguage = SampleLanguage.SimplifiedChinese;
                    break;
                default:
                    currentLanguage = SampleLanguage.Korean;
                    break;
            }

            var dropdownObject = DefaultControls.CreateDropdown(
                new DefaultControls.Resources()
            );
            dropdownObject.name = "LanguageDropdown";
            dropdownObject.transform.SetParent(
                FindRequired<RectTransform>("Background"),
                false
            );
            dropdownObject.transform.SetAsLastSibling();
            Anchor(
                (RectTransform)dropdownObject.transform,
                Vector2.one,
                Vector2.one,
                Vector2.one,
                new Vector2(-24f, -24f),
                new Vector2(196f, 42f)
            );

            languageDropdown = dropdownObject.GetComponent<Dropdown>();
            languageDropdown.navigation = new Navigation
            {
                mode = Navigation.Mode.None,
            };
            languageDropdown.options = new List<Dropdown.OptionData>
            {
                new Dropdown.OptionData("KR · 한국어"),
                new Dropdown.OptionData("US · English"),
                new Dropdown.OptionData("JP · 日本語"),
                new Dropdown.OptionData("ES · Español"),
                new Dropdown.OptionData("TW · 繁體中文"),
                new Dropdown.OptionData("CN · 简体中文"),
            };
            languageDropdown.SetValueWithoutNotify((int)currentLanguage);
            languageDropdown.RefreshShownValue();
            languageDropdown.template.sizeDelta = new Vector2(0f, 216f);
            languageDropdown.template.gameObject.SetActive(false);

            var dropdownImage = dropdownObject.GetComponent<Image>();
            dropdownImage.sprite = loginCardSprite;
            dropdownImage.type = Image.Type.Sliced;
            dropdownImage.color = Color.white;
            var outline = dropdownObject.AddComponent<Outline>();
            outline.effectColor = new Color32(32, 37, 41, 32);
            outline.effectDistance = new Vector2(1f, -1f);
            outline.useGraphicAlpha = true;

            var templateImage = languageDropdown.template.GetComponent<Image>();
            templateImage.sprite = loginCardSprite;
            templateImage.type = Image.Type.Sliced;
            templateImage.color = Color.white;
            var templateOutline = languageDropdown.template.gameObject.AddComponent<Outline>();
            templateOutline.effectColor = new Color32(32, 37, 41, 32);
            templateOutline.effectDistance = new Vector2(1f, -1f);
            templateOutline.useGraphicAlpha = true;

            var dropdownScroll = languageDropdown.template.GetComponent<ScrollRect>();
            dropdownScroll.horizontal = false;
            dropdownScroll.verticalScrollbar = null;
            var scrollbar = languageDropdown.template.Find("Scrollbar");
            if (scrollbar != null)
            {
                scrollbar.gameObject.SetActive(false);
            }
            var dropdownViewport = languageDropdown.template.Find("Viewport") as RectTransform;
            Stretch(
                dropdownViewport,
                Vector2.zero,
                Vector2.one,
                new Vector2(6f, 6f),
                new Vector2(-6f, -6f)
            );
            var dropdownItem = languageDropdown.template.Find(
                "Viewport/Content/Item"
            ) as RectTransform;
            dropdownItem.sizeDelta = new Vector2(0f, 34f);
            var dropdownItemLabel = dropdownItem.Find("Item Label") as RectTransform;
            Stretch(
                dropdownItemLabel,
                Vector2.zero,
                Vector2.one,
                new Vector2(12f, 4f),
                new Vector2(-8f, -4f)
            );
            var dropdownItemCheckmark = dropdownItem.Find("Item Checkmark");
            if (dropdownItemCheckmark != null)
            {
                dropdownItemCheckmark.gameObject.SetActive(false);
            }
            var dropdownItemToggle = dropdownItem.GetComponent<Toggle>();
            var dropdownItemBackground = dropdownItem.Find(
                "Item Background"
            ).GetComponent<Image>();
            dropdownItemToggle.targetGraphic = dropdownItemBackground;
            dropdownItemToggle.navigation = new Navigation
            {
                mode = Navigation.Mode.None,
            };
            dropdownItemBackground.color = Color.white;
            dropdownItemToggle.colors = new ColorBlock
            {
                normalColor = Color.white,
                highlightedColor = new Color32(229, 244, 249, 255),
                pressedColor = new Color32(214, 237, 245, 255),
                selectedColor = new Color32(229, 244, 249, 255),
                disabledColor = new Color32(245, 246, 247, 255),
                colorMultiplier = 1f,
                fadeDuration = 0.08f,
            };

            foreach (var label in dropdownObject.GetComponentsInChildren<Text>(true))
            {
                label.font = font;
                label.fontSize = 15;
                label.color = new Color32(32, 37, 41, 255);
            }
            var arrowObject = new GameObject(
                "Label",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Text)
            );
            arrowObject.transform.SetParent(dropdownObject.transform.Find("Arrow"), false);
            Stretch(
                (RectTransform)arrowObject.transform,
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                Vector2.zero
            );
            var arrow = arrowObject.GetComponent<Text>();
            arrow.font = font;
            arrow.fontSize = 12;
            arrow.alignment = TextAnchor.MiddleCenter;
            arrow.color = new Color32(32, 37, 41, 255);
            arrow.text = "▼";
            languageDropdown.captionText.alignment = TextAnchor.MiddleLeft;
            languageDropdown.itemText.alignment = TextAnchor.MiddleLeft;
            languageDropdown.onValueChanged.AddListener(OnLanguageChanged);
        }

        private void OnLanguageChanged(int languageIndex)
        {
            currentLanguage = (SampleLanguage)languageIndex;
            ApplyLanguage();
            LayoutRebuilder.ForceRebuildLayoutImmediate(controlsScroll.content);
        }

        // 화면 문구만 선택 언어로 바꾸고 API 호출 기록과 Unity 로그는 한국어로 유지한다.
        private void ApplyLanguage()
        {
            SetLocalizedText("Background/Header", SampleText.Header);
            SetLocalizedText("Background/Warning", SampleText.Warning);
            SetLocalizedText(ContentPath + "UserSection/Title", SampleText.UserTitle);
            SetLocalizedText(ContentPath + "DataSection/Title", SampleText.DataTitle);
            SetLocalizedText(
                ContentPath + "LeaderboardSection/Title",
                SampleText.LeaderboardTitle
            );
            SetLocalizedText(
                ContentPath + "CurrencySection/Title",
                SampleText.CurrencyTitle
            );

            SetLocalizedText(ContentPath + "DataSection/DataKeyRow/Label", SampleText.DataKey);
            SetLocalizedText(
                ContentPath + "DataSection/DataValueRow/Label",
                SampleText.DataValue
            );
            SetLocalizedText(
                ContentPath + "LeaderboardSection/ScoreRow/Label",
                SampleText.SetScoreLabel
            );
            SetLocalizedText(
                ContentPath + "LeaderboardSection/IncrementRow/Label",
                SampleText.IncrementScoreLabel
            );
            SetLocalizedText(
                ContentPath + "LeaderboardSection/TopLimitRow/Label",
                SampleText.TopLimitLabel
            );
            SetLocalizedText(
                ContentPath + "LeaderboardSection/AroundRangeRow/Label",
                SampleText.AroundRangeLabel
            );
            SetLocalizedText(
                ContentPath + "CurrencySection/ConsumeAmountRow/Label",
                SampleText.ConsumeAmount
            );
            SetLocalizedText(
                ContentPath + "CurrencySection/ItemNameRow/Label",
                SampleText.ItemName
            );
            SetLocalizedText(
                ContentPath + "CurrencySection/ItemDescriptionRow/Label",
                SampleText.ItemDescription
            );
            SetLocalizedText(
                ContentPath + "CurrencySection/SkipConfirmRow/Label",
                SampleText.SkipConfirmation
            );

            SetLocalizedButton(
                "SessionSection/Buttons/InitializeLoginButton",
                SampleText.Login
            );
            sessionSubtitle.text = Localize(SampleText.LoginSubtitle);
            SetLocalizedButton("SessionSection/Buttons/LogoutButton", SampleText.Logout);
            SetLocalizedButton(
                "UserSection/Buttons/VerifyUserButton",
                SampleText.VerifyUser
            );
            SetLocalizedButton(
                "UserSection/Buttons/UserInfoButton",
                SampleText.UserInfo
            );
            SetLocalizedButton(
                "DataSection/ButtonsPrimary/SaveBulkButton",
                SampleText.SaveBulk
            );
            SetLocalizedButton(
                "DataSection/ButtonsPrimary/LoadDataButton",
                SampleText.LoadAll
            );
            SetLocalizedButton(
                "DataSection/ButtonsPrimary/SaveKeyButton",
                SampleText.SaveKey
            );
            SetLocalizedButton(
                "DataSection/ButtonsDelete/DeleteKeyButton",
                SampleText.DeleteKey
            );
            SetLocalizedButton(
                "DataSection/ButtonsDelete/DeleteAllButton",
                SampleText.DeleteAll
            );
            SetLocalizedButton(
                "LeaderboardSection/ButtonsPrimary/SetScoreButton",
                SampleText.SetScore
            );
            SetLocalizedButton(
                "LeaderboardSection/ButtonsPrimary/IncrementScoreButton",
                SampleText.IncrementScore
            );
            SetLocalizedButton(
                "LeaderboardSection/ButtonsPrimary/MyRankButton",
                SampleText.MyRank
            );
            SetLocalizedButton(
                "LeaderboardSection/ButtonsSecondary/TopRanksButton",
                SampleText.TopRanks
            );
            SetLocalizedButton(
                "LeaderboardSection/ButtonsSecondary/AroundRanksButton",
                SampleText.AroundRanks
            );
            SetLocalizedButton(
                "CurrencySection/Buttons/RequestChargeButton",
                SampleText.OpenCharge
            );
            SetLocalizedButton(
                "CurrencySection/Buttons/ConsumeButton",
                SampleText.Consume
            );
            SetLocalizedText("Background/LogPanel/Title", SampleText.LogTitle);
            if (logs.Count == 0)
            {
                logText.text = Localize(SampleText.EmptyLog);
            }
            SetButtonLabel(
                FindRequired<Button>("Background/LogPanel/ClearLogButton"),
                Localize(SampleText.ClearLog)
            );

            sessionTitle.text = isAuthenticatedView
                ? string.Format(
                    Localize(SampleText.SessionConnected),
                    GetLoginProviderName()
                )
                : "RPLAY GAMES";
            UpdateStatus(currentStatusAction);
        }

        private void SetLocalizedText(string path, SampleText text)
        {
            FindRequired<Text>(path).text = Localize(text);
        }

        private void SetLocalizedButton(string relativePath, SampleText text)
        {
            SetButtonLabel(
                FindRequired<Button>(ContentPath + relativePath),
                Localize(text)
            );
        }

        private string Localize(SampleText text)
        {
            return LocalizedTexts[text][(int)currentLanguage];
        }

        private void OnControlsScrollChanged(Vector2 _)
        {
            UpdateControlsScrollHint();
        }

        // 스크롤할 내용이 없거나 맨 아래에 도달하면 그라데이션이 본문을 가리지 않게 숨긴다.
        private void UpdateControlsScrollHint()
        {
            var hasMoreContent =
                controlsScroll.content.rect.height > controlsScroll.viewport.rect.height + 1f;
            controlsScrollHint.SetActive(
                hasMoreContent && controlsScroll.verticalNormalizedPosition > 0.001f
            );
        }

        // 응답 모델을 반환하는 API 버튼을 공통 실행·오류 처리·로그 출력 흐름에 연결한다.
        private void BindResponseButton<T>(
            string relativePath,
            string label,
            Func<Task<T>> request
        ) where T : RPlayResponse
        {
            var button = FindRequired<Button>(ContentPath + relativePath);
            ConfigureButton(button);
            button.onClick.AddListener(() => RunResponseAsync(label, request));
        }

        // 버튼 그래픽과 라벨 영역을 보정하고 요청 중 잠글 버튼 목록에 등록한다.
        private void ConfigureButton(Button button)
        {
            if (!button.TryGetComponent<Image>(out var buttonBackground))
            {
                throw new InvalidOperationException("버튼의 배경 이미지가 없습니다: " + button.name);
            }
            button.targetGraphic = buttonBackground;
            buttonBackground.sprite = loginCardSprite;
            buttonBackground.type = Image.Type.Sliced;
            button.navigation = new Navigation
            {
                mode = Navigation.Mode.None,
            };
            var label = button.transform.Find("Label") as RectTransform;
            if (label != null)
            {
                Stretch(
                    label,
                    Vector2.zero,
                    Vector2.one,
                    new Vector2(6f, 3f),
                    new Vector2(-6f, -3f)
                );
            }
            actionButtons.Add(button);
        }

        // 씬에 저장된 버튼 문구를 현재 플랫폼과 샘플 동작에 맞는 문구로 교체한다.
        private static void SetButtonLabel(Button button, string value)
        {
            var label = button.transform.Find("Label");
            if (label == null || !label.TryGetComponent<Text>(out var labelText))
            {
                throw new InvalidOperationException(
                    "버튼의 텍스트 구성 요소가 없습니다: " + button.name
                );
            }
            labelText.text = value;
        }

        // 로그인 전에는 통합 서비스명, 로그인 후에는 사용자가 선택한 플랫폼명을 표시한다.
        private string GetLoginProviderName()
        {
            if (RPlayGames.ConnectedPlatform == RPlayPlatform.StoryEngine)
            {
                return "StoryEngine";
            }
            if (RPlayGames.ConnectedPlatform == RPlayPlatform.RPlay)
            {
                return "RPlay";
            }
            return "RPlay Games";
        }

        // 로그인 전에는 로그인 카드만, 로그인 후에는 API 영역과 응답 로그를 함께 보여 준다.
        private void SetAuthenticatedState(bool authenticated)
        {
            isAuthenticatedView = authenticated;
            FindRequired<RectTransform>("Background/Header").gameObject.SetActive(authenticated);
            FindRequired<RectTransform>("Background/Warning").gameObject.SetActive(authenticated);
            statusText.gameObject.SetActive(authenticated);
            sessionSubtitle.gameObject.SetActive(!authenticated);
            loginButton.gameObject.SetActive(!authenticated);
            logoutButton.gameObject.SetActive(authenticated);
            logPanel.SetActive(authenticated);
            foreach (var section in apiSections)
            {
                section.SetActive(authenticated);
            }
            if (authenticated)
            {
                Stretch(
                    controlsPanel,
                    new Vector2(0f, 0f),
                    new Vector2(0.62f, 1f),
                    new Vector2(24f, 24f),
                    new Vector2(-12f, -164f)
                );
            }
            else
            {
                Anchor(
                    controlsPanel,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f),
                    Vector2.zero,
                    new Vector2(500f, 196f)
                );
            }

            Stretch(
                controlsScroll.viewport,
                Vector2.zero,
                Vector2.one,
                authenticated ? new Vector2(8f, 10f) : new Vector2(32f, 24f),
                authenticated ? new Vector2(-8f, -10f) : new Vector2(-32f, -24f)
            );
            controlsScroll.enabled = authenticated;
            backgroundImage.color = authenticated
                ? Color.white
                : new Color32(247, 249, 250, 255);
            controlsPanelShadow.enabled = !authenticated;
            sessionTitle.alignment = authenticated
                ? TextAnchor.MiddleLeft
                : TextAnchor.MiddleCenter;
            sessionTitle.fontSize = authenticated ? 20 : 24;
            sessionTitle.color = authenticated
                ? RPlayGames.ConnectedPlatform == RPlayPlatform.StoryEngine
                    ? new Color32(99, 46, 255, 255)
                    : new Color32(37, 150, 190, 255)
                : new Color32(32, 37, 41, 255);
            ApplyLanguage();
            LayoutRebuilder.ForceRebuildLayoutImmediate(controlsScroll.content);
            Canvas.ForceUpdateCanvases();
            UpdateControlsScrollHint();
        }

        private void OnDestroy()
        {
            if (controlsScroll != null)
            {
                controlsScroll.onValueChanged.RemoveListener(OnControlsScrollChanged);
            }
            if (languageDropdown != null)
            {
                languageDropdown.onValueChanged.RemoveListener(OnLanguageChanged);
            }
            if (controlsScrollHintSprite != null)
            {
                var texture = controlsScrollHintSprite.texture;
                Destroy(controlsScrollHintSprite);
                Destroy(texture);
            }
            if (loginCardSprite != null)
            {
                var texture = loginCardSprite.texture;
                Destroy(loginCardSprite);
                Destroy(texture);
            }
        }

        // RectTransform을 지정한 앵커 영역에 맞춰 늘리고 기존 씬 스케일의 영향을 제거한다.
        private static void Stretch(
            RectTransform rectTransform,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 offsetMin,
            Vector2 offsetMax
        )
        {
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.offsetMin = offsetMin;
            rectTransform.offsetMax = offsetMax;
            rectTransform.localScale = Vector3.one;
        }

        // 고정 기준점이 필요한 텍스트와 버튼에 앵커, 위치와 크기를 한 번에 적용한다.
        private static void Anchor(
            RectTransform rectTransform,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 pivot,
            Vector2 anchoredPosition,
            Vector2 sizeDelta
        )
        {
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.pivot = pivot;
            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.sizeDelta = sizeDelta;
            rectTransform.localScale = Vector3.one;
        }

        // 샘플 씬 계층이 바뀌어 필수 UI가 사라지면 조용히 실패하지 않고 즉시 원인을 알린다.
        private T FindRequired<T>(string path) where T : Component
        {
            var child = transform.Find(path);
            if (child == null || !child.TryGetComponent<T>(out var component))
            {
                throw new InvalidOperationException(
                    "샘플 UI 구성 요소를 찾을 수 없습니다: " + path
                );
            }

            return component;
        }

        // Unity 버튼 이벤트용 async void 경계에서 예외를 받아 상태와 로그를 항상 복구한다.
        private async void RunActionAsync(string label, Func<Task> action)
        {
            SetBusy(true, label + " 처리 중");
            try
            {
                await action();
                AppendLog(label + " 완료");
            }
            catch (Exception exception)
            {
                AppendError(label, exception);
            }
            finally
            {
                SetBusy(false, "대기 중");
            }
        }

        // HTTP 성공 여부와 별개인 응답 success 및 원문 JSON을 개발자가 확인할 수 있게 출력한다.
        private async void RunResponseAsync<T>(string label, Func<Task<T>> request)
            where T : RPlayResponse
        {
            SetBusy(true, label + " 처리 중");
            try
            {
                var response = await request();
                var body = string.IsNullOrWhiteSpace(response.RawJson)
                    ? "success=" + response.Success +
                      ", status=" + response.Status +
                      ", errorCode=" + response.ErrorCode
                    : response.RawJson;
                AppendLog(label + Environment.NewLine + body);
            }
            catch (Exception exception)
            {
                AppendError(label, exception);
            }
            finally
            {
                SetBusy(false, "대기 중");
            }
        }

        // 요청 중 API 버튼은 잠그지만 로그인 대기 중에는 같은 인증 페이지를 다시 열 수 있게 둔다.
        private void SetBusy(bool busy, string action)
        {
            foreach (var button in actionButtons)
            {
                button.interactable = !busy;
            }
            if (loginButton.gameObject.activeSelf)
            {
                loginButton.interactable = true;
            }
            UpdateStatus(action);
        }

        // 현재 동작과 SDK 초기화·로그인 상태를 한 줄로 갱신한다.
        private void UpdateStatus(string action)
        {
            currentStatusAction = action;
            var actionText = action == "대기 중"
                ? Localize(SampleText.Ready)
                : Localize(SampleText.Processing);
            statusText.text =
                Localize(SampleText.Status) + ": " + actionText +
                "    " + Localize(SampleText.Initialized) + ": " +
                Localize(
                    RPlayGames.IsInitialized ? SampleText.Yes : SampleText.No
                ) +
                "    " + Localize(SampleText.LoggedIn) + ": " +
                Localize(
                    RPlayGames.IsAuthenticated ? SampleText.Yes : SampleText.No
                );
        }

        // 최신 결과가 위에 오도록 최대 40건만 유지하고 Unity Console에도 같은 내용을 남긴다.
        private void AppendLog(string message)
        {
            logs.Insert(0, "[" + DateTime.Now.ToString("HH:mm:ss") + "] " + message);
            if (logs.Count > 40)
            {
                logs.RemoveAt(logs.Count - 1);
            }
            logText.text = string.Join(Environment.NewLine + Environment.NewLine, logs);
            logScroll.verticalNormalizedPosition = 1f;
            Debug.Log("[RPlay Games 샘플] " + message, this);
        }

        // 실패한 버튼 이름과 오류 메시지를 샘플 로그와 Unity 오류 로그에 함께 남긴다.
        private void AppendError(string label, Exception exception)
        {
            var message = label + " 실패: " + exception.Message;
            AppendLog(message);
            Debug.LogError("[RPlay Games 샘플] " + message, this);
        }

        // 서버 데이터는 건드리지 않고 이 샘플 화면의 호출 기록만 비운다.
        private void ClearLogs()
        {
            logs.Clear();
            logText.text = Localize(SampleText.EmptyLog);
        }

        // 운영체제의 소수점 표기와 무관하게 API에 전달할 유한한 숫자만 허용한다.
        private static double ParseDouble(string rawValue, string label)
        {
            if (
                !double.TryParse(
                    rawValue,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out var value
                )
                || double.IsNaN(value)
                || double.IsInfinity(value)
            )
            {
                throw new FormatException(label + "에 올바른 숫자를 입력하세요.");
            }

            return value;
        }

        // 충전과 소비 금액처럼 0보다 커야 하는 숫자를 추가로 검증한다.
        private static double ParsePositiveDouble(string rawValue, string label)
        {
            var value = ParseDouble(rawValue, label);
            if (value <= 0d)
            {
                throw new FormatException(label + "은 0보다 커야 합니다.");
            }

            return value;
        }

        // 리더보드 조회 개수를 서버가 허용하는 정수 범위 안으로 제한한다.
        private static int ParseInt(string rawValue, string label, int minimum, int maximum)
        {
            if (
                !int.TryParse(
                    rawValue,
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out var value
                )
                || value < minimum
                || value > maximum
            )
            {
                throw new FormatException(
                    label + "은 " + minimum + " 이상 " + maximum + " 이하여야 합니다."
                );
            }

            return value;
        }
    }
}
