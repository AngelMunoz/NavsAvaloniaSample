module NavsAvalonia.Routing

open System
open Avalonia.Controls
open FSharp.Data.Adaptive

open Navs
open Navs.Avalonia

open NavsAvalonia.Views
open System.Diagnostics


let getRouter
  (
    supabase: Supabase.Client,
    appState: aval<AppState>
  ) : IRouter<Control> =
  let routes = [
    Route.define("Home", "/", Home.get)
    |> Route.canActivate(fun _ _ -> async {

      let appState = appState |> AVal.force

      Debug.WriteLine("Checking if authenticated, {0}", appState)

      if appState.Authenticated then
        return Continue
      else
        return Redirect "/auth"
    })
    Route.define("Auth", "/auth", Auth.get(supabase.Auth))
  ]

  AvaloniaRouter(routes, splash = (fun nav -> Splash.get nav))
