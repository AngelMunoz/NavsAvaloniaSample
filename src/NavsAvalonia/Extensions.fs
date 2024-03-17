[<AutoOpen>]
module NavsAvalonia.Extensions

open System
open Avalonia.Controls
open Avalonia.Data

open FSharp.Data.Adaptive

module AVal =

  let toObservable (value: aval<'a>) =
    { new IObservable<obj> with
        member _.Subscribe(observer) = value.AddCallback(observer.OnNext)
    }

  let toObserver (value: cval<'a>) =
    { new IObserver<obj> with
        member _.OnNext(newValue) =
          match newValue with
          | :? 'a as newValue -> transact(fun _ -> value.Value <- newValue)
          | _ -> ()

        member _.OnError(exn) = ()
        member _.OnCompleted() = ()
    }

  let bindTwoWay (value: cval<'a>) =
    { new IBinding with
        member this.Initiate
          (
            target: Avalonia.AvaloniaObject,
            targetProperty: Avalonia.AvaloniaProperty,
            anchor: obj,
            enableDataValidation: bool
          ) : InstancedBinding =
          let observable = toObservable value
          let observer = toObserver value

          InstancedBinding.TwoWay(
            observable,
            observer,
            priority = BindingPriority.LocalValue
          )
    }
