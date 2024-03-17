module NavsAvalonia.Supabase

open System
open Supabase
open Supabase.Gotrue.Interfaces

module SessionHandler =
  open System.IO
  open System.Text.Json
  open System.Text.Json.Serialization

  let AppDir =
    Path.Join(AppDomain.CurrentDomain.BaseDirectory, "current-session.json")

  let saveSession session =
    let content = JsonSerializer.SerializeToUtf8Bytes<Gotrue.Session>(session)

    use file =
      File.Open(AppDir, FileMode.Create, FileAccess.Write, FileShare.Read)

    file.Write(ReadOnlySpan(content))

  let destroySession () =
    try
      File.Delete(AppDir)
    with :? FileNotFoundException ->
      ()

  let loadSession () =
    try
      use file = File.OpenRead(AppDir)
      let session = JsonSerializer.Deserialize<Gotrue.Session>(file)
      session
    with :? FileNotFoundException ->
      null

  let get () =
    { new IGotrueSessionPersistence<Gotrue.Session> with
        override _.LoadSession() = loadSession()
        override _.DestroySession() = destroySession()
        override _.SaveSession(session) = saveSession session
    }


let getSupabaseClient sessionHandler (url, key) =
  let client =
    Client(
      url,
      key,
      options =
        SupabaseOptions(
          AutoRefreshToken = true,
          SessionHandler = sessionHandler
        )
    )

  client.Auth.SetPersistence(sessionHandler)
  client
