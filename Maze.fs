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
        | Empty
        | Wall
        | Exit
        member this.ToAscii =
            match this with
            | Empty -> ' '
            | Wall -> '+'
            | Exit -> 'x'
    type Path =
        {
            Value : Coord
            Branches : List<Path>
        }
    
    let Tiles = [
        for i in 1 .. Width ->
            [for j in 1 .. Height ->
                match i with
                | 1 | Width -> Wall
                | _ -> match j with
                        | 1 | Height -> Wall
                        | _ -> Empty
            ]
    ]

    let private generateTiles paths =
        let tiles = Array2D.init Width Height (fun i j ->
            if i % 2 = 0 && j % 2 = 0 then
                Empty
            else
                Wall
        )
        for path in paths do
            for branch in path.Branches do
                let x = path.Value.X + branch.Value.X
                let y = path.Value.Y + branch.Value.Y
                tiles.[x,y] <- Empty
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
            walk.Tail
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

    let private getPotentialBranches path mapWidth mapHeight =
        getDirectionOptions path.Value mapWidth mapHeight
        |> List.filter (fun coord -> (not (List.contains coord (path.Branches |> List.map (fun branch -> branch.Value)))))
    let rec private getPathStart rng mapWidth mapHeight (paths:Path[]) =
        let index = roll rng paths.Length
        let start = paths.[index]
        let potentialBranches = getPotentialBranches start mapWidth mapHeight
        if potentialBranches.IsEmpty then
            getPathStart rng mapWidth mapHeight paths
        else
            (index,potentialBranches.[roll rng potentialBranches.Length])
    let private convertWalkToPath branchingPath coords =
        coords
        |> List.fold (fun acc elem -> { Value = elem; Branches = [if not acc.IsEmpty then yield acc.Head else yield branchingPath] }::acc) []
        |> List.fold (fun acc elem -> { Value = elem.Value; Branches = if acc.IsEmpty then elem.Branches else acc.Head::elem.Branches }::acc) []
    let rec private wilsonMazeRec rng width height (paths:Path[]) =
        if paths.Length = width * height then
            paths
        else
            let test = paths.Length
            let (branchingPathIndex,start) = getPathStart rng width height paths
            let terminals = set (Array.map (fun path -> path.Value) paths)
            let branchingPath = paths.[branchingPathIndex]
            let walk = convertWalkToPath branchingPath (loopErasedRandomWalk rng start width height terminals)
            if not walk.IsEmpty then
                paths.[branchingPathIndex] <- {branchingPath with Branches = walk.Head::branchingPath.Branches}
            wilsonMazeRec rng width height (Array.append paths (List.toArray walk))
    let private wilsonMaze seed =
        generateTiles (wilsonMazeRec (System.Random(seed)) (Width / 2) (Height / 2) [|{ Value = { X = 0; Y = 0 }; Branches = [] }|])

    let AsciiMap seed =
        let maze = wilsonMaze seed
        new string(
            [|for i in 0 .. (Width - 1) do
                for j in 0 .. (Height - 1) do
                    yield maze.[i,j].ToAscii
                yield '\n'
        |])