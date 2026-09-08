# Native localization

`Gens.Localization` loads flat JSON catalogs keyed by stable dotted identifiers such as `menu.new_game` and `screen.household`. English (`en`) is the base and fallback locale. Lookup tries the selected locale, then English; a missing base key is `⟦missing:key⟧` in development and the key in production, with a diagnostic event. `{name}` placeholders are named and required: a missing argument throws instead of silently producing broken copy. The API leaves plural selection above lookup so an ICU-compatible catalog can be added without changing keys.

The desktop client copies `Assets/Localization/en.json` into packages and switches locale at runtime. `qps-ploc` derives from English, accents characters, wraps the result, and adds approximately 40% padding. It is shown only when the developer console setting is enabled. Static primary-menu, Settings, Credits, roster, estate, report, confirmation, and save/navigation labels have stable keys. Historical proper names and generated appearance grammar remain English/source-authored by design.

The current font fixture is licensed Noto Sans. HarfBuzz shaping and visible missing-glyph tofu prevent silent omission, but a packaged multi-font Latin/Greek/Cyrillic/Arabic/CJK fallback chain is not yet present and remains a release gap.
