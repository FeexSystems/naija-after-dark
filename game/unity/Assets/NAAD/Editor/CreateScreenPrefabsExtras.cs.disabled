using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace NAAD.Editor
{
    public static class CreateScreenPrefabsExtras
    {
        private const string Dir = "Assets/NAAD/Prefabs/UI/Screens";

        public static void CreatePingMessagingScreen()
        {
            var r = new GameObject("PingMessagingScreen");
            var c = r.AddComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay; c.sortingOrder = 30;
            var cs = r.AddComponent<CanvasScaler>();
            cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            cs.referenceResolution = new Vector2(1080, 1920);
            cs.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            cs.matchWidthOrHeight = 0.5f;
            r.AddComponent<GraphicRaycaster>();

            var bg = new GameObject("Background", typeof(Image));
            bg.transform.SetParent(r.transform, false);
            bg.GetComponent<Image>().color = new Color(0.05f, 0.05f, 0.07f, 1f);
            var bgr = bg.GetComponent<RectTransform>();
            bgr.anchorMin = Vector2.zero; bgr.anchorMax = Vector2.one;
            bgr.offsetMin = Vector2.zero; bgr.offsetMax = Vector2.zero;

            var hdr = new GameObject("Header", typeof(Image), typeof(RectTransform));
            hdr.transform.SetParent(r.transform, false);
            hdr.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.15f, 1f);
            var hdrr = hdr.GetComponent<RectTransform>();
            hdrr.anchorMin = new Vector2(0, 1); hdrr.anchorMax = new Vector2(1, 1);
            hdrr.pivot = new Vector2(0.5f, 1); hdrr.anchoredPosition = Vector2.zero;
            hdrr.sizeDelta = new Vector2(0, 100);

            var ht = new GameObject("Title", typeof(Text));
            ht.transform.SetParent(hdr.transform, false);
            var htc = ht.GetComponent<Text>();
            htc.text = "Ping Messages"; htc.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            htc.fontSize = 28; htc.color = new Color(1f, 0.85f, 0.1f);
            htc.alignment = TextAnchor.MiddleLeft;
            var htr = ht.GetComponent<RectTransform>();
            htr.anchorMin = new Vector2(0.05f, 0.2f); htr.anchorMax = new Vector2(0.95f, 0.8f);
            htr.offsetMin = Vector2.zero; htr.offsetMax = Vector2.zero;

            var lst = new GameObject("MessagesList", typeof(RectTransform), typeof(VerticalLayoutGroup));
            lst.transform.SetParent(r.transform, false);
            var vl = lst.GetComponent<VerticalLayoutGroup>();
            vl.spacing = 8; vl.padding = new RectOffset(20, 20, 20, 20);
            vl.childControlWidth = true; vl.childForceExpandWidth = true;
            var lstr = lst.GetComponent<RectTransform>();
            lstr.anchorMin = new Vector2(0, 0); lstr.anchorMax = new Vector2(1, 1);
            lstr.offsetMin = new Vector2(0, 100); lstr.offsetMax = new Vector2(0, -20);
            lst.AddComponent<ScrollRect>();

            r.AddComponent<NAAD.UI.PingMessagingController>();
            Save(r, "PingMessagingScreen");
        }

        public static void CreateNightSummaryScreen()
        {
            var r = new GameObject("NightSummaryScreen");
            var c = r.AddComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay; c.sortingOrder = 40;
            var cs = r.AddComponent<CanvasScaler>();
            cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            cs.referenceResolution = new Vector2(1080, 1920);
            cs.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            cs.matchWidthOrHeight = 0.5f;
            r.AddComponent<GraphicRaycaster>();

            var bg = new GameObject("Background", typeof(Image));
            bg.transform.SetParent(r.transform, false);
            bg.GetComponent<Image>().color = new Color(0.03f, 0.03f, 0.05f, 1f);
            var bgr = bg.GetComponent<RectTransform>();
            bgr.anchorMin = Vector2.zero; bgr.anchorMax = Vector2.one;
            bgr.offsetMin = Vector2.zero; bgr.offsetMax = Vector2.zero;

            var tt = new GameObject("Title", typeof(Text));
            tt.transform.SetParent(r.transform, false);
            var ttc = tt.GetComponent<Text>();
            ttc.text = "Night Summary"; ttc.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            ttc.fontSize = 36; ttc.color = new Color(1f, 0.85f, 0.1f);
            ttc.alignment = TextAnchor.MiddleCenter;
            var ttr = tt.GetComponent<RectTransform>();
            ttr.anchorMin = new Vector2(0.1f, 0.85f); ttr.anchorMax = new Vector2(0.9f, 0.95f);
            ttr.offsetMin = Vector2.zero; ttr.offsetMax = Vector2.zero;

            var sg = new GameObject("StatsGrid", typeof(RectTransform), typeof(GridLayoutGroup));
            sg.transform.SetParent(r.transform, false);
            var gl = sg.GetComponent<GridLayoutGroup>();
            gl.cellSize = new Vector2(200, 150); gl.spacing = new Vector2(16, 16);
            gl.padding = new RectOffset(40, 40, 20, 20);
            gl.startAxis = GridLayoutGroup.Axis.Horizontal;
            gl.childAlignment = TextAnchor.UpperCenter;
            gl.constraint = GridLayoutGroup.Constraint.FixedColumnCount; gl.constraintCount = 2;

            var sgr = sg.GetComponent<RectTransform>();
            sgr.anchorMin = new Vector2(0, 0.3f); sgr.anchorMax = new Vector2(1, 0.8f);
            sgr.offsetMin = Vector2.zero; sgr.offsetMax = Vector2.zero;

            string[] labels = { "Venues Visited", "Drinks Bought", "People Met", "Money Spent", "Distance", "Rating" };
            string[] vals = { "5", "12", "8", "\u20a645,000", "12.3 km", "\u2b50 4.8" };
            for (int i = 0; i < labels.Length; i++) CreateStatCard(sg.transform, labels[i], vals[i]);

            var cb = new GameObject("CloseButton", typeof(Button), typeof(Image), typeof(RectTransform));
            cb.transform.SetParent(r.transform, false);
            cb.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.25f, 1f);
            var cbr = cb.GetComponent<RectTransform>();
            cbr.anchorMin = new Vector2(0.2f, 0.05f); cbr.anchorMax = new Vector2(0.8f, 0.15f);
            cbr.offsetMin = Vector2.zero; cbr.offsetMax = Vector2.zero;

            var ct = new GameObject("Text", typeof(Text));
            ct.transform.SetParent(cb.transform, false);
            var ctc = ct.GetComponent<Text>();
            ctc.text = "CONTINUE"; ctc.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            ctc.fontSize = 24; ctc.color = new Color(1f, 0.85f, 0.1f);
            ctc.alignment = TextAnchor.MiddleCenter;
            var ctr = ct.GetComponent<RectTransform>();
            ctr.anchorMin = Vector2.zero; ctr.anchorMax = Vector2.one;
            ctr.offsetMin = Vector2.zero; ctr.offsetMax = Vector2.zero;

            r.AddComponent<NAAD.UI.NightSummaryController>();
            Save(r, "NightSummaryScreen");
        }

        private static void CreateStatCard(Transform p, string lbl, string val)
        {
            var c = new GameObject("Stat_" + lbl, typeof(Image), typeof(RectTransform));
            c.transform.SetParent(p, false);
            c.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.15f, 1f);

            var l = new GameObject("Label", typeof(Text));
            l.transform.SetParent(c.transform, false);
            var lc = l.GetComponent<Text>();
            lc.text = lbl; lc.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            lc.fontSize = 16; lc.color = new Color(0.7f, 0.7f, 0.75f);
            lc.alignment = TextAnchor.LowerCenter;
            var lr = l.GetComponent<RectTransform>();
            lr.anchorMin = new Vector2(0.1f, 0.1f); lr.anchorMax = new Vector2(0.9f, 0.4f);
            lr.offsetMin = Vector2.zero; lr.offsetMax = Vector2.zero;

            var v = new GameObject("Value", typeof(Text));
            v.transform.SetParent(c.transform, false);
            var vc = v.GetComponent<Text>();
            vc.text = val; vc.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            vc.fontSize = 28; vc.color = new Color(1f, 0.85f, 0.1f);
            vc.alignment = TextAnchor.UpperCenter;
            var vr = v.GetComponent<RectTransform>();
            vr.anchorMin = new Vector2(0.1f, 0.45f); vr.anchorMax = new Vector2(0.9f, 0.9f);
            vr.offsetMin = Vector2.zero; vr.offsetMax = Vector2.zero;
        }

        public static void CreateBottomNavigationScreen()
        {
            var r = new GameObject("BottomNavigationScreen");
            var c = r.AddComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay; c.sortingOrder = 50;
            var cs = r.AddComponent<CanvasScaler>();
            cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            cs.referenceResolution = new Vector2(1080, 1920);
            cs.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            cs.matchWidthOrHeight = 0.5f;
            r.AddComponent<GraphicRaycaster>();

            var nb = new GameObject("NavBar", typeof(Image), typeof(RectTransform));
            nb.transform.SetParent(r.transform, false);
            nb.GetComponent<Image>().color = new Color(0.08f, 0.08f, 0.1f, 0.95f);
            var nbr = nb.GetComponent<RectTransform>();
            nbr.anchorMin = new Vector2(0, 0); nbr.anchorMax = new Vector2(1, 0);
            nbr.pivot = new Vector2(0.5f, 0); nbr.anchoredPosition = Vector2.zero;
            nbr.sizeDelta = new Vector2(0, 100);

            var ni = new GameObject("NavItems", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            ni.transform.SetParent(nb.transform, false);
            var hl = ni.GetComponent<HorizontalLayoutGroup>();
            hl.spacing = 0; hl.padding = new RectOffset(0, 0, 10, 10);
            hl.childAlignment = TextAnchor.MiddleCenter;
            hl.childControlWidth = hl.childControlHeight = true;
            hl.childForceExpandWidth = hl.childForceExpandHeight = true;

            var nir = ni.GetComponent<RectTransform>();
            nir.anchorMin = Vector2.zero; nir.anchorMax = Vector2.one;
            nir.offsetMin = Vector2.zero; nir.offsetMax = Vector2.zero;

            string[] labs = { "Home", "Social", "City", "Life", "Profile" };
            string[] icons = { "\ud83c\udfe0", "\ud83d\udc65", "\ud83d\udfd9", "\u2764", "\ud83d\udc64" };
            for (int i = 0; i < labs.Length; i++) CreateNavItem(ni.transform, labs[i], icons[i], i == 0);

            r.AddComponent<NAAD.UI.BottomNavigationController>();
            Save(r, "BottomNavigationScreen");
        }

        private static void CreateNavItem(Transform p, string lbl, string icn, bool sel)
        {
            var i = new GameObject("Nav_" + lbl, typeof(Button), typeof(RectTransform));
            i.transform.SetParent(p, false);
            var vl = i.AddComponent<VerticalLayoutGroup>();
            vl.spacing = 4; vl.childAlignment = TextAnchor.MiddleCenter;
            vl.childControlWidth = vl.childControlHeight = true;

            var ic = new GameObject("Icon", typeof(Text));
            ic.transform.SetParent(i.transform, false);
            var icc = ic.GetComponent<Text>();
            icc.text = icn; icc.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            icc.fontSize = 28; icc.color = sel ? new Color(1f, 0.85f, 0.1f) : new Color(0.6f, 0.6f, 0.65f);
            icc.alignment = TextAnchor.MiddleCenter;
            var icr = ic.GetComponent<RectTransform>(); icr.sizeDelta = new Vector2(0, 36);

            var lb = new GameObject("Label", typeof(Text));
            lb.transform.SetParent(i.transform, false);
            var lbc = lb.GetComponent<Text>();
            lbc.text = lbl; lbc.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            lbc.fontSize = 12; lbc.color = sel ? new Color(1f, 0.85f, 0.1f) : new Color(0.6f, 0.6f, 0.65f);
            lbc.alignment = TextAnchor.MiddleCenter;
            var lbr = lb.GetComponent<RectTransform>(); lbr.sizeDelta = new Vector2(0, 20);
        }

        private static void Save(GameObject r, string n)
        {
            PrefabUtility.SaveAsPrefabAsset(r, Dir + "/" + n + ".prefab");
            Object.DestroyImmediate(r);
        }
    }
}
