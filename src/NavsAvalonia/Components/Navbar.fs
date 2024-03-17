module NavsAvalonia.Components.Navbar

open Avalonia.Controls
open NXUI.FSharp.Extensions
open FSharp.Data.Adaptive
open Navs
open Navs.Avalonia
open NavsAvalonia
open System.Diagnostics

let leftSide (router: INavigable<Control>) =
  StackPanel()
    .children(
      Button()
        .content("Home")
        .OnClickHandler(fun _ _ -> router.Navigate("/") |> ignore)
    )

let rightSide (router: INavigable<Control>) = StackPanel().children()

let center (router: INavigable<Control>) = StackPanel().children()


let get (router: IRouter<_>, appState: aval<AppState>) =
  DockPanel()
    .lastChildFill(true)
    .children(
      (leftSide router).DockLeft().HorizontalAlignmentLeft(),
      (rightSide router).DockRight().HorizontalAlignmentRight(),
      (center router).DockTop().HorizontalAlignmentCenter()
    )
    .isVisible(
      adaptive {
        let! isAuthenticated = appState |> AVal.map _.Authenticated
        let! route = router.Route

        match route with
        | ValueSome { path = "/auth" } -> return isAuthenticated
        | _ -> return true
      }
      |> AVal.toBinding
    )
