# Full embedded Companion dashboard - 0.13.2

Unity 6000.6.3f1 validation resolved the immutable Companion candidate d5221d7b5543f5d514ae5dba8b659bf39837975d and published God 0.26.2 from Git in a closed, isolated Windows project with licensed Odin Inspector and Quantum Console.

- Companion Editor suite: 157/157 passed, including both rendered embedded cards, offline opening/resizing/scrolling, standalone-window preservation and an embedded content confirmation that installs exactly once.
- God suite: 346 passed, zero failed and six fixture/Windows-permission checks skipped in the final combined run. Earlier combined runs exposed intermittent existing setup-navigation and Unity UI Toolkit modal-layout failures; the setup test passed in the subsequent combined run and the full Companion-only rerun passed with Unity exit code 0. These reruns do not establish that the existing UI test timing is deterministic.
- Actual God rendering was captured through Unity Editor APIs at 1000 and 560 Editor points. Check for updates, documentation and package version/update cards, dependencies and setup remained visible or reachable by scrolling; Back to God stayed pinned. Capture rows were normalized for upright PNG output.
- No C# compiler errors or warnings were reported in these runs. Static package structure validation passed; no God package or assembly dependency was added.

Rendered evidence used a dark Windows host at 1x scale with isolated status fixtures. Light host, high scaling and manual hover/keyboard acceptance remain unverified. No live consumer packages, scenes or installed documentation were updated.

## Historical 0.13.0 validation

# Embedded Documentation tools - 0.13.0

Unity 6000.6.3f1 validation used immutable Git candidates in isolated projects with the separately licensed Odin Inspector and Quantum Console.

- Standalone without God: all 154 Editor checks passed, including the independent factory and unchanged deliberate-opening checks.
- Combined God 0.26.0 and Documentation suite: 491 passed with six unrelated fixture-dependent checks skipped.
- Real embedded Documentation content was captured at 1000 and 560 Editor points inside God. Package/content update cards and the check group were absent; the update button rectangle remained empty.
- The factory returns an unshown independent window and starts no update check. Closing that view leaves a separate standalone window independent. A callback retained by a disposed view cannot reopen God content.
- Generated theme parity and authoritative documentation validation passed. No God assembly or package dependency was added.

Rendered evidence used a dark host at 1x scale. Light host, high scaling and manual hover/keyboard acceptance remain unverified. No live consumer packages, scenes or installed documentation were updated.
