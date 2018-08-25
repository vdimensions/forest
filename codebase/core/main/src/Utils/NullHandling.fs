namespace Forest

module internal NullHandling =
    let inline null2opt arg = if System.Object.ReferenceEquals(arg, null) then None else Some arg

    let inline null2vopt arg = if System.Object.ReferenceEquals(arg, null) then ValueNone else ValueSome arg

    //let inline nullable2opt (arg: System.Nullable<'a>) : 'a option = if arg.HasValue then Some (arg.GetValueOrDefault()) else None

    //let inline nullable2vopt (arg: System.Nullable<'a>) : 'a voption = if arg.HasValue then ValueSome (arg.GetValueOrDefault()) else ValueNone

    //let inline vopt2opt (arg: 'a voption): 'a option = match arg with ValueSome a -> Some a | ValueNone -> None

    let inline isNotNull argName arg = if System.Object.ReferenceEquals(arg, null) then nullArg argName else arg 

    let inline (|NotNull|) argName arg = arg |> isNotNull argName

    let inline nil<'T when 'T: not struct> = Unchecked.defaultof<'T>