namespace GoryMoon.StreamEngineer.Actions
{
    public class MeteorAction: IAction
    {
        public string Message { get; set; }
        public void Execute()
        {
            Plugin.Static.Game.Invoke(Plugin.Static.QueueMeteors, "QueueMeteorsDonation");
        }
    }
}