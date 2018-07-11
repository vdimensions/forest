using Forest.Dom;


namespace Forest.Security
{
    public sealed class SecurityDomVisitor : AbstractDomVisitor
    {
        private readonly IForestContext _context;

        public SecurityDomVisitor(IForestContext context)
        {
            _context = context;
        }
    }
}
