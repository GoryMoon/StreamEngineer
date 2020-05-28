using Nett;
using Nett.Coma;

namespace GoryMoon.StreamEngineer.Config
{
    public class Configuration
    {
        public static Config<TokenConfig> Token;
        public static Config<PluginConfig> Plugin;


        public class TokenConfig
        {
            [TomlComment("Login to Streamlabs and go to this link: https://streamlabs.com/dashboard#/settings/api-settings")]
            [TomlComment("Click on 'API Tokens' and copy the 'Your Socket API Token' here")]
            public string StreamlabsToken { get; set; } = "";
            
            [TomlComment("Add the Stream Engineer extension to your channel and copy the token from the configuration page")]
            public string TwitchExtensionToken { get; set; } = "";

            public static void Init(string path)
            {
                Token = Nett.Coma.Config.CreateAs()
                    .MappedToType(() => new TokenConfig())
                    .StoredAs(builder =>
                        builder.CustomStore(CustomFileStore.Create($"{path}/token-settings{Toml.FileExtension}")))
                    .Initialize();
            }
        }

        public class PluginConfig
        {
            [TomlComment("Shows a popup when the game start to inform about this file, auto sets to false")]
            public bool ShowMenuPopup { get; set; } = true;

            [TomlComment("Display name of your steam account so actions know what player to target")]
            public string SteamName { get; set; } = "";
            
            [TomlComment("The 'SpawnGroupDefinition' drones to be able to spawn when in space")]
            public string[] SpaceDrones { get; set; } =
            {
                "Blue Drone MK1", "Blue Drone MK2", "Red Drone MK1", "Light patrol drone", "Escord Drone", "Vulture Drone",
                "ProtectoBot", "Raider Drone", "Salvage Drone", "Seeker Mine", "Snub Fighter", "Stash-satellite",
                "V2-Gunboat", "Hostile Miner", "Tusk", "DroneS 1GG_1", "DroneS 1GG_2", "DroneS 1GG_3",
                "DroneS Drill.Warhead", "DroneS 2GG.1GT", "DroneS 1MT.2GG", "DroneS 2GG.1GT.1RL", "DroneS 2GG.2GT",
                "DroneL 1RL.1GG.2IT", "BossDroneL 1RL.2GT.1MT.2IT"
            };
            
            [TomlComment("The 'SpawnGroupDefinition' drones to be able to spawn when on a planet")]
            public string[] PlanetDrones { get; set; } = { "Vulture Drone" };
            
            public EventMessages Events { get; set; } = new EventMessages();

            public static void Init(string path)
            {
                Plugin = Nett.Coma.Config.CreateAs()
                    .MappedToType(() => new PluginConfig())
                    .StoredAs(builder =>
                        builder.CustomStore(CustomFileStore.Create($"{path}/settings{Toml.FileExtension}")))
                    .Initialize();
            }

            public class EventMessages
            {
                [TomlComment("0:Name of donor, 1:Formatted amount donated")]
                public SingleMessageEvent Donation { get; set; } = new SingleMessageEvent("{0} donated {1}!", true);

                public TwitchSubscriptionEvent TwitchSubscription { get; set; } = new TwitchSubscriptionEvent();

                [TomlComment("0:Name of cheerer, 1:Amount cheered")]
                public SingleMessageEvent TwitchBits { get; set; } = new SingleMessageEvent("{0} cheered with {1} bits!", true);

                [TomlComment("0:Name of follower")] public SingleMessageEvent TwitchFollowed { get; set; } = new SingleMessageEvent("{0} followed!");

                [TomlComment("0:Name of user hosting, 1:Amount of viewers")]
                public SingleMessageEvent TwitchHost { get; set; } = new SingleMessageEvent("{0} hosted with {1} viewers!");

                [TomlComment("0:Name of raider, 1:Amount of viewers")]
                public SingleMessageEvent TwitchRaid { get; set; } = new SingleMessageEvent("{0} raided with {1} viewers!");

                [TomlComment("0:Name of sender, 1:Amount bits sent")]
                public TwitchExtensionEvent TwitchExtension { get; set; } = new TwitchExtensionEvent();
                
                [TomlComment("0:Name of user")]
                public SingleMessageEvent TwitchChannelPoints { get; set; } = new SingleMessageEvent("{0} ran action with channel points!", true);
                
                [TomlComment("0:Name of subscriber")]
                public SingleMessageEvent YoutubeSubscription { get; set; } = new SingleMessageEvent("{0} subscribed!");

                public YoutubeSponsorEvent YoutubeSponsor { get; set; } = new YoutubeSponsorEvent();

                [TomlComment("0:Name of superchatter, 1:Formatted amount superchatted")]
                public SingleMessageEvent YoutubeSuperchat { get; set; } = new SingleMessageEvent("{0} superchatted with {1}!");

                public MixerSponsorEvent MixerSubscription { get; set; } = new MixerSponsorEvent();

                [TomlComment("0:Name of follower")]
                public SingleMessageEvent MixerFollowMessage { get; set; } = new SingleMessageEvent("{0} followed!");

                [TomlComment("0:Name of user hosting, 1:Amount of viewers")]
                public SingleMessageEvent MixerHostMessage { get; set; } = new SingleMessageEvent("{0} hosted with {1} viewers!");
            }

            public class SingleMessageEvent
            {
                public SingleMessageEvent()
                {
                }

                public SingleMessageEvent(string message, bool alwaysSendMessage = false)
                {
                    Message = message;
                    AlwaysSendMessage = alwaysSendMessage;
                }

                public string Message { get; set; }

                [TomlComment("If an message should be sent even if no action is executed")]
                public bool AlwaysSendMessage { get; set; }
            }
            

            public class TwitchSubscriptionEvent
            {
                [TomlComment("0:Name of subscriber, 1:Tier of subscription")]
                public string NewMessage { get; set; } = "{0} subscribed with a {1} subscription!";

                [TomlComment("0:Name of subscriber, 1:Tier of subscription, 2:Amount of months")]
                public string ResubMessage { get; set; } = "{0} subscribed with a {1} subscription for {2} months!";

                [TomlComment("If an message should be sent event if no action is executed")]
                public bool AlwaysSendMessage { get; set; } = true;

                public string Tier1 { get; set; } = "Tier 1";
                public string Tier2 { get; set; } = "Tier 2";
                public string Tier3 { get; set; } = "Tier 3";
            }

            public class TwitchExtensionEvent
            {
                public TwitchExtensionEvent()
                {
                }

                public string WithMessage { get; set; } = "{0} sent {1} bits for action:";
                
                public string WithoutMessage { get; set; } = "{0} sent {1} bits!";
            }
            
            public class YoutubeSponsorEvent
            {
                [TomlComment("0:Name of sponsor")] public string NewMessage { get; set; } = "{0} sponsored the channel!";

                [TomlComment("0:Name of sponsor, 1:Amount of months")]
                public string ResubMessage { get; set; } = "{0} sponsored the channel for {1} months!";

                [TomlComment("If an message should be sent event if no action is executed")]
                public bool AlwaysSendMessage { get; set; } = false;
            }

            public class MixerSponsorEvent
            {
                [TomlComment("0:Name of subscriber")] public string NewMessage { get; set; } = "{0} subscribed to the channel!";

                [TomlComment("0:Name of subscriber, 1:Amount of months")]
                public string ResubMessage { get; set; } = "{0} subscribed for {1} months!";

                [TomlComment("If an message should be sent event if no action is executed")]
                public bool AlwaysSendMessage { get; set; } = false;
            }
        }
    }
}