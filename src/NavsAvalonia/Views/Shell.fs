module NavsAvalonia.Views.Shell

open System
open Avalonia
open Avalonia.Animation
open Avalonia.Controls
open Avalonia.Data

open NXUI.FSharp.Extensions
open FSharp.Data.Adaptive

open Navs
open Navs.Avalonia

open NavsAvalonia.Components

open NavsAvalonia


let get (env: AppEnv) =
  DockPanel()
    .lastChildFill(true)
    .children(
      Navbar.get(env.Router, env.AppState).DockTop(),
      RouterOutlet().router(env.Router)
    )
