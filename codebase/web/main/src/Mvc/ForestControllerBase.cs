using Microsoft.AspNetCore.Mvc;

namespace Forest.Web.AspNetCore.Mvc
{
    public abstract class ForestControllerBase : ControllerBase
    {
        protected ForestControllerBase(ForestRequestExecutor forestRequestExecutor)
        {
            ForestRequestExecutor = forestRequestExecutor;
        }

        protected ForestRequestExecutor ForestRequestExecutor { get; }
        
    }
}