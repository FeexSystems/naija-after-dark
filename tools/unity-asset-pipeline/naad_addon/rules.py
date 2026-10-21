"""Pure-Python NAAD asset rules: naming, district/venue tags, collections.

No Blender imports and no `unity_cloud` imports on purpose. This module is the
single source of truth for the NAAD convention described in
`docs/art-pipeline.md` and is unit-tested outside Blender
(`tools/unity-asset-pipeline/tests/test_rules.py`).
"""

import re
import unicodedata
from typing import Dict, List, Optional, Tuple

# ---------------------------------------------------------------------------
# Districts (game/unity/Assets/NAAD/Content/ASSET_NAMING_CONVENTION.md,
# DISTRICT_VARIANTS.md)
# ---------------------------------------------------------------------------

# code -> (display tag, English name)
DISTRICTS: Dict[str, Tuple[str, str]] = {
    "ND": ("district-night", "Night District"),
    "ML": ("district-mainland", "Mainland Streets"),
    "IS": ("district-island", "Island Nightlife"),
    "CR": ("district-creative", "Creative"),
    "CS": ("district-coastal", "Coastal"),
}
DEFAULT_DISTRICT = "ND"

# ---------------------------------------------------------------------------
# Venues = `public.locations` seeded in supabase/migrations/0005_night_district_world.sql
# Changing a venue code changes cloud tags -> update docs/art-pipeline.md too.
# ---------------------------------------------------------------------------

VERSION = "0.1.0"
MAX_TAGS = 12

VENUES: Dict[str, Tuple[str, str, str]] = {
    "APT": ("venue-apartment", "Apartment", "ND"),
    "SYU": ("venue-suya", "Suya Spot", "ML"),
    "STR": ("venue-street", "Street", "ML"),
    "NCL": ("venue-nightclub", "Nightclub", "IS"),
    "BCH": ("venue-beach", "Beach", "CS"),
}
DEFAULT_VENUE = "STR"

# ---------------------------------------------------------------------------
# Categories (subset of ASSET_NAMING_CONVENTION.md used by the add-on)
# ---------------------------------------------------------------------------

CATEGORIES: Dict[str, str] = {
    "APT": "Apartment",
    "CHR": "Character",
    "INT": "Interior",
    "NCL": "Nightclub",
    "PRP": "Prop",
    "STR": "Street",
    "SYU": "Suya",
    "BCH": "Beach",
    "VHC": "Vehicle",
}

CATEGORY_EXTRA_TAGS: Dict[str, List[str]] = {
    "CHR": ["type-character"],
    "VHC": ["type-vehicle"],
    "PRP": ["type-prop"],
}

# ---------------------------------------------------------------------------
# Name grammar: <CAT>_<Part>_<DISTRICT>[_<size>][_v<NNN>]
# e.g. NCL_Booth_IS_v002, SYU_Grill_ML_L_v001
# ---------------------------------------------------------------------------

TAG_RE = re.compile(r"^[a-z0-9]+(?:-[a-z0-9]+)*$")
_NAME_RE = re.compile(
    r"^(?P<cat>[A-Z]{3})"
    r"_(?P<part>[A-Za-z0-9]+)"
    r"_(?P<district>[A-Z]{2})"
    r"(?:_(?P<size>[SML]|XL))?"
    r"(?:_v(?P<ver>\d{3}))?"
    r"$"
)

SIZE_TAGS = {"S": "size-small", "M": "size-medium", "L": "size-large", "XL": "size-xl"}

# ASCII-only placement descriptor for path_mode='COPY' exports: anything outside
# this set gets folded away, so "Café_Neon" and "Cafe Neon" would collide.
_ASCII_FOLD = {
    "\u00e9": "e", "\u00e8": "e", "\u00ea": "e", "\u00e1": "a", "\u00e0": "a",
    "\u00e2": "a", "\u00e4": "a", "\u00ed": "i", "\u00ec": "i", "\u00ee": "i",
    "\u00f3": "o", "\u00f2": "o", "\u00f4": "o", "\u00f6": "o", "\u00fa": "u",
    "\u00f9": "u", "\u00fb": "u", "\u00fc": "u", "\u00f1": "n", "\u00e7": "c",
    "\u0301": "", "\u0300": "", "\u0302": "", "\u0308": "",
    "\u2019": "", "\u2018": "", "\u20a6": "NGN",
}


def slugify(text: str) -> str:
    """Lowercase ASCII slug. `Aso Rock Café` -> `aso-rock-cafe`."""
    if not text:
        return ""
    folded = "".join(_ASCII_FOLD.get(ch, ch) for ch in text)
    folded = unicodedata.normalize("NFKD", folded).encode("ascii", "ignore").decode("ascii")
    folded = folded.replace("&", " and ")
    folded = re.sub(r"[^A-Za-z0-9]+", "-", folded).strip("-").lower()
    return folded


def sanitize_part(part: str) -> str:
    """Turn free text into a valid `Part` segment. Returns '' if nothing usable."""
    words = [w for w in re.split(r"[^A-Za-z0-9]+", part or "") if w]
    if not words:
        return ""
    head, tail = words[0], words[1:]
    return head[:1].upper() + head[1:] + "".join(w[:1].upper() + w[1:] for w in tail)


def next_part(prefix: str) -> Optional[str]:
    """`Booth_03` -> `Booth_04`. None when there is no numeric suffix."""
    match = re.search(r"(\d+)(?!.*\d)", prefix or "")
    if not match:
        return None
    digits = match.group(1)
    bumped = str(int(digits) + 1).zfill(len(digits))
    return prefix[: match.start(1)] + bumped + prefix[match.end(1):]


def district_for_venue(venue: str) -> str:
    """District a venue belongs to; unknown venues fall back to the default venue's."""
    return VENUES.get(venue, VENUES[DEFAULT_VENUE])[2]


def venue_tag(venue: str) -> str:
    return VENUES.get(venue, VENUES[DEFAULT_VENUE])[0]


def district_tag(district: str) -> str:
    key = district if district in DISTRICTS else DEFAULT_DISTRICT
    return DISTRICTS[key][0]


def split_asset_name(name: str) -> Optional[Dict[str, Optional[str]]]:
    """Parse a NAAD asset name. Returns None when it does not follow the grammar."""
    match = _NAME_RE.match((name or "").strip())
    if not match or match.group("district") not in DISTRICTS:
        return None
    return match.groupdict()


def suggest_asset_name(
    category: str,
    part: str,
    district: str,
    version_tag: int = 1,
    size: Optional[str] = None,
) -> Tuple[Optional[str], List[str]]:
    """Build `<CAT>_<Part>_<DISTRICT>[_<size>]_v<NNN>`.

    Returns (name, problems). `name` is None when the result would not satisfy
    the grammar (unknown category, empty part, bad size).
    """
    problems: List[str] = []
    cat = (category or "").strip().upper()
    if cat not in CATEGORIES:
        problems.append("unknown category '%s' (expected one of %s)"
                        % (category, ", ".join(sorted(CATEGORIES))))

    clean_part = sanitize_part(part)
    if not clean_part:
        problems.append("asset name needs a part, e.g. 'Booth 03'")

    dist = (district or "").strip().upper()
    if dist not in DISTRICTS:
        problems.append("unknown district '%s' (expected one of %s)"
                        % (district, ", ".join(sorted(DISTRICTS))))

    size_tag = (size or "").strip().upper() or None
    if size_tag is not None and size_tag not in SIZE_TAGS:
        problems.append("unknown size '%s' (expected S, M, L or XL)" % size)
        size_tag = None

    version_no = int(version_tag) if version_tag else 1
    if not 1 <= version_no <= 999:
        problems.append("version must be between 001 and 999")
        version_no = 1

    if problems:
        return None, problems

    name = "%s_%s_%s" % (cat, clean_part, dist)
    if size_tag:
        name += "_" + size_tag
    name = "%s_v%03d" % (name, version_no)

    # Self-check: whatever we generate must satisfy the grammar we enforce.
    if split_asset_name(name) is None:
        return None, ["generated name '%s' does not re-parse" % name]
    return name, problems


def validate_tags(tags: List[str]) -> List[str]:
    """Returns human-readable problems for a list of cloud tags."""
    problems: List[str] = []
    seen = set()
    for tag in tags:
        if not TAG_RE.match(tag or ""):
            problems.append("tag '%s' must be lowercase a-z0-9 with single hyphens" % tag)
        elif tag in seen:
            problems.append("duplicate tag '%s'" % tag)
        seen.add(tag)
    if len(tags) > MAX_TAGS:
        problems.append("%d tags exceed the %d-tag limit" % (len(tags), MAX_TAGS))
    return problems


def build_tags(
    category: str,
    district: str,
    venue: str,
    size: Optional[str] = None,
    extra: Optional[List[str]] = None,
) -> List[str]:
    """Deterministic tag list every NAAD asset in Asset Manager must carry.

    Order: type-detail, category, district, venue, size, then extras.
    """
    cat = (category or "").strip().upper()
    tags: List[str] = list(CATEGORY_EXTRA_TAGS.get(cat, ["type-environment"]))
    tags.append("cat-%s" % (cat.lower() if cat in CATEGORIES else "unknown"))
    tags.append(district_tag(district))
    tags.append(venue_tag(venue))

    size_tag = (size or "").strip().upper()
    if size_tag in SIZE_TAGS:
        tags.append(SIZE_TAGS[size_tag])

    for tag in extra or []:
        slug = slugify(tag)
        if slug and slug not in tags:
            tags.append(slug)
    return tags


def collection_for(district: str, venue: str) -> str:
    """Asset Manager collection: `<district-display> / <venue-key>`, e.g. `ND/APT`."""
    dist = district if district in DISTRICTS else DEFAULT_DISTRICT
    ven = venue if venue in VENUES else DEFAULT_VENUE
    return "%s/%s" % (dist, ven)
