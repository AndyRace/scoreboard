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

function Ticker(groups) {
    var self = this;
    self.groups = groups;

    // get the current values
    self.initialValues = groups.map(
        function (group) {
            return {
                group: group, originalValue: group.value
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
            self.groups.forEach(function (group) {
                group.value = NaN;
            });
            return;
        }

        self.initialValues.forEach(function (groupValue) {
            groupValue.group.value = groupValue.originalValue;
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
                value.group.value = NaN;
                return;
            }

            value.group.value = value.multiplier * self.state;
        });
    };

    this.stopTicker = function (button) {
        self.initialValues.forEach(function (groupValue) {
            groupValue.group.value = groupValue.originalValue;
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
    scoreboard.findGroup(groupName).value = value;
}