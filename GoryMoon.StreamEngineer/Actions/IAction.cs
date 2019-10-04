namespace GoryMoon.StreamEngineer.Actions
{
    public interface IAction
    {
        string Message { get; set; }

        void Execute();
    }
}