namespace MazeOnline

module Triangulation =
    type Point =
        {
            X : double
            Y : double
        }
    type Triangle =
        {
            A : Point
            B : Point
            C : Point
        }
    type Edge =
        {
            A : Point
            B : Point
        }
    type CCWTriangle = CCWTriangle of Triangle
    let IsPointInCircumcircle point ccwTriangle =
        let (CCWTriangle triangle) = ccwTriangle
        let dif = array2D [
            [(triangle.A.X - point.X);(triangle.A.Y - point.Y)]
            [(triangle.B.X - point.X);(triangle.B.Y - point.Y)]
            [(triangle.C.X - point.X);(triangle.C.Y - point.Y)]
        ]
        let matrix = array2D [
            [triangle.A.X - point.X ; triangle.A.Y - point.Y ; (dif.[0,0] * dif.[0,0]) + (dif.[0,1] * dif.[0,1])]
            [triangle.B.X - point.X ; triangle.B.Y - point.Y ; (dif.[1,0] * dif.[1,0]) + (dif.[1,1] * dif.[1,1])]
            [triangle.C.X - point.X ; triangle.C.Y - point.Y ; (dif.[2,0] * dif.[2,0]) + (dif.[2,1] * dif.[2,1])]
        ]
        let determinant =
            matrix.[0,0] * matrix.[1,1] * matrix.[2,2] + 
            matrix.[0,1] * matrix.[1,2] * matrix.[2,0] +
            matrix.[0,2] * matrix.[1,0] * matrix.[2,1] -
            matrix.[0,2] * matrix.[1,1] * matrix.[2,0] -
            matrix.[0,1] * matrix.[1,0] * matrix.[2,2] -
            matrix.[0,0] * matrix.[1,2] * matrix.[2,1]
        determinant > 0.0
    let CounterClockwiseTriangle (triangle:Triangle) =
        let edgeSum =
            (triangle.B.X - triangle.A.X) * (triangle.B.Y + triangle.A.Y) +
            (triangle.C.X - triangle.B.X) * (triangle.C.Y + triangle.B.Y) +
            (triangle.A.X - triangle.C.X) * (triangle.A.Y + triangle.C.Y)
        if edgeSum > 0.0 then
            CCWTriangle { A = triangle.C; B = triangle.B; C = triangle.A }
        else
            CCWTriangle triangle
    let private getIntersectingTriangles point triangles =
        triangles |> Seq.filter (IsPointInCircumcircle point)
    let private getOrderedEdge a b =
        if a.X < b.X then { A = a; B = b }
        else if b.X < a.X then { A = b; B = a }
        else if a.Y < b.Y then { A = a; B = b }
        else { A = b; B = a }
    let private getOuterEdges (triangles:seq<Triangle>) =
        triangles
        |> Seq.collect (fun x -> [getOrderedEdge x.A x.B; getOrderedEdge x.B x.C; getOrderedEdge x.C x.A])
        |> Seq.countBy id
        |> Seq.filter (fun x -> snd x = 1)
        |> Seq.map fst
    let private getTrianglesForHole point holeEdges =
        holeEdges
        |> Seq.map ((fun edge -> { A = edge.A; B = edge.B; C = point }) >> CounterClockwiseTriangle)
    let rec private delaunayTriangulationRec (triangulation:Set<CCWTriangle>) (points:List<Point>) =
        if points.IsEmpty then triangulation
        else
        let point = points.Head
        let intersectingTriangles = getIntersectingTriangles point triangulation
        let newTriangles =
            intersectingTriangles
            |> Seq.map (function |CCWTriangle(t) -> t)
            |> getOuterEdges
            |> getTrianglesForHole point
        let newTriangulation =
            triangulation
            |> (fun x -> Set.difference x (set intersectingTriangles))
            |> Set.union (set newTriangles)
        let itDebug = Seq.toList intersectingTriangles
        let ntDebug = Seq.toList newTriangles
        let ntgDebug = Seq.toList newTriangulation
        delaunayTriangulationRec newTriangulation points.Tail
    let private getSuperTriangle points =
        let xs = points |> Seq.map (fun p -> p.X)
        let ys = points |> Seq.map (fun p -> p.Y)
        let min = {
            X = Seq.min xs - 1.0
            Y = Seq.min ys - 1.0
        }
        let max = {
            X = Seq.max xs + 1.0
            Y = Seq.max ys + 1.0
        }
        CCWTriangle {
            A = { X = min.X; Y = min.Y }
            B = { X = min.X + ((max.X - min.X) * 2.0); Y = min.Y }
            C = { X = min.X; Y = min.Y + ((max.Y - min.Y) * 2.0) }
        }
    let private removeSuperTriangle (superTriangle:CCWTriangle) (triangulation:seq<CCWTriangle>) =
        let (CCWTriangle st) = superTriangle
        let trianglePoints = set [st.A;st.B;st.C]
        triangulation
        |> Seq.filter (fun x ->
            let (CCWTriangle t) = x
            not (trianglePoints.Contains t.A || trianglePoints.Contains t.B || trianglePoints.Contains t.C))
    let DelaunayTriangulation points =
        let superTriangle = getSuperTriangle points
        (delaunayTriangulationRec (set [superTriangle]) points)
        |> removeSuperTriangle superTriangle
    let PointAsJson point =
        "{\"X\":" + string point.X + ",\"Y\":" + string point.Y + "}"
    let TriangleAsJson (triangle:Triangle) =
        "{\"A\":" + PointAsJson triangle.A + ",\"B\":" + PointAsJson triangle.B + ",\"C\":" + PointAsJson triangle.C + "}"
    let TriangulationAsJson triangulation points =
        "{\"Triangles\":[" +
        String.concat "," (
            triangulation |> Seq.map TriangleAsJson
        ) +
        "],\"Points\":[" +
        String.concat "," (
            points |> Seq.map PointAsJson
        ) +
        "]}"
    let TestTriangulation =
        let rng = System.Random(1)
        let points = (
            [ for i in 1 .. 200 do { X = rng.NextDouble() * 800.0; Y = rng.NextDouble() * 800.0 } ]
            |> List.distinct
        )
        points
        |> DelaunayTriangulation
        |> Seq.map (function |CCWTriangle(t) -> t)
        |> (fun x -> TriangulationAsJson x points)

// get all triangles where the point is in the circumcircle of the triangle, remove them from the triangulation
// get all edges of the triangles that are not shared by any two triangles (this is the outer edge of the polygonal hole)
// add a new triangle to the triangulation for each edge, consisting of the edge and the inserted point
