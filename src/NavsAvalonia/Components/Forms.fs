module NavsAvalonia.Components.Forms

open System
open System.Collections.Generic

open Avalonia.Controls
open FSharp.Data.Adaptive
open FsToolkit.ErrorHandling
open NXUI.FSharp.Extensions

open Navs.Avalonia

open NavsAvalonia
open Avalonia.Layout


[<Struct>]
type InputKind =
  | Text
  | Password
  | Masked of mask: string

type Input =

  static member get
    (
      value: cval<'a>,
      ?label: string,
      ?placeholder: string,
      ?kind: InputKind,
      ?validate: 'a -> Validation<unit, 'OnError>
    ) =
    let validate = defaultArg validate (fun _ -> Ok())
    let label = defaultArg label ""
    let watermark = defaultArg placeholder ""
    let kind = defaultArg kind Text

    let touched, setTouched = AVal.useState false

    let hasErrors =
      adaptive {
        let! value = value
        let! touched = touched

        if not touched then
          return false
        else

          match validate value with
          | Ok _ -> return false
          | Error _ -> return true
      }
      |> AVal.toBinding

    let errors =
      adaptive {
        let! value = value
        let! touched = touched

        if not touched then
          return ResizeArray() :> ICollection<_>
        else
          match validate value with
          | Ok _ -> return ResizeArray() :> ICollection<_>
          | Error err -> return ResizeArray(err)
      }
      |> AVal.toBinding

    let field =
      let field =
        match kind with
        | Masked mask -> MaskedTextBox().mask(mask) :> TextBox
        | _ -> TextBox()

      field
        .classes("form-field-input")
        .text(value |> AVal.bindTwoWay)
        .watermark(watermark)
        .useFloatingWatermark(true)
        .hasErrors(hasErrors)
        .errors(errors)
        .OnGotFocusHandler(fun _ _ -> setTouched(fun _ -> true))

    let field =
      match kind with
      | Password -> field.passwordChar('*')
      | Text
      | Masked _ -> field

    StackPanel()
      .spacing(4.)
      .classes("form-field")
      .children(
        TextBlock().classes("form-field-label").text(label),
        field,
        DataValidationErrors().owner(field)
      )
