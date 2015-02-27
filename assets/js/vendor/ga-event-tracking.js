(function ($, w) {

    "use strict";

    // What mode we are running in.
    var isUniversal = w.ga && !w._gaq;

    // Custom selector.
    $.extend($.expr[":"], {
        attrStart: function (el, i, props) {
            /// <summary>Custom selector extension to allow attribute starts with selection.</summary>
            /// <param name="el" type="DOM">The element to test against.</param>
            /// <param name="i" type="Number">The index of the element in the stack.</param>
            /// <param name="props" type="Object">Metadata for the element.</param>
            /// <returns type="Boolean">True if the element is a match; otherwise, false.</returns>
            var hasAttribute = false;

            $.each(el.attributes, function () {
                if (this.name.indexOf(props[3]) === 0) {
                    hasAttribute = true;
                    return false;  // Exit the iteration.
                }
                return true;
            });

            return hasAttribute;
        }
    });

    var pseudoUnique = function (length) {
        /// <summary>Returns a pseudo unique alpha-numeric string of the given length.</summary>
        /// <param name="length" type="Number">The length of the string to return. Defaults to 8.</param>
        /// <returns type="String">The pseudo unique alpha-numeric string.</returns>

        var len = length || 8,
            text = "",
            possible = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789",
            max = possible.length;

        if (len > max) {
            len = max;
        }

        for (var i = 0; i < len; i += 1) {
            text += possible.charAt(Math.floor(Math.random() * max));
        }

        return text;
    };

    var getDataOptions = function ($elem, filter) {
        /// <summary>Creates an object containing options populated from an elements data attributes.</summary>
        /// <param name="$elem" type="jQuery">The object representing the DOM element.</param>
        /// <param name="filter" type="String">The prefix with filter to identify the data attribute.</param>
        /// <returns type="Object">The extended object.</returns>

        var options = {};
        $.each($elem.data(), function (key, val) {

            if (key.indexOf(filter) === 0 && key.length > filter.length) {

                // Build a key with the correct format.
                var length = filter.length,
                    newKey = key.charAt(length).toLowerCase() + key.substring(length + 1);

                options[newKey] = val;
            }
        });

        // Can't use Object.keys for IE8 support.
        var keys = 0, i; for (i in options) { if (options.hasOwnProperty(i)) { keys++; } }
        return keys ? options : $elem.data();
    };

    var GoogleTracking = function (element, options) {
        /// <summary> Tracks google events via a click event.</summary>
        /// <param name="element" type="DOM">The DOM element.</param>
        /// <param name="options" type="String">The options to pass to the Google ga() function.</param>

        this.element = element;
        this.id = element.id || (element.id = pseudoUnique());
        this.$element = $(element);
        this.$delegate = $("body");
        this.isUniversal = isUniversal;
        this.newWindow = /_blank/i.test(this.$element.attr("target"));
        this.defaults = {
            debug: false,
            type: isUniversal ? "event" : "_trackEvent"
        };

        this.options = $.extend({}, this.defaults, options);

        // Bind click events.
        this.$delegate.one("click", "#" + this.id, $.proxy(this.click, this));
    };

    GoogleTracking.prototype.createArgs = function (options, callback) {
        /// <summary> Creates the parameter array to pass to the Google ga() function.</summary>
        /// <param name="options" type="Object">The options to pass to the arguments</param>
        /// <param name="callback" type="Function">The callback to fire once tracking is done.</param>
        /// <returns type="Array">The array of necessary parameters.</returns>

        var pageParams = isUniversal ? ["type", "location", "page", "title", "options"] : ["type", "url"],
            eventParams = ["type", "category", "action", "label", "value", (isUniversal ? "options" : "nonInteraction")],
            params = isUniversal ? ["send"] : [],
            defaults = {
                "hitCallback": callback
            };

        var toParse = /event|_trackEvent/.test(options.type) ? eventParams : pageParams;

        // Loop through and build the parameters.
        $.each(toParse, function (key, val) {

            var value = options[val];

            if (value || value === null) {
                params.push(value);
            }

        });

        if (isUniversal) {

            // Merge the defaults to add the callback.      
            var last = params[params.length - 1];

            if (!$.isPlainObject(last)) {
                params.push(defaults);
            } else {
                $.extend(last, defaults);
            }
        }
        return params;
    };

    GoogleTracking.prototype.click = function (event) {
        /// <summary>Handles any click events bound to the element.</summary>
        /// <param name="event" type="Object">The triggered event.</param>

        var element = this.element,
            newWindow = this.newWindow,
            doClick = false,
            original = event.originalEvent,
            prevented = function (e) {
                return e.defaultPrevented ||
               e.defaultPrevented === undefined &&
               e.returnValue === false ? true : false;
            },
            callback = function () {
                if (!newWindow) {
                    if (doClick) {
                        // Trigger the elements click event.
                        element.click();
                    }
                }
            };

        var args = this.createArgs(this.options, callback);

        if (this.isUniversal) {
            if (this.options.debug) {
                // Log the data.
                console && console.log(args);
                window.setTimeout(callback, 100);
            } else {
                // Push the data.
                w.ga.apply(w.ga, args);
            }
        } else {
            if (this.options.debug) {
                console && console.log(args);
            } else {
                w._gaq.push(args);
            }
            w.setTimeout(callback, 100);
        }

        // Only allow callback "click" in strict situations.
        if (!newWindow && !prevented(original)) {
            doClick = true;
            event.preventDefault();
        }
    };

    // No conflict.
    var old = $.fn.gaTracking;

    // Plug-in definition 
    $.fn.gaTracking = function (options) {
        /// <summary>Tracks google events bound to an element triggered via "click".</summary>
        /// <param name="options" type="Object">The options to pass to the Google ga() function.</param>

        return this.each(function () {

            var $this = $(this),
                data = $this.data("gaTracking");

            if (!data) {
                // Check the data and assign if not present.
                $this.data("gaTracking", new GoogleTracking(this, options));
            }
        });
    };

    // Set the public constructor.
    $.fn.gaTracking.Constructor = GoogleTracking;

    $.fn.gaTracking.noConflict = function () {
        $.fn.gaTracking = old;
        return this;
    };

    // Data API
    $(document).on("ready.ga.data-api", function () {
        $(":attrStart(data-ga)").each(function () {
            var $this = $(this);
            $this.gaTracking(getDataOptions($this, "ga"));
        });
    });

}(jQuery, window));