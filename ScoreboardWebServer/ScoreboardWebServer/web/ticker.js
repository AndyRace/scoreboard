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