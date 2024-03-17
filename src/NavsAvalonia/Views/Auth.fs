module NavsAvalonia.Views.Auth

open System
open System.Diagnostics

open Avalonia
open Avalonia.Controls
open Avalonia.Animation
open Avalonia.Layout

open NXUI.FSharp.Extensions

open FSharp.Data.Adaptive

open FsToolkit.ErrorHandling

open Supabase.Gotrue
open Supabase.Gotrue.Interfaces


open Navs
open Navs.Avalonia

open NavsAvalonia.Components
open System.Threading.Tasks
open Supabase.Gotrue.Exceptions

[<Struct>]
type AuthView =
  | Login
  | ConfirmOTP

[<Struct>]
type LoginFormError =
  | EmptyField of fieldname: string
  | MissingRequirement of emailReqName: string

module Data =

  let emailRegex = Text.RegularExpressions.Regex(@"^\S+@\S+$")


  let validateEmail (value: string) = validation {
    let! _ = value |> Result.requireNotEmpty(EmptyField "Email")

    and! _ =
      emailRegex.IsMatch(value)
      |> Result.requireTrue(MissingRequirement "Email must contain '@'")

    return ()
  }

  let enableButton (email) (enableButton) =
    adaptive {
      let! email = email
      let! enableButton = enableButton

      if not enableButton then
        return false
      else
        return
          match validateEmail email with
          | Ok _ -> true
          | _ -> false
    }
    |> AVal.toBinding

module Views =
  open Avalonia.Media

  let signIn (tryLogin) (email: cval<string>) =
    let enableSubmit = cval true

    let emailField =
      Forms.Input.get(
        email,
        label = "Email",
        placeholder = "Type your email:",
        validate = Data.validateEmail
      )

    UserControl()
      .content(
        StackPanel()
          .spacing(4)
          .children(
            emailField,
            Button()
              .content("Login")
              .isEnabled(Data.enableButton email enableSubmit)
              .OnClickHandler(fun _ _ ->
                let email = email.Value
                enableSubmit.setValue(false)

                Debug.WriteLine($"Tried to login with {email}")

                task {
                  let! success = tryLogin(email)

                  if not success then
                    Debug.WriteLine("Login failed")
                    enableSubmit.setValue(true)
                }
                |> ignore
              )
              .DockTop()
              .HorizontalAlignmentCenter()
          )
      )

  let confirmOTP onConfirm enableConfirm (email: aval<string>) =
    let otp = cval ""
    let showNotConfirmedMessage = cval false

    let otpField =
      Forms.Input.get(
        otp,
        label = "Confirm your one time password",
        placeholder = "One time password",
        kind = Forms.InputKind.Masked "#-#-#-#-#-#"
      )

    let notConfirmedMessage =
      adaptive {
        let! showNotConfirmedMessage = showNotConfirmedMessage

        if showNotConfirmedMessage then
          return
            StackPanel()
              .spacing(4)
              .children(
                TextBlock()
                  .text("The provided one time password is not valid.")
                  .fontSize(16.)
                  .foreground(Brushes.Red),
                TextBlock()
                  .text(
                    "please make sure you have copied the entire code from the email and try again."
                  )
                  .fontSize(16.)
                  .foreground(Brushes.Red)
              )
            :> Control
        else
          return TextBlock().text("").fontSize(16.)
      }
      |> AVal.toBinding

    UserControl()
      .content(
        StackPanel()
          .spacing(4)
          .children(
            TextBlock()
              .text("We've sent a magic link to your email")
              .fontSize(24.),
            TextBlock()
              .text(
                $"Please check your email at '{AVal.force email}', copy and paste the one time password here"
              )
              .fontSize(16.),
            otpField,
            ContentControl().content(notConfirmedMessage),
            Button()
              .content("Confirm")
              .isEnabled(enableConfirm |> AVal.toBinding)
              .OnClickHandler(fun _ _ ->
                task {
                  let! confirmed = onConfirm(otp.Value.Replace("-", ""))

                  if confirmed then
                    Debug.WriteLine("Confirmed")
                    showNotConfirmedMessage.setValue(false)
                  else
                    Debug.WriteLine("Not confirmed")
                    showNotConfirmedMessage.setValue(true)
                }
                |> ignore
              )
          )
      )

let viewOutlet email signIn confirmOTP currentView =
  TransitioningContentControl()
    .pageTransition(
      PageSlide(TimeSpan.FromMilliseconds(300), PageSlide.SlideAxis.Horizontal)
    )
    .content(
      adaptive {
        match! currentView with
        | Login -> return signIn(email)
        | ConfirmOTP -> return confirmOTP(email)
      }
      |> AVal.toBinding
    )

module Actions =
  let tryLogin (auth: IGotrueClient<_, _>, setView) (email: string) = task {
    try
      let! sent = auth.SignInWithOtp(SignInWithPasswordlessEmailOptions(email))

      Debug.WriteLine("Magic Link Sent: {0}", sent)
      setView(fun _ -> ConfirmOTP)
      return true
    with :? GotrueException as ex ->
      Debug.WriteLine("Error logging in: {0}", ex)
      return false
  }

  let tryVerifyOTP
    (auth: IGotrueClient<_, _>,
     nav: INavigable<Control>,
     email: cval<string>,
     enableConfirm: cval<bool>)
    (otp: string)
    =
    task {
      try
        enableConfirm.setValue(false)

        let! (session: Session) =
          auth.VerifyOTP(email.Value, otp, Constants.EmailOtpType.MagicLink)

        Debug.WriteLine("Session: {0}", session)

        do! nav.NavigateByName("Home") :> Task
        return true
      with :? GotrueException as ex ->
        enableConfirm.setValue(true)
        Debug.WriteLine("Error verifying OTP: {0}", ex)
        return false
    }

let get (auth: IGotrueClient<User, Session>) =

  fun (ctx: RouteContext) (nav: INavigable<Control>) -> async {
    let view, setView = AVal.useState Login
    let enableConfirm = cval true
    let email = cval ""

    let loginView = Views.signIn(Actions.tryLogin(auth, setView))

    let confirmOTP =
      Views.confirmOTP
        (Actions.tryVerifyOTP(auth, nav, email, enableConfirm))
        enableConfirm

    let viewOutlet = viewOutlet email loginView confirmOTP

    return
      DockPanel()
        .lastChildFill(true)
        .children(
          viewOutlet(view)
            .DockTop()
            .VerticalAlignmentCenter()
            .HorizontalAlignmentCenter()
            .HorizontalContentAlignmentStretch()
        )
  }
