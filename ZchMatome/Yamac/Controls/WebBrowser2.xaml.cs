using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Phone.Reactive;
using System.Net;
using System.IO;
using ICSharpCode.SharpZipLib.GZip;
using ZchMatome;
using System.Text.RegularExpressions;

namespace Yamac.Controls
{
    public partial class WebBrowser2 : UserControl
    {
        private const string DEFAULT_USER_AGENT = "Mozilla/5.0 (iPhone; CPU iPhone OS 5_0 like Mac OS X) AppleWebKit/534.46 (KHTML, like Gecko) Version/5.1 Mobile/9A334 Safari/7534.48.3";
        private Regex headTagRegex = new Regex(@"(<head[^>]*>)", RegexOptions.IgnoreCase);
        private Regex embedTagRegex = new Regex(@"<embed [^>]*?src=""?(?:https?://)?(?:www\.)?(?:youtu\.be/|youtube\.com(?:/v/|/embed/|/watch\?v=))([\w-]{10,12}).*?""[^>]*?>", RegexOptions.IgnoreCase);
        private Regex userGadTagRegex = new Regex(@"(<div class=""google-user-ad"")>", RegexOptions.IgnoreCase);

        private bool inTheWebBrowser;
        private bool isNavigating;
        private int numNavigates;
        private Stack<NavigationHistory> historyGoBackStack;
        public string UserAgent { get; set; }
        private Uri mainSource;
        private string mainHtml;

        private class NavigationHistory
        {
            public Uri Uri { get; set; }
            public int NavigationCount { get; set; }
        }

        #region Public methods

        public WebBrowser2()
        {
            InitializeComponent();
            TheWebBrowser.Visibility = Visibility.Visible;
            TheWebBrowser.IsScriptEnabled = true;
            SubWebBrowser.Visibility = Visibility.Collapsed;
            SubWebBrowser.IsScriptEnabled = true;
            inTheWebBrowser = true;
            isNavigating = false;
            UserAgent = DEFAULT_USER_AGENT;
            historyGoBackStack = new Stack<NavigationHistory>();
        }

        public void Navigate(Uri uri)
        {
            if (uri.Scheme.Equals("http"))
            {
                var req = WebRequest.CreateHttp(uri);
                req.UserAgent = Constants.Net.UserAgent;
                req.Headers[HttpRequestHeader.AcceptEncoding] = "gzip";

                IsBusy = true;
                mainSource = new Uri(uri.ToString());

                Observable
                .FromAsyncPattern<WebResponse>(req.BeginGetResponse, req.EndGetResponse)()
                .Select
                (
                    res =>
                    {
                        // ストリームを取得
                        Stream stream = res.GetResponseStream();
                        if (string.Equals("gzip", res.Headers[HttpRequestHeader.ContentEncoding], StringComparison.OrdinalIgnoreCase))
                        {
                            stream = new GZipInputStream(stream);
                        }

                        // ダウンロード
                        StreamReader reader = new StreamReader(stream, Encoding.Unicode);
                        mainHtml = reader.ReadToEnd();
                        reader.Close();

                        // ストリームを閉じる
                        stream.Close();

                        // 整形
                        string contentType = res.Headers[HttpRequestHeader.ContentType];
                        if (contentType.ToUpper().IndexOf("UTF-8") > 0)
                        {
                            byte[] bytes;
                            bytes = Encoding.Unicode.GetBytes(mainHtml);
                            mainHtml = Encoding.UTF8.GetString(bytes, 0, bytes.Length);

                            string docBase = uri.AbsoluteUri;
                            if (mainSource.Query.Length > 0)
                            {
                                docBase = docBase.Substring(0, docBase.LastIndexOf(mainSource.Query));
                            }
                            int lastSlash = docBase.LastIndexOf('/');
                            if (lastSlash >= 0)
                            {
                                docBase = docBase.Substring(0, lastSlash + 1);
                            }

                            mainHtml = headTagRegex.Replace(mainHtml, "$1<base href=\"" + docBase + "\"/>");
                            mainHtml = embedTagRegex.Replace(mainHtml, "<iframe src=\"http://www.youtube.com/embed/$1\" width=\"100%%\"></iframe>");
                            mainHtml = userGadTagRegex.Replace(mainHtml, "$1 style=\"display:none\"$2");

                            bytes = Encoding.UTF8.GetBytes(mainHtml);
                            mainHtml = Encoding.Unicode.GetString(bytes, 0, bytes.Length);
                        }

                        return 0;
                    }
                )
                .ObserveOnDispatcher()
                .Subscribe
                (
                    _ =>
                    {
                        // WebBrowserへセット
                        TheWebBrowser.NavigateToString(mainHtml);
                    }
                    ,
                    _ =>
                    {
                        IsBusy = false;

                        // ハンドラー
                        if (NavigationFailed != null)
                        {
                            NavigationFailed(TheWebBrowser, null);
                        }
                    }
                );
            }
            else
            {
                TheWebBrowser.Navigate(uri);
            }
        }

        public void GoBack()
        {
            try
            {
                if (historyGoBackStack.Count == 1)
                {
                    CanGoBack = false;
                    inTheWebBrowser = true;
                    TheWebBrowser.Visibility = Visibility.Visible;
                    SubWebBrowser.Visibility = Visibility.Collapsed;
                    historyGoBackStack.Pop();
                    SubWebBrowser.NavigateToString("");
                }
                else
                {
                    isNavigating = true;
                    SubWebBrowser.Navigate(new Uri("javascript:history.back();"));
                }
            }
            catch
            {
            }
        }

        public void Refresh()
        {
            try
            {
                isNavigating = true;
                if (inTheWebBrowser)
                {
                    TheWebBrowser.Navigate(new Uri("javascript:history.go();"));
                }
                else
                {
                    SubWebBrowser.Navigate(new Uri("javascript:history.go();"));
                }
            }
            catch
            {
            }
        }

        #endregion

        #region Private methods

        private void setScript()
        {
            // POST動作の発生をフックするためのスクリプトを実行
            string script =
                  "var pos = location.href.indexOf(location.pathname);"
                + "var root = location.href.substr(0, pos);"
                + "var path = location.href.substr(0, pos + location.pathname.length);"
                + "pos = path.lastIndexOf('/');"
                + "var base = path.substr(0, pos);"
                + "var forms = document.getElementsByTagName('form');"
                + "for (var i = 0; i < forms.length; i++)"
                + "{"
                + "    var form = forms[i];"
                + "    if (form.method == 'post') {"
                + "        form.onsubmit = function() {"
                + "            var postData = [];"
                + "            window.external.notify('test:' + this.id);"
                + "            for (var j = 0; j < this.elements.length; j++) {"
                + "                var element = this.elements[j];"
                + "                 window.external.notify('test:' + element.name + '=' + element.value);"
                + "                if (!element.name || !element.type) continue;"
                + "                if (element.name.length == 0 || element.type == 'file') continue;"
                + "                postData.push(encodeURIComponent(element.name) + '=' + encodeURIComponent(element.value));"
                + "            }"
                + "            var action = this.action;"
                + "            if (action && action.length > 0) {"
                + "                if (action.charAt[0] == '/') {"
                + "                    action = root + action;"
                + "                } else if (action.indexOf('://') == -1) {"
                + "                    action = base + '/' + action;"
                + "                }"
                + "            } else {"
                + "                action = path;"
                + "            }"
                + "            window.external.notify('post' + '\t' + this.method + '\t' + action + '\t' + this.enctype + '\t' + postData.join('&'));"
                + "            return false;"
                + "        }"
                + "    }"
                + "}";
            Dispatcher.BeginInvoke(() =>
            {
                try
                {
                    SubWebBrowser.Navigate(new Uri("javascript:;"));
                    SubWebBrowser.InvokeScript("eval", script);
                }
                catch
                {
                    try
                    {
                        SubWebBrowser.Navigate(new Uri("javascript:;"));
                        Thread.Sleep(100);
                        SubWebBrowser.InvokeScript("eval", script);
                    }
                    catch
                    {
                    }
                }
            });
        }

        #endregion

        #region Dependency properties

        /// <summary>
        /// ブラウザーが処理を実行中かどうかを取得します。
        /// </summary>
        public bool IsBusy
        {
            get { return (bool)GetValue(IsBusyProperty); }
            set { SetValue(IsBusyProperty, value); }
        }

        public static readonly DependencyProperty IsBusyProperty =
            DependencyProperty.Register("IsBusy", typeof(bool),
            typeof(WebBrowser2), new PropertyMetadata((bool)false));

        /// <summary>
        /// ブラウザーが前のページへ戻ることができるかどうかを取得します。
        /// </summary>
        public bool CanGoBack
        {
            get { return (bool)GetValue(CanGoBackProperty); }
            set { SetValue(CanGoBackProperty, value); }
        }

        public static readonly DependencyProperty CanGoBackProperty =
            DependencyProperty.Register("CanGoBack", typeof(bool),
            typeof(WebBrowser2), new PropertyMetadata((bool)false));

        /// <summary>
        /// ウェブページのURIを設定・取得します。
        /// </summary>
        public Uri Source
        {
            get { return (Uri)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value);  }
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register(
            "Source",
            typeof(Uri),
            typeof(WebBrowser2),
            new PropertyMetadata(OnSourcePropertyChanged));

        private static void OnSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            WebBrowser2 source = d as WebBrowser2;
            if (source != null)
            {
                source.Navigate((Uri)e.NewValue);
            }
        }

        #endregion

        #region Event handlers

        public event EventHandler<Microsoft.Phone.Controls.NavigatingEventArgs> Navigating;
        public event EventHandler<System.Windows.Navigation.NavigationEventArgs> Navigated;
        public event EventHandler<System.Windows.Navigation.NavigationFailedEventArgs> NavigationFailed;
        public event EventHandler<System.Windows.Navigation.NavigationEventArgs> LoadCompleted;

        #endregion

        #region Internal event handlers

        private void TheWebBrowser_Navigating(object sender, Microsoft.Phone.Controls.NavigatingEventArgs e)
        {
            // 通常のナビゲーションをフック
            isNavigating = true;
            IsBusy = true;
            e.Cancel = true;
            /*
            if ("about".Equals(e.Uri.Scheme))
            {
                string url = mainSource.AbsoluteUri;
                if (mainSource.Query.Length > 0)
                {
                    url = url.Substring(0, url.LastIndexOf(mainSource.Query));
                }
                int lastSlash = url.LastIndexOf('/');
                if (lastSlash >= 0)
                {
                    url = url.Substring(0, lastSlash + 1);
                }
                url += e.Uri.AbsoluteUri.Substring(6);
                SubWebBrowser.Navigate(new Uri(url, UriKind.Absolute), null, "User-Agent: " + UserAgent);
            }
            else
            {
                SubWebBrowser.Navigate(e.Uri, null, "User-Agent: " + UserAgent);
            }
             */
            SubWebBrowser.Navigate(e.Uri, null, "User-Agent: " + UserAgent);
            SubWebBrowser.Visibility = Visibility.Visible;
            TheWebBrowser.Visibility = Visibility.Collapsed;
            inTheWebBrowser = false;
            return;
        }

        private void TheWebBrowser_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            IsBusy = false;

            // ハンドラー
            if (Navigated != null)
            {
                Navigated(sender, e);
            }
        }

        private void SubWebBrowser_Navigating(object sender, Microsoft.Phone.Controls.NavigatingEventArgs e)
        {
            // 通常のナビゲーションをフック
            string url = e.Uri.ToString();
            if (!isNavigating && !url.StartsWith("javascript:"))
            {
                isNavigating = true;
                numNavigates = 0;
                e.Cancel = true;
                (sender as Microsoft.Phone.Controls.WebBrowser).Navigate(e.Uri, null, "User-Agent: " + UserAgent);
                return;
            }

            // ブラウザーの戻る・進むに対するヒストリーの保持件数を調整
            if (url.StartsWith("javascript:"))
            {
                // 関数名と引数を取得
                string func = url.Substring(11);
                int leftBracketPos = func.IndexOf('(');
                int rightBracketPos = func.LastIndexOf(')');
                string funcName = "";
                string funcValue = "";
                if (leftBracketPos >= 0)
                {
                    funcName = func.Substring(0, leftBracketPos).Trim();
                    funcValue = func.Substring(leftBracketPos + 1, rightBracketPos - leftBracketPos - 1).Trim();
                }
                else
                {
                    funcName = func;
                }

                // 関数名による処理分岐
                if (funcName.Equals("history.back"))
                {
                    NavigationHistory history = historyGoBackStack.Pop();
                    CanGoBack = (historyGoBackStack.Count >= 1 ? true : false);
                }
                else if (funcName.Equals("history.go"))
                {
                    int funcValueInt = 0;
                    if (!string.IsNullOrEmpty(funcValue))
                    {
                        int.TryParse(funcValue, out funcValueInt);
                    }
                    if (funcValueInt == -1)
                    {
                        NavigationHistory history = historyGoBackStack.Pop();
                        CanGoBack = (historyGoBackStack.Count >= 1 ? true : false);
                    }
                }
                return;
            }

            IsBusy = true;
            numNavigates++;

            // ハンドラー
            if (Navigating != null)
            {
                Navigating(sender, e);
            }
        }

        private void SubWebBrowser_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            isNavigating = false;

            // ヒストリーの保持件数を調整
            if (e.NavigationMode == System.Windows.Navigation.NavigationMode.New)
            {
                bool toAddHistory = false;
                if (historyGoBackStack.Count == 0)
                {
                    toAddHistory = true;
                }
                else
                {
                    var lastHistory = historyGoBackStack.Peek();
                    if (!e.Uri.Equals(lastHistory.Uri))
                    {
                        toAddHistory = true;
                    }
                }
                if (toAddHistory)
                {
                    if (e.Uri.IsAbsoluteUri)
                    {
                        historyGoBackStack.Push(new NavigationHistory { Uri = e.Uri, NavigationCount = numNavigates });
                        CanGoBack = (historyGoBackStack.Count >= 1 ? true : false);
                    }
                }
            }

            // ハンドラー
            if (Navigated != null)
            {
                Navigated(sender, e);
            }
        }

        private void SubWebBrowser_NavigationFailed(object sender, System.Windows.Navigation.NavigationFailedEventArgs e)
        {
            // トゥームストーン状態から復帰すると発生する現象を回避
            string url = e.Uri.ToString();
            if (url.StartsWith("javascript:"))
            {
                return;
            }

            isNavigating = false;
            IsBusy = false;

            // ヒストリーの保持件数を調整
            historyGoBackStack.Push(new NavigationHistory { Uri = e.Uri, NavigationCount = numNavigates });
            CanGoBack = (historyGoBackStack.Count >= 1 ? true : false);

            // ハンドラー
            if (NavigationFailed != null)
            {
                NavigationFailed(sender, e);
            }
        }

        private void SubWebBrowser_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            IsBusy = false;

            // ブラウザー用の拡張JavaScriptを実行
            Thread thread = new Thread(new ThreadStart(setScript));
            thread.Start();

            // ハンドラー
            if (LoadCompleted != null)
            {
                LoadCompleted(sender, e);
            }
        }

        private void SubWebBrowser_ScriptNotify(object sender, Microsoft.Phone.Controls.NotifyEventArgs e)
        {
            // ブラウザー用の拡張JavaScriptからの通知を処理
            string[] values = e.Value.Split('\t');
            if (values.Length > 0)
            {
                // 処理種別によりフック
                string type = values[0];
                if (type.Equals("post"))
                {
                    // post: POST動作の発生をフック
                    string method = values[1];
                    string action = values[2];
                    string encType = values[3];
                    byte[] postData = Encoding.UTF8.GetBytes(values[4]);
                    isNavigating = true;
                    numNavigates = 0;
                    (sender as Microsoft.Phone.Controls.WebBrowser).Navigate(new Uri(action), postData, "User-Agent: " + UserAgent + "\nContent-Type: " + encType);
                }
            }
        }

        #endregion
    }
}
