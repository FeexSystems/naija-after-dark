#!/usr/bin/env python3
"""Git filter-repo script to remove large files from history"""

from git_filter_repo import Blob, Callback

# Only drop blobs at least this large. Path patterns match anything that merely
# contains the string, so the size gate is what keeps small text/config files
# (e.g. a docs page mentioning "deno.exe") in history.
MIN_BLOB_SIZE = 5 * 1024 * 1024  # 5 MiB

REMOVE_PATTERNS = (
    b'LlamaLib-v2.0.5',
    b'_scratch_deno',
    b'deno.exe',
    b'UnitOptions.db',
)


def _name_matches(blob: Blob) -> bool:
    return any(pattern in blob.name for pattern in REMOVE_PATTERNS)


def blob_callback(blob: Blob):
    if _name_matches(blob) and blob.original_id_size >= MIN_BLOB_SIZE:
        return None
    return blob


Callback.register_blob_callback(blob_callback)
