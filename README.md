# Comments
Easy to use Comments middleware for ASP.NET Core (compiled to netstandard1.6).
Adds comments capability to existing ASP.NET Core applications, comments are stored
locally in Sqlite database.

**Demo:** [http://commentsdemo.stanacev.com/](http://commentsdemo.stanacev.com/)

```
PM> Install-Package Comments
```
```
> dotnet add package Comments
```

## Quick start
1. Install nuget package `Comments`
2. Add middleware to you application (in `Configure` method of `Startup` class)
```csharp
app.UseComments();
```
3. On page on which you would like to display comments add div (which will display comments):
```xml
<div id="comments-middleware"><div>
```
4. Somewhere below on the same page add script:
```xml
<script src="/comments-middleware/loader.js"></script>
```

---
If your application/website is SPA, you can reload comments on page load with:
```javascript
document.reloadCommentsMiddleware();
```
For configuration scroll down.

---
Source contains demo project which can be used as a sample to get you
up and running.


## Contributing
Contributions are welcome. What needs to be done next is support for
internationalization. After that support for other languages can be added.

Please note: If you are submitting pull request you are automatically
dedicating any and all copyright interest to the public domain.

## Configuration
Configuration of the middleware is done during registration of the 
middleware. As an example following code sample sets maximum number
of allowed characters when posting a comment.

``` csharp
app.UseComments(o =>
{
    o.CommentSourceMaxLength = 1000;
});
```

Following options are available:

| Property | Default value | Description |
| --- | --- | --- |
| `int CommentSourceMaxLength` | `600` | Maximum allowed character when posting comment. |
| `bool LoadCss` | `true` | Could be set to false, so you can load your own fancy CSS. |
| `bool IncludeHashInUrl` | `false` | Indicates whether hash part of the URL should be included in page URL when posting comments. |
| `string BaseUrl` | `"/comments-middleware"` | Base URL on which middleware responds, don't change this unless you really need to scratch that itch. |
| `string SqliteDbFilePath` | `"comments.sqlite"` | Location of the database file which stores comments. You possibly want to change this to full path of the file. File will be created if one does not already exists. |
| `string NoCommentsTemplate` | `"no comments"` | String indicating how to display zero comments count label. |
| `string OneCommentTemplate` | `"1 comment"` | String indicating how to display comments count label when there is only one comment. |
| `string MoreThanOneCommentTemplate` | `"{count} comments"` | Template that will be used to format label when there is more than one comment. |
| `Func<int, string> CommentCountFormatter` | `null` | Function that can be used to format label displaying comments count. If not null, previous three properties will never be used. |
| `LoadJsDependenciesOptions LoadJsDependencies` | `LoadJsDependenciesOptions.AutoDetect` | Comments middleware depends on knockout js library. This property indicates whether knockout should be automatically loaded or not. |
| `Func<HttpContext, bool> IsUserAdminModeratorCheck` | *(a)* | Function that is invoked when middleware needs to determine if logged in user is comments moderator. |
| `bool RequireCommentApproval` | `false` | If true, comment's will be visible only after approval of moderator. |
| `bool IncludeQueryInUrl` | `false` | If `true` query part of the URL will be used to detect page when loading and posting comments (You should almost always set this to **false**, or leave it intact)  |
| `bool DisplayPostCommentDivOnLoad` | `true` | Indicated if post comment form should be automatically visible on page load, if not, user will have to click on label "Post comment" to see the form |
| `MarkdownPipeline` MarkdigPipeline | `null` | Pipeline of the Markdig markdown to HTML converted. Check [Markdig project](https://github.com/lunet-io/markdig) for details. By default, the most basic pipeline with disabled HTML is used. |
| `Action<CommentModel> InformModerator` | `null` | If not null, it will be called after comment is posted, so moderator can approve/delete comment. Developers could implement this action (send email, or whatever is desired to happen when comment is posted). If call is async, it will be called in fire and forget mode. Developer **should manually handle exceptions** in this action, calling code is not wrapping it in try block.  |

*(a)* = `ctx => ctx.User.Identity.IsAuthenticated && ctx.User.IsInRole("commentsmod")`
