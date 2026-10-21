using System;
using System.Collections.Generic;
using NAAD.Networking;
using NAAD.Networking.Supabase;
using NAAD.Mock;

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
            var auth = new MockAuthenticationService();
            var players = MockPlayerService.GetOrCreate();
            var world = new MockWorldStateService();
            var commands = new MockCommandService(auth, players, world, null);
            var phone = new MockPhoneService();

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