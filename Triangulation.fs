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
    let CounterClockwiseTriangle triangle =
        let edgeSum =
            (triangle.B.X - triangle.A.X) * (triangle.B.Y + triangle.A.Y) +
            (triangle.C.X - triangle.B.X) * (triangle.C.Y + triangle.B.Y) +
            (triangle.A.X - triangle.C.X) * (triangle.A.Y + triangle.A.Y)
        if edgeSum > 0.0 then
            CCWTriangle { A = triangle.C; B = triangle.B; C = triangle.A }
        else
            CCWTriangle triangle
    let private getIntersectingTriangles point triangles =
        triangles |> List.filter (IsPointInCircumcircle point)

// get all triangles whose point is in the circumcircle of the triangle
// get all edges of the triangles that are not shared by any two triangles (this is the outer edge of the polygonal hole)
// add a new triangle to the triangulation for each edge, consisting of the edge and the inserted point
