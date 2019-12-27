namespace MazeOnline

module Maze =
    [<Literal>]
    let Width = 180
    [<Literal>]
    let Height = 180
    type Coord =
        {
            X : int
            Y : int
        }
    type Tile =
        | Empty = 0
        | Wall = 1
        | Exit = 2
    let TileToChar tile =
        match tile with
        | Tile.Empty -> ' '
        | Tile.Wall -> '+'
        | Tile.Exit -> 'x'
        | _ -> '!'
    type Path =
        {
            Value : Coord
            Branches : List<Path>
        }
    
    let Tiles = [
        for i in 1 .. Width ->
            [for j in 1 .. Height ->
                match i with
                | 1 | Width -> Tile.Wall
                | _ -> match j with
                        | 1 | Height -> Tile.Wall
                        | _ -> Tile.Empty
            ]
    ]

    let private generateTiles paths =
        let tiles = Array2D.init Width Height (fun i j ->
            if i % 2 = 0 && j % 2 = 0 then
                Tile.Empty
            else
                Tile.Wall
        )
        for path in paths do
            for branch in path.Branches do
                let x = path.Value.X + branch.Value.X
                let y = path.Value.Y + branch.Value.Y
                tiles.[x,y] <- Tile.Empty
        tiles

    let private roll (rng:System.Random) faces = (rng.Next() % faces)
    
    let private getDirectionOptions position mapWidth mapHeight =
        [
            if position.Y <> (mapHeight - 1) then yield { X = position.X; Y = position.Y + 1 }
            if position.Y <> 0 then yield { X = position.X; Y = position.Y - 1 }
            if position.X <> (mapWidth - 1) then yield { X = position.X + 1; Y = position.Y }
            if position.X <> 0 then yield { X = position.X - 1; Y = position.Y }
        ]
    let private getStep rng position mapWidth mapHeight =
        let directionOptions = getDirectionOptions position mapWidth mapHeight
        directionOptions.[(roll rng directionOptions.Length)]
    let rec private loopErasedRandomWalkRec rng (walk:List<Coord>) (walkLookup:Set<Coord>) mapWidth mapHeight (terminals:Set<Coord>) =
        if terminals.Contains walk.Head then
            walk
        elif walkLookup.Contains walk.Head then
            // erase the loop
            let loopErasedWalk = walk |> List.tail |> List.skipWhile (fun x -> x <> walk.Head)
            // include all steps in the path except the last one
            let loopErasedWalkLookup = set loopErasedWalk.Tail
            loopErasedRandomWalkRec rng loopErasedWalk loopErasedWalkLookup mapWidth mapHeight terminals
        else
            // extend the walk, add the previous step to the lookup
            let step = getStep rng walk.Head mapWidth mapHeight
            loopErasedRandomWalkRec rng (step::walk) (walkLookup.Add walk.Head) mapWidth mapHeight terminals
    let private loopErasedRandomWalk rng start mapWidth mapHeight terminals =
        loopErasedRandomWalkRec rng [start] (set []) mapWidth mapHeight terminals

    let private convertWalkToPath branchingPath coords =
        coords
        |> List.fold (fun acc elem -> { Value = elem; Branches = [if not acc.IsEmpty then yield acc.Head else yield branchingPath] }::acc) []
        |> List.fold (fun acc elem -> { Value = elem.Value; Branches = if acc.IsEmpty then elem.Branches else acc.Head::elem.Branches }::acc) []
    let rec private wilsonMazeRec rng width height (paths:Map<Coord,Path>) (unusedCoords:Set<Coord>) =
        if unusedCoords.IsEmpty then
            paths |> Map.toSeq |> Seq.map snd
        else
            let start = unusedCoords.MinimumElement
            let terminals = set (paths |> Map.toSeq |> Seq.map fst)
            // the walk will contain the new path, with the head being one of the terminal coordinates
            let walk = loopErasedRandomWalk rng start width height terminals
            let branchingPath = paths.[walk.Head]
            let walkPath = convertWalkToPath branchingPath walk.Tail
            let updatedPaths = 
                if not walkPath.IsEmpty then
                    paths.Add (walk.Head, {branchingPath with Branches = walkPath.Head::branchingPath.Branches})
                else
                    paths
            let mergedPaths = List.fold (fun (acc:Map<Coord,Path>) elem -> acc.Add (elem.Value,elem)) updatedPaths walkPath
            let updatedUnusedCoords = walk.Tail |> List.fold (fun (acc:Set<Coord>) elem -> acc.Remove elem) unusedCoords
            wilsonMazeRec rng width height mergedPaths updatedUnusedCoords
    let private wilsonMaze seed =
        let coords = set [
            for i in 0..((Width / 2) - 1) do
                for j in 0..((Height / 2) - 1) do
                    yield { X = i; Y = j }
        ]
        let seedCoord = { X = 0; Y = 0 }
        let paths = Map.add seedCoord { Value = seedCoord; Branches = [] } Map.empty
        generateTiles (wilsonMazeRec (System.Random(seed)) (Width / 2) (Height / 2) paths (Set.remove seedCoord coords))
    
    let private cacheMap (seed:int) (map:Tile[,]) =
        map |> Array2D.map byte |> Seq.cast |> FileRepo.CacheFile (string seed)
    let private cachedMap seed =
        match FileRepo.CachedFile (string seed) with
        | None ->
            let maze = wilsonMaze seed
            cacheMap seed maze
            maze
        | Some(m) -> m |> Array.map (int >> enum) |> Array.chunkBySize Height |> array2D
    let AsciiMap seed =
        let maze = cachedMap seed
        new string(
            [|for i in 0 .. (Width - 1) do
                for j in 0 .. (Height - 1) do
                    yield TileToChar maze.[i,j]
                yield '\n'
        |])
    let ByteArray seed =
        let maze = cachedMap seed
        [|for i in 0 .. (Width - 1) do
            for j in 0 .. (Height - 1) do
                yield byte maze.[i,j]|]
        |> Array.map char