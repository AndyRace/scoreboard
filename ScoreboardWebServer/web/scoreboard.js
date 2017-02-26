// todo: SCRIPT438: Object doesn't support property or method 'find'
// scoreboard.js(141, 9)


const cTestInterval = 500;

function debugInfo(info) {
    // alert(info);
}

function Group(scoreboard, groupName, valueElement, nDigits) {
    var self = this;

    this.scoreboard = scoreboard;
    this.name = groupName;
    this.valueElement = valueElement;
    this.nDigits = nDigits;
    this.maxValue = Math.pow(10, nDigits) - 1;
    this._value = NaN;

    Object.defineProperty(this, 'value', {
        get: function () {
            return this._value;
        },

        set: function (value) {
            if (value === "") {
                value = NaN;
            } else {
                value = Number(value);
                if (value < 0) { value = NaN; }
                if (value > this.maxValue) { value = this.maxValue; }
            }

            this._value = value;
        }
    });

    // todo: HtmlEncode(text)
    this.setDisplayText = function (text, scoreboardHasResponded) {
        if (isNaN(text)) text = "";

        if (scoreboardHasResponded) {
            this.valueElement.innerText = text;
        }
        else {
            this.valueElement.innerHTML = "<span class='unconfirmed'>" + text + "</span>";
        }
    };

    this.inc = function (incValue) {
        incValue = Number(incValue);
        if (isNaN(incValue)) incValue = 0;

        var newvalue = this.value;

        if (isNaN(newvalue)) newvalue = 0;

        this.value = newvalue + incValue;
    };

    this.refresh = function () {
        // called to update the display to the current value
    };
}

function Scoreboard() {
    this.groups = [];

    this.findGroup = function (group) {
        return this.groups.find(function (value) { return value.name === group; });
    };

    this.reset = function () {
        this.groups.forEach(function (group) {
            group.value = NaN;
        });
    };

    this.refresh = function () {
        this.groups.forEach(function (group) {
            group.refresh();
        });
    };

    this.cycle = function (button) {
        if (button.locked) { return; }
        try {
            button.locked = true;

            if (button.ticker !== undefined) {
                // stop ticking
                button.ticker.stopTicker();
                button.ticker = undefined;
                button.innerText = button.initialText;
                return;
            }

            button.initialText = button.innerText;
            button.innerText = "Stop";
            button.ticker = new Ticker(this.groups);
        } finally {
            button.locked = false;
        }
    };

    this.test = function (button) {
        alert("Test");
    };

    this.displayText = function (text) {
        alert(text);
    };

    this.createGroup = function (groupName, value, nDigits, update) {
        return new Group(this, groupName, value, nDigits, update);
    };
}

function inc(groupName, value) {
    scoreboard.findGroup(groupName).inc(value);
}

function setValue(groupName, value) {
    if (value === "" || !isNaN(Number(value))) {
        scoreboard.findGroup(groupName).value = value;
    }
}