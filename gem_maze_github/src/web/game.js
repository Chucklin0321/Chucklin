// 寶石迷宮 - 網頁版遊戲邏輯

// 遊戲常量
const CELL_SIZE = 40; // 單元格大小
const PLAYER_SIZE = 30; // 玩家大小
const GEM_SIZE = 20; // 寶石大小
const EXIT_SIZE = 30; // 出口大小
const WALL_THICKNESS = 4; // 牆壁厚度

// 寶石類型
const GEM_TYPES = {
    COMMON: { color: '#4d79ff', value: 1, chance: 0.7 },
    RARE: { color: '#4dff4d', value: 5, chance: 0.2 },
    EPIC: { color: '#b34dff', value: 20, chance: 0.08 },
    LEGENDARY: { color: '#ffff4d', value: 100, chance: 0.02 }
};

// 遊戲狀態
let gameState = {
    level: 1,
    score: 0,
    timeLeft: 120,
    gemsCollected: 0,
    totalGems: 10,
    isGameOver: false,
    isLevelComplete: false,
    isPaused: false
};

// 遊戲元素
let maze = [];
let player = { x: 0, y: 0, dx: 0, dy: 0 };
let gems = [];
let exit = { x: 0, y: 0 };

// 畫布和上下文
let canvas;
let ctx;

// 遊戲計時器
let gameTimer;

// 方向鍵狀態
let keys = {
    ArrowUp: false,
    ArrowDown: false,
    ArrowLeft: false,
    ArrowRight: false
};

// 初始化遊戲
function initGame() {
    // 獲取畫布和上下文
    canvas = document.getElementById('game-canvas');
    ctx = canvas.getContext('2d');
    
    // 設置畫布大小
    resizeCanvas();
    
    // 監聽窗口大小變化
    window.addEventListener('resize', resizeCanvas);
    
    // 監聽鍵盤事件
    window.addEventListener('keydown', handleKeyDown);
    window.addEventListener('keyup', handleKeyUp);
    
    // 監聽觸摸控制按鈕
    document.getElementById('btn-up').addEventListener('touchstart', () => { keys.ArrowUp = true; });
    document.getElementById('btn-up').addEventListener('touchend', () => { keys.ArrowUp = false; });
    document.getElementById('btn-down').addEventListener('touchstart', () => { keys.ArrowDown = true; });
    document.getElementById('btn-down').addEventListener('touchend', () => { keys.ArrowDown = false; });
    document.getElementById('btn-left').addEventListener('touchstart', () => { keys.ArrowLeft = true; });
    document.getElementById('btn-left').addEventListener('touchend', () => { keys.ArrowLeft = false; });
    document.getElementById('btn-right').addEventListener('touchstart', () => { keys.ArrowRight = true; });
    document.getElementById('btn-right').addEventListener('touchend', () => { keys.ArrowRight = false; });
    
    // 監聽按鈕點擊事件
    document.getElementById('btn-start').addEventListener('click', startGame);
    document.getElementById('btn-instructions').addEventListener('click', showInstructions);
    document.getElementById('btn-back').addEventListener('click', hideInstructions);
    document.getElementById('btn-next-level').addEventListener('click', nextLevel);
    document.getElementById('btn-main-menu').addEventListener('click', showMainMenu);
    document.getElementById('btn-retry').addEventListener('click', restartGame);
    document.getElementById('btn-main-menu-2').addEventListener('click', showMainMenu);
    document.getElementById('btn-resume').addEventListener('click', resumeGame);
    document.getElementById('btn-restart').addEventListener('click', restartGame);
    document.getElementById('btn-main-menu-3').addEventListener('click', showMainMenu);
    
    // 顯示開始畫面
    showScreen('start-screen');
}

// 調整畫布大小
function resizeCanvas() {
    const container = document.getElementById('game-ui');
    const header = document.getElementById('game-header');
    const controls = document.getElementById('game-controls');
    
    canvas.width = container.clientWidth;
    canvas.height = container.clientHeight - header.clientHeight - (controls.style.display === 'flex' ? controls.clientHeight : 0);
}

// 開始遊戲
function startGame() {
    // 隱藏開始畫面
    hideAllScreens();
    
    // 初始化遊戲狀態
    gameState.level = 1;
    gameState.score = 0;
    gameState.timeLeft = 120;
    gameState.gemsCollected = 0;
    gameState.totalGems = 10;
    gameState.isGameOver = false;
    gameState.isLevelComplete = false;
    gameState.isPaused = false;
    
    // 更新UI
    updateUI();
    
    // 生成迷宮
    generateMaze();
    
    // 開始遊戲循環
    startGameLoop();
    
    // 開始計時器
    startTimer();
}

// 生成迷宮
function generateMaze() {
    // 根據關卡設置迷宮大小
    const width = 10 + Math.floor(gameState.level / 2);
    const height = 10 + Math.floor(gameState.level / 2);
    
    // 初始化迷宮
    maze = [];
    for (let x = 0; x < width; x++) {
        maze[x] = [];
        for (let y = 0; y < height; y++) {
            maze[x][y] = {
                visited: false,
                walls: [true, true, true, true] // 上、右、下、左
            };
        }
    }
    
    // 使用深度優先搜索算法生成迷宮
    const stack = [];
    let current = { x: 0, y: 0 };
    maze[current.x][current.y].visited = true;
    stack.push(current);
    
    while (stack.length > 0) {
        current = stack.pop();
        const neighbors = getUnvisitedNeighbors(current, width, height);
        
        if (neighbors.length > 0) {
            stack.push(current);
            
            // 隨機選擇一個未訪問的鄰居
            const next = neighbors[Math.floor(Math.random() * neighbors.length)];
            
            // 移除當前單元格和下一個單元格之間的牆
            removeWallsBetween(current, next);
            
            maze[next.x][next.y].visited = true;
            stack.push(next);
        }
    }
    
    // 設置玩家初始位置
    player = { x: 0, y: 0, dx: 0, dy: 0 };
    
    // 設置出口位置
    exit = { x: width - 1, y: height - 1 };
    
    // 放置寶石
    placeGems(width, height);
}

// 獲取未訪問的鄰居
function getUnvisitedNeighbors(cell, width, height) {
    const neighbors = [];
    const directions = [
        { x: 0, y: -1 }, // 上
        { x: 1, y: 0 },  // 右
        { x: 0, y: 1 },  // 下
        { x: -1, y: 0 }  // 左
    ];
    
    for (let i = 0; i < directions.length; i++) {
        const nx = cell.x + directions[i].x;
        const ny = cell.y + directions[i].y;
        
        if (nx >= 0 && nx < width && ny >= 0 && ny < height && !maze[nx][ny].visited) {
            neighbors.push({ x: nx, y: ny });
        }
    }
    
    return neighbors;
}

// 移除兩個單元格之間的牆
function removeWallsBetween(current, next) {
    const dx = next.x - current.x;
    const dy = next.y - current.y;
    
    if (dx === 1) { // 右
        maze[current.x][current.y].walls[1] = false;
        maze[next.x][next.y].walls[3] = false;
    } else if (dx === -1) { // 左
        maze[current.x][current.y].walls[3] = false;
        maze[next.x][next.y].walls[1] = false;
    } else if (dy === 1) { // 下
        maze[current.x][current.y].walls[2] = false;
        maze[next.x][next.y].walls[0] = false;
    } else if (dy === -1) { // 上
        maze[current.x][current.y].walls[0] = false;
        maze[next.x][next.y].walls[2] = false;
    }
}

// 放置寶石
function placeGems(width, height) {
    gems = [];
    gameState.totalGems = 5 + gameState.level;
    
    // 放置寶石
    while (gems.length < gameState.totalGems) {
        const x = Math.floor(Math.random() * width);
        const y = Math.floor(Math.random() * height);
        
        // 避免在起點和終點放置寶石
        if ((x === 0 && y === 0) || (x === width - 1 && y === height - 1)) {
            continue;
        }
        
        // 檢查該位置是否已有寶石
        let gemExists = false;
        for (let i = 0; i < gems.length; i++) {
            if (gems[i].x === x && gems[i].y === y) {
                gemExists = true;
                break;
            }
        }
        
        if (!gemExists) {
            // 決定寶石類型
            const rand = Math.random();
            let gemType;
            
            if (rand < GEM_TYPES.LEGENDARY.chance) {
                gemType = 'LEGENDARY';
            } else if (rand < GEM_TYPES.LEGENDARY.chance + GEM_TYPES.EPIC.chance) {
                gemType = 'EPIC';
            } else if (rand < GEM_TYPES.LEGENDARY.chance + GEM_TYPES.EPIC.chance + GEM_TYPES.RARE.chance) {
                gemType = 'RARE';
            } else {
                gemType = 'COMMON';
            }
            
            gems.push({ x, y, type: gemType });
        }
    }
    
    // 更新UI
    document.getElementById('gems-total').textContent = gameState.totalGems;
}

// 開始遊戲循環
function startGameLoop() {
    // 使用requestAnimationFrame進行遊戲循環
    function gameLoop() {
        if (!gameState.isPaused && !gameState.isGameOver && !gameState.isLevelComplete) {
            update();
            render();
        }
        requestAnimationFrame(gameLoop);
    }
    
    gameLoop();
}

// 開始計時器
function startTimer() {
    // 清除之前的計時器
    if (gameTimer) {
        clearInterval(gameTimer);
    }
    
    // 每秒更新時間
    gameTimer = setInterval(() => {
        if (!gameState.isPaused && !gameState.isGameOver && !gameState.isLevelComplete) {
            gameState.timeLeft--;
            
            // 更新UI
            document.getElementById('timer-value').textContent = gameState.timeLeft;
            
            // 時間不多時改變顏色
            if (gameState.timeLeft <= 30) {
                document.getElementById('timer-value').style.color = 'red';
            }
            
            // 時間結束，遊戲結束
            if (gameState.timeLeft <= 0) {
                gameOver();
            }
        }
    }, 1000);
}

// 更新遊戲狀態
function update() {
    // 更新玩家位置
    updatePlayerPosition();
    
    // 檢查寶石收集
    checkGemCollection();
    
    // 檢查是否到達出口
    checkExit();
}

// 更新玩家位置
function updatePlayerPosition() {
    // 根據按鍵狀態設置移動方向
    player.dx = 0;
    player.dy = 0;
    
    if (keys.ArrowUp) player.dy = -1;
    if (keys.ArrowDown) player.dy = 1;
    if (keys.ArrowLeft) player.dx = -1;
    if (keys.ArrowRight) player.dx = 1;
    
    // 檢查牆壁碰撞
    if (player.dx !== 0 || player.dy !== 0) {
        const newX = player.x + player.dx;
        const newY = player.y + player.dy;
        
        // 檢查是否超出邊界
        if (newX < 0 || newX >= maze.length || newY < 0 || newY >= maze[0].length) {
            return;
        }
        
        // 檢查牆壁
        if (player.dx === 1 && maze[player.x][player.y].walls[1]) return; // 右牆
        if (player.dx === -1 && maze[player.x][player.y].walls[3]) return; // 左牆
        if (player.dy === 1 && maze[player.x][player.y].walls[2]) return; // 下牆
        if (player.dy === -1 && maze[player.x][player.y].walls[0]) return; // 上牆
        
        // 更新位置
        player.x = newX;
        player.y = newY;
    }
}

// 檢查寶石收集
function checkGemCollection() {
    for (let i = 0; i < gems.length; i++) {
        if (player.x === gems[i].x && player.y === gems[i].y) {
            // 增加分數
            gameState.score += GEM_TYPES[gems[i].type].value;
            
            // 增加收集的寶石數量
            gameState.gemsCollected++;
            
            // 更新UI
            document.getElementById('score-value').textContent = gameState.score;
            document.getElementById('gems-collected').textContent = gameState.gemsCollected;
            
            // 移除寶石
            gems.splice(i, 1);
            break;
        }
    }
}

// 檢查是否到達出口
function checkExit() {
    if (player.x === exit.x && player.y === exit.y) {
        levelComplete();
    }
}

// 渲染遊戲
function render() {
    // 清除畫布
    ctx.clearRect(0, 0, canvas.width, canvas.height);
    
    // 計算單元格大小
    const cellWidth = canvas.width / maze.length;
    const cellHeight = canvas.height / maze[0].length;
    const cellSize = Math.min(cellWidth, cellHeight);
    
    // 計算偏移量，使迷宮居中
    const offsetX = (canvas.width - cellSize * maze.length) / 2;
    const offsetY = (canvas.height - cellSize * maze[0].length) / 2;
    
    // 繪製迷宮
    for (let x = 0; x < maze.length; x++) {
        for (let y = 0; y < maze[0].length; y++) {
            const cell = maze[x][y];
            const cellX = offsetX + x * cellSize;
            const cellY = offsetY + y * cellSize;
            
            // 繪製單元格背景
            ctx.fillStyle = '#16213e';
            ctx.fillRect(cellX, cellY, cellSize, cellSize);
            
            // 繪製牆壁
            ctx.strokeStyle = '#e94560';
            ctx.lineWidth = WALL_THICKNESS;
            
            if (cell.walls[0]) { // 上牆
                ctx.beginPath();
                ctx.moveTo(cellX, cellY);
                ctx.lineTo(cellX + cellSize, cellY);
                ctx.stroke();
            }
            
            if (cell.walls[1]) { // 右牆
                ctx.beginPath();
                ctx.moveTo(cellX + cellSize, cellY);
                ctx.lineTo(cellX + cellSize, cellY + cellSize);
                ctx.stroke();
            }
            
            if (cell.walls[2]) { // 下牆
                ctx.beginPath();
                ctx.moveTo(cellX, cellY + cellSize);
                ctx.lineTo(cellX + cellSize, cellY + cellSize);
                ctx.stroke();
            }
            
            if (cell.walls[3]) { // 左牆
                ctx.beginPath();
                ctx.moveTo(cellX, cellY);
                ctx.lineTo(cellX, cellY + cellSize);
                ctx.stroke();
            }
        }
    }
    
    // 繪製出口
    const exitX = offsetX + exit.x * cellSize + cellSize / 2;
    const exitY = offsetY + exit.y * cellSize + cellSize / 2;
    
    ctx.fillStyle = '#4dff4d';
    ctx.beginPath();
    ctx.arc(exitX, exitY, EXIT_SIZE / 2, 0, Math.PI * 2);
    ctx.fill();
    
    // 繪製寶石
    for (let i = 0; i < gems.length; i++) {
        const gemX = offsetX + gems[i].x * cellSize + cellSize / 2;
        const gemY = offsetY + gems[i].y * cellSize + cellSize / 2;
        
        ctx.fillStyle = GEM_TYPES[gems[i].type].color;
        ctx.beginPath();
        ctx.arc(gemX, gemY, GEM_SIZE / 2, 0, Math.PI * 2);
        ctx.fill();
        
        // 添加閃光效果
        ctx.strokeStyle = 'white';
        ctx.lineWidth = 2;
        ctx.beginPath();
        ctx.arc(gemX, gemY, GEM_SIZE / 2, 0, Math.PI * 2);
        ctx.stroke();
    }
    
    // 繪製玩家
    const playerX = offsetX + player.x * cellSize + cellSize / 2;
    const playerY = offsetY + player.y * cellSize + cellSize / 2;
    
    ctx.fillStyle = '#e94560';
    ctx.beginPath();
    ctx.arc(playerX, playerY, PLAYER_SIZE / 2, 0, Math.PI * 2);
    ctx.fill();
    
    // 添加玩家眼睛
    ctx.fillStyle = 'white';
    ctx.beginPath();
    ctx.arc(playerX - 5, playerY - 5, 3, 0, Math.PI * 2);
    ctx.arc(playerX + 5, playerY - 5, 3, 0, Math.PI * 2);
    ctx.fill();
}

// 處理鍵盤按下事件
function handleKeyDown(e) {
    if (['ArrowUp', 'ArrowDown', 'ArrowLeft', 'ArrowRight'].includes(e.key)) {
        keys[e.key] = true;
        e.preventDefault();
    }
    
    // 暫停遊戲
    if (e.key === 'Escape' || e.key === 'p' || e.key === 'P') {
        togglePause();
    }
}

// 處理鍵盤釋放事件
function handleKeyUp(e) {
    if (['ArrowUp', 'ArrowDown', 'ArrowLeft', 'ArrowRight'].includes(e.key)) {
        keys[e.key] = false;
        e.preventDefault();
    }
}

// 更新UI
function updateUI() {
    document.getElementById('score-value').textContent = gameState.score;
    document.getElementById('level-value').textContent = gameState.level;
    document.getElementById('timer-value').textContent = gameState.timeLeft;
    document.getElementById('gems-collected').textContent = gameState.gemsCollected;
    document.getElementById('gems-total').textContent = gameState.totalGems;
}

// 關卡完成
function levelComplete() {
    gameState.isLevelComplete = true;
    
    // 計算時間獎勵
    const timeBonus = gameState.timeLeft;
    gameState.score += timeBonus;
    
    // 更新UI
    document.getElementById('final-score').textContent = gameState.score;
    document.getElementById('time-bonus').textContent = timeBonus;
    document.getElementById('gems-result').textContent = gameState.gemsCollected + '/' + gameState.totalGems;
    
    // 顯示關卡完成畫面
    showScreen('level-complete-screen');
}

// 遊戲結束
function gameOver() {
    gameState.isGameOver = true;
    
    // 更新UI
    document.getElementById('game-over-score').textContent = gameState.score;
    document.getElementById('game-over-gems').textContent = gameState.gemsCollected + '/' + gameState.totalGems;
    
    // 顯示遊戲結束畫面
    showScreen('game-over-screen');
}

// 下一關
function nextLevel() {
    // 增加關卡
    gameState.level++;
    
    // 重置遊戲狀態
    gameState.timeLeft = 120 + gameState.level * 10;
    gameState.gemsCollected = 0;
    gameState.isLevelComplete = false;
    
    // 更新UI
    updateUI();
    
    // 生成新迷宮
    generateMaze();
    
    // 隱藏關卡完成畫面
    hideAllScreens();
}

// 重新開始遊戲
function restartGame() {
    // 重置遊戲狀態
    gameState.score = 0;
    gameState.level = 1;
    gameState.timeLeft = 120;
    gameState.gemsCollected = 0;
    gameState.isGameOver = false;
    gameState.isLevelComplete = false;
    gameState.isPaused = false;
    
    // 更新UI
    updateUI();
    
    // 生成新迷宮
    generateMaze();
    
    // 隱藏所有畫面
    hideAllScreens();
}

// 切換暫停狀態
function togglePause() {
    if (gameState.isGameOver || gameState.isLevelComplete) return;
    
    gameState.isPaused = !gameState.isPaused;
    
    if (gameState.isPaused) {
        showScreen('pause-screen');
    } else {
        hideAllScreens();
    }
}

// 恢復遊戲
function resumeGame() {
    gameState.isPaused = false;
    hideAllScreens();
}

// 顯示主選單
function showMainMenu() {
    // 清除計時器
    if (gameTimer) {
        clearInterval(gameTimer);
    }
    
    // 顯示開始畫面
    showScreen('start-screen');
}

// 顯示遊戲說明
function showInstructions() {
    showScreen('instructions-screen');
}

// 隱藏遊戲說明
function hideInstructions() {
    showScreen('start-screen');
}

// 顯示指定畫面
function showScreen(screenId) {
    // 隱藏所有畫面
    hideAllScreens();
    
    // 顯示指定畫面
    document.getElementById(screenId).style.display = 'flex';
}

// 隱藏所有畫面
function hideAllScreens() {
    const screens = document.querySelectorAll('.screen');
    screens.forEach(screen => {
        screen.style.display = 'none';
    });
}

// 當頁面加載完成時初始化遊戲
window.addEventListener('load', initGame);
