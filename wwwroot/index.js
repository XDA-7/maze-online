//@ts-check
const body = document.createElement('body')
document.body = body
const canvasElem = document.createElement('canvas')
canvasElem.width = 4320
canvasElem.height = 4320
body.appendChild(canvasElem)
const canvas = canvasElem.getContext('2d')

const Width = 180
const Height = 180
const WallWidth = 24
const WallValue = 2

let seed = Math.floor(Math.random() * 2500)

const resource_url = 'http://localhost:5000'
fetch(resource_url + '/game/new/' + seed)
.then(res => res.body.getReader().read())
.then(arr => {
    let nullIndex = getStringBreak(arr.value)
    localStorage.setItem('playerId', String.fromCharCode.apply(null, arr.value.slice(0,nullIndex)))
    drawMaze(arr.value.slice(nullIndex + 1))
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
 * @param {Uint8Array} binaryArr
 */
function drawMaze(binaryArr) {
    canvas.clearRect(0,0,canvasElem.width,canvasElem.height)

    let nullIndex = getStringBreak(binaryArr)
    let playerPos = getNumber(binaryArr,nullIndex)
    binaryArr = binaryArr.slice(nullIndex + 1)

    nullIndex = getStringBreak(binaryArr)
    let destinationPos = getNumber(binaryArr,nullIndex)
    binaryArr = binaryArr.slice(nullIndex + 1)

    canvas.fillStyle = 'black'
    for(let i = 0; i < Width; ++i) {
        for(let j = 0; j < Height; ++j) {
            let index = i * Height + j
            if (binaryArr[index] == WallValue) {
                canvas.fillRect(i * WallWidth, j * WallWidth, WallWidth, WallWidth)
            }
            if (index == playerPos) {
                canvas.fillStyle = 'blue'
                canvas.fillRect(i * WallWidth, j * WallWidth, WallWidth, WallWidth)
                //canvas.arc(i * WallWidth, j * WallWidth, WallWidth / 2, 0, Math.PI * 2)
                canvas.fillStyle = 'black'
            }
            if (index == destinationPos) {
                canvas.fillStyle = 'red'
                canvas.fillRect(i * WallWidth, j * WallWidth, WallWidth, WallWidth)
                //canvas.arc(i * WallWidth, j * WallWidth, WallWidth / 2, 0, Math.PI * 2)
                canvas.fillStyle = 'black'
            }
        }
    }
}

/**
 * @param {string} direction
 */
function move(direction) {
    fetch(resource_url + '/game/' + localStorage.getItem('playerId') + '/move/' + direction)
    .then(res => res.body.getReader().read())
    .then(arr => drawMaze(arr.value))
}

/**
 * @param {Uint8Array} array
 * @returns {number}
 */
function getStringBreak(array) {
    let nullIndex = 0
    while(array[nullIndex] != 0) {
        ++nullIndex
    }
    return nullIndex
}

/**
 * @param {Uint8Array} array
 * @param {number} nullIndex
 * @returns {number}
 */
function getNumber(array,nullIndex) {
    return String.fromCharCode.apply(null, array.slice(0,nullIndex))
}