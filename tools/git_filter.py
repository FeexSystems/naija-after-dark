#!/usr/bin/env python3
"""Git filter-repo script to remove large files from history"""

from git_filter_repo import Blob, Callback

def blob_callback(blob: Blob):
    # Remove LlamaLib-v2.0.5 files
    if b'LlamaLib-v2.0.5' in blob.name:
        return None
    
    # Remove scratch files
    if b'_scratch_deno' in blob.name or b'deno.exe' in blob.name:
        return None
    
    # Remove Unity generated databases
    if b'UnitOptions.db' in blob.name:
        return None
    
    return blob

Callback.register_blob_callback(blob_callback)
