using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RPlay.Games.Internal
{
    internal sealed class RPlayConsumePrompt
    {
        internal string ItemName { get; set; }

        internal string ItemDescription { get; set; }

        internal double Amount { get; set; }

        internal bool IsStoryEngine { get; set; }
    }

    internal sealed class RPlayConfirmationOverlay : MonoBehaviour
    {
        private const float ConfirmationTimeoutSeconds = 180f;
        private TaskCompletionSource<bool> completion;
        private CancellationTokenRegistration cancellationRegistration;
        private GameObject overlayRoot;
        private Sprite roundedSprite;
        private float deadline;
        private bool cancelRequested;
        private static Font defaultFont;

        internal static Task<bool> ShowAsync(
            RPlayConsumePrompt prompt,
            CancellationToken cancellationToken
        )
        {
            var overlay = RPlayRuntimeHost.Instance.gameObject.AddComponent<RPlayConfirmationOverlay>();
            return overlay.Show(prompt, cancellationToken);
        }

        private Task<bool> Show(
            RPlayConsumePrompt prompt,
            CancellationToken cancellationToken
        )
        {
            completion = new TaskCompletionSource<bool>();
            cancellationRegistration = cancellationToken.Register(() => cancelRequested = true);
            deadline = Time.realtimeSinceStartup + ConfirmationTimeoutSeconds;
            BuildUi(prompt);
            return completion.Task;
        }

        private void Update()
        {
            if (completion == null || completion.Task.IsCompleted)
            {
                return;
            }

            if (cancelRequested)
            {
                Finish(false, true);
                return;
            }

            // 서버의 소비 확인 대기 시간과 맞춰 종료하되 카운트다운은 사용자에게 노출하지 않는다.
            if (Time.realtimeSinceStartup >= deadline)
            {
                Finish(false, false);
            }
        }

        private void OnDestroy()
        {
            cancellationRegistration.Dispose();
            if (roundedSprite != null)
            {
                var texture = roundedSprite.texture;
                Destroy(roundedSprite);
                Destroy(texture);
                roundedSprite = null;
            }
            if (completion != null && !completion.Task.IsCompleted)
            {
                completion.TrySetResult(false);
            }
        }

        private void BuildUi(RPlayConsumePrompt prompt)
        {
            EnsureEventSystem();

            overlayRoot = new GameObject(
                "RPlay 소비 확인",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster)
            );
            overlayRoot.transform.SetParent(transform, false);

            var canvas = overlayRoot.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 32000;

            var scaler = overlayRoot.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            var dim = CreateImage("배경", overlayRoot.transform, new Color(0f, 0f, 0f, 0.42f));
            Stretch(dim.rectTransform);

            roundedSprite = CreateRoundedSprite();
            var panel = CreateImage("확인 창", dim.transform, Color.white);
            panel.sprite = roundedSprite;
            panel.type = Image.Type.Sliced;
            var panelRect = panel.rectTransform;
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(620f, 460f);

            var textColor = new Color32(32, 37, 41, 255);
            var mutedTextColor = new Color32(82, 90, 97, 255);
            var accentColor = prompt.IsStoryEngine
                ? new Color32(99, 46, 255, 255)
                : new Color32(37, 150, 190, 255);

            CreateText(
                "제목",
                panel.transform,
                RPlayLocalization.Get("consume_title"),
                34,
                FontStyle.Bold,
                TextAnchor.MiddleCenter,
                new Vector2(40f, -112f),
                new Vector2(-40f, -60f),
                textColor
            );

            CreateText(
                "아이템",
                panel.transform,
                prompt.ItemName,
                28,
                FontStyle.Bold,
                TextAnchor.MiddleCenter,
                new Vector2(50f, -176f),
                new Vector2(-50f, -132f),
                textColor
            );

            CreateText(
                "설명",
                panel.transform,
                string.IsNullOrWhiteSpace(prompt.ItemDescription)
                    ? RPlayLocalization.Get("consume_description")
                    : prompt.ItemDescription,
                21,
                FontStyle.Normal,
                TextAnchor.MiddleCenter,
                new Vector2(50f, -244f),
                new Vector2(-50f, -196f),
                mutedTextColor
            );

            var amount = prompt.IsStoryEngine
                ? prompt.Amount * 14d
                : prompt.Amount;
            var currency = prompt.IsStoryEngine
                ? RPlayLocalization.Get("credits")
                : RPlayLocalization.Get("coins");
            CreateText(
                "금액",
                panel.transform,
                string.Format("{0:N0} {1}", amount, currency),
                36,
                FontStyle.Bold,
                TextAnchor.MiddleCenter,
                new Vector2(50f, -316f),
                new Vector2(-50f, -264f),
                accentColor
            );

            CreateButton(
                "취소",
                panel.transform,
                RPlayLocalization.Get("cancel"),
                new Vector2(46f, 60f),
                new Vector2(294f, 124f),
                new Color32(241, 243, 245, 255),
                textColor,
                roundedSprite,
                () => Finish(false, false)
            );
            CreateButton(
                "확인",
                panel.transform,
                RPlayLocalization.Get("confirm"),
                new Vector2(326f, 60f),
                new Vector2(574f, 124f),
                accentColor,
                Color.white,
                roundedSprite,
                () => Finish(true, false)
            );
        }

        private void Finish(bool confirmed, bool cancelledByToken)
        {
            if (completion == null || completion.Task.IsCompleted)
            {
                return;
            }

            if (cancelledByToken)
            {
                completion.TrySetCanceled();
            }
            else
            {
                completion.TrySetResult(confirmed);
            }

            if (overlayRoot != null)
            {
                Destroy(overlayRoot);
            }

            Destroy(this);
        }

        private static void EnsureEventSystem()
        {
            if (EventSystem.current != null)
            {
                return;
            }

            var eventSystemObject = new GameObject("RPlay UI EventSystem", typeof(EventSystem));
            DontDestroyOnLoad(eventSystemObject);
            var inputSystemModule = Type.GetType(
                "UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem"
            );
            if (inputSystemModule != null)
            {
                eventSystemObject.AddComponent(inputSystemModule);
            }
            else
            {
                eventSystemObject.AddComponent<StandaloneInputModule>();
            }
        }

        private static Image CreateImage(string name, Transform parent, Color color)
        {
            var gameObject = new GameObject(name, typeof(RectTransform), typeof(Image));
            gameObject.transform.SetParent(parent, false);
            var image = gameObject.GetComponent<Image>();
            image.color = color;
            return image;
        }

        private static Sprite CreateRoundedSprite()
        {
            const int textureSize = 32;
            const float cornerRadius = 10f;
            var texture = new Texture2D(
                textureSize,
                textureSize,
                TextureFormat.RGBA32,
                false
            )
            {
                name = "RPlay 소비 확인 라운드 배경",
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

            var sprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, textureSize, textureSize),
                new Vector2(0.5f, 0.5f),
                100f,
                0,
                SpriteMeshType.FullRect,
                new Vector4(11f, 11f, 11f, 11f)
            );
            sprite.name = "RPlay 소비 확인 라운드 배경";
            sprite.hideFlags = HideFlags.HideAndDontSave;
            return sprite;
        }

        private static Text CreateText(
            string name,
            Transform parent,
            string value,
            int fontSize,
            FontStyle style,
            TextAnchor alignment,
            Vector2 offsetMin,
            Vector2 offsetMax,
            Color? color = null
        )
        {
            var gameObject = new GameObject(name, typeof(RectTransform), typeof(Text));
            gameObject.transform.SetParent(parent, false);
            var rect = gameObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;

            var text = gameObject.GetComponent<Text>();
            text.font = GetDefaultFont();
            text.text = value ?? string.Empty;
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.alignment = alignment;
            text.color = color ?? Color.white;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            return text;
        }

        private static void CreateButton(
            string name,
            Transform parent,
            string label,
            Vector2 offsetMin,
            Vector2 offsetMax,
            Color color,
            Color textColor,
            Sprite backgroundSprite,
            UnityEngine.Events.UnityAction onClick
        )
        {
            var image = CreateImage(name, parent, color);
            image.sprite = backgroundSprite;
            image.type = Image.Type.Sliced;
            var rect = image.rectTransform;
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(0f, 0f);
            rect.pivot = new Vector2(0f, 0f);
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;

            var button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(onClick);

            var text = CreateText(
                "텍스트",
                image.transform,
                label,
                23,
                FontStyle.Bold,
                TextAnchor.MiddleCenter,
                Vector2.zero,
                Vector2.zero,
                textColor
            );
            Stretch(text.rectTransform);
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static Font GetDefaultFont()
        {
            if (defaultFont != null)
            {
                return defaultFont;
            }

            string[] fontNames;
            switch (Application.systemLanguage)
            {
                case SystemLanguage.Korean:
                    fontNames = new[]
                    {
                        "Apple SD Gothic Neo",
                        "Malgun Gothic",
                        "Noto Sans CJK KR",
                        "Arial Unicode MS",
                    };
                    break;
                case SystemLanguage.Japanese:
                    fontNames = new[]
                    {
                        "Hiragino Sans",
                        "Yu Gothic",
                        "Noto Sans CJK JP",
                        "Arial Unicode MS",
                    };
                    break;
                case SystemLanguage.ChineseSimplified:
                    fontNames = new[]
                    {
                        "PingFang SC",
                        "Microsoft YaHei",
                        "Noto Sans CJK SC",
                        "Arial Unicode MS",
                    };
                    break;
                case SystemLanguage.ChineseTraditional:
                    fontNames = new[]
                    {
                        "PingFang TC",
                        "Microsoft JhengHei",
                        "Noto Sans CJK TC",
                        "Arial Unicode MS",
                    };
                    break;
                default:
                    fontNames = new[] { "Arial", "Liberation Sans" };
                    break;
            }

            defaultFont = Font.CreateDynamicFontFromOSFont(fontNames, 24);
            if (defaultFont != null)
            {
                return defaultFont;
            }

#if UNITY_6000_0_OR_NEWER
            defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
#else
            defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
#endif
            return defaultFont;
        }
    }

    internal static class RPlayLocalization
    {
        internal static string Get(string key)
        {
            switch (Application.systemLanguage)
            {
                case SystemLanguage.Korean:
                    return GetKorean(key);
                case SystemLanguage.Japanese:
                    return GetJapanese(key);
                case SystemLanguage.Spanish:
                    return GetSpanish(key);
                case SystemLanguage.ChineseSimplified:
                    return GetChineseSimplified(key);
                case SystemLanguage.ChineseTraditional:
                    return GetChineseTraditional(key);
                default:
                    return GetEnglish(key);
            }
        }

        private static string GetKorean(string key)
        {
            switch (key)
            {
                case "consume_title": return "구매하시겠습니까?";
                case "consume_description": return "게임 아이템 구매를 확인해 주세요.";
                case "coins": return "코인";
                case "credits": return "크레딧";
                case "confirm": return "구매";
                case "cancel": return "취소";
                case "login_completed_title": return "로그인이 완료되었습니다.";
                case "login_completed_description": return "이 창을 닫고 게임으로 돌아가세요.";
                default: return key;
            }
        }

        private static string GetEnglish(string key)
        {
            switch (key)
            {
                case "consume_title": return "Confirm purchase";
                case "consume_description": return "Please confirm this in-game purchase.";
                case "coins": return "Coins";
                case "credits": return "Credits";
                case "confirm": return "Purchase";
                case "cancel": return "Cancel";
                case "login_completed_title": return "Sign-in complete";
                case "login_completed_description": return "Close this window and return to the game.";
                default: return key;
            }
        }

        private static string GetJapanese(string key)
        {
            switch (key)
            {
                case "consume_title": return "購入しますか？";
                case "consume_description": return "ゲーム内アイテムの購入を確認してください。";
                case "coins": return "コイン";
                case "credits": return "クレジット";
                case "confirm": return "購入";
                case "cancel": return "キャンセル";
                case "login_completed_title": return "ログインが完了しました。";
                case "login_completed_description": return "この画面を閉じてゲームに戻ってください。";
                default: return key;
            }
        }

        private static string GetSpanish(string key)
        {
            switch (key)
            {
                case "consume_title": return "Confirmar compra";
                case "consume_description": return "Confirma esta compra dentro del juego.";
                case "coins": return "Monedas";
                case "credits": return "Créditos";
                case "confirm": return "Comprar";
                case "cancel": return "Cancelar";
                case "login_completed_title": return "Inicio de sesión completado";
                case "login_completed_description": return "Cierra esta ventana y vuelve al juego.";
                default: return key;
            }
        }

        private static string GetChineseSimplified(string key)
        {
            switch (key)
            {
                case "consume_title": return "确认购买";
                case "consume_description": return "请确认此游戏内购买。";
                case "coins": return "金币";
                case "credits": return "点数";
                case "confirm": return "购买";
                case "cancel": return "取消";
                case "login_completed_title": return "登录已完成";
                case "login_completed_description": return "请关闭此窗口并返回游戏。";
                default: return key;
            }
        }

        private static string GetChineseTraditional(string key)
        {
            switch (key)
            {
                case "consume_title": return "確認購買";
                case "consume_description": return "請確認此遊戲內購買。";
                case "coins": return "金幣";
                case "credits": return "點數";
                case "confirm": return "購買";
                case "cancel": return "取消";
                case "login_completed_title": return "登入已完成";
                case "login_completed_description": return "請關閉此視窗並返回遊戲。";
                default: return key;
            }
        }
    }
}
