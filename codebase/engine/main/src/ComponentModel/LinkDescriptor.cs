namespace Forest.ComponentModel
{
    internal sealed class LinkDescriptor : AttributeDescriptor<LinkToAttribute>, ILinkDescriptor
    {
        public LinkDescriptor(LinkToAttribute attribute) : base(attribute)
        {
        }

        public string Name => Attribute.Tree;
    }
}