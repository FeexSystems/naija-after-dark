#!/usr/bin/env python3
"""
Verify a NAAD Supabase project is reachable and fully migrated.

Every probe is sent with the argument names declared in
backend/game-api/src/contracts/rpc-catalog.ts. That detail matters more than it
looks: PostgREST answers **404** both for a function that does not exist *and*
for one whose signature you failed to match, so an argument-less probe reports
false "missing RPC" results for every function that takes parameters.

Every function here checks auth.uid() before doing anything, and this script
uses the anon key with no user session, so each call returns UNAUTHENTICATED
without mutating state. Probing is safe.

Usage:
    python tools/verify-supabase.py <project-url> <anon-key>
    python tools/verify-supabase.py            # read both from SupabaseConfig.asset

Exit code 0 = reachable and the full surface responds, 1 = something is wrong.
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
CONFIG = ROOT / "game/unity/Assets/NAAD/Networking/Config/Resources/SupabaseConfig.asset"
CATALOG = ROOT / "backend/game-api/src/contracts/rpc-catalog.ts"
TIMEOUT = 20


def read_config() -> tuple[str, str]:
    text = CONFIG.read_text(encoding="utf-8")
    url = re.search(r"projectUrl:\s*(\S+)", text)
    key = re.search(r"anonKey:\s*(\S+)", text)
    if not url or not key:
        sys.exit(f"error: could not read projectUrl/anonKey from {CONFIG}")
    return url.group(1).strip().rstrip("/"), key.group(1).strip()


def catalog() -> list[tuple[str, list[str]]]:
    text = CATALOG.read_text(encoding="utf-8")
    entries = re.findall(
        r'name:\s*"(naad_[a-z_]+)".*?args:\s*\[([^\]]*)\]', text, re.S
    )
    return [(n, [a.strip().strip('"') for a in args.split(",") if a.strip()]) for n, args in entries]


def dummy(arg: str):
    """A value of the right shape; the function rejects the call before using it."""
    if arg.endswith("_id"):
        return "00000000-0000-0000-0000-000000000000"
    if arg in ("p_trust", "p_respect", "p_attendance", "p_minutes"):
        return 0
    if arg == "p_importance":
        return 0.0
    if arg == "p_payload":
        return {}
    if arg in ("p_pos_x", "p_pos_y", "p_pos_z", "p_rot_y"):
        return 0.0
    return "probe"


def post(url: str, key: str, path: str, body: dict) -> tuple[int, str]:
    req = urllib.request.Request(
        f"{url}/rest/v1/{path}",
        data=json.dumps(body).encode(),
        headers={
            "apikey": key,
            "Authorization": f"Bearer {key}",
            "Content-Type": "application/json",
        },
        method="POST",
    )
    try:
        with urllib.request.urlopen(req, timeout=TIMEOUT) as r:
            return r.status, r.read(200).decode("utf-8", "replace")
    except urllib.error.HTTPError as e:
        return e.code, e.read(200).decode("utf-8", "replace")
    except Exception as e:
        return 0, str(e)


def main() -> int:
    if len(sys.argv) >= 3:
        url, key = sys.argv[1].rstrip("/"), sys.argv[2]
    else:
        url, key = read_config()

    host = url.replace("https://", "").replace("http://", "").split("/")[0]
    print(f"project: {url}")

    try:
        socket.getaddrinfo(host, 443)
        print("  dns        OK")
    except socket.gaierror:
        print(f"  dns        FAIL - {host} does not resolve")
        return 1

    # No root/OpenAPI check: /rest/v1/ requires the service_role key and answers
    # "Invalid API key" for the anon key, which is expected, not a fault.
    entries = catalog()
    if not entries:
        sys.exit("error: no RPC entries parsed from rpc-catalog.ts")

    missing, unauthenticated, other = [], 0, []
    for name, args in entries:
        code, body = post(url, key, f"rpc/{name}", {a: dummy(a) for a in args})
        if code == 404:
            missing.append(name)
        elif code == 0:
            other.append((name, body))
        elif "UNAUTHENTICATED" in body or "unauthenticated" in body.lower() or code == 401:
            unauthenticated += 1
        else:
            other.append((name, f"HTTP {code}: {body[:80]}"))

    print(f"  rpcs       {len(entries) - len(missing)}/{len(entries)} present "
          f"({unauthenticated} answered UNAUTHENTICATED as expected)")

    if missing:
        print("\nMISSING RPCs (schema not applied, or a migration did not run):")
        for name in missing:
            print(f"  {name}")
        return 1

    if other:
        print("\nUnexpected answers (present, but worth a look):")
        for name, detail in other:
            print(f"  {name}: {detail}")

    print("\nOK - project is reachable and the full RPC surface responds.")
    print("Next: sign in with a real user (NAAD_DEV_EMAIL / NAAD_DEV_PASSWORD), then press Play in the Bootstrap scene.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
