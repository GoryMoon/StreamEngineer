namespace GoryMoon.StreamEngineer.Data
{
    public interface IDataHandler
    {
        ILogger Logger { get; }
        
        void OnDonation(string name, int amount, string formatted);
        
        void OnTwitchSubscription(string name, int months, string tier, bool resub);
        
        void OnYoutubeSponsor(string name, int months);
        
        void OnMixerSubscription(string name, int months);
        
        void OnTwitchFollow(string name);
        
        void OnYoutubeSubscription(string name);
        
        void OnMixerFollow(string name);
        
        void OnTwitchHost(string name, int viewers);
        
        void OnMixerHost(string name, int viewers);
        
        void OnTwitchBits(string name, int amount);
        
        void OnTwitchRaid(string name, int amount);
        
        void OnYoutubeSuperchat(string name, int amount, string formatted);
    }
}