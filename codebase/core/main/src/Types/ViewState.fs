namespace Forest

open System
open System.Runtime.InteropServices
open Axle.Verification

#if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
[<Serializable>]
[<StructLayout(LayoutKind.Sequential)>]
#endif
[<NoComparison>] 
type ViewState = 
    {
        Model : obj
    }

module ViewState =

    let internal withModelUnchecked (model) = 
        {
            Model = model
        }
    let withModel (NotNull "model" model) =  withModelUnchecked model