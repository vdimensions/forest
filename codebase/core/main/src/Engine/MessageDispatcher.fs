namespace Forest

module internal MessageDispatcher =
    type [<Sealed;NoComparison>] View() = 
        inherit LogicalView()
        interface ISystemView