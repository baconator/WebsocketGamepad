$(document).ready(function(){
    var Circle = function(element){
        this.element = $(element);
    };
    Circle.prototype.distance = function(compare){
        return Math.sqrt(Math.pow(this.centre().x-compare.x, 2) + Math.pow(this.centre().y-compare.y, 2));
    };
    Circle.prototype.innerRadius = function(){
        return this.element.width()/2;
    };
    Circle.prototype.outerRadius = function(){
        return this.element.outerWidth()/2;
    };
    Circle.prototype.move = function(destination, bound){
        // Check if the destination is within the bound; move up to the bound if not.
        if(bound === undefined || (bound.distance(destination) + this.outerRadius() <= bound.innerRadius())){
            this.element.offset({
                top: destination.y-this.outerRadius(),
                left: destination.x-this.outerRadius()
            });
        }else{
            var inputDistance = bound.distance(destination);
            var maxDistance = bound.innerRadius() - this.outerRadius();
            var ratio = maxDistance/inputDistance;
            
            this.element.offset({
                top: ratio*(destination.y-bound.centre().y)+bound.centre().y-this.outerRadius(), // Measure from centre of bound, not top left of the page.
                left: ratio*(destination.x-bound.centre().x)+bound.centre().x-this.outerRadius()
            });
        }
        this.element.css("visibility", "visible");
    };
    Circle.prototype.centre = function(){
        return {
            x: this.element.offset().left + this.outerRadius(),
            y: this.element.offset().top + this.outerRadius()
        };
    };
    Circle.prototype.percentPosition = function(compare){ // {x: +/-100, y: +/-100}
        return {
            x: (compare.centre().x - this.centre().x)*100/(this.outerRadius()-compare.innerRadius()),
            y: (compare.centre().y - this.centre().y)*100/(this.outerRadius()-compare.innerRadius())
        };
    };
    
    var Control = function(element, id){
        this.element = $(element);
        this.id = id;
    };
    Control.prototype.touchStart = function(finger){};
    Control.prototype.touchMove = function(finger){};
    Control.prototype.touchEnd = function(ev){};
    
    var DirectionalPad = function(){
        Control.prototype.constructor.apply(this, arguments);
    };
    DirectionalPad.prototype = Object.create(Control.prototype, {
        getName: {
            value: function(el){
                var target = $(el);
                if(target.hasClass("gamepad-button")){
                    var name = target.attr("name");
                    name = name || target.index();
                    return name;
                }
            }
        },
        touchStart: {
            value: function(finger){
                return {
                    subId: this.getName(finger.target),
                    id: this.id,
                    type: "down"
                };
            }
        },
        touchMove: {
            value: function(finger){
                return {
                    subId: this.getName(finger.target),
                    id: this.id,
                    type: "move"
                };
            }
        },
        touchEnd: {
            value: function(ev){
                return {
                    subId: this.getName(ev.target),
                    id: this.id,
                    type: "up"
                };
            }
        }
    });
    
    var AnalogStick = function(){
        Control.prototype.constructor.apply(this, arguments);
    };
    AnalogStick.prototype = Object.create(Control.prototype, {
        touchStart: {
            value: function(finger){
                Control.prototype.touchStart.apply(this, arguments);
                var container = new Circle(this.element.find(".gamepad-analog").first());
                var stick = new Circle(container.element.find(".gamepad-stick").first());

                container.move({x: finger.pageX, y: finger.pageY});
                stick.move({x: finger.pageX, y: finger.pageY});

                return {
                    value: stick.percentPosition(container),
                    id: this.id
                };
            }
        },
        touchMove: {
            value: function(finger){
                Control.prototype.touchMove.apply(this, arguments);
                var container = new Circle(this.element.find(".gamepad-analog").first());
                var stick = new Circle(container.element.find(".gamepad-stick").first());

                stick.move({x: finger.pageX, y: finger.pageY}, container);

                return {
                    value: stick.percentPosition(container),
                    id: this.id
                };
            }
        },
        touchEnd: {
            value: function(ev){
                Control.prototype.touchEnd.apply(this, arguments);
                var container = new Circle(this.element.find(".gamepad-analog").first());
                var stick = new Circle(container.element.find(".gamepad-stick").first());

                stick.move(container.centre());

                return {
                    value: stick.percentPosition(container),
                    id: this.id
                };
            }
        }
    });
    AnalogStick.prototype.constructor = AnalogStick;
    
    var Region = function(element){
        this.element = $(element);
        if(this.element.find(".gamepad-analog").length){
            this.control = new AnalogStick(this.element, "analog0");
        }else if(this.element.find(".gamepad-button").length){
            this.control = new DirectionalPad(this.element, "dpad0");
        }
    };
    Region.prototype.contains = function(compare){
        if((this.element.offset().left <= compare.pageX) && (this.element.offset().top <= compare.pageY)
          && ((this.element.offset().left+this.element.width()) >= (compare.pageX))
          && ((this.element.offset().top+this.element.height()) >= (compare.pageY))){
            return true;
        }else{
            return false;
        }
    };
    Region.prototype.touchStart = function(finger){
        return {
            result: this.control.touchStart(finger),
            type: "touchStart"
        };
    };
    Region.prototype.touchMove = function(finger){
        return {
            result: this.control.touchMove(finger),
            type: "touchMove"
        };
    };
    Region.prototype.touchEnd = function(finger){
        return {
            result: this.control.touchEnd(finger),
            type: "touchEnd"
        };
    };
    Region.prototype.id = function(){
        return this.element.attr("id");
    };
    
    var Gamepad = function(element){
        var that = this;
        this.element = $(element).first();
        
        var fireEvent = function(region, retValue){
            var callbacks = that.listeners[region.id()] || [];
            _.each(callbacks, function(callback){
                callback(region.id(), retValue);
            });
        };
        
        var regionContainsFinger = function(finger){
            return function(region){
                return (region.element.index(finger.target) > -1) || (region.element.find(finger.target).length > 0);
            };
        };
        
        this.element.bind("touchstart", function(ev){
            //if($(ev.currentTarget).parents(".gamepad-touch-area").length === 0) return;
            ev.preventDefault();
            // For each finger, set up a mapping to correlate regions.
            var fingers = ev.originalEvent.changedTouches;
            
            // Dispatch the event to the appropriate region.
            _.each(fingers, function(finger){
                var region = _.find(that.regions(), regionContainsFinger(finger));
                fireEvent(region, region.touchStart(finger));
            });
        });
        
        this.element.bind("touchmove", function(ev){
            // Find which region the finger belongs to, then move its sticks as appropriate.
            _.each(ev.originalEvent.touches, function(finger, i){
                var region = _.find(that.regions(), regionContainsFinger(finger));
                if(region) fireEvent(region, region.touchMove(finger));
            });
        });
        
        this.element.bind("touchend", function(ev){
            // Only end the touches which are being ended ...
            _.each(ev.originalEvent.changedTouches, function(finger, i){
                var region = _.find(that.regions(), regionContainsFinger(finger));
                if(region) fireEvent(region, region.touchEnd(ev.originalEvent));
            });
        });
        
        this.listeners = {};
    };
    Gamepad.prototype.regions = function(){
        return this.element.find(".gamepad-touch-area").map(function(i, el){return new Region(el);});
    };
    Gamepad.prototype.listen = function(id, callback){
        if(!this.listeners[id]) this.listeners[id] = [];
        this.listeners[id].push(callback);
    };
    Gamepad.prototype.ignore = function(callback){
        this.listeners = this.listeners.filter(function(cb){return cb != callback;});
    };
    
    var percentApproximation = function(percentage){
        var time = 50;
        var divisions = 2;
        var pulseDuration = time/divisions;
        
        return _.flatten(_.map(_.range(divisions), function(){
            return [pulseDuration/percentage, pulseDuration-pulseDuration/percentage];
        }));
    };
    
    $.connection.hub.logging = true;
    var hub = $.connection.phoneHub;

    var userId = (function () {
        var array = new Uint32Array(4);
        window.crypto.getRandomValues(array);
        var existing = $.cookie("userId");
        if (!existing) {
            $.cookie("userId", Array.prototype.join.call(array, ","));
        }
        return function () {
            return $.cookie("userId");
        };
    }());

    $.connection.phoneHub.client.updateGamepadNumber = function (num) {
        alert(num);
    };

    $.connection.hub.start().done(function () {
        var gp = new Gamepad($(".gamepad").first());
        gp.listen("right-buttons", function (region, response) {
            if (response.result.type != "move") {
                navigator.vibrate(percentApproximation(100));
                hub.server.updateState(userId(), response.result.id + ":" + response.result.subId, response.result.type);
            }
        });
        gp.listen("left-analog", function (region, response) {
            hub.server.updateState(userId(), response.result.id, JSON.stringify(response.result.value));
        })
    });

});