// 網頁相容性工具
var eventUtil = {
    getEvent: function (event) {
        return event || window.event;
    },
    getPageX: function (event) {
        return event.pageX || event.clientX + document.documentElement.scrollLeft;
    },
    getPageY: function (event) {
        return event.pageY || event.clientY + document.documentElement.scrollTop;
    },
    stopPropagation: function (event) {
        if (event.stopPropagation) {
            event.stopPropagation();
        } else {
            event.cancelBubble = true;
        }
    },
    getTarget: function (event) {
        return event.target || event.srcElement;
    }
};

// 獲得使用者視窗的寬度和高度 (相容性)
function client() {
    return {
        width: window.innerWidth || document.documentElement.clientWidth || document.body.clientWidth || 0,
        height: window.innerHeight || document.documentElement.clientHeight || document.body.clientHeight || 0
    };
}

var gameover = false;
var letters = null;
var timechecker = null;

function StartGrabWordGame(quiz, gameHub) {
    var letters = quiz;
    gameover = false;

    for (var i = 0; i < letters.length ; i++) {
        new letter(
            Math.random() * 900 + 100,
            0,
            Math.random() * 4 + 1,
            letters[i]
            );
    }

    
    timechecker = setInterval(function () {
        if ($(".letter").length == 0) {
            gameover = true;
            clearInterval(timechecker);
        }
    }, 1000);


}

function registerGrabWordClientMethod() {
    gameHub.client.removeWord = function (word, graberId) {
        var word = $(".letter:contains('" + word + "')");
        // 自己或別人抓取的加上樣式
        if ($.connection.hub.id == graberId) {
            word.css("color", "green");
        } else { 
            word.css("color", "red");
        }

        word.fadeOut("slow", function () {
            word.remove();
        });
    }
}

// 封裝的字母物件
function letter(x, y, speedY, value) {
    this.x = x; // x軸位置
    this.y = y;
    this.speedY = speedY; // 掉落速度
    this.value = value; //字母


    // 創造字母元素
    var letDiv = document.createElement("div");
    letDiv.className = "letter";
    letDiv.style.top = this.y + "px";
    letDiv.style.left = this.x + "px";
    letDiv.innerHTML = this.value;
    document.body.appendChild(letDiv);

    // 字母元素往下掉
    var that = this;
    this.timer = setInterval(function () {
        that.y += that.speedY;
        // 字母掉落到底部
        if (that.y >= client().height - letDiv.offsetHeight) {  //offsetHeight包含元素的height、padding、border
            // 回歸起點
            that.y = 0;
            that.x = x;
        }

        console.log(gameover);
        if (!gameover) {
            // 更新位置
            letDiv.style.left = that.x + "px";
            letDiv.style.top = that.y + "px";
        } else {
            clearInterval(that.timer);
        }
    }, 15);
}