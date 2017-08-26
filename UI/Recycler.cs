using Android.Support.V7.Widget;

namespace Warpten.Utils.UI
{
    public sealed class Recycler<T> where T : RecyclerView.Adapter
    {
        public RecyclerView View { get; }

        public T Adapter
        {
            get => View.GetAdapter() as T;
            set => View.SetAdapter(value);
        }

        public RecyclerView.LayoutManager LayoutManager
        {
            get => View.GetLayoutManager();
            set => View.SetLayoutManager(value);
        }

        public Recycler(RecyclerView view)
        {
            View = view;
        }
    }
}
