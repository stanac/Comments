(function () {
    function onReady() {
        var options = { loadJs: 'auto', middlewareRoot: '/comments-middleware' };

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

        loadKo();
        var commentsSrc = options.middlewareRoot + '/comments.js';
        addScriptTag(commentsSrc);
    }

    // on document loaded
    if (document.readyState != 'loading') onReady();
    else if (document.addEventListener) document.addEventListener('DOMContentLoaded', onReady);
    else document.attachEvent('onreadystatechange', function () {
        if (document.readyState == 'complete') onReady();
    });
})();