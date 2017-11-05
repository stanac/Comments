(function () {
    // debugger;

    var options = { baseUrl: "/comments-middleware", commentMaxLength: 600, includeHash: false, includeQuery: false, displayPostCommentOnLoad: true } // replace this, TODO
    var pageLocation = location.pathname;
    if (options.includeQuery) pageLocation += location.search || "";
    if (options.includeHash) pageLocation += location.hash || "";

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
            url = options.baseUrl + '/count?url=' + encodeURIComponent(pageUrl);
            ajaxHelper.get(url, success);
        };

        self.getView = function (callback) {
            url = options.baseUrl + '/view.html';
            ajaxHelper.get(url, callback);
        };

        self.preview = function (markdown, callback) {
            url = options.baseUrl + '/preview';
            ajaxHelper.post(url, markdown, callback);
        };

        self.addComment = function (comment, callback) {
            url = options.baseUrl + '/create';
            ajaxHelper.post(url, JSON.stringify(comment), callback);
        };

        self.loadComments = function (pageUrl, callback) {
            url = options.baseUrl + '/all-comments?url=' + encodeURIComponent(pageUrl);
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
        self.postCommentVisible = ko.observable(options.displayPostCommentOnLoad);
        self.loading = ko.observable(false);
        self.previewVisible = ko.observable(false);
        self.source = ko.observable("");
        self.rendered = ko.observable("");
        self.charsLeft = ko.observable(options.commentMaxLength);
        self.errors = ko.observableArray([]);
        self.useMarkdown = ko.observable(false);
        self.errors = ko.observableArray([]);
        self.email = ko.observable("");
        self.name = ko.observable("");
        self.comments = ko.observableArray([]);

        self.useMarkdown.subscribe(function () {
            if (!self.useMarkdown()) {
                self.previewVisible(false);
            }
            self.errors.removeAll();
        });

        self.togglePostCommentVisible = function() {
            self.postCommentVisible(!self.postCommentVisible());
        };

        self.email.subscribe(function () { self.errors.removeAll(); });
        self.name.subscribe(function () { self.errors.removeAll(); });
        self.source.subscribe(function () {
            self.errors.removeAll();
            self.charsLeft(options.commentMaxLength - self.source().length);
        });
        
        var onCommentPosted = function (result) {
            var comment = JSON.parse(result);
            comment.ImgUrl = "http://www.gravatar.com/avatar/" + comment.EmailHash + "?size=56";
            self.comments.push(comment);
            loadCurrenPageCommentsCount();
            self.source('');
        };

        var post = function () {
            var comment = {
                pageUrl: pageLocation,
                posterName: self.name(),
                posterEmail: self.email(),
                commentContentSource: self.source(),
                isMarkdown: self.useMarkdown()
            };
            ajax.addComment(comment, onCommentPosted);
        };

        self.validateAndPost = function () {
            self.errors.removeAll();
            var cleanText = function (text) {
                return text.replace(/\s/g, '');
            };
            var validateName = function () {
                if (cleanText(self.name()).length === 0) {
                    self.errors.push("Name field is required.");
                }
            };
            var validateEmail = function () {
                var emailReg = /^(([^<>()\[\]\\.,;:\s@"]+(\.[^<>()\[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
                if (cleanText(self.email()).length === 0) {
                    self.errors.push("Email field is required.");
                }
                else if (!emailReg.test(self.email())) {
                    self.errors.push("Email is not in correct format.");
                }
            };
            var validateText = function () {
                if (cleanText(self.source()).length === 0) {
                    self.errors.push("Comment is required.");
                }
                else if (self.source().length > options.commentMaxLength) {
                    self.errors.push("Your comment length exceeds allowed limit.");
                }
            };
            validateName();
            validateEmail();
            validateText();
            if (self.errors().length == 0) {
                post();
            }
        }

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
        
        // display comments, TODO: maybe create a separate view/viewmodel
        self.commentsCount = ko.observable("");
        var loadCurrenPageCommentsCount = function () {
            ajax.getCommentsCount(pageLocation, function (retValue) {
                self.commentsCount(retValue);
            });
        };
        loadCurrenPageCommentsCount();
        var loadAllComments = function () {
            ajax.loadComments(pageLocation, function (json) {
                self.comments([]);
                var comments = JSON.parse(json);
                for (var i = 0; i < comments.length; i++) {
                    comments[i].ImgUrl = "http://www.gravatar.com/avatar/" + comments[i].EmailHash + "?size=56";
                }
                self.comments(comments);
            });
        };
        loadAllComments();
    }

    function setView() {
        var div = document.getElementById('comments-middleware');
        if (div == null) return;

        ajax.getView(function (result) {
            div.innerHTML = result;
            ko.cleanNode(div);
            ko.applyBindings(new ViewModel(), div);
        });
    }

    function onReady(reload) {
        reload = reload || false;
        if (!reload) setAllCounts();
        setView();
    }

    document.reloadCommentsMiddlewareViewModel = function() {
        onReady(true);
    }

    // on document loaded
    if (document.readyState !== 'loading') onReady();
    else if (document.addEventListener) document.addEventListener('DOMContentLoaded', onReady);
    else document.attachEvent('onreadystatechange', function () {
        if (document.readyState === 'complete') onReady();
    });
})();