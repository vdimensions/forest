using Forest.Dom;


namespace Forest.Security
{
    public sealed class SecurityDomVisitor : AbstractDomVisitor
    {
        private readonly IForestContext context;

        public SecurityDomVisitor(IForestContext context)
        {
            this.context = context;
        }
    }
}
