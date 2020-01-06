open MazeOnline.Triangulation

[<EntryPoint>]
let main argv =
    let triangle = {
        A = { X = 0.0; Y = 12.0 }
        B = { X = 6.0; Y = 0.0 }
        C = { X = -6.0; Y = 0.0 }
    }
    let ccwTriangle = CounterClockwiseTriangle triangle
    let ps = [|
        { X = 0.0; Y = 0.0 }
        { X = 0.0; Y = -1.0 }
        { X = 0.0; Y = -5.0 }
    |]
    printfn "%b" (IsPointInCircumcircle ps.[0] ccwTriangle)
    printfn "%b" (IsPointInCircumcircle ps.[1] ccwTriangle)
    printfn "%b" (IsPointInCircumcircle ps.[2] ccwTriangle)
    0
