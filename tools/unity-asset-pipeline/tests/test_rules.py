"""Unit tests for the NAAD naming/tagging grammar. No Blender, no unity_cloud.

    python tools/unity-asset-pipeline/tests/test_rules.py
"""

import os
import sys
import unittest

sys.path.insert(0, os.path.abspath(os.path.join(os.path.dirname(__file__), "..")))

from paths import load_rules  # noqa: E402

rules = load_rules()


class SlugTests(unittest.TestCase):
    def test_ascii_and_case(self):
        self.assertEqual(rules.slugify("Aso Rock Café"), "aso-rock-cafe")
        self.assertEqual(rules.slugify("Lekki & Ikoyi"), "lekki-and-ikoyi")
        self.assertEqual(rules.slugify("  "), "")

    def test_sanitize_part(self):
        self.assertEqual(rules.sanitize_part("booth 03"), "Booth03")
        self.assertEqual(rules.sanitize_part("Grill-stand/Large"), "GrillStandLarge")
        self.assertEqual(rules.sanitize_part("!!!"), "")

    def test_next_part(self):
        self.assertEqual(rules.next_part("Booth_03"), "Booth_04")
        self.assertEqual(rules.next_part("Booth_09"), "Booth_10")
        self.assertEqual(rules.next_part("Booth_99"), "Booth_100")
        self.assertIsNone(rules.next_part("Booth"))


class NamingTests(unittest.TestCase):
    def test_build(self):
        name, problems = rules.suggest_asset_name("NCL", "Booth 03", "IS", 2)
        self.assertEqual(problems, [])
        self.assertEqual(name, "NCL_Booth03_IS_v002")

    def test_size_and_default_version(self):
        name, _ = rules.suggest_asset_name("SYU", "Grill", "ML", size="l")
        self.assertEqual(name, "SYU_Grill_ML_L_v001")

    def test_rejects_unknown_category(self):
        name, problems = rules.suggest_asset_name("XYZ", "Booth", "IS")
        self.assertIsNone(name)
        self.assertTrue(any("unknown category" in p for p in problems))

    def test_rejects_missing_part(self):
        name, problems = rules.suggest_asset_name("NCL", "  ", "IS")
        self.assertIsNone(name)
        self.assertTrue(any("part" in p for p in problems))

    def test_parse_roundtrip(self):
        name, _ = rules.suggest_asset_name("APT", "Door 2", "ML", 12, "XL")
        parsed = rules.split_asset_name(name)
        self.assertIsNotNone(parsed, name)
        self.assertEqual(parsed["cat"], "APT")
        self.assertEqual(parsed["part"], "Door2")
        self.assertEqual(parsed["district"], "ML")
        self.assertEqual(parsed["size"], "XL")
        self.assertEqual(parsed["ver"], "012")

    def test_parse_rejects_free_text(self):
        for bad in ("Booth", "NCL_Booth", "NCL_Booth_ZZ_v001", "NCL_Booth_IS_v1", "ncl_booth_IS_v001"):
            with self.subTest(bad=bad):
                self.assertIsNone(rules.split_asset_name(bad))


class TagTests(unittest.TestCase):
    def test_environment_asset(self):
        tags = rules.build_tags("NCL", "IS", "NCL", "L")
        self.assertEqual(tags, ["type-environment", "cat-ncl", "district-island",
                                "venue-nightclub", "size-large"])

    def test_character_asset(self):
        tags = rules.build_tags("CHR", "ML", "SYU")
        self.assertEqual(tags[0], "type-character")
        self.assertIn("district-mainland", tags)
        self.assertIn("venue-suya", tags)

    def test_extras_are_slugged_and_deduped(self):
        tags = rules.build_tags("PRP", "CS", "BCH", extra=["Hero Prop", "hero-prop", "Landmark"])
        self.assertEqual(tags.count("hero-prop"), 1)
        self.assertIn("landmark", tags)

    def test_unknown_venue_falls_back(self):
        tags = rules.build_tags("PRP", "ND", "XXX")
        self.assertIn("venue-street", tags)

    def test_validate(self):
        self.assertEqual(rules.validate_tags(["type-environment", "cat-prp"]), [])
        self.assertTrue(rules.validate_tags(["Mixed Case"]))
        self.assertTrue(rules.validate_tags(["double--dash"]))
        self.assertTrue(rules.validate_tags(["a", "a"]))
        self.assertTrue(rules.validate_tags(["t%d" % i for i in range(rules.MAX_TAGS + 1)]))

    def test_every_district_and_venue_is_valid(self):
        for district in rules.DISTRICTS:
            for venue in rules.VENUES:
                with self.subTest(district=district, venue=venue):
                    self.assertEqual(rules.validate_tags(rules.build_tags("PRP", district, venue)), [])

    def test_collection_and_district_mapping(self):
        self.assertEqual(rules.collection_for("IS", "NCL"), "IS/NCL")
        self.assertEqual(rules.district_for_venue("BCH"), "CS")
        self.assertEqual(rules.district_for_venue("nope"), rules.DEFAULT_DISTRICT)
        self.assertEqual(rules.collection_for("zz", "xx"), "ND/STR")


if __name__ == "__main__":
    unittest.main(verbosity=2)
