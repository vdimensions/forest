namespace Forest.Dom
{
    public interface IDomProcessor
    {
        DomNode ProcessNode(DomNode node, bool isNodeUpdated);
    }
}