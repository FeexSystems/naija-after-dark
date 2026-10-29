#!/usr/bin/env python3
"""Validate Unity .meta GUIDs for CI.

Checks every .meta file below the supplied root for exactly one valid 32-character
hexadecimal GUID and rejects duplicate GUIDs. The checker is intentionally
read-only and has no third-party dependencies.
"""

from __future__ import annotations

import re
import sys
from collections import defaultdict
from pathlib import Path

GUID_LINE = re.compile(r"^guid:\s*(.*?)\s*$", re.MULTILINE)
VALID_GUID = re.compile(r"^[0-9a-fA-F]{32}$")


def main() -> int:
    root = Path(sys.argv[1] if len(sys.argv) > 1 else "game/unity").resolve()
    if not root.is_dir():
        print(f"error: Unity root does not exist: {root}", file=sys.stderr)
        return 2

    problems: list[str] = []
    by_guid: defaultdict[str, list[Path]] = defaultdict(list)
    files = sorted(root.rglob("*.meta"))

    for path in files:
        relative = path.relative_to(root)
        try:
            text = path.read_text(encoding="utf-8")
        except (OSError, UnicodeDecodeError) as exc:
            problems.append(f"{relative}: cannot read file: {exc}")
            continue

        matches = GUID_LINE.findall(text)
        if len(matches) != 1:
            problems.append(
                f"{relative}: expected exactly one guid line, found {len(matches)}"
            )
            continue

        guid = matches[0]
        if not VALID_GUID.fullmatch(guid):
            problems.append(
                f"{relative}: invalid GUID {guid!r}; expected 32 hexadecimal characters"
            )
            continue
        by_guid[guid.lower()].append(relative)

    for guid, paths in sorted(by_guid.items()):
        if len(paths) > 1:
            joined = ", ".join(str(path) for path in paths)
            problems.append(f"duplicate GUID {guid}: {joined}")

    print(f"Checked {len(files)} Unity .meta file(s) under {root}")
    if problems:
        print(f"Found {len(problems)} metadata problem(s):")
        for problem in problems:
            print(f"- {problem}")
        return 1

    print("Unity metadata integrity check passed.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
