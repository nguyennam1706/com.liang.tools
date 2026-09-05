namespace LiangTools.Debugging
{
    public interface IDebugPage
    {
        string Title { get; }

        int Order { get; }

        void Draw(DebugUi ui);
    }
}
