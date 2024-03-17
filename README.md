[Avalonia]: https://docs.avaloniaui.net/docs/welcome
[NXUI]: https://github.com/wieslawsoltes/NXUI
[Navs]: https://github.com/AngelMunoz/Navs
[Supabase]: https://supabase.com

# The Navs testing ground.

I'm currently working on a routing library called "Navs" which is a web-inspired routing solution for Avalonia Applications.

This project is a mix of these libraries:

- [Avalonia]
- [NXUI]
- [Navs]
- [Supabase]

At the time of writing, this project just has a simple login screen and "home" screen. But as I keep trying out features for navs I may add more features to this project.

But the important bits is how `Navs` is used for routing and navigation between screens. The usage of adaptive values ([FSharp.Data.Adaptive]), how they integrate with Avalonia, the usage of Supabase with F# code and lastly the code first approach to Avalonia as a showcase that given the right effort you can actually use Plain Avalonia without XAML and without a virtual dom.

And by the way Navs and NXUI are also available for C# (and likely VB as well), so don't think this is an F# only kind of approach!

### Debug in VSCode

- Open the project in VSCode with the Ionide extension installed.
- Make sure to create a `.env.local` file in the root of the project with the env vars that are shown in the `.env` file.
- Press `F5` to start debugging.
