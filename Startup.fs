namespace MazeOnline

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting

type Startup() =

    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    member this.ConfigureServices(services: IServiceCollection) =
        ()

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    member this.Configure(app: IApplicationBuilder, env: IWebHostEnvironment) =
        if env.IsDevelopment() then
            app.UseDeveloperExceptionPage() |> ignore

        app.UseStaticFiles() |> ignore

        app.UseRouting() |> ignore

        app.UseEndpoints(fun endpoints ->
            endpoints.MapGet("/maze/ascii/{seed}", fun context -> context.Response.WriteAsync(Maze.AsciiMap (int ((context.Request.RouteValues.Item "seed") :?> string)))) |> ignore
            endpoints.MapGet("/maze/{seed}", fun context ->
                let (Maze.Tiles bytes) = Maze.ByteArray (int ((context.Request.RouteValues.Item "seed") :?> string))
                let byteStr = new string(bytes |> Array.map char)
                context.Response.WriteAsync(byteStr)) |> ignore
            endpoints.MapGet("/game/new/{seed}", fun context ->
                let seed = (int ((context.Request.RouteValues.Item "seed") :?> string))
                context.Response.WriteAsync(new string(Game.StartGame seed))) |> ignore
            endpoints.MapGet("/game/{playerId}/move/{direction}", fun context ->
                let direction = (Maze.StringToDirection ((context.Request.RouteValues.Item "direction") :?> string)).Value
                let playerId = int (context.Request.RouteValues.Item "playerId" :?> string)
                context.Response.WriteAsync(new string(Game.Move playerId direction))) |> ignore
            ) |> ignore
