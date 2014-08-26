(function () {
    // C# extension for prism.
    // http://www.broculos.net/2013/05/why-we-chose-prism-syntax-highlighter.html#.Uq7lYvRdXjV
    Prism.languages.csharp = Prism.languages.extend("clike", {
        'keyword': /\b(abstract|as|base|bool|break|byte|case|catch|char|checked|class|const|continue|decimal|default|delegate|do|double|else|enum|event|explicit|extern|false|finally|fixed|float|for|foreach|goto|if|implicit|in|int|interface|internal|is|lock|long|namespace|new|null|object|operator|out|override|params|private|protected|public|readonly|ref|return|sbyte|sealed|short|sizeof|stackalloc|static|string|struct|switch|this|throw|true|try|typeof|uint|ulong|unchecked|unsafe|ushort|using|virtual|void|volatile|while|add|alias|ascending|async|await|descending|dynamic|from|get|global|group|into|join|let|orderby|partial|remove|select|set|value|var|where|yield)\b/g,
        'string': /@?("|')(\\?.)*?\1/g,
        'preprocessor': /^\s*#.*/gm,
        'number': /\b-?(0x)?\d*\.?\d+\b/g
    });
}());

(function ($, w, d) {
    "use strict";

    // Ensure the navigation is accessible.
    var resisizeTimer,
        $navigation = $("#navigation"),
        resize = function () {
            var currentIndex = $.support.currentGrid().index;

            // Small.
            if (currentIndex >= 1) {
                $navigation.attr({
                    "aria-hidden": false,
                    "tabindex": ""
                });
            } else if ($navigation.hasClass("collapse")) {
                $navigation.attr({
                    "aria-hidden": true,
                    "tabindex": -1
                });
            }
        };

    $(w).on("resize orientationchange", function () {
        if (resisizeTimer) {
            w.clearTimeout(resisizeTimer);
        }
        resisizeTimer = w.setTimeout(resize, 50);
    });

    $(d).on("ready", function () {
        resize();
    });

    //Back to Top scroll
    $("button.to-top").on("click", function (event) {
        event.preventDefault();

        // Normalize the velocity. Lets's say 100ms to travel 1000px.
        var baseVelocity = 1000 / 100,
            distance = $(this).offset().top,
            relativeTime = (distance / baseVelocity);

        $("html, body").animate({
            scrollTop: 0
        }, relativeTime);
    });

}(jQuery, window, document));