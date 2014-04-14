/*! Responsive v2.5.3 | MIT License | responsivebp.com */

(function () {

    "use strict";

    /** IE10 in Windows (Phone) 8
     * Support for responsive views via media queries do not work in IE10 on mobile for
     * versions prior to WP8 Update 3 (GDR3).
     * This script has to be inserted in the head before any other scripts due to timing issues.*/
    if (navigator.userAgent.match(/IEMobile\/10\.0/)) {
        var msViewportStyle = document.createElement("style");
        msViewportStyle.appendChild(document.createTextNode("@-ms-viewport{width:auto!important}"));
        document.querySelector("head").appendChild(msViewportStyle);
    }

}());
