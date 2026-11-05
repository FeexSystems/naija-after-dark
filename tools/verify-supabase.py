#!/usr/bin/env python3
"""
Verify a NAAD Supabase project is reachable and fully migrated.

Checks the project URL resolves, the REST gateway answers, and every RPC in
backend/game-api/src/contracts/rpc-catalog.ts actually exists in the database
(PostgREST answers 404 for a missing function, 401/400/403 for one that exists
but rejects the anonymous call).

Usage:
    python tools/verify-supabase.py <project-url> <anon-key>
    python tools/verify-supabase.py            # read both from SupabaseConfig.asset

Exit code 0 = all good, 1 = something is wrong.
"""

from __future__ import annotations

import json
import re
import socket
import sys
import urllib.error
import urllib.request
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
CONFIG = ROOT / "game" / "unity" / "Assets" / "NAAD" / "Networking" / "Config" / "Resources" / "SupabaseConfig.asset"
CATALOG = ROOT / "backend" / "game-api" / "src" / "contracts" / "rpc-catalog.ts"
INTERNAL = ROOT / "backend" / "game-api" / "src" / "contracts" / "rpc-catalog.ts"
TIMEOUT = 20


def read_config() -> tuple[str, str]:
    text = CONFIG.read_text(encoding="utf-8")
    url = re.search(r"projectUrl:\s*(\S+)", text)
    key = re.search(r"anonKey:\s*(\S+)", text)
    if not url or not key:
        sys.exit(f"error: could not read projectUrl/anonKey from {CONFIG}")
    return url.group(1).strip(), key.group(1).strip()


def catalog_names() -> list[str]:
    text = CATALOG.read_text(encoding="utf-8")
    return sorted(set(re.findall(r'name:\s*"(naad_[a-z_]+)"', text)))


def post(url: str, key: str, path: str, body: bytes = b"{}") -> int:
    req = urllib.request.Request(
        f"{url}/rest/v1/{path}",
        data=body,
        headers={
            "apikey": key,
            "Authorization": f"Bearer {key}",
            "Content-Type": "application/json",
        },
        method="POST",
    )
    try:
        with urllib.request.urlopen(req, timeout=TIMEOUT) as r:
            return r.status
    except urllib.error.HTTPError as e:
        return e.code
    except Exception:
        return 0


def main() -> int:
    if len(sys.argv) >= 3:
        url, key = sys.argv[1].rstrip("/"), sys.argv[2]
    else:
        url, key = read_config()
        url = url.rstrip("/")

    host = url.replace("https://", "").replace("http://", "").split("/")[0]
    print(f"project: {url}")

    try:
        socket.getaddrinfo(host, 443)
        print("  dns        OK")
    except socket.gaierror:
        print(f"  dns        FAIL - {host} does not resolve (deleted/paused project?)")
        return 1

    try:
        req = urllib.request.Request(f"{url}/rest/v1/", headers={"apikey": key})
        with urllib.request.urlopen(req, timeout=TIMEOUT) as r:
            print(f"  rest       OK ({r.status})")
    except urllib.error.HTTPError as e:
        print(f"  rest       OK ({e.code} - gateway answered)")
    except Exception as e:
        print(f"  rest       FAIL - {e}")
        return 1

    names = catalog_names()
    if not names:
        sys.exit("error: no RPC names found in rpc-catalog.ts")

    missing, present = [], []
    for name in names:
        code = post(url, key, f"rpc/{name}")
        # 404 => function absent. 401/403/400/422 => present but rejected the anon call,
        # which is the expected answer for everything requiring auth.uid().
        (present if code in (200, 201, 400, 401, 403, 422) else missing).append((name, code))

    print(f"  rpcs       {len(present)}/{len(names)} present")
    if missing:
        print("\nMISSING RPCs (schema not applied, or a migration did not run):")
        for name, code in missing:
            print(f"  {name} -> HTTP {code}")
        return 1

    print("\nOK - project is reachable and the full RPC surface exists.")
    print("Next: set projectUrl + anonKey in SupabaseConfig.asset, then press Play in the Bootstrap scene.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
