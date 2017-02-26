// todo: SCRIPT438: Object doesn't support property or method 'find'
// scoreboard.js(141, 9)


const cTestInterval = 500;

function debugInfo(info) {
    // alert(info);
}

function Group(scoreboard, group, value, nDigits, newValue) {
    var self = this;

    this.scoreboard = scoreboard;
    this.name = group;
    this.valueElement = document.getElementById(value);
    this.nDigits = nDigits;
    this.maxValue = Math.pow(10, nDigits) - 1;
    this.newValueElement = document.getElementById(newValue);
    this.value = NaN;

    this.getValue = function () {
        return this.value;
    };

    this.setValue = function (value) {
        if (value === "") {
            value = NaN;
        } else {
            value = Number(value);
            if (value < 0) { value = NaN; }
            if (value > this.maxValue) { value = this.maxValue; }
        }

        this.value = value;
    };

    this.setNewValueText = function (text) {
        if (this.newValueElement !== null) {
            this.newValueElement.value = text;
        }
    };

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

        var value = this.value;
        if (isNaN(value)) value = 0;

        this.setValue(value + incValue);
    };

    this.refresh = function () {
        // called to update the display to the current value
    };
}

function Ticker(groups) {
    var self = this;
    self.groups = groups;

    // get the current values
    self.initialValues = groups.map(
        function (group) {
            return {
                group: group, originalValue: group.getValue()
            };
        });

    this.tickAlternate = function () {
        // this == window
        if (isNaN(self.state)) {
            self.state = 0;
        } else {
            self.state++;
        }

        if (self.state === 0) {
            self.groups.forEach(function (value) {
                value.setValue(NaN);
            });
            return;
        }

        self.initialValues.forEach(function (value) {
            value.group.setValue(value.originalValue);
        });

        self.state = NaN;
    };

    this.tick = function () {
        // this == window
        if (isNaN(self.state)) {
            self.multipliers = self.groups.map(function (group) {
                var result = 0;
                for (var digit = 0; digit < group.nDigits; digit++) {
                    result = result * 10 + 1;
                }

                return { group: group, multiplier: result };
            });
            self.state = 0;
        } else {
            self.state++;
            if (self.state === 11) { self.state = 0; }
        }

        self.multipliers.forEach(function (value) {
            if (self.state === 10) {
                value.group.setValue(NaN);
                return;
            }

            value.group.setValue(value.multiplier * self.state);
        });
    };

    this.stopTicker = function (button) {
        self.initialValues.forEach(function (value) {
            value.group.setValue(value.originalValue);
        });

        clearInterval(self.intervalVar);
        self.intervalVar = null;
    };

    this.intervalVar = setInterval(this.tick, cTestInterval);
}

function Scoreboard() {
    this.groups = [];

    this.findGroup = function (group) {
        return this.groups.find(function (value) { return value.name === group; });
    };

    this.reset = function () {
        this.groups.forEach(function (group) {
            group.setValue(NaN);
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

    this.createGroup = function (groupName, value, nDigits, newValue, update) {
        return new Group(this, groupName, value, nDigits, newValue, update);
    };
}

function inc(groupName, value) {
    scoreboard.findGroup(groupName).inc(value);
}

function setValue(groupName, value) {
    scoreboard.findGroup(groupName).setValue(value);
}