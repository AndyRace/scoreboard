const cTestInterval = 500;

function debugInfo(info) {
    // alert(info);
}

// default controller
function Controller(group) {
    this.group = group;

    this.setValue = function (value) {
        debugInfo("Controller.SetValue: " + this.group + ", value: " + value);
        this.group.setDisplayText(value);
        this.group.setNewValueText(value);
    };
}

function Group(group, value, nDigits, newValue, update) {
    var self = this;

    this.controller = new Controller(self);
    this.name = group;
    this.valueElement = document.getElementById(value);
    this.nDigits = nDigits;
    this.maxValue = Math.pow(10, nDigits) - 1;
    this.newValueElement = document.getElementById(newValue);
    this.updateElement = document.getElementById(update);
    this.value = NaN;

    this.getValue = function () {
        return this.value;
    };

    this.setValue = function (value) {
        value = Number(value);
        if (value < 0) { value = NaN; }
        if (value > this.maxValue) { value = this.maxValue; }

        this.value = value;

        // plug in the model here
        this.controller.setValue(value);
    };

    this.setNewValueText = function (text) {
        if (this.newValueElement !== null) {
            this.newValueElement.value = text;
        }
    };

    this.setDisplayText = function (text) {
        this.valueElement.innerText = text;
    };

    this.inc = function (incValue) {
        incValue = Number(incValue);
        if (isNaN(incValue)) incValue = 0;

        var value = this.value;
        if (isNaN(value)) value = 0;

        this.setValue(value + incValue);
    };

    if (this.updateElement !== null) {
        this.updateElement.onclick = function (event) {
            self.setValue(self.newValueElement.value);
        };
    }
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

    this.update = function () {
        this.groups.forEach(function (group) {
            group.setValue(group.getValue());
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

    this.createGroup = function (group, value, nDigits, newValue, update) {
        return new Group(group, value, nDigits, newValue, update);
    };
}

function inc(group, value) {
    scoreboard.findGroup(group).inc(value);
}