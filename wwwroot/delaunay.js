//@ts-check
/**
 * @typedef Point
 * @property {number} X
 * @property {number} Y
 */

/**
 * @typedef Triangle
 * @property {Point} A
 * @property {Point} B
 * @property {Point} C
 */

/**
 * @typedef Triangulation
 * @property {Array<Triangle>} Triangles
 * @property {Array<Point>} Points
 */

fetch('http://localhost:5000/dt/test')
.then(res => res.json())
.then(triangulation => drawTriangulation(triangulation))

/**
 * @param {Triangulation} triangulation
 */
function drawTriangulation(triangulation) {
    /**@type {HTMLCanvasElement} */
    let canvasElem = document.createElement('canvas')
    canvasElem.width = 4320
    canvasElem.height = 4320
    document.body.appendChild(canvasElem)
    let canvas = canvasElem.getContext('2d')
    triangulation.Triangles.forEach(function(triangle){
        drawTriangle(canvas,triangle)
    })
    drawPoints(canvas,triangulation.Points)
}

/**
 * @param {CanvasRenderingContext2D} canvas
 * @param {Array<Point>} points
 */
function drawPoints(canvas,points){
    canvas.fillStyle = 'black'
    points.forEach(function(point){
        canvas.fillRect(point.X - 2.5, point.Y - 2.5, 5.0, 5.0)
    })
}

/**
 * @param {CanvasRenderingContext2D} canvas
 * @param {Triangle} triangle
 */
function drawTriangle(canvas, triangle){
    canvas.beginPath()
    canvas.fillStyle = getRndColor()
    canvas.moveTo(triangle.A.X,triangle.A.Y)
    canvas.lineTo(triangle.B.X,triangle.B.Y)
    canvas.lineTo(triangle.C.X,triangle.C.Y)
    canvas.closePath()
    canvas.fill()
}

function getRndColor() {
    let r = 255*Math.random()|0
    let g = 255*Math.random()|0
    let b = 255*Math.random()|0
    return 'rgb(' + r + ',' + g + ',' + b + ')'
}