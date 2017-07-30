(function () {
    function onReady() {
        
    }

    // on document loaded
    if (document.readyState != 'loading') onReady();
    else if (document.addEventListener) document.addEventListener('DOMContentLoaded', onReady);
    else document.attachEvent('onreadystatechange', function () {
        if (document.readyState == 'complete') onReady();
    });
})();