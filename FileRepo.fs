namespace MazeOnline

open System.IO

module FileRepo =
    let private cachePath = "cache/"

    let private initCache =
        if not (Directory.Exists "cache") then
            Directory.CreateDirectory "cache" |> ignore

    let CacheFile path file =
        initCache
        let byteArray = Seq.toArray file
        File.WriteAllBytes (cachePath + path,byteArray)

    let CachedFile path =
        initCache
        if File.Exists (cachePath + path) then
            Some(File.ReadAllBytes (cachePath + path))
        else
            None