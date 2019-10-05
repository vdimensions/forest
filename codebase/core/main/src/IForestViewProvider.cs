using Forest.ComponentModel;
using Forest.Engine;

namespace Forest
{
    [RequiresForest]
    public interface IForestViewProvider
    {
        void RegisterViews(IViewRegistry registry);
    }
}