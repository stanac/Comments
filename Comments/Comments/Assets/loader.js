(function () {
    function onReady() {
        var options = { loadJs: 'auto', middlewareRoot: '/comments-middleware', loadMinified: true, loadCss: true }; // replace this

        function loadCss() {
            if (!options.loadCss) return;
            var head = document.head;
            var link = document.createElement("link");

            link.type = "text/css";
            link.rel = "stylesheet";
            link.href = options.middlewareRoot + "/style.css";

            head.appendChild(link)
        }

        function addScriptTag(url) {
            var script = document.createElement('script'),
                scripts = document.getElementsByTagName('script')[0];
            script.src = url;
            scripts.parentNode.insertBefore(script, scripts);
        }

        function loadKo() {
            var load = false;
            if (options.loadJs === 'yes') laod = true;
            else if (options.loadJs === 'auto') {
                if (typeof ko === 'undefined') load = true;
            }

            if (!load) return;
            
            var cdnSrc = 'https://cdnjs.cloudflare.com/ajax/libs/knockout/3.4.2/knockout-min.js';
            addScriptTag(cdnSrc);
        }

        loadCss();
        loadKo();
        var commentsSrc = options.middlewareRoot + '/comments.js';
        if (options.loadMinified) var commentsSrc = options.middlewareRoot + '/comments.min.js';
        addScriptTag(commentsSrc);
    }

    // on document loaded
    if (document.readyState !== 'loading') onReady();
    else if (document.addEventListener) document.addEventListener('DOMContentLoaded', onReady);
    else document.attachEvent('onreadystatechange', function () {
        if (document.readyState === 'complete') onReady();
    });
})();