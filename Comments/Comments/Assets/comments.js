(function () {
    // debugger;

    var baseUrl = '/comments-middleware'; // replace this
    var ajaxHelper = new (function () {
        var self = this;
        self.get = function (url, success) {
            var xhr = window.XMLHttpRequest ? new XMLHttpRequest() : new ActiveXObject('Microsoft.XMLHTTP');
            xhr.open('GET', url);
            xhr.onreadystatechange = function () {
                if (xhr.readyState > 3 && xhr.status >= 200 && xhr.status < 300) success(xhr.responseText);
            };
            xhr.setRequestHeader('X-Requested-With', 'XMLHttpRequest');
            xhr.send();
            return xhr;
        };
        self.post = function (url, data, success) {
            var xhr = window.XMLHttpRequest ? new XMLHttpRequest() : new ActiveXObject("Microsoft.XMLHTTP");
            xhr.open('POST', url);
            xhr.onreadystatechange = function () {
                if (xhr.readyState > 3 && xhr.status >= 200 && xhr.status < 300) { success(xhr.responseText); }
            };
            xhr.setRequestHeader('X-Requested-With', 'XMLHttpRequest');
            xhr.send(params);
            return xhr;
        };
    })();

    var ajax = new (function (ajaxHelper) {
        var self = this;

        self.getCommentsCount = function (pageUrl, success) {
            url = baseUrl + '/count?url=' + encodeURIComponent(pageUrl);
            ajaxHelper.get(url, success);
        };

        self.getView = function (callback) {
            url = baseUrl + '/view.html';
            ajaxHelper.get(url, callback);
        };

    })(ajaxHelper);
    
    function setAllCounts() {
        var allSpans = document.getElementsByTagName('span');
        for (var i = 0, n = allSpans.length; i < n; i++) {
            var span = allSpans[i];
            var attrib = allSpans[i].getAttribute('data-comments-url');
            if (attrib != null) {
                ajax.getCommentsCount(attrib, function (res) {
                    span.innerText = res;
                });
            }
        }
    }

    var ViewModel = function () {
        var self = this;
        self.loading = ko.observable(false);
        self.previewVisible = ko.observable(false);
        self.source = ko.observable("");
        self.rendered = ko.observable("");
        self.useMarkdown = ko.observable(false);

        self.useMarkdown.subscribe(function () {
            if (!self.useMarkdown()) {
                self.previewVisible(false);
            }
        });

        function validateNewComment() {
            return true;
        }

        self.doPost = function () {
            if (!validateNewComment()) return;

            console.log("post comment");
        }
    }

    function setView() {
        var div = document.getElementById('comments-middleware');
        ajax.getView(function (result) {
            div.innerHTML = result;
        });
    }

    function onReady() {
        setAllCounts();
        setView();
    }

    // on document loaded
    if (document.readyState !== 'loading') onReady();
    else if (document.addEventListener) document.addEventListener('DOMContentLoaded', onReady);
    else document.attachEvent('onreadystatechange', function () {
        if (document.readyState === 'complete') onReady();
    });
})();