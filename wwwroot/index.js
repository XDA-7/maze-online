const body = document.createElement('body')
document.body = body
const canvasElem = document.createElement('canvas')
canvasElem.width = 1500
canvasElem.height = 1500
body.appendChild(canvasElem)
const canvas = canvasElem.getContext('2d')

const resource_url = 'http://localhost:5000'
fetch(resource_url + '/maze/5')
.then(res => res.body.getReader().read())
.then(arr => drawMaze(arr.value))

const Width = 180
const Height = 180
const WallWidth = 6
const WallValue = 1

/**
 * @param {Uint8Array} asciiArray
 */
function drawMaze(asciiArray) {
    canvas.fillStyle = 'black'
    for(let i = 0; i < Width; ++i) {
        for(let j = 0; j < Height; ++j) {
            if (asciiArray[i * Height + j] == WallValue) {
                canvas.fillRect(i * WallWidth, j * WallWidth, WallWidth, WallWidth)
            }
        }
    }
}