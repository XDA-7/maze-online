namespace MazeOnline

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Rewrite
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
            endpoints.MapGet("/maze/{seed}", fun context -> context.Response.WriteAsync(new string(Maze.ByteArray (int ((context.Request.RouteValues.Item "seed") :?> string))))) |> ignore
            ) |> ignore
