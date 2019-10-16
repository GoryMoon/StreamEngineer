using System;
using Newtonsoft.Json.Linq;

namespace GoryMoon.StreamEngineer.Data
{
    public abstract class BaseDataHandler: IDisposable
    {
        public ILogger Logger { get; }

        protected BaseDataHandler(ILogger logger)
        {
            Logger = logger;
        }

        public abstract void OnDonation(string name, int amount, string formatted);
        public abstract void OnTwitchSubscription(string name, int months, string tier, bool resub);
        public abstract void OnYoutubeSponsor(string name, int months);
        public abstract void OnMixerSubscription(string name, int months);
        public abstract void OnTwitchFollow(string name);
        public abstract void OnYoutubeSubscription(string name);
        public abstract void OnMixerFollow(string name);
        public abstract void OnTwitchHost(string name, int viewers);
        public abstract void OnMixerHost(string name, int viewers);
        public abstract void OnTwitchBits(string name, int amount);
        public abstract void OnTwitchRaid(string name, int amount);
        public abstract void OnYoutubeSuperchat(string name, int amount, string formatted);
        public abstract void OnTwitchExtension(string name, int amount, string action, JToken settings);
        public abstract void Dispose();
    }
}