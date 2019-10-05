namespace Forest.Templates
{
    public interface ITemplateProvider
    {
        Template Load(string name);
    }
}
