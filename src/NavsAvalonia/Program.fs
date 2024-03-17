open System
open System.Threading.Tasks

open Avalonia
open Avalonia.Controls
open Avalonia.Controls.ApplicationLifetimes
open Avalonia.Themes.Fluent
open Avalonia.Styling

open NXUI.FSharp.Extensions

open FSharp.Data.Adaptive

open Navs
open NavsAvalonia
open NavsAvalonia.Views

type App(env: AppEnv) =
  inherit Application()

  override this.Initialize() =
    this.Styles.Add(FluentTheme())
    this.RequestedThemeVariant <- ThemeVariant.Default

  override this.OnFrameworkInitializationCompleted() =
    let content = Shell.get(env)

    match this.ApplicationLifetime with
    | :? IClassicDesktopStyleApplicationLifetime as desktop ->
      desktop.MainWindow <-
        Window(Content = content).height(400.).width(400.).title("Navs Sample!")
    | :? ISingleViewApplicationLifetime as singleView ->
      singleView.MainView <- content
    | _ -> ()

AppBuilder
  .Configure<App>(fun _ ->

    let sessionHandler = Supabase.SessionHandler.get()

    let getSupabaseClient = Supabase.getSupabaseClient sessionHandler

    let supabase =
      getSupabaseClient(
        Environment.GetEnvironmentVariable("SUPABASE_URL"),
        Environment.GetEnvironmentVariable("SUPABASE_KEY")
      )

    task {
      do! supabase.InitializeAsync() :> Task
      supabase.Auth.LoadSession()
      do! supabase.Auth.RetrieveSessionAsync() :> Task
    }
    |> Async.AwaitTask
    |> Async.RunSynchronously

    let appState = AppState.get supabase

    let router = Routing.getRouter(supabase, appState)

    router.Navigate("/")
    |> Async.AwaitTask
    |> Async.Ignore
    |> Async.StartImmediate


    let appEnv: AppEnv = {
      Router = router
      Supabase = supabase
      AppState = appState
    }

    App(appEnv)
  )
  .UsePlatformDetect()
  .StartWithClassicDesktopLifetime(Environment.GetCommandLineArgs())
|> ignore
