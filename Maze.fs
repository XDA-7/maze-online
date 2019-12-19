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
        let tiles = Array2D.init Width Height (fun i j -> Wall)
        for path in paths do
            tiles.[path.X,path.Y] <- Empty
        tiles

    let private roll (rng:System.Random) faces = (rng.Next() % faces)
    let rec private extendWalkRec walk direction i steps =
        match i with
            | 0 -> walk
            | _ -> extendWalkRec ({ X = (List.head walk).X + direction.X; Y = (List.head walk).Y + direction.Y }::walk) direction (i - 1) steps
    let private extendWalk walk direction steps =
        extendWalkRec walk direction steps steps
    let rec private randomWalkRec rng walk steps stepSizeMin stepSizeRange =
        match steps with
            | 0 -> walk
            | _ ->
                let direction = match roll rng 4 with
                                | 0 -> { X = 1; Y = 0 }
                                | 1 -> { X = -1; Y = 0 }
                                | 2 -> { X = 0; Y = 1 }
                                | 3 -> { X = 0; Y = -1 }
                                | _ -> { X = 0; Y = 0 }
                let variedStepSize = (rng.Next() % stepSizeRange) + stepSizeMin
                let extendedWalk = extendWalk walk direction variedStepSize
                (randomWalkRec rng extendedWalk (steps - 1) stepSizeMin stepSizeRange)
    let private randomWalk rng start steps stepSizeMin stepSizeRange =
        randomWalkRec rng [start] steps stepSizeMin stepSizeRange

    
    let private randomWalkMaze seed =
        let openTiles =
            randomWalk (System.Random(seed)) { X = Width / 2; Y = Height / 2 } 3600 3 7
            |> List.distinct
            |> List.filter (fun c -> c.X >= 0 && c.X < Width && c.Y >= 0 && c.Y < Height)
        generateTiles openTiles

    let AsciiMap seed =
        let maze = randomWalkMaze seed
        new string(
            [|for i in 0 .. (Width - 1) do
                for j in 0 .. (Height - 1) do
                    yield maze.[i,j].ToAscii
                yield '\n'
        |])