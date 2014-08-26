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

    $.buildDataOptions = function ($elem, options, prefix, namespace) {
        /// <summary>Creates an object containing options populated from an elements data attributes.</summary>
        /// <param name="$elem" type="jQuery">The object representing the DOM element.</param>
        /// <param name="options" type="Object">The object to extend</param>
        /// <param name="prefix" type="String">The prefix with which to identify the data attribute.</param>
        /// <param name="namespace" type="String">The namespace with which to segregate the data attribute.</param>
        /// <returns type="Object">The extended object.</returns>
        $.each($elem.data(), function (key, val) {

            if (key.indexOf(prefix) === 0 && key.length > prefix.length) {

                // Build a key with the correct format.
                var length = prefix.length,
                    newKey = key.charAt(length).toLowerCase() + key.substring(length + 1);

                options[newKey] = val;

                // Clean up.
                $elem.removeData(key);
            }

        });

        if (namespace) {
            $elem.data(namespace + "." + prefix + "Options", options);
        } else {
            $elem.data(prefix + "Options", options);
        }

        return options;
    };

    var createArgs = function (options, callback) {
        /// <summary>
        ///     Creates the parameter array to pass to the Google ga() method.
        /// </summary>
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

            if (value) {
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

    var GoogleTracking = function (element, options) {

        this.$element = $(element);
        this.defaults = {
            track: true,
            type: isUniversal ? "event" : "_trackEvent"
        };

        this.options = $.extend({}, this.defaults, options);

        // Check to see if the plug-in is set to track and trigger 
        // the correct internal method if so.
        if (this.options.track) {
            this.track();
        }

    };

    GoogleTracking.prototype.track = function () {

        var $element = this.$element,
            newWindow = this.options.newWindow,
            callback = function () {
                if (!newWindow) {
                    // Trigger the elements click event.
                    $element.off("click.ga")[0].click();
                } else {
                    $element.off("click.ga");
                }
            };

        var args = createArgs(this.options, callback);

        if (isUniversal) {
            // Push the data.
            w.ga.apply(w.ga, args);
        } else {
            w._gaq.push(args);
            w.setTimeout(callback, 100);
        }
    };

    $.fn.gaTracking = function (options) {

        return this.each(function () {

            var $this = $(this),
                data = $this.data("ga"),
                opts = typeof options === "object" ? options : null;

            if (!data) {
                // Check the data and reassign if not present.
                $this.data("ga", (data = new GoogleTracking(this, opts)));
            }

            // Run the appropriate function if a string is passed.
            if (typeof options === "string") {
                data[options]();
            }
        });
    };

    // Set the public constructor.
    $.fn.gaTracking.Constructor = GoogleTracking;

    // No conflict.
    var old = $.fn.gaTracking;
    $.fn.gaTracking.noConflict = function () {
        $.fn.gaTracking = old;
        return this;
    };

    var handler = function (e) {
        var $this = $(this),
            data = $this.data("gaOptions"),
            options = data || $.buildDataOptions($this, {}, "ga");

        // Prevent popup blocker.
        options.newWindow = $this.attr("target") === "_blank";
        if (!options.newWindow) {
            e.preventDefault();
        }

        e.stopImmediatePropagation();

        // Parse specific attributes from anchors.
        options.href || (options.href = $this.attr("href"));

        var params = $this.data("ga") ? "track" : options;

        // Run the tracking method.
        $this.gaTracking(params);
    };

    // Google tracking data api initialization.
    $(":attrStart(data-ga)").on("click.ga", handler);

}(jQuery, window));