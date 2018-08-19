#!fsharp
namespace Forest

/// <summary>
/// A helper class to allow the railway-programming approach to be easily applied to a number of similar types:
/// <para>
/// * <see cref="Option{T}"/>
/// </para>
/// <para>
/// * <see cref="Result{T, E}"/>
/// </para>
/// <para>
/// * <see cref="Array"/>
/// </para>
/// <para>
/// * <see cref="List"/>
/// </para>
/// Approach taken from https://stackoverflow.com/a/25643212/795158
/// </summary>
type Monad = 
    | Bind
    static member (?<-) (Bind, m: 'a option, _: 'b option) = 
        fun (f: (_-> 'b option)) ->
            match m with
            | None -> None
            | Some x -> f x

    static member (?<-) (Bind, m: Result<'a, 'e>, _: Result<'b, 'e>) = 
        fun (f: (_-> Result<'b, 'e>)) ->
            match m with
            | Ok x -> f x
            | Error e -> Error e

    static member (?<-) (Bind, m: System.Nullable<'a>, _: System.Nullable<'b>) = 
        fun (f: (_-> System.Nullable<'b>)) ->
            match nullable2opt m with
            | Some x -> f x
            | None -> System.Nullable<'b>()

    static member (?<-) (Bind, m: 'a array, _: 'b array) = 
        fun (f: (_ -> 'b array)) ->
            Array.map f m |> Array.concat

    static member (?<-) (Bind, m: 'a list, _: 'b list) = 
        fun (f: (_ -> 'b list)) ->
            List.map f m |> List.concat

[<AutoOpen>]
module Monad =
    let inline private _bind fn input : 'R = ( (?<-) Monad.Bind input Unchecked.defaultof<'R>) fn

    let inline (|>=) input fn = _bind fn input

    let inline (>>=) f g = f >> (_bind g)
