//@ts-check
const canvasElem = document.createElement('canvas')
canvasElem.width = 4320
canvasElem.height = 4320
document.body.appendChild(canvasElem)
const canvas = canvasElem.getContext('2d')

const Width = 180
const Height = 180
const WallWidth = 24
const WallValue = 2

let seed = Math.floor(Math.random() * 2500)

const resource_url = 'http://localhost:5000'
fetch(resource_url + '/game/new/' + seed)
.then(res => res.text())
.then(text => {
    let nullIndex = getStringBreak(text)
    localStorage.setItem('playerId', text.slice(0,nullIndex))
    drawMaze(text.slice(nullIndex + 1))
})

document.addEventListener('keydown',function(event){
    if (event.key == 'ArrowUp') {
        move('down')
    }
    if (event.key == 'ArrowDown') {
        move('up')
    }
    if (event.key == 'ArrowLeft') {
        move('left')
    }
    if (event.key == 'ArrowRight') {
        move('right')
    }
    event.preventDefault()
})

/**
 * @param {string} text
 */
function drawMaze(text) {
    let nullIndex = getStringBreak(text)
    let playerPos = +text.slice(0,nullIndex)
    text = text.slice(nullIndex + 1)

    nullIndex = getStringBreak(text)
    let destinationPos = +text.slice(0,nullIndex)
    text = text.slice(nullIndex + 1)

    canvas.clearRect(0,0,canvasElem.width,canvasElem.height)
    for(let i = 0; i < Width; ++i) {
        for(let j = 0; j < Height; ++j) {
            let index = i * Height + j
            let charCode = text.charCodeAt(index)
            if (charCode == WallValue) {
                canvas.fillStyle = 'black'
            }
            else if (index == playerPos) {
                canvas.fillStyle = 'blue'
            }
            else if (index == destinationPos) {
                canvas.fillStyle = 'red'
            }
            else {
                continue;
            }
            canvas.beginPath()
            canvas.rect(i * WallWidth, j * WallWidth, WallWidth, WallWidth)
            canvas.fill()
            canvas.closePath()
        }
    }
}

/**
 * @param {string} direction
 */
function move(direction) {
    fetch(resource_url + '/game/' + localStorage.getItem('playerId') + '/move/' + direction)
    .then(res => res.text())
    .then(text => drawMaze(text))
}

/**
 * @param {string} text
 * @returns {number}
 */
function getStringBreak(text) {
    let nullIndex = 0
    while(text[nullIndex] != "\0") {
        ++nullIndex
    }
    return nullIndex
}