namespace Forest.Globalization
{
    public interface ISupportsCustomGlobalizationKey<TModel> : IView<TModel>, ISupportsCustomGlobalizationKey
    {
        string ObtainGlobalizationKey(TModel model);
    }
    public interface ISupportsCustomGlobalizationKey : IView
    {
        string ObtainGlobalizationKey(object model);
    }
}