module NavsAvalonia.Views.Home

open Avalonia.Controls
open Avalonia.Layout

open NXUI.FSharp.Extensions

open Navs

let get (ctx: RouteContext) (nav: INavigable<Control>) =
  UserControl()
    .content(
      StackPanel()
        .children(

          TextBlock()
            .text("Hello, world!")
            .fontSize(24.)
            .horizontalAlignment(HorizontalAlignment.Center)
            .verticalAlignment(VerticalAlignment.Center)

        )
    )
