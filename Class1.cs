using Exiled.API.Features; // Contains the main Player class and Log
using Exiled.API.Interfaces; // Contains IConfig
using Exiled.Events.EventArgs.Player; // Contains JoinedEventArgs
using Exiled.Events.Handlers; // Contains the static Players class for events
using MEC; // Contains Timing and CoroutineHandle
using RemoteAdmin.Communication;
using System;
using System.Collections; // Required for IEnumerator (Coroutines)
using System.Collections.Generic; // Required for IEnumerator<float>
// Removed using System.Numerics; as it's not used

namespace InfinityStamina // Your specified namespace
{
    // Configuration class for the plugin
    public class InfinityStaminaConfig : IConfig
    {
        public bool IsEnabled { get; set; } = true; // Required property for plugin status
        public bool Debug { get; set; } = false; // Optional debug mode

        // Configuration item: Interval for stamina restoration in seconds
        // Lower values make stamina restoration faster but might slightly increase server load.
        public float StaminaRestoreInterval { get; set; } = 0.1f; // Default: restore every 0.1 seconds
    }

    // Main plugin class, inheriting from Plugin<ConfigClass>
    public class Class1 : Plugin<InfinityStaminaConfig>
    {
        // Plugin information properties
        public override string Name => "InfinityStamina"; // Plugin name
        public override string Author => "StellarMatix"; // Plugin author (replace with your name)
        public override string Prefix => "IS"; // Plugin prefix for logs
        public override Version Version { get; } = new Version(1, 1, 0); // Plugin version
        public override Version RequiredExiledVersion { get; } = new Version(8, 0, 0); // Required EXILED version

        // Reference to the running coroutine, now using MEC.CoroutineHandle
        private CoroutineHandle _infiniteStaminaCoroutine;

        // Called when the plugin is enabled
        public override void OnEnabled()
        {
            Log.Info($"{Name} v{Version} 已启用! by {Author}"); // Log plugin enabled status
            RegisterEvents(); // Register event handlers
            // Start the coroutine for infinite stamina, using Timing.RunCoroutine and assigning to CoroutineHandle
            _infiniteStaminaCoroutine = Timing.RunCoroutine(InfiniteStaminaCoroutine());
            base.OnEnabled();
        }

        // Called when the plugin is disabled
        public override void OnDisabled()
        {
            UnregisterEvents(); // Unregister event handlers
            // Stop the infinite stamina coroutine using Timing.KillCoroutine (singular)
            if (_infiniteStaminaCoroutine.IsRunning) // Check if the coroutine is running before killing
            {
                Timing.KillCoroutines(_infiniteStaminaCoroutine); // Corrected method name
                // No need to set to null after killing, CoroutineHandle is a struct
            }
            Log.Info($"{Name} 已禁用!"); // Log plugin disabled status
            base.OnDisabled();
        }

        // Called when the plugin is reloaded
        public override void OnReloaded()
        {
            // Handle plugin reload logic if needed
            base.OnReloaded();
        }

        // Method to register event handlers
        private void RegisterEvents()
        {
            // Subscribe to the PlayerJoined event using Exiled.Events.Handlers.Player
            Exiled.Events.Handlers.Player.Joined += OnPlayerJoined;
        }

        // Method to unregister event handlers
        private void UnregisterEvents()
        {
            // Unsubscribe from the PlayerJoined event
            Exiled.Events.Handlers.Player.Joined -= OnPlayerJoined;
        }

        // Event handler for PlayerJoined event
        private void OnPlayerJoined(JoinedEventArgs ev)
        {
            // ev.Player is already of type Exiled.API.Features.Player
            //Log.Info($"玩家 {ev.Player.DisplayNickname} ({ev.Player.UserId}) 加入了服务器!"); // Log player join
            // Send a welcome message to the joining player
            ev.Player.SendConsoleMessage("欢迎来到服务器！你拥有无限体力。", "green");
        }

        // Coroutine to continuously restore player stamina
        // Now returns IEnumerator<float> as required by MEC
        private IEnumerator<float> InfiniteStaminaCoroutine()
        {
            while (true) // Infinite loop to keep the coroutine running
            {
                // Iterate through all connected players using Exiled.API.Features.Player.List
                foreach (Exiled.API.Features.Player player in Exiled.API.Features.Player.List)
                {
                    // Check if the player is valid and connected
                    player.IsUsingStamina = false;
                    player.ResetStamina();
                    //if (player != null && player.IsConnected)
                    //{
                    //    // Set the player's stamina to a fixed maximum value (workaround if MaxStamina is unavailable)
                    //    // The standard way is player.Stamina = player.MaxStamina;
                    //    player.Stamina = 1f; // Setting stamina to 1 (usually max)
                    //}
                }

                // Wait for the specified interval before the next iteration
                yield return Timing.WaitForSeconds(Config.StaminaRestoreInterval);
            }
        }
    }
}
