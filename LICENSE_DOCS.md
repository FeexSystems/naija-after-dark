# Third-Party License Documentation

## Summary
- **Project license**: Apache-2.0 (see LICENSE file)
- **Third-party assets requiring review**: 6 vendor packs + 2 Unity packages
- **Critical note**: Glitch9 AI DevKit is AGPL-3.0-or-later and is studied only, not integrated. Must not be merged into NAAD proprietary code without legal review.

## Vendor Asset Packs (Gitignored)
| Pack | Publisher | License Status | Action Required |
|------|-----------|----------------|----------------|
| Glitch9 AI DevKit | Glitch9 | AGPL-3.0-or-later | **Do NOT merge** - study only |
| Kevin Iglesias | Kevin Iglesias | Unknown | Review license terms |
| Marcos Schultz | Marcos Schultz | Unknown | Review license terms |
| Awbmecreations | Awbmecreations | Unknown | Review license terms |
| Brick Project Studio | Brick Project Studio | Unknown | Review license terms |
| PalmTreePack | Unknown | Unknown | Review license terms |
| Versatile Studio Assets | Versatile Studio | Unknown | Review license terms |
| FreeFlyCamera | Unknown | Unknown | Review license terms |

## Unity Packages
| Package | License | Status |
|---------|---------|--------|
| LLMUnity | Apache 2.0 | Safe to use |
| TextMesh Pro | Unity (built-in) | Safe to use |

## Glitch9 AI DevKit - AGPL-3.0 Risk
- **License**: AGPL-3.0-or-later (strong copyleft)
- **Current status**: Studied only, not merged into NAAD proprietary code
- **Requirement**: AGPL-3.0 requires derivative works to be AGPL with source available
- **Action**: Do not merge Glitch9 source into NAAD code; reimplement patterns if needed
- **Legal review**: Required before any integration

## Apache-2.0 License Choice Rationale
- Glitch9 is not integrated into NAAD codebase (study only)
- Apache-2.0 provides patent protection and is widely adopted
- Suitable for both open-source and commercial use
- If Glitch9 is ever integrated, project must switch to AGPL-3.0

## Action Items
- [ ] Legal review of all vendor pack licenses
- [ ] Confirm commercial redistribution rights for each pack
- [ ] Document approved vendor pack URLs/IDs
- [ ] Create asset provenance spreadsheet
- [ ] Update this document as licenses are reviewed
