#if NAAD_LEGACY_EDITOR_TOOLS
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NAAD.Core.Bootstrap;
using NAAD.UI.Scripts;

namespace NAAD.UI.Editor
{
    /// <summary>
    /// Editor utility that creates all NAAD MVP UI screen prefabs with
    /// proper hierarchy, components, and view script bindings.
    /// <para>
    /// Invoke via the menu: <b>NAAD > UI > Create All Screen Prefabs</b>.
    /// </para>
    /// </summary>
    public static class UIScreenFactory
    {
        private const string RootFolder = "Assets/NAAD/UI";
        private const string ViewsFolder = RootFolder + "/Views";
        private const string ScriptsFolder = RootFolder + "/Scripts";

        [MenuItem("NAAD/UI/Create All Screen Prefabs", priority = 300)]
        public static void CreateAllScreenPrefabs()
        {
            EnsureFolders();

            CreateBootstrapView();
            CreateMainHudView();
            CreatePhoneHomeView();
            CreatePingMessagingView();
            CreateNightSummaryView();
            CreateBottomNavigationView();
            CreateConfigurationView();
            CreateAuthView();

            AssetDatabase.Refresh();
            Debug.Log("[UIScreenFactory] All UI screen prefabs created.");
        }

        private static void EnsureFolders()
        {
            if (!AssetDatabase.IsValidFolder(RootFolder))
            {
                AssetDatabase.CreateFolder("Assets/NAAD", "UI");
            }
            if (!AssetDatabase.IsValidFolder(ViewsFolder))
            {
                AssetDatabase.CreateFolder(RootFolder, "Views");
            }
            if (!AssetDatabase.IsValidFolder(ScriptsFolder))
            {
                AssetDatabase.CreateFolder(RootFolder, "Scripts");
            }
        }

        // ------------------------------------------------------------------
        // Canvas helper
        // ------------------------------------------------------------------
        private static GameObject CreateCanvas(string name)
        {
            var canvasGo = new GameObject(name);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();
            canvasGo.AddComponent<SafeAreaFitter>();
            return canvasGo;
        }

        private static GameObject CreatePanel(GameObject parent, string name, Color? bgColor = null)
        {
            var panel = new GameObject(name);
            panel.transform.SetParent(parent.transform, false);
            var rt = panel.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var img = panel.AddComponent<Image>();
            img.color = bgColor ?? new Color(0.1f, 0.1f, 0.15f, 0.95f);
            img.raycastTarget = true;

            return panel;
        }

        private static TMP_Text CreateText(GameObject parent, string name, string text, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, int fontSize = 24, Color? color = null)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;

            var tmp = go.AddComponent<TMP_Text>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color ?? Color.white;
            tmp.alignment = TextAlignmentOptions.Center;
            return tmp;
        }

        private static Button CreateButton(GameObject parent, string name, string text, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, Color? bgColor = null)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;

            var btn = go.AddComponent<Button>();
            var img = go.AddComponent<Image>();
            img.color = bgColor ?? new Color(0.2f, 0.6f, 1f, 1f);
            img.raycastTarget = true;

            var txtGo = new GameObject("Label");
            txtGo.transform.SetParent(go.transform, false);
            var txtRt = txtGo.AddComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero;
            txtRt.anchorMax = Vector2.one;
            txtRt.offsetMin = Vector2.zero;
            txtRt.offsetMax = Vector2.zero;

            var txt = txtGo.AddComponent<TMP_Text>();
            txt.text = text;
            txt.fontSize = 20;
            txt.color = Color.white;
            txt.alignment = TextAlignmentOptions.Center;

            return btn;
        }

        // ------------------------------------------------------------------
        // Bootstrap View
        // ------------------------------------------------------------------
        private static void CreateBootstrapView()
        {
            var canvas = CreateCanvas("BootstrapCanvas");
            var panel = CreatePanel(canvas, "Panel");

            var status = CreateText(panel, "StatusText", "Initializing...",
                new Vector2(0.25f, 0.6f), new Vector2(0.75f, 0.7f),
                new Vector2(0, 0), new Vector2(0, 0), 24);

            var errorText = CreateText(panel, "ErrorText", "Error occurred",
                new Vector2(0.25f, 0.35f), new Vector2(0.75f, 0.45f),
                new Vector2(0, 0), new Vector2(0, 0), 20, Color.red);

            var retryBtn = CreateButton(panel, "RetryButton", "Retry",
                new Vector2(0.4f, 0.2f), new Vector2(0.6f, 0.3f),
                new Vector2(0, 0), new Vector2(0, 0));

            var loadingPanel = new GameObject("LoadingPanel");
            loadingPanel.transform.SetParent(panel.transform, false);
            var loadingRt = loadingPanel.AddComponent<RectTransform>();
            loadingRt.anchorMin = Vector2.zero;
            loadingRt.anchorMax = Vector2.one;
            loadingRt.offsetMin = Vector2.zero;
            loadingRt.offsetMax = Vector2.zero;

            var contentPanel = new GameObject("ContentPanel");
            contentPanel.transform.SetParent(panel.transform, false);
            var contentRt = contentPanel.AddComponent<RectTransform>();
            contentRt.anchorMin = Vector2.zero;
            contentRt.anchorMax = Vector2.one;
            contentRt.offsetMin = Vector2.zero;
            contentRt.offsetMax = Vector2.zero;

            var errorPanel = new GameObject("ErrorPanel");
            errorPanel.transform.SetParent(panel.transform, false);
            var errorRt = errorPanel.AddComponent<RectTransform>();
            errorRt.anchorMin = Vector2.zero;
            errorRt.anchorMax = Vector2.one;
            errorRt.offsetMin = Vector2.zero;
            errorRt.offsetMax = Vector2.zero;

            var view = panel.AddComponent<BootstrapView>();
            view._statusText = status;
            view._errorText = errorText;
            view._retryButton = retryBtn;
            view._loadingPanel = loadingPanel;
            view._contentPanel = contentPanel;
            view._errorPanel = errorPanel;

            var prefabPath = Path.Combine(ViewsFolder, "BootstrapView.prefab");
            PrefabUtility.SaveAsPrefabAsset(canvas, prefabPath);
            Object.DestroyImmediate(canvas);
            Debug.Log($"[UIScreenFactory] Created {prefabPath}");
        }

        // ------------------------------------------------------------------
        // Main HUD View
        // ------------------------------------------------------------------
        private static void CreateMainHudView()
        {
            var canvas = CreateCanvas("MainHudCanvas");
            var panel = CreatePanel(canvas, "Panel", new Color(0f, 0f, 0f, 0.6f));

            var location = CreateText(panel, "LocationText", "Apartment",
                new Vector2(0.05f, 0.9f), new Vector2(0.4f, 0.97f),
                new Vector2(0, 0), new Vector2(0, 0), 22);

            var balance = CreateText(panel, "BalanceText", "₦0",
                new Vector2(0.6f, 0.9f), new Vector2(0.95f, 0.97f),
                new Vector2(0, 0), new Vector2(0, 0), 22, Color.yellow);

            var interactBtn = CreateButton(panel, "InteractionButton", "Interact",
                new Vector2(0.7f, 0.05f), new Vector2(0.95f, 0.15f),
                new Vector2(0, 0), new Vector2(0, 0));

            var promptPanel = new GameObject("InteractionPromptPanel");
            promptPanel.transform.SetParent(panel.transform, false);
            var promptRt = promptPanel.AddComponent<RectTransform>();
            promptRt.anchorMin = new Vector2(0.3f, 0.4f);
            promptRt.anchorMax = new Vector2(0.7f, 0.6f);
            promptRt.offsetMin = Vector2.zero;
            promptRt.offsetMax = Vector2.zero;

            var promptImg = promptPanel.AddComponent<Image>();
            promptImg.color = new Color(0f, 0f, 0f, 0.8f);

            var promptTxt = CreateText(promptPanel, "PromptText", "Press to interact",
                new Vector2(0f, 0f), new Vector2(1f, 1f),
                new Vector2(0, 0), new Vector2(0, 0), 20);

            var view = panel.AddComponent<MainHudView>();
            view._locationText = location;
            view._balanceText = balance;
            view._interactionButton = interactBtn;
            view._interactionPromptPanel = promptPanel;
            view._interactionPromptText = promptTxt;

            var prefabPath = Path.Combine(ViewsFolder, "MainHudView.prefab");
            PrefabUtility.SaveAsPrefabAsset(canvas, prefabPath);
            Object.DestroyImmediate(canvas);
            Debug.Log($"[UIScreenFactory] Created {prefabPath}");
        }

        // ------------------------------------------------------------------
        // Phone Home View
        // ------------------------------------------------------------------
        private static void CreatePhoneHomeView()
        {
            var canvas = CreateCanvas("PhoneHomeCanvas");
            var panel = CreatePanel(canvas, "Panel");

            var badge = CreateText(panel, "UnreadBadge", "3",
                new Vector2(0.8f, 0.85f), new Vector2(0.9f, 0.92f),
                new Vector2(0, 0), new Vector2(0, 0), 18, Color.red);

            var appGrid = new GameObject("AppGrid");
            appGrid.transform.SetParent(panel.transform, false);
            var gridRt = appGrid.AddComponent<RectTransform>();
            gridRt.anchorMin = new Vector2(0.1f, 0.2f);
            gridRt.anchorMax = new Vector2(0.9f, 0.8f);
            gridRt.offsetMin = Vector2.zero;
            gridRt.offsetMax = Vector2.zero;

            var gridLayout = appGrid.AddComponent<GridLayoutGroup>();
            gridLayout.cellSize = new Vector2(120, 120);
            gridLayout.spacing = new Vector2(20, 20);
            gridLayout.startCorner = GridLayoutGroup.StartCorner.UpperLeft;
            gridLayout.startAxis = GridLayoutGroup.StartAxis.Horizontal;
            gridLayout.childAlignment = TextAnchor.UpperLeft;

            var refreshBtn = CreateButton(panel, "RefreshButton", "Refresh",
                new Vector2(0.05f, 0.05f), new Vector2(0.15f, 0.12f),
                new Vector2(0, 0), new Vector2(0, 0));

            var view = panel.AddComponent<PhoneHomeView>();
            view._unreadBadgeText = badge;
            view._appGridParent = appGrid.transform;
            view._refreshButton = refreshBtn;

            var prefabPath = Path.Combine(ViewsFolder, "PhoneHomeView.prefab");
            PrefabUtility.SaveAsPrefabAsset(canvas, prefabPath);
            Object.DestroyImmediate(canvas);
            Debug.Log($"[UIScreenFactory] Created {prefabPath}");
        }

        // ------------------------------------------------------------------
        // Ping Messaging View
        // ------------------------------------------------------------------
        private static void CreatePingMessagingView()
        {
            var canvas = CreateCanvas("PingMessagingCanvas");
            var panel = CreatePanel(canvas, "Panel");

            var contactName = CreateText(panel, "ContactNameText", "Tunde",
                new Vector2(0.25f, 0.85f), new Vector2(0.75f, 0.92f),
                new Vector2(0, 0), new Vector2(0, 0), 26);

            var messageBody = CreateText(panel, "MessageBodyText", "You pulling up to the beach tonight?",
                new Vector2(0.1f, 0.55f), new Vector2(0.9f, 0.75f),
                new Vector2(0, 0), new Vector2(0, 0), 22);

            var responseParent = new GameObject("ResponseButtonParent");
            responseParent.transform.SetParent(panel.transform, false);
            var respRt = responseParent.AddComponent<RectTransform>();
            respRt.anchorMin = new Vector2(0.2f, 0.25f);
            respRt.anchorMax = new Vector2(0.8f, 0.5f);
            respRt.offsetMin = Vector2.zero;
            respRt.offsetMax = Vector2.zero;

            var respLayout = responseParent.AddComponent<VerticalLayoutGroup>();
            respLayout.spacing = 15;
            respLayout.childAlignment = TextAnchor.MiddleCenter;
            respLayout.childForceExpandWidth = true;
            respLayout.childForceExpandHeight = false;

            var sendBtn = CreateButton(panel, "SendButton", "Send",
                new Vector2(0.7f, 0.05f), new Vector2(0.95f, 0.15f),
                new Vector2(0, 0), new Vector2(0, 0));

            var inputField = new GameObject("InputField");
            inputField.transform.SetParent(panel.transform, false);
            var inputRt = inputField.AddComponent<RectTransform>();
            inputRt.anchorMin = new Vector2(0.05f, 0.05f);
            inputRt.anchorMax = new Vector2(0.65f, 0.15f);
            inputRt.offsetMin = Vector2.zero;
            inputRt.offsetMax = Vector2.zero;
            var inputImg = inputField.AddComponent<Image>();
            inputImg.color = new Color(0.2f, 0.2f, 0.25f, 1f);
            var input = inputField.AddComponent<TMP_InputField>();

            var refreshBtn = CreateButton(panel, "RefreshButton", "Refresh",
                new Vector2(0.05f, 0.05f), new Vector2(0.15f, 0.12f),
                new Vector2(0, 0), new Vector2(0, 0));

            var view = panel.AddComponent<PingMessagingView>();
            view._contactNameText = contactName;
            view._messageBodyText = messageBody;
            view._responseButtonParent = responseParent.transform;
            view._sendButton = sendBtn;
            view._inputField = input;
            view._refreshButton = refreshBtn;

            var prefabPath = Path.Combine(ViewsFolder, "PingMessagingView.prefab");
            PrefabUtility.SaveAsPrefabAsset(canvas, prefabPath);
            Object.DestroyImmediate(canvas);
            Debug.Log($"[UIScreenFactory] Created {prefabPath}");
        }

        // ------------------------------------------------------------------
        // Night Summary View
        // ------------------------------------------------------------------
        private static void CreateNightSummaryView()
        {
            var canvas = CreateCanvas("NightSummaryCanvas");
            var panel = CreatePanel(canvas, "Panel", new Color(0.05f, 0.05f, 0.1f, 0.95f));

            var spent = CreateText(panel, "SpentText", "₦25,000",
                new Vector2(0.25f, 0.7f), new Vector2(0.75f, 0.78f),
                new Vector2(0, 0), new Vector2(0, 0), 28, Color.yellow);

            var people = CreateText(panel, "PeopleMetText", "3 people met",
                new Vector2(0.25f, 0.6f), new Vector2(0.75f, 0.68f),
                new Vector2(0, 0), new Vector2(0, 0), 22);

            var bestMoment = CreateText(panel, "BestMomentText", "Dancing at the club with friends",
                new Vector2(0.2f, 0.45f), new Vector2(0.8f, 0.55f),
                new Vector2(0, 0), new Vector2(0, 0), 20);

            var continueBtn = CreateButton(panel, "ContinueButton", "Continue",
                new Vector2(0.3f, 0.2f), new Vector2(0.45f, 0.3f),
                new Vector2(0, 0), new Vector2(0, 0));

            var skipBtn = CreateButton(panel, "SkipButton", "Skip",
                new Vector2(0.55f, 0.2f), new Vector2(0.7f, 0.3f),
                new Vector2(0, 0), new Vector2(0, 0));

            var retryBtn = CreateButton(panel, "RetryButton", "Retry",
                new Vector2(0.4f, 0.1f), new Vector2(0.6f, 0.18f),
                new Vector2(0, 0), new Vector2(0, 0));

            var view = panel.AddComponent<NightSummaryView>();
            view._spentText = spent;
            view._peopleMetText = people;
            view._bestMomentText = bestMoment;
            view._continueButton = continueBtn;
            view._skipButton = skipBtn;
            view._retryButton = retryBtn;

            var prefabPath = Path.Combine(ViewsFolder, "NightSummaryView.prefab");
            PrefabUtility.SaveAsPrefabAsset(canvas, prefabPath);
            Object.DestroyImmediate(canvas);
            Debug.Log($"[UIScreenFactory] Created {prefabPath}");
        }

        // ------------------------------------------------------------------
        // Bottom Navigation View
        // ------------------------------------------------------------------
        private static void CreateBottomNavigationView()
        {
            var canvas = CreateCanvas("BottomNavigationCanvas");
            var panel = CreatePanel(canvas, "Panel");

            var homeBtn = CreateButton(panel, "HomeButton", "Home",
                new Vector2(0.1f, 0.1f), new Vector2(0.2f, 0.25f),
                new Vector2(0, 0), new Vector2(0, 0));
            var socialBtn = CreateButton(panel, "SocialButton", "Social",
                new Vector2(0.3f, 0.1f), new Vector2(0.4f, 0.25f),
                new Vector2(0, 0), new Vector2(0, 0));
            var cityBtn = CreateButton(panel, "CityButton", "City",
                new Vector2(0.5f, 0.1f), new Vector2(0.6f, 0.25f),
                new Vector2(0, 0), new Vector2(0, 0));
            var lifeBtn = CreateButton(panel, "LifeButton", "Life",
                new Vector2(0.7f, 0.1f), new Vector2(0.8f, 0.25f),
                new Vector2(0, 0), new Vector2(0, 0));
            var profileBtn = CreateButton(panel, "ProfileButton", "Profile",
                new Vector2(0.9f, 0.1f), new Vector2(1f, 0.25f),
                new Vector2(0, 0), new Vector2(0, 0));

            var badge = CreateText(panel, "BadgeText", "0",
                new Vector2(0.95f, 0.95f), new Vector2(1f, 1f),
                new Vector2(-30, -10), new Vector2(0, 0), 14, Color.yellow);

            var view = panel.AddComponent<BottomNavigationView>();
            view._tabButtonParent = panel.transform;
            view._badgeText = badge;

            var prefabPath = Path.Combine(ViewsFolder, "BottomNavigationView.prefab");
            PrefabUtility.SaveAsPrefabAsset(canvas, prefabPath);
            Object.DestroyImmediate(canvas);
            Debug.Log($"[UIScreenFactory] Created {prefabPath}");
        }

        // ------------------------------------------------------------------
        // Configuration View
        // ------------------------------------------------------------------
        private static void CreateConfigurationView()
        {
            var canvas = CreateCanvas("ConfigurationCanvas");
            var panel = CreatePanel(canvas, "Panel");

            var configText = CreateText(panel, "ConfigText", "No configuration errors detected.",
                new Vector2(0.3f, 0.5f), new Vector2(0.7f, 0.6f),
                new Vector2(0, 0), new Vector2(0, 0), 24);

            var refreshBtn = CreateButton(panel, "RefreshButton", "Refresh",
                new Vector2(0.4f, 0.1f), new Vector2(0.6f, 0.2f),
                new Vector2(0, 0), new Vector2(0, 0));

            var retryBtn = CreateButton(panel, "RetryButton", "Retry",
                new Vector2(0.35f, 0.05f), new Vector2(0.65f, 0.15f),
                new Vector2(0, 0), new Vector2(0, 0));

            var view = panel.AddComponent<ConfigurationView>();
            view._statusText = configText;
            view._refreshButton = refreshBtn;
            view._retryButton = retryBtn;

            var prefabPath = Path.Combine(ViewsFolder, "ConfigurationView.prefab");
            PrefabUtility.SaveAsPrefabAsset(canvas, prefabPath);
            Object.DestroyImmediate(canvas);
            Debug.Log($"[UIScreenFactory] Created {prefabPath}");
        }

        // ------------------------------------------------------------------
        // Auth View
        // ------------------------------------------------------------------
        private static void CreateAuthView()
        {
            var canvas = CreateCanvas("AuthCanvas");
            var panel = CreatePanel(canvas, "Panel", new Color(0.05f, 0.05f, 0.1f, 0.95f));

            var title = CreateText(panel, "TitleText", "Sign In",
                new Vector2(0.3f, 0.75f), new Vector2(0.7f, 0.85f),
                new Vector2(0, 0), new Vector2(0, 0), 36);

            var subtitle = CreateText(panel, "SubtitleText", "Enter your credentials to continue",
                new Vector2(0.3f, 0.65f), new Vector2(0.7f, 0.75f),
                new Vector2(0, 0), new Vector2(0, 0), 18, Color.grey);

            var emailInput = new GameObject("EmailInput");
            emailInput.transform.SetParent(panel.transform, false);
            var emailRt = emailInput.AddComponent<RectTransform>();
            emailRt.anchorMin = new Vector2(0.15f, 0.4f);
            emailRt.anchorMax = new Vector2(0.85f, 0.5f);
            emailRt.offsetMin = Vector2.zero;
            emailRt.offsetMax = Vector2.zero;
            var emailImg = emailInput.AddComponent<Image>();
            emailImg.color = new Color(0.2f, 0.2f, 0.25f, 1f);
            var emailInputComp = emailInput.AddComponent<TMP_InputField>();

            var passInput = new GameObject("PasswordInput");
            passInput.transform.SetParent(panel.transform, false);
            var passRt = passInput.AddComponent<RectTransform>();
            passRt.anchorMin = new Vector2(0.15f, 0.3f);
            passRt.anchorMax = new Vector2(0.85f, 0.4f);
            passRt.offsetMin = Vector2.zero;
            passRt.offsetMax = Vector2.zero;
            var passImg = passInput.AddComponent<Image>();
            passImg.color = new Color(0.2f, 0.2f, 0.25f, 1f);
            var passInputComp = passInput.AddComponent<TMP_InputField>();

            var signUpBtn = CreateButton(panel, "SignUpButton", "Create Account",
                new Vector2(0.3f, 0.08f), new Vector2(0.55f, 0.18f),
                new Vector2(0, 0), new Vector2(0, 0));
            var signInBtn = CreateButton(panel, "SignInButton", "Sign In",
                new Vector2(0.45f, 0.03f), new Vector2(0.55f, 0.13f),
                new Vector2(0, 0), new Vector2(0, 0));

            var view = panel.AddComponent<AuthView>();
            view._emailInput = emailInputComp;
            view._passwordInput = passInputComp;
            view._signUpButton = signUpBtn;
            view._signInButton = signInBtn;
            view._restoreButton = signInBtn;
            view._authPanel = authPanel;

            var prefabPath = Path.Combine(ViewsFolder, "AuthView.prefab");
            PrefabUtility.SaveAsPrefabAsset(canvas, prefabPath);
            Object.DestroyImmediate(canvas);
            Debug.Log($"[UIScreenFactory] Created {prefabPath}");
        }
    }
}
#endif
