namespace NavsAvalonia

open Avalonia.Controls

open FSharp.Data.Adaptive

open type Supabase.Gotrue.Constants

open Navs
open Navs.Avalonia


[<Struct>]
type AppState = { Authenticated: bool }

module AppState =
  let get (supabase: Supabase.Client) =
    let state =
      match supabase.Auth.CurrentSession |> ValueOption.ofObj with
      | ValueSome session -> {
          Authenticated = not(session.Expired())
        }
      | ValueNone -> { Authenticated = false }

    let state, setState = AVal.useState(state)

    supabase.Auth.AddStateChangedListener(fun user session ->

      match session with
      | AuthState.SignedIn ->
        setState(fun state -> { state with Authenticated = true })
      | AuthState.SignedOut ->
        setState(fun state -> { state with Authenticated = false })
      | AuthState.Shutdown ->
        setState(fun state -> { state with Authenticated = false })
      | _ -> ()
    )

    state :> aval<_>


[<NoComparison; NoEquality; Struct>]
type AppEnv = {
  Router: IRouter<Control>
  Supabase: Supabase.Client
  AppState: AppState aval
}
