namespace MazeOnline
    module Game =
        type GameState =
            {
                Maze : Maze.Tiles
                PlayerPosition : int
                Destination : int
            }
        let mutable private games:Map<int,GameState> = Map.empty
        let mutable private playerId = 0
        let private gameStateToChars game =
            let (Maze.Tiles byteArray) = game.Maze
            Array.concat [
                (string game.PlayerPosition).ToCharArray()
                [|char 0|]
                (string game.Destination).ToCharArray()
                [|char 0|]
                byteArray |> Array.map char
            ]
        let StartGame seed =
            let maze = Maze.ByteArray seed
            let state = {
                Maze = maze
                PlayerPosition = Maze.GetEmptySpaceNearStart maze
                Destination = Maze.GetRandomEmptySpace maze
            }
            playerId <- playerId + 1
            games <- games.Add(playerId,state)
            Array.concat [(string playerId).ToCharArray();[|char 0|]; gameStateToChars state]
        let Move playerId direction =
            let state = games.Item playerId
            let newState = match Maze.GetTile state.Maze state.PlayerPosition direction with
                           | (pos,Maze.Tile.Empty) -> { state with PlayerPosition = pos }
                           | (_,_) -> state
            games <- games.Add(playerId,newState)
            gameStateToChars newState