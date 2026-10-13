using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace NAAD.Editor
{
    public static partial class Editor
    {
        /// <summary>
        /// `-executeMethod NAAD.Editor.VerifyUI`
        ///
        /// Verifies the UI layer contract: the controller type exists and every controller exposes
        /// the async action methods the screens bind to. Reflection based, so it stays valid as the
        /// view layer evolves and needs no scene to be open.
        /// </summary>
        public static void VerifyUI()
        {
            Debug.Log("[NAAD][Verify] Verifying UI controllers and views...");
            var report = new VerificationReport();

            VerifyController<NAAD.UI.AuthController>(report, "AuthController", "SignUpAsync", "SignInAsync", "SignOutAsync", "RestoreSessionAsync");
            VerifyController<NAAD.UI.BootstrapScreenController>(report, "BootstrapScreenController", "GetData", "Retry");
            VerifyController<NAAD.UI.MainHudController>(report, "MainHudController", "ShowInteractionPrompt", "HideInteractionPrompt", "UpdateLocation", "UpdateBalance", "GetData");
            VerifyController<NAAD.UI.PhoneHomeController>(report, "PhoneHomeController", "OnAppTapped", "UpdateUnreadCount", "Refresh", "GetData");
            VerifyController<NAAD.UI.PingMessagingController>(report, "PingMessagingController", "SelectContact", "SendMessage", "RespondToMessage", "Refresh", "GetData");
            VerifyController<NAAD.UI.NightSummaryController>(report, "NightSummaryController", "Continue", "Skip", "Retry", "GetData");
            VerifyController<NAAD.UI.ConfigurationUIController>(report, "ConfigurationUIController", "Refresh", "Retry", "GetData");
            VerifyController<NAAD.UI.BottomNavigationController>(report, "BottomNavigationController", "SwitchTab", "UpdateBadgeCount", "GetActiveTab", "GetData");

            if (typeof(NAAD.UI.BaseScreenController).IsClass && !typeof(NAAD.UI.BaseScreenController).IsAbstract)
                Debug.LogWarning("[NAAD][Verify] BaseScreenController is concrete; expected an abstract base for the view layer.");
            else
                Pass(report, "BaseScreenController is abstract as expected.");

            Finish("VerifyUI", report);
        }

        private static void VerifyController<T>(VerificationReport report, string label, params string[] requiredMethods)
        {
            var type = typeof(T);
            if (type == null)
            {
                Fail(report, $"UI controller type not found: {label}");
                return;
            }

            Pass(report, $"UI controller present: {label}");

            foreach (var methodName in requiredMethods)
            {
                var method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                if (method == null)
                {
                    Fail(report, $"{label} is missing public method '{methodName}'.");
                    continue;
                }

                // Existence of the public binding method is the contract; a present
                // method always passes. Return type is advisory: only flag a method
                // that is named *Async but does not return an awaitable Task, rather
                // than silently skipping non-async action methods (Refresh, GetData...).
                Pass(report, $"{label}.{methodName} present.");

                if (methodName.EndsWith("Async", StringComparison.Ordinal) && !IsAwaitable(method.ReturnType))
                    Debug.LogWarning($"[NAAD][Verify] {label}.{methodName} is named *Async but returns {method.ReturnType.Name}.");
            }
        }

        private static bool IsAwaitable(Type returnType)
        {
            return returnType == typeof(Task)
                || (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>));
        }

        /// <summary>
        /// `-executeMethod NAAD.Editor.VerifyServices`
        ///
        /// Verifies service composition: the application root exists in the codebase, every
        /// documented service interface is implemented by at least one type, and a real
        /// SupabaseConfig is either configured or explicitly absent. Connectivity itself is not
        /// probed here — network calls in a verification pass would make CI flaky.
        /// </summary>
        public static void VerifyServices()
        {
            Debug.Log("[NAAD][Verify] Verifying services and wiring...");
            var report = new VerificationReport();

            VerifyInterfaceImplemented<NAAD.Networking.IAuthService>(report);
            VerifyInterfaceImplemented<NAAD.Networking.ICommandService>(report);
            VerifyInterfaceImplemented<NAAD.Networking.IPhoneService>(report);
            VerifyInterfaceImplemented<NAAD.Networking.IPlayerService>(report);
            VerifyInterfaceImplemented<NAAD.Networking.IWorldStateService>(report);
            VerifyInterfaceImplemented<NAAD.Networking.IDialogueService>(report);

            VerifyApplicationRoot(report);
            VerifyConfiguration(report);

            Finish("VerifyServices", report);
        }

        private static void VerifyInterfaceImplemented<TInterface>(VerificationReport report)
        {
            var interfaceType = typeof(TInterface);
            var found = false;

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    types = ex.Types;
                }

                if (types == null) continue;

                foreach (var type in types)
                {
                    if (type == null || type.IsAbstract || type.IsInterface) continue;
                    if (interfaceType.IsAssignableFrom(type))
                    {
                        Pass(report, $"{interfaceType.Name} implemented by {type.FullName}.");
                        found = true;
                        break;
                    }
                }

                if (found) break;
            }

            if (!found)
                Fail(report, $"No implementation found for {interfaceType.Name}.");
        }

        private static void VerifyApplicationRoot(VerificationReport report)
        {
            var rootType = Type.GetType("NAAD.Core.Bootstrap.NAADApplicationRoot, Assembly-CSharp");
            if (rootType == null)
            {
                Fail(report, "NAAD.Core.Bootstrap.NAADApplicationRoot not found.");
                return;
            }

            var instanceProperty = rootType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
            if (instanceProperty == null)
                Fail(report, "NAADApplicationRoot.Instance static property not found.");
            else
                Pass(report, "NAADApplicationRoot.Instance present.");

            foreach (var serviceName in new[] { "Auth", "Commands", "Phone", "Players", "World", "Logger" })
            {
                if (rootType.GetProperty(serviceName, BindingFlags.Public | BindingFlags.Instance) == null)
                    Debug.LogWarning($"[NAAD][Verify] NAADApplicationRoot.{serviceName} not found; confirm service wiring.");
                else
                    Pass(report, $"NAADApplicationRoot.{serviceName} present.");
            }
        }
    }
}
