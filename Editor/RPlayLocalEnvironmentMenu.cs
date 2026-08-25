using UnityEditor;

namespace RPlay.Games.Editor
{
    /// <summary>
    /// RPlay 로컬 서버 연결을 메뉴에서 켜고 끕니다.
    ///
    /// RPlay 서버를 직접 띄워 개발하는 경우에만 사용합니다. 켜면 SDK가 운영 서버 대신
    /// localhost를 바라보므로, 로컬 서버가 없으면 로그인과 API 호출이 실패합니다.
    /// </summary>
    internal static class RPlayLocalEnvironmentMenu
    {
        private const string MenuPath = "Tools/RPlay/RPlay 로컬 서버에 연결 (내부 개발용)";

        [MenuItem(MenuPath, priority = 100)]
        private static void Toggle()
        {
            RPlayGamesSettings.IsLocalDevelopmentEnvironment =
                !RPlayGamesSettings.IsLocalDevelopmentEnvironment;
        }

        // 현재 상태를 메뉴의 체크 표시로 보여 줍니다.
        [MenuItem(MenuPath, true)]
        private static bool ToggleValidate()
        {
            Menu.SetChecked(
                MenuPath,
                RPlayGamesSettings.IsLocalDevelopmentEnvironment
            );
            return true;
        }
    }
}
