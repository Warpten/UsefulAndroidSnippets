using System;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V7.Widget;
using Android.Views;

namespace Warpten.Utils.UI
{
    public class CardViewFragment<T> : Fragment where T : RecyclerView.Adapter
    {
        public Recycler<T> Recycler { get; protected set; }
        public T Adapter => Recycler.Adapter;
        public RecyclerView.LayoutManager LayoutManager => Recycler.LayoutManager;

        public event Action ViewCreated;
        public event Action ViewDestroyed;

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            ViewCreated?.Invoke();

            base.OnViewCreated(view, savedInstanceState);
        }

        public override void OnDestroyView()
        {
            ViewDestroyed?.Invoke();

            base.OnDestroyView();
        }
    }
}
