﻿function initialiseLayout() {
    var header = document.getElementById("header");

    // <img class="centered" src="http://miltoncricket.org.uk/wp-content/themes/mcc-theme/images/milton-cricket-club-logo.png" alt="Milton Cricket Club Logo" />
    var img = document.createElement("img");
    img.src = "http://miltoncricket.org.uk/wp-content/themes/mcc-theme/images/milton-cricket-club-logo.png";
    img.alt = "Milton Cricket Club Logo";
    img.className = "centered";
    header.appendChild(img);

    // <h1 class="centered">Milton CC Scoreboard</h1>
    //var title = document.createElement("h1");
    //title.className = "centered";
    //title.innerText = "Scoreboard";
    //header.appendChild(title);
}

// MCC and controller-specific code
//var scoreboard = new MicroControllerScoreboard();
var scoreboard;

function DummyGroup(scoreboard, groupName, value, nDigits) {
    this.__proto__ = new Group(scoreboard, groupName, value, nDigits);

    Object.defineProperty(this, 'value', {
        set: function (value) {
            this.__proto__.value = value;

            value = this.__proto__.value;

            console.log("DummyScoreboard[" + this.name + "].value=" + value);
            this.setDisplayText(value, true);
        },

        // it appears that JS doesn't search up the prototype chain for getters
        get: function() {
            return this.__proto__.value;
        }
    });
}

function DummyScoreboard() {
    this.__proto__ = new Scoreboard();

    this.createGroup = function (groupName, value, nDigits) {
        return new DummyGroup(this, groupName, value, nDigits);
    };
}

function setConnected(isConnected) {
    if (isConnected) {
        scoreboard = new MicroControllerScoreboard();
    } else {
        scoreboard = new DummyScoreboard();
    }

    scoreboard.groups.push(scoreboard.createGroup('total', 'totalValue', 3));
    scoreboard.groups.push(scoreboard.createGroup('wickets', 'wicketsValue', 1));
    scoreboard.groups.push(scoreboard.createGroup('overs', 'oversValue', 2));
    scoreboard.groups.push(scoreboard.createGroup('firstInnings', 'firstInningsValue', 3));

    scoreboard.refresh();
}

function initialise() {
    initialiseLayout();

    // todo: find a way to determine whether or not it's connected
    switch (window.location.protocol) {
        case 'http:':
        case 'https:':
            //remote file over http or https
            setConnected(true);
            break;
        case 'file:':
            //local file
            setConnected(false);
            break;
        default:
        //some other protocol
    }
}
