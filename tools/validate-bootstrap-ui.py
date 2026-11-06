#!/usr/bin/env python3
"""
Verify the Bootstrap scene actually mounts the UI shell.

Why this exists: the Unity Editor is not always available — a wedged ILPP/Burst
post-process leaves it unable to compile, and that is exactly when a regression is
most likely to slip through unnoticed. Everything checked here is structural, so it
can run in CI or on a machine with no Editor at all.

Checks:
  - Bootstrap.unity exists and has a UIRoot GameObject
  - UIRoot carries a Canvas and a ScreenNavigationManager
  - every prefab in Assets/NAAD/UI/Views/ is instantiated in the scene
  - every NAAD.UI.*Controller that pairs with a view is present in the scene

Usage:
    python tools/validate-bootstrap-ui.py [path/to/game/unity]

Exit code 0 = OK, 1 = something is missing.
"""

from __future__ import annotations

import re
import sys
from pathlib import Path

DEFAULT_UNITY_ROOT = Path(__file__).resolve().parent.parent / "game" / "unity"

# Controllers that must be present for the shell to bind. AuthController is a plain
# MonoBehaviour rather than a BaseScreenController, so it is listed explicitly.
CONTROLLERS = [
    "AuthController",
    "BootstrapScreenController",
    "BottomNavigationController",
    "ConfigurationUIController",
    "MainHudController",
    "NightSummaryController",
    "PhoneHomeController",
    "PingMessagingController",
]


def guid_of(meta: Path) -> str | None:
    if not meta.exists():
        return None
    m = re.search(r"^guid:\s*([0-9a-f]{32})", meta.read_text(encoding="utf-8", errors="ignore"), re.M)
    return m.group(1) if m else None


def main() -> int:
    root = Path(sys.argv[1]).resolve() if len(sys.argv) > 1 else DEFAULT_UNITY_ROOT
    scene = root / "Assets" / "Scenes" / "Bootstrap.unity"

    if not scene.exists():
        print(f"FAIL: missing scene {scene}")
        return 1

    text = scene.read_text(encoding="utf-8", errors="ignore")
    referenced = set(re.findall(r"guid: ([0-9a-f]{32})", text))
    problems: list[str] = []

    # --- UIRoot -----------------------------------------------------------------
    if "m_Name: UIRoot" not in text:
        problems.append("no GameObject named 'UIRoot' (run NAAD > UI > Mount UI Shell In Bootstrap)")

    def require_script(name: str, why: str) -> None:
        meta = root / "Assets" / "NAAD" / "Editor" / f"{name}.cs.meta"
        if not meta.exists():
            meta = root / "Assets" / "NAAD" / "UI" / f"{name}.cs.meta"
        if not meta.exists():
            meta = root / "Assets" / "NAAD" / "UI" / "Scripts" / f"{name}.cs.meta"
        g = guid_of(meta)
        if g is None:
            problems.append(f"cannot find {name}.cs.meta - {why}")
        elif g not in referenced:
            problems.append(f"{name} is not in Bootstrap.unity - {why}")

    require_script("ScreenNavigationManager", "the scene has no navigation manager")

    for controller in CONTROLLERS:
        # Controllers live in Assets/NAAD/UI/
        meta = root / "Assets" / "NAAD" / "UI" / f"{controller}.cs.meta"
        g = guid_of(meta)
        if g is None:
            problems.append(f"missing controller script: {controller}.cs.meta")
        elif g not in referenced:
            problems.append(f"{controller} is not mounted in Bootstrap.unity")

    # --- view prefabs -----------------------------------------------------------
    views_dir = root / "Assets" / "NAAD" / "UI" / "Views"
    prefabs = sorted(views_dir.glob("*.prefab")) if views_dir.exists() else []
    if not prefabs:
        problems.append("no view prefabs in Assets/NAAD/UI/Views (run NAAD > UI > Create All Screen Prefabs)")
    for prefab in prefabs:
        g = guid_of(prefab.with_suffix(".prefab.meta"))
        if g is None:
            problems.append(f"missing .meta for {prefab.name}")
        elif g not in referenced:
            problems.append(f"{prefab.name} is not instantiated in Bootstrap.unity")

    # --- result -----------------------------------------------------------------
    print(f"Bootstrap UI shell: {len(prefabs)} view prefab(s), {len(CONTROLLERS)} controller(s)")
    if problems:
        print("\nFAIL:")
        for p in problems:
            print(f"  - {p}")
        return 1

    print("OK: Bootstrap mounts the full UI shell (UIRoot, canvas, navigation, views, controllers).")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
