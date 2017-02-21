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

function DummyGroup(groupName, value, nDigits, newValue, update) {
    this.__proto__ = new Group(groupName, value, nDigits, newValue, update);

    this.setValue = function (value) {
        this.__proto__.setValue(value);

        debugInfo("DummyScoreboard.SetValue: " + this.group + ", value: " + value);
        this.group.setDisplayText(value, true);
        //this.group.setNewValueText(value);
    };
}

function DummyScoreboard() {
    this.__proto__ = new Scoreboard();

    this.createGroup = function (groupName, value, nDigits, newValue, update) {
        return new DummyGroup(groupName, value, nDigits, newValue, update);
    };
}

function changeConnection(isConnected) {
    if (isConnected) {
        scoreboard = new MicroControllerScoreboard();
    } else {
        scoreboard = new DummyScoreboard();
    }

    scoreboard.groups.push(scoreboard.createGroup('total', 'totalValue', 3, 'totalNew'));
    scoreboard.groups.push(scoreboard.createGroup('firstInnings', 'firstInningsValue', 3, 'firstInningsNew'));
    scoreboard.groups.push(scoreboard.createGroup('overs', 'oversValue', 2));
    scoreboard.groups.push(scoreboard.createGroup('wickets', 'wicketsValue', 1));

    scoreboard.refresh();
}

function initialise() {
    initialiseLayout();

    // todo: find a way to determine whether or not it's connected
    changeConnection(true);
}
