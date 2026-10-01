// IMPORTANT: This script must comply with GeurtsGameForgeCommandments/GeurtsTechniques/GeurtsTechnicalTechnique.md and folder placement rules in GeurtsGameForgeCommandments/GeurtsTechniques/GeurtsFolderStructureTechnique.md.
using System;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using UnityEditor;

namespace Geurts.GameForge.Documentation
{
    /// <summary>Compatibility adapter to God's single Commandments Editor service; owns no updates.</summary>
    public static class DocumentationIntegration
    {
        /// <summary>Identifies this passive adapter to God without invoking content work.</summary>
        public static int AdapterProtocolVersion => 1;
        /// <summary>Reads IsBusy from the God-owned service without starting work.</summary>
        public static bool IsBusy => GodForwarder.Read<bool>("IsBusy", false);
        /// <summary>Reads ModuleEnabled from the God-owned service without starting work.</summary>
        public static bool ModuleEnabled => GodForwarder.Read<bool>("ModuleEnabled", false);
        /// <summary>Reads ModuleToggleUnavailableReason from the God-owned service without starting work.</summary>
        public static string ModuleToggleUnavailableReason => GodForwarder.Read<string>("ModuleToggleUnavailableReason", null);
        /// <summary>Reads StatusMessage from the God-owned service without starting work.</summary>
        public static string StatusMessage => GodForwarder.Read<string>("StatusMessage", GodForwarder.MissingMessage);
        /// <summary>Reads InstalledVersion from the God-owned service without starting work.</summary>
        public static string InstalledVersion => GodForwarder.Read<string>("InstalledVersion", null);
        /// <summary>Reads AvailableVersion from the God-owned service without starting work.</summary>
        public static string AvailableVersion => GodForwarder.Read<string>("AvailableVersion", null);
        /// <summary>Reads Availability from the God-owned service without starting work.</summary>
        public static string Availability => GodForwarder.Read<string>("Availability", null);
        /// <summary>Reads Failed from the God-owned service without starting work.</summary>
        public static bool Failed => GodForwarder.Read<bool>("Failed", false);
        /// <summary>Reads ActionUnavailableReason from the God-owned service without starting work.</summary>
        public static string ActionUnavailableReason => GodForwarder.Read<string>("ActionUnavailableReason", GodForwarder.MissingMessage);
        /// <summary>Reads Progress from the God-owned service without starting work.</summary>
        public static float? Progress => GodForwarder.Read<float?>("Progress", null);
        /// <summary>Observes God's shared service; unsubscribe when the host closes.</summary>
        public static event Action Changed
        {
            add { GodForwarder.Content?.GetEvent("Changed")?.AddEventHandler(null, value); }
            remove { GodForwarder.Content?.GetEvent("Changed")?.RemoveEventHandler(null, value); }
        }
        /// <summary>Changes the retained project preference without starting work.</summary>
        /// <param name="enabled">Whether new content operations may start.</param>
        public static void SetModuleEnabled(bool enabled) => GodForwarder.Invoke(false, "SetModuleEnabled", enabled);
        /// <summary>Explicitly checks authoritative metadata through God.</summary>
        /// <returns>The shared check task.</returns>
        public static Task CheckForUpdatesAsync() => (Task)GodForwarder.Invoke(false, "CheckForUpdatesAsync");
        /// <summary>Shows God's existing cancel-default content confirmation.</summary>
        public static void UpdateDocumentation() => GodForwarder.Invoke(false, "UpdateCommandments");
        /// <summary>Rejects obsolete consent for the old documentation destination before work.</summary>
        /// <param name="stillAuthorized">Legacy authorization callback; never evaluated.</param>
        /// <returns>Always throws; renew schema-3 consent through God.</returns>
        public static Task UpdateDocumentationAutomaticallyAsync(Func<bool> stillAuthorized)
        {
            if (stillAuthorized == null) throw new ArgumentNullException(nameof(stillAuthorized));
            throw new InvalidOperationException("Documentation schema-2 automatic consent cannot update Commandments. Update God and explicitly renew automatic-update consent.");
        }
        /// <summary>Uses God's bounded automatic operation with live schema-3 consent.</summary>
        /// <param name="stillAuthorized">True only during the explicitly authorized host opening.</param>
        /// <returns>The shared bounded operation.</returns>
        public static Task UpdateCommandmentsAutomaticallyAsync(Func<bool> stillAuthorized) =>
            (Task)GodForwarder.Invoke(false, "UpdateCommandmentsAutomaticallyAsync", stillAuthorized);
        /// <summary>Opens the God-owned view without starting checks.</summary>
        public static void OpenWindow() => GodForwarder.Invoke(false, "OpenWindow");
        /// <summary>Creates a hidden God-owned view; the caller owns disposal.</summary>
        /// <param name="navigate">Optional host navigation callback.</param>
        /// <returns>An independent hidden Editor window.</returns>
        public static EditorWindow CreateEmbeddedWindow(Action<string> navigate) =>
            (EditorWindow)GodForwarder.Invoke(false, "CreateEmbeddedWindow", navigate);
        /// <summary>Registers a conflict guard in God's single service.</summary>
        /// <param name="unavailableReason">A reason while a host operation conflicts.</param>
        /// <returns>A token to dispose at host teardown.</returns>
        public static IDisposable RegisterOperationGuard(Func<string> unavailableReason) =>
            (IDisposable)GodForwarder.Invoke(false, "RegisterOperationGuard", unavailableReason);
    }

    internal static class GodForwarder
    {
        internal const string MissingMessage = "Commandments has moved into Game Forge God. Install or update God to 0.29.0 or newer. Local Commandments remain readable without God.";
        internal static Type Content => Type.GetType("Geurts.GameForge.God.Editor.CommandmentsIntegration, Geurts.GameForge.God.Editor", false);
        private static Type Setup => Type.GetType("Geurts.GameForge.God.Editor.CommandmentsSetupIntegration, Geurts.GameForge.God.Editor", false);
        internal static T Read<T>(string name, T fallback) => Content == null ? fallback : (T)Content.GetProperty(name).GetValue(null);
        internal static object Invoke(bool setup, string name, params object[] args)
        {
            Type type = setup ? Setup : Content;
            var method = type?.GetMethod(name, BindingFlags.Public | BindingFlags.Static);
            if (method == null) throw new InvalidOperationException(MissingMessage);
            try { return method.Invoke(null, args); }
            catch (TargetInvocationException error) when (error.InnerException != null)
            { ExceptionDispatchInfo.Capture(error.InnerException).Throw(); throw; }
        }
    }
}

namespace Geurts.GameForge.Commandments
{
    /// <summary>Canonical optional Editor API for Commandments Companion; legacy integration remains supported.</summary>
    public static class CommandmentsIntegration
    {
        /// <summary>Reports existing shared controller activity without starting work.</summary>
        public static bool IsBusy => Documentation.DocumentationIntegration.IsBusy;
        /// <summary>Reads the retained project module preference.</summary>
        public static bool ModuleEnabled => Documentation.DocumentationIntegration.ModuleEnabled;
        /// <summary>Explains why the shared module preference cannot change now.</summary>
        public static string ModuleToggleUnavailableReason => Documentation.DocumentationIntegration.ModuleToggleUnavailableReason;
        /// <summary>Reads the shared controller status message.</summary>
        public static string StatusMessage => Documentation.DocumentationIntegration.StatusMessage;
        /// <summary>Reads the installed Commandments content version.</summary>
        public static string InstalledVersion => Documentation.DocumentationIntegration.InstalledVersion;
        /// <summary>Reads the last explicitly checked content version.</summary>
        public static string AvailableVersion => Documentation.DocumentationIntegration.AvailableVersion;
        /// <summary>Reads the shared written availability state.</summary>
        public static string Availability => Documentation.DocumentationIntegration.Availability;
        /// <summary>Reports the current shared operation failure.</summary>
        public static bool Failed => Documentation.DocumentationIntegration.Failed;
        /// <summary>Explains why an explicit content action is unavailable.</summary>
        public static string ActionUnavailableReason => Documentation.DocumentationIntegration.ActionUnavailableReason;
        /// <summary>Reads measured progress; null means the underlying operation supplies no percentage.</summary>
        public static float? Progress => Documentation.DocumentationIntegration.Progress;
        /// <summary>Changes when the existing shared controller changes; unsubscribe on disposal.</summary>
        public static event System.Action Changed { add { Documentation.DocumentationIntegration.Changed += value; } remove { Documentation.DocumentationIntegration.Changed -= value; } }
        /// <summary>Sets the existing project module preference without starting work.</summary>
        /// <param name="enabled">Requested project module preference.</param>
        public static void SetModuleEnabled(bool enabled) => Documentation.DocumentationIntegration.SetModuleEnabled(enabled);
        /// <summary>Explicitly checks metadata through the shared controller.</summary>
        /// <returns>The existing controller metadata check.</returns>
        public static System.Threading.Tasks.Task CheckForUpdatesAsync() => Documentation.DocumentationIntegration.CheckForUpdatesAsync();
        /// <summary>Shows the schema-3 cancel-default confirmation before acquisition.</summary>
        public static void UpdateCommandments() => Documentation.DocumentationIntegration.UpdateDocumentation();
        /// <summary>Compatibility method used by supported legacy host integrations; still shows the new confirmation.</summary>
        public static void UpdateDocumentation() => UpdateCommandments();
        /// <summary>Updates for a host with explicit, live schema-3 target consent.</summary>
        /// <param name="stillAuthorized">Checks live explicit consent for schema-3 targets.</param>
        /// <returns>The bounded shared controller update.</returns>
        public static System.Threading.Tasks.Task UpdateCommandmentsAutomaticallyAsync(System.Func<bool> stillAuthorized) => Documentation.DocumentationIntegration.UpdateCommandmentsAutomaticallyAsync(stillAuthorized);
        /// <summary>Opens the existing independently owned Companion dashboard.</summary>
        public static void OpenWindow() => Documentation.DocumentationIntegration.OpenWindow();
        /// <summary>Creates a hidden full dashboard, without network work on opening.</summary>
        /// <param name="navigate">Optional host brick navigation.</param>
        /// <returns>A hidden dashboard owned and disposed by the host.</returns>
        public static UnityEditor.EditorWindow CreateEmbeddedWindow(System.Action<string> navigate) => Documentation.DocumentationIntegration.CreateEmbeddedWindow(navigate);
        /// <summary>Registers a host conflict guard; dispose the returned token at teardown.</summary>
        /// <param name="unavailableReason">Returns a conflict reason, or null when actions are allowed.</param>
        /// <returns>A subscription token that the host must dispose.</returns>
        public static System.IDisposable RegisterOperationGuard(System.Func<string> unavailableReason) => Documentation.DocumentationIntegration.RegisterOperationGuard(unavailableReason);
    }
}
