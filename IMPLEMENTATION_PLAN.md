# Privacy Metadata Inspector implementation plan

## Product rules

- A successful write is not considered verified until the output file is read again.
- Verification passes when no privacy-sensitive findings remain. Structural image metadata such as dimensions and compression is allowed.
- Privacy scores are deterministic, capped at 100, and explained by category rather than presented as a scientific guarantee.
- All inspection and report generation remains local. Map viewing is an explicit user action that opens a URL containing the image coordinates.

## Vertical slice 1: trustworthy clean result

1. Interpret raw metadata into human-friendly names, explanations, privacy categories, and an explainable score.
2. Extract GPS coordinates when present and expose a map action.
3. Snapshot metadata before cleaning, re-read the output, compare sensitive findings, and mark each queue item verified or failed verification.
4. Present risk, findings, and before/after counts in the existing responsive inspector.
5. Export the selected image's assessment and verification result as a self-contained local HTML report.
6. Cover the scoring, comparison, report rendering, and ViewModel orchestration with tests.

## Later slices

- Add policy presets that choose which metadata categories to preserve.
- Add richer timeline visualization and reverse-geocoded place names (opt-in because it requires a network service).
- Add batch report export and signed/hashed reports for organizational workflows.
- Add report history only after retention and deletion behavior is designed explicitly.
- Expand metadata rules for XMP/IPTC/MakerNote vendor variants using a maintained compatibility corpus.
