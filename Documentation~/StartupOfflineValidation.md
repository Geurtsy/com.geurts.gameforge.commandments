# Offline startup — Documentation Companion 0.10.1

Validated on Windows in the existing isolated fixture with Unity 6000.3.24f1, Odin Inspector 4.0.2.4, Quantum Console 2.6.7, Input System 1.20.0 and Test Framework 1.6.0.

The Unity startup checker and its automatic availability popup were removed. Enabling/restoring the real Documentation dashboard requests no metadata. Deliberately opening its menu still requests the combined package/content checks once. Closing removes the pending callback. Active, explicitly started package requests keep their existing reload continuation.

All Documentation Editor tests passed in the combined 209-test God/Documentation run (208 initial passes; one God test-window cleanup fixture required correction and a successful 2/2 focused rerun). Existing confirmation, content replacement, saved consent, package status and version metadata coverage passed. Native God validation also confirmed the companion integration path remains usable after deliberate opted-in opening. No real content or package mutation was performed by these tests.

No companion visual layout changed. Restored-window and explicit-open behavior were exercised through its real Editor window. The canonical theme and generated companion copy were untouched. Light skin and high-DPI were unavailable. The live project was not upgraded, restarted or rewritten. Evidence is retained in the integration workspace at `.local/bf/Evidence/StartupOfflineTests.xml` and `StartupOfflineVisualEvidence.txt`.
