using System;

namespace GoryMoon.StreamEngineer.Data
{
    public class Testing
    {

        public static void Main()
        {
            var dataHandler = new TestDataHandler();
            var streamlabsData = new StreamlabsData(dataHandler);
            var token = Environment.GetEnvironmentVariable("token");
            streamlabsData.Init(token);
            Console.WriteLine("Stop with: q");
            
            while (true)
            {
                var text = Console.ReadLine();
                if ("q".Equals(text))
                {
                    streamlabsData.Dispose();
                    return;
                }
            }
            
        }
        
        private class TestLogger: ILogger 
        {
            public void WriteLine(string msg)
            {
                Console.WriteLine(msg);
            }

            public void WriteLine(Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private class TestDataHandler: IDataHandler
        {
            public ILogger Logger => new TestLogger();

            public void OnDonation(string name, int amount, string formatted)
            {
                Logger.WriteLine("OnDonation");
            }

            public void OnTwitchSubscription(string name, int months, string tier, bool resub)
            {
                Logger.WriteLine("OnTwitchSubscription");
            }

            public void OnYoutubeSponsor(string name, int months)
            {
                Logger.WriteLine("OnYoutubeSponsor");
            }

            public void OnMixerSubscription(string name, int months)
            {
                Logger.WriteLine("OnMixerSubscription");
            }

            public void OnTwitchFollow(string name)
            {
                Logger.WriteLine("OnTwitchFollow");
            }

            public void OnYoutubeSubscription(string name)
            {
                Logger.WriteLine("OnYoutubeSubscription");
            }

            public void OnMixerFollow(string name)
            {
                Logger.WriteLine("OnMixerFollow");
            }

            public void OnTwitchHost(string name, int viewers)
            {
                Logger.WriteLine("OnTwitchHost");
            }

            public void OnMixerHost(string name, int viewers)
            {
                Logger.WriteLine("OnMixerHost");
            }

            public void OnTwitchBits(string name, int amount)
            {
                Logger.WriteLine("OnTwitchBits");
            }

            public void OnTwitchRaid(string name, int amount)
            {
                Logger.WriteLine("OnTwitchRaid");
            }

            public void OnYoutubeSuperchat(string name, int amount, string formatted)
            {
                Logger.WriteLine("OnYoutubeSuperchat");
            }
        }
    }
}