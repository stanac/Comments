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
            xhr.send(data);
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

        self.preview = function (markdown, callback) {
            url = baseUrl + '/preview';
            ajaxHelper.post(url, markdown, callback);
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
        self.charsLeft = ko.observable(500);

        self.togglePreview = function () {
            var isPreview = self.previewVisible();
            if (isPreview) {
                self.previewVisible(false);
            } else {
                self.loading(true);
                ajax.preview(self.source(), function (rendered) {
                    self.rendered(rendered)
                    self.previewVisible(true);
                    self.loading(false);
                });
            }
        }

        function validateNewComment() {
            return false;
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
            ko.applyBindings(new ViewModel(), div);
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