using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace NAAD.Editor
{
    public static class CreateScreenPrefabs
    {
        private const string Dir = "Assets/NAAD/Prefabs/UI/Screens";

        [MenuItem("NAAD/UI/Create All Screen Prefabs")]
        public static void CreateAllScreenPrefabs()
        {
            Debug.Log("[NAAD][UI] Creating all MVP screen prefabs...");
            EnsureDirs();
            CreateBootstrapScreen();
            CreateMainHUDScreen();
            CreatePhoneHomeScreen();
            CreateScreenPrefabsExtras.CreatePingMessagingScreen();
            CreateScreenPrefabsExtras.CreateNightSummaryScreen();
            CreateScreenPrefabsExtras.CreateBottomNavigationScreen();
            AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
            Debug.Log("[NAAD][UI] All MVP screen prefabs created!");
        }

        private static void EnsureDirs()
        {
            foreach (var d in new[] { "Assets/NAAD/Prefabs", "Assets/NAAD/Prefabs/UI", Dir })
                if (!AssetDatabase.IsValidFolder(d))
                    AssetDatabase.CreateFolder(System.IO.Path.GetDirectoryName(d), System.IO.Path.GetFileName(d));
        }

        private static void CreateBootstrapScreen()
        {
            var r = new GameObject("BootstrapScreen");
            r.AddComponent<Canvas>();
            var cs = r.AddComponent<CanvasScaler>();
            cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            cs.referenceResolution = new Vector2(1080, 1920);
            cs.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            cs.matchWidthOrHeight = 0.5f;
            r.AddComponent<GraphicRaycaster>();

            var bg = new GameObject("Background", typeof(Image));
            bg.transform.SetParent(r.transform, false);
            bg.GetComponent<Image>().color = new Color(0.05f, 0.05f, 0.07f, 1f);
            var br = bg.GetComponent<RectTransform>();
            br.anchorMin = Vector2.zero; br.anchorMax = Vector2.one;
            br.offsetMin = Vector2.zero; br.offsetMax = Vector2.zero;

            var sc = new GameObject("LoadingSpinner", typeof(RectTransform));
            sc.transform.SetParent(r.transform, false);
            var scr = sc.GetComponent<RectTransform>();
            scr.anchorMin = scr.anchorMax = new Vector2(0.5f, 0.5f);
            scr.anchoredPosition = Vector2.zero; scr.sizeDelta = new Vector2(120, 120);

            var sp = new GameObject("Spinner", typeof(Image));
            sp.transform.SetParent(sc.transform, false);
            sp.GetComponent<Image>().color = new Color(1f, 0.85f, 0.1f, 1f);
            var sr = sp.GetComponent<RectTransform>();
            sr.anchorMin = Vector2.zero; sr.anchorMax = Vector2.one;
            sr.offsetMin = Vector2.zero; sr.offsetMax = Vector2.zero;

            var lt = new GameObject("LoadingText", typeof(Text));
            lt.transform.SetParent(r.transform, false);
            var ltc = lt.GetComponent<Text>();
            ltc.text = "Loading Naija After Dark...";
            ltc.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            ltc.fontSize = 24; ltc.color = Color.white;
            ltc.alignment = TextAnchor.MiddleCenter;
            var ltr = lt.GetComponent<RectTransform>();
            ltr.anchorMin = ltr.anchorMax = new Vector2(0.5f, 0.5f);
            ltr.anchoredPosition = new Vector2(0, -100); ltr.sizeDelta = new Vector2(400, 40);

            var ep = new GameObject("ErrorPanel", typeof(Image), typeof(RectTransform));
            ep.transform.SetParent(r.transform, false); ep.SetActive(false);
            ep.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.15f, 0.95f);
            var epr = ep.GetComponent<RectTransform>();
            epr.anchorMin = new Vector2(0.1f, 0.3f); epr.anchorMax = new Vector2(0.9f, 0.7f);
            epr.offsetMin = Vector2.zero; epr.offsetMax = Vector2.zero;

            var et = new GameObject("ErrorText", typeof(Text));
            et.transform.SetParent(ep.transform, false);
            var etc = et.GetComponent<Text>();
            etc.text = "Failed to initialize. Check your connection and retry.";
            etc.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            etc.fontSize = 20; etc.color = new Color(1f, 0.4f, 0.4f);
            etc.alignment = TextAnchor.MiddleCenter;
            var etr = et.GetComponent<RectTransform>();
            etr.anchorMin = new Vector2(0.1f, 0.4f); etr.anchorMax = new Vector2(0.9f, 0.8f);
            etr.offsetMin = Vector2.zero; etr.offsetMax = Vector2.zero;

            var rb = new GameObject("RetryButton", typeof(Button), typeof(Image), typeof(RectTransform));
            rb.transform.SetParent(ep.transform, false);
            rb.GetComponent<Image>().color = new Color(1f, 0.85f, 0.1f);
            var rbr = rb.GetComponent<RectTransform>();
            rbr.anchorMin = new Vector2(0.3f, 0.15f); rbr.anchorMax = new Vector2(0.7f, 0.35f);
            rbr.offsetMin = Vector2.zero; rbr.offsetMax = Vector2.zero;

            var bt = new GameObject("Text", typeof(Text));
            bt.transform.SetParent(rb.transform, false);
            var btc = bt.GetComponent<Text>();
            btc.text = "RETRY";
            btc.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            btc.fontSize = 22; btc.color = new Color(0.05f, 0.05f, 0.07f);
            btc.alignment = TextAnchor.MiddleCenter;
            var btr = bt.GetComponent<RectTransform>();
            btr.anchorMin = Vector2.zero; btr.anchorMax = Vector2.one;
            btr.offsetMin = Vector2.zero; btr.offsetMax = Vector2.zero;

            r.AddComponent<NAAD.UI.BootstrapScreenController>();
            Save(r, "BootstrapScreen");
        }

        private static void Save(GameObject r, string n) { PrefabUtility.SaveAsPrefabAsset(r, Dir + "/" + n + ".prefab"); Object.DestroyImmediate(r); }
    }
}
        private static void CreateMainHUDScreen()
        {
            var r = new GameObject("MainHUDScreen");
            var c = r.AddComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay; c.sortingOrder = 10;
            var cs = r.AddComponent<CanvasScaler>();
            cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            cs.referenceResolution = new Vector2(1080, 1920);
            cs.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            cs.matchWidthOrHeight = 0.5f;
            r.AddComponent<GraphicRaycaster>();

            var tb = new GameObject("TopBar", typeof(Image), typeof(RectTransform));
            tb.transform.SetParent(r.transform, false);
            tb.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.7f);
            var tbr = tb.GetComponent<RectTransform>();
            tbr.anchorMin = new Vector2(0, 1); tbr.anchorMax = new Vector2(1, 1);
            tbr.pivot = new Vector2(0.5f, 1); tbr.anchoredPosition = Vector2.zero;
            tbr.sizeDelta = new Vector2(0, 120);

            var loc = new GameObject("LocationText", typeof(Text));
            loc.transform.SetParent(tb.transform, false);
            var lc = loc.GetComponent<Text>();
            lc.text = "Victoria Island, Lagos";
            lc.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            lc.fontSize = 22; lc.color = Color.white;
            lc.alignment = TextAnchor.MiddleLeft;
            var lcr = loc.GetComponent<RectTransform>();
            lcr.anchorMin = new Vector2(0.05f, 0.2f); lcr.anchorMax = new Vector2(0.6f, 0.8f);
            lcr.offsetMin = Vector2.zero; lcr.offsetMax = Vector2.zero;

            var tm = new GameObject("TimeText", typeof(Text));
            tm.transform.SetParent(tb.transform, false);
            var tmc = tm.GetComponent<Text>();
            tmc.text = "23:42";
            tmc.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            tmc.fontSize = 28; tmc.color = new Color(1f, 0.85f, 0.1f);
            tmc.alignment = TextAnchor.MiddleRight;
            var tmr = tm.GetComponent<RectTransform>();
            tmr.anchorMin = new Vector2(0.6f, 0.2f); tmr.anchorMax = new Vector2(0.95f, 0.8f);
            tmr.offsetMin = Vector2.zero; tmr.offsetMax = Vector2.zero;

            var bp = new GameObject("BalancePanel", typeof(Image), typeof(RectTransform));
            bp.transform.SetParent(r.transform, false);
            bp.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.6f);
            var bpr = bp.GetComponent<RectTransform>();
            bpr.anchorMin = bpr.anchorMax = new Vector2(0, 0);
            bpr.pivot = new Vector2(0, 0); bpr.anchoredPosition = new Vector2(20, 20);
            bpr.sizeDelta = new Vector2(200, 60);

            var bt = new GameObject("BalanceText", typeof(Text));
            bt.transform.SetParent(bp.transform, false);
            var btc = bt.GetComponent<Text>();
            btc.text = "\u20a6125,000";
            btc.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            btc.fontSize = 24; btc.color = new Color(1f, 0.85f, 0.1f);
            btc.alignment = TextAnchor.MiddleLeft;
            var btr = bt.GetComponent<RectTransform>();
            btr.anchorMin = new Vector2(0.1f, 0); btr.anchorMax = new Vector2(1, 1);
            btr.offsetMin = Vector2.zero; btr.offsetMax = Vector2.zero;

            var pp = new GameObject("InteractionPrompt", typeof(Image), typeof(RectTransform));
            pp.transform.SetParent(r.transform, false); pp.SetActive(false);
            pp.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.8f);
            var ppr = pp.GetComponent<RectTransform>();
            ppr.anchorMin = ppr.anchorMax = new Vector2(0.5f, 0);
            ppr.pivot = new Vector2(0.5f, 0); ppr.anchoredPosition = new Vector2(0, 180);
            ppr.sizeDelta = new Vector2(500, 80);

            var pt = new GameObject("PromptText", typeof(Text));
            pt.transform.SetParent(pp.transform, false);
            var ptc = pt.GetComponent<Text>();
            ptc.text = "Press to interact";
            ptc.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            ptc.fontSize = 24; ptc.color = Color.white;
            ptc.alignment = TextAnchor.MiddleCenter;
            var ptr = pt.GetComponent<RectTransform>();
            ptr.anchorMin = Vector2.zero; ptr.anchorMax = Vector2.one;
            ptr.offsetMin = Vector2.zero; ptr.offsetMax = Vector2.zero;

            var pb = new GameObject("PhoneButton", typeof(Button), typeof(Image), typeof(RectTransform));
            pb.transform.SetParent(r.transform, false);
            pb.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.15f, 0.9f);
            var pbr = pb.GetComponent<RectTransform>();
            pbr.anchorMin = pbr.anchorMax = new Vector2(1, 0);
            pbr.pivot = new Vector2(1, 0); pbr.anchoredPosition = new Vector2(-20, 20);
            pbr.sizeDelta = new Vector2(80, 80);

            var pi = new GameObject("PhoneIcon", typeof(Text));
            pi.transform.SetParent(pb.transform, false);
            var pic = pi.GetComponent<Text>();
            pic.text = "\ud83d\udcf1";
            pic.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            pic.fontSize = 36; pic.alignment = TextAnchor.MiddleCenter;
            var pir = pi.GetComponent<RectTransform>();
            pir.anchorMin = Vector2.zero; pir.anchorMax = Vector2.one;
            pir.offsetMin = Vector2.zero; pir.offsetMax = Vector2.zero;

            r.AddComponent<NAAD.UI.MainHudController>();
            Save(r, "MainHUDScreen");
        }

        private static void CreatePhoneHomeScreen()
        {
            var r = new GameObject("PhoneHomeScreen");
            var c = r.AddComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay; c.sortingOrder = 20;
            var cs = r.AddComponent<CanvasScaler>();
            cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            cs.referenceResolution = new Vector2(1080, 1920);
            cs.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            cs.matchWidthOrHeight = 0.5f;
            r.AddComponent<GraphicRaycaster>();

            var fr = new GameObject("PhoneFrame", typeof(Image));
            fr.transform.SetParent(r.transform, false);
            fr.GetComponent<Image>().color = new Color(0.05f, 0.05f, 0.07f, 1f);
            var frr = fr.GetComponent<RectTransform>();
            frr.anchorMin = Vector2.zero; frr.anchorMax = Vector2.one;
            frr.offsetMin = Vector2.zero; frr.offsetMax = Vector2.zero;

            var sb = new GameObject("StatusBar", typeof(Image), typeof(RectTransform));
            sb.transform.SetParent(fr.transform, false);
            sb.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.8f);
            var sbr = sb.GetComponent<RectTransform>();
            sbr.anchorMin = new Vector2(0, 1); sbr.anchorMax = new Vector2(1, 1);
            sbr.pivot = new Vector2(0.5f, 1); sbr.anchoredPosition = Vector2.zero;
            sbr.sizeDelta = new Vector2(0, 60);

            var ag = new GameObject("AppGrid", typeof(RectTransform), typeof(GridLayoutGroup));
            ag.transform.SetParent(fr.transform, false);
            var gl = ag.GetComponent<GridLayoutGroup>();
            gl.cellSize = new Vector2(160, 160); gl.spacing = new Vector2(20, 20);
            gl.padding = new RectOffset(40, 40, 100, 40);
            gl.startAxis = GridLayoutGroup.Axis.Horizontal;
            gl.childAlignment = TextAnchor.UpperCenter;
            gl.constraint = GridLayoutGroup.Constraint.FixedColumnCount; gl.constraintCount = 3;

            var agr = ag.GetComponent<RectTransform>();
            agr.anchorMin = Vector2.zero; agr.anchorMax = Vector2.one;
            agr.offsetMin = Vector2.zero; agr.offsetMax = Vector2.zero;

            string[] apps = { "Ping", "NaijaGram", "NightPulse", "QuickCash", "RideNow", "MarketHub", "MetroMap", "Music", "Settings" };
            Color[] cols = { new Color(0.2f, 0.6f, 1f), new Color(0.9f, 0.3f, 0.5f), new Color(0.8f, 0.2f, 0.8f),
                new Color(0.2f, 0.8f, 0.4f), new Color(1f, 0.5f, 0.1f), new Color(0.6f, 0.4f, 0.9f),
                new Color(0.1f, 0.7f, 0.7f), new Color(0.9f, 0.2f, 0.3f), new Color(0.5f, 0.5f, 0.6f) };

            for (int i = 0; i < apps.Length; i++) CreateAppIcon(ag.transform, apps[i], cols[i], i == 0);

            r.AddComponent<NAAD.UI.PhoneHomeController>();
            Save(r, "PhoneHomeScreen");
        }

        private static void CreateAppIcon(Transform p, string n, Color c, bool badge = false)
        {
            var i = new GameObject("App_" + n, typeof(Button), typeof(Image), typeof(RectTransform));
            i.transform.SetParent(p, false);
            i.GetComponent<Image>().color = c;

            var l = new GameObject("Label", typeof(Text));
            l.transform.SetParent(i.transform, false);
            var lc = l.GetComponent<Text>();
            lc.text = n; lc.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            lc.fontSize = 18; lc.color = Color.white;
            lc.alignment = TextAnchor.MiddleCenter;
            var lr = l.GetComponent<RectTransform>();
            lr.anchorMin = new Vector2(0, 0); lr.anchorMax = new Vector2(1, 0);
            lr.pivot = new Vector2(0.5f, 0); lr.anchoredPosition = new Vector2(0, -25);
            lr.sizeDelta = new Vector2(0, 30);

            if (badge)
            {
                var b = new GameObject("Badge", typeof(Image), typeof(RectTransform));
                b.transform.SetParent(i.transform, false);
                b.GetComponent<Image>().color = Color.red;
                var br = b.GetComponent<RectTransform>();
                br.anchorMin = br.anchorMax = new Vector2(1, 1);
                br.pivot = new Vector2(0.5f, 0.5f); br.anchoredPosition = new Vector2(-10, -10);
                br.sizeDelta = new Vector2(24, 24);

                var bt = new GameObject("Count", typeof(Text));
                bt.transform.SetParent(b.transform, false);
                var btc = bt.GetComponent<Text>();
                btc.text = "3"; btc.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                btc.fontSize = 12; btc.color = Color.white;
                btc.alignment = TextAnchor.MiddleCenter;
                var btr = bt.GetComponent<RectTransform>();
                btr.anchorMin = Vector2.zero; btr.anchorMax = Vector2.one;
                btr.offsetMin = Vector2.zero; btr.offsetMax = Vector2.zero;
            }
        }
