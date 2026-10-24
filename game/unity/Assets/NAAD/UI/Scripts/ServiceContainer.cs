using System;
using System.Collections.Generic;
using NAAD.Core.Logging;
using NAAD.Networking;
using NAAD.Networking.Supabase;

namespace NAAD.UI.Scripts
{
    /// <summary>
    /// Central service container for dependency injection in the NAAD UI layer.
    /// <para>
    /// In automated tests, all services are stubbed via Mock implementations.
    /// In production, services are composed by <see cref="NAADApplicationRoot"/>
    /// and resolved through the container.
    /// </para>
    /// </summary>
    public static class ServiceContainer
    {
        private static readonly Dictionary<Type, object> _registrations = new();

        /// <summary>
        /// Register a service instance.
        /// </summary>
        public static void Register<T>(T implementation) where T : class
        {
            _registrations[typeof(T)] = implementation;
        }

        /// <summary>
        /// Resolve a service.
        /// </summary>
        public static T Resolve<T>() where T : class
        {
            if (_registrations.TryGetValue(typeof(T), out var obj))
            {
                return (T)obj;
            }
            return null;
        }

        /// <summary>
        /// Resolve a service or return the default.
        /// </summary>
        public static T? TryResolve<T>() where T : class
        {
            return _registrations.TryGetValue(typeof(T), out var obj) ? (T)obj : null;
        }

        /// <summary>
        /// Register a mock service for testing.
        /// </summary>
        public static void RegisterMockServices()
        {
            // Uses the current NAAD.Networking stub services (the same implementations
            // NAADApplicationRoot composes when no SupabaseConfig is assigned). The
            // previous NAAD.Mock.* copies targeted a removed command DTO API and never
            // compiled; they have been deleted.
            var logger = new UnityNAADLogger();
            IAuthService auth = new StubAuthService(logger);
            IPlayerService players = new StubPlayerService(auth, logger);
            IWorldStateService world = new StubWorldStateService(logger);
            ICommandService commands = new StubCommandService(logger);
            IPhoneService phone = new StubPhoneService(logger);

            Register(auth);
            Register(players);
            Register(world);
            Register(commands);
            Register(phone);
        }

        /// <summary>
        /// Register stubs for production use (called by NAADApplicationRoot).
        /// </summary>
        public static void RegisterProdServices(IAuthService auth, IPlayerService players, IWorldStateService world, ICommandService commands, IPhoneService phone, IDialogueService dialogue)
        {
            Register(auth);
            Register(players);
            Register(world);
            Register(commands);
            Register(phone);
            Register(dialogue);
        }

        public static void Clear()
        {
            _registrations.Clear();
        }
    }
}