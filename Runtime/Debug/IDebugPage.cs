namespace LiangTools.Debugging
{
    public interface IDebugPage
    {
        string Title { get; }

        int Order { get; }

        void Draw(DebugUi ui);

        /// <summary>
        /// Short text shown on the page's tab — a count of something worth noticing
        /// without opening the page. Null or empty for no badge.
        /// </summary>
        string Badge => null;
    }
}
