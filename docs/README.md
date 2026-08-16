# Documentation

Project documentation is organized by audience and purpose:

- [`design/`](design/README.md) contains the game vision, system specifications,
  setting references, and authored-content plans. Two supporting references live
  directly under `docs/`: the
  [Canonical Object & Data Registry](gens-canonical-registry-design.md) and the
  [Design Authority Registry](gens-design-authority-registry.md).
- [`engineering/`](engineering/) records implementation constraints and technical
  decisions that contributors must preserve, including the technical baseline
  ([`tech-stack.md`](engineering/tech-stack.md)), the
  [comprehensive build roadmap](engineering/gens-comprehensive-build-roadmap.md),
  the [cross-system field ledger](engineering/gens-field-ledger.md), the
  [open question queues](engineering/gens-open-question-queues.md), the
  [simulation scale targets](engineering/gens-simulation-scale-targets.md), the
  [vertical-slice quantification](engineering/gens-vertical-slice-quantification.md),
  and the [architecture decision records](engineering/adr/README.md).

The design documents describe intent. The implementation, automated tests, and
content schemas remain authoritative for current runtime behavior. When a change
alters both behavior and design intent, update both in the same pull request.
