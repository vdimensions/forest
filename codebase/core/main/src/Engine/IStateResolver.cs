using Forest.StateManagement;

namespace Forest.Engine
{
    internal interface IStateResolver
    {
        ForestState ResolveState();
    }
}