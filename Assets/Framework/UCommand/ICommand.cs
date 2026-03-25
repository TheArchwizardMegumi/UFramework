namespace UFramework
{
    public interface ICommand
    {
        bool CanUndo { get; }
        void Execute();
        void Undo(); 
        void Redo();
    }
}
