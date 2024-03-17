module NavsAvalonia.Views.Splash

open System.Threading.Tasks

open Avalonia.Controls
open Avalonia.Layout
open NXUI.FSharp.Extensions

open Navs
open System.Diagnostics

let get () =
  UserControl()
    .content(
      StackPanel()
        .children(
          TextBlock()
            .text("Loading...")
            .fontSize(24.)
            .horizontalAlignment(HorizontalAlignment.Center)
            .verticalAlignment(VerticalAlignment.Center)

        )
    )
    .VerticalAlignmentCenter()
    .HorizontalAlignmentCenter()
