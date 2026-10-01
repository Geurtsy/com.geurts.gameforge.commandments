# Compatibility adapter 0.15.0 validation

Windows Unity **6000.6.3f1** compiled the adapter as an immutable Git package with God, Odin Inspector and Quantum Console absent. All three adapter tests passed with no compiler warnings. The same three tests passed alongside God 0.29.0 and BigBang 0.2.3 in the combined 580-pass suite.

Native UPM migration from God 0.28.1 and Companion 0.14.1 passed in both update orders. During the adapter-first transition, the adapter accepts the old God's guard registration without creating work, allowing old God to initialize and update packages. Content actions still report the missing supported God owner. New God with old Companion pauses both content owners until transition completes.

Final readback verified exact Git revisions, package versions, shared public API forwarding, retained window GUID, module preference and schema-3 commit/consent state. Package installation acquired no content and preserved every fixture sentinel, including guides, AI routes, old local documentation and game design. The live Unity project was never used as a validation fixture.

There are no adapter menu/command registrations, automatic initializers, runtime assemblies, network transports or independent updater. Historical startup/embedding reports in this repository describe earlier releases; the [migration guide](README.md) defines the current passive boundary.
