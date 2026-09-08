# Native packaging

Run `./scripts/publish-native-client.ps1 -Runtime win-x64`, `linux-x64`, or `osx-arm64`. It creates a self-contained publish beneath `artifacts/native`, verifies the executable, managed entry assembly, Noto font, English catalog, and Windows SDL payload, rejects Unity project content, writes release metadata, and produces ZIP (Windows) or tar.gz (Unix) archives. End users do not need the .NET SDK.

Windows x64 is the only currently runnable package: SDL3 is app-local and Skia/HarfBuzz native assets come from packages. The CI matrix builds/tests/publishes Windows, Linux, and macOS; Windows also runs packaged software-renderer `--smoke-test` from its publish directory. Linux and macOS archives are build evidence, not release-ready packages: repository-owned SDL binaries are absent, and macOS Skia/HarfBuzz payload validation is still required. Wayland/X11, IME, audio, and OS accessibility checks require real hosts.

`--smoke-test` creates platform paths and settings, loads localization/font/content, starts a deterministic campaign, renders a procedural roster portrait through software Skia, captures a PNG, and exits 0. Failure reaches local crash capture and exits nonzero. Use `--user-data=<path>` for an isolated Unicode or package test location.

No signing credentials are stored. Windows Authenticode and macOS codesign/notarization should run after archive validation using CI secrets; notarization must staple the accepted ticket before distribution. Application icon artwork is also outstanding. Packages contain no Unity runtime or project files.

Local validation on 2026-09-08 produced 46.5 MB Windows x64, 43.2 MB Linux x64, and 43.2 MB macOS arm64 archives. The Windows package completed the pseudo-localized, reduced-motion, high-contrast software smoke, including SDL audio-stream initialization, in 3.395 seconds and used a Unicode user-data directory. This timing is process start through clean smoke exit, not the distinct main-menu-interactive benchmark required before release.
