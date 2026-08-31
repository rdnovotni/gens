# Gens Historical Figures Reference — Pass 15

This pass continues the historical-figures research/reference layer from the supplied pass-14 dataset.

## Artifact identity

- Base: `historical_figures_reference_polished_expanded_pass14(1).csv`
- Full merged artifact: `gens_historical_figures_reference_pass15_polished.csv`
- Rows: **2,527**
- Columns: **76**
- New figures: **19**
- Existing rows with substantive field-level patches: **34**
- Full merged CSV SHA-256: `597be6219b05a8cbcf7bf191b515bf9bfe8a58c2a2772581cd785d65857e716f`
- Pass token: `gap_alias_source_hardening_pass_15`

The complete merged reference is the validated artifact named `gens_historical_figures_reference_pass15_polished.csv` (digest above). Because the connected repository write path cannot stream the ~4.1 MB merged CSV directly, this branch stores all new rows in four reviewable full-schema batches plus an exact field-level patch ledger for the pre-existing rows.

## Expansion focus

Pass 15 targets coverage gaps rather than adding volume indiscriminately:

- Syrian / Levantine Arab client-dynasty figures and frontier politics.
- Late-antique North African rulers, rebels, and Byzantine/Vandal transition figures.
- Julianic and late-antique intellectual/religious networks.
- Sasanian religious-political opposition.
- AD 565 boundary continuity for later sixth-century church/history figures.

### Added figures

- Sampsiceramus I
- Sosigenes
- Sohaemus of Emesa
- Faraxen
- Sopater of Apamea
- Maximus of Ephesus
- Saturninius Secundus Salutius
- Mawiyya
- Nemesius of Emesa
- Mundzuk
- Heraclianus
- Masties
- Mazdak
- Masuna
- Cabaon
- Stotzas
- Gregory I
- Areobindus
- Gregory of Tours

## Polish and hardening

- Filled all remaining blank `era_tags` values in the pass-14 base (26 rows).
- Filled the remaining blank `overlaps_playable_133BC_to_AD565` values (2 rows).
- Added search aliases for Hero/Heron of Alexandria, Rua/Rugila/Ruga, Shanakdakheto/Shanakhdakheto, and Monobaz II/Monobazus II.
- Normalized all pass-15 region and system-hook values to the existing controlled vocabulary.
- Replaced the provisional Faraxen source with accessible inscription/epigraphy-oriented references.
- Added reciprocal/high-confidence family edges for Mundzuk ↔ Attila/Bleda/Octar/Rua, Sampsiceramus I ↔ Iamblichus I, and Areobindus ↔ Praejecta.

## Validation

The merged pass-15 CSV was re-read with a strict CSV parser and passed these checks:

- 2,527 data rows and exactly 76 fields on every row.
- No duplicate `id` values.
- No duplicate `name` values.
- No blank `era_tags`.
- No blank `overlaps_playable_133BC_to_AD565`.
- No blank `source_url`.
- No unresolved relationship IDs among all pass-15 additions/touched relationship rows.
- No new region or system-hook tokens outside the pass-14 vocabulary.
- No regressions in columns that were complete in pass 14.
- Every row includes `gap_alias_source_hardening_pass_15` in `data_pass`.

## Companion files

- `gens_historical_figures_reference_pass15_additions_part1.csv` through `part4.csv` — all 19 new rows, each using the full 76-column schema for easy review.
- `gens_historical_figures_reference_pass15_existing_row_patches.csv` — every substantive pre-existing-row field change, excluding the universal `data_pass` token append.
- The complete merged CSV is distributed as the validated pass-15 artifact and identified by the SHA-256 digest above.
