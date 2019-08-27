namespace Forest.Templates
{
    public interface IForestTemplateMarshallerRegistry
    {
        IForestTemplateMarshallerRegistry Register(IForestTemplateMarshaller marshaller);
    }
}