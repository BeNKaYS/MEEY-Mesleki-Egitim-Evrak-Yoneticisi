using System;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Web.WebView2.Core;

namespace MEEY.Controls
{
    public partial class MetinEditorHost : UserControl
    {
        private CoreWebView2Environment? webViewEnvironment;

        public MetinEditorHost()
        {
            InitializeComponent();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                string webViewDataPath = Path.Combine(GetEditorDataRoot(), "WebView2");
                Directory.CreateDirectory(webViewDataPath);
                webViewEnvironment = await CoreWebView2Environment.CreateAsync(userDataFolder: webViewDataPath);
                await EditorWebView.EnsureCoreWebView2Async(webViewEnvironment);

                var assetsPath = ResolveAssetsPath();
                if (string.IsNullOrWhiteSpace(assetsPath))
                {
                    EditorWebView.NavigateToString(BuildMissingAssetsHtml());
                    return;
                }

                EditorWebView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                    "appassets",
                    assetsPath,
                    CoreWebView2HostResourceAccessKind.Allow);

                EditorWebView.NavigateToString(BuildEditorHtml());
            }
            catch (Exception ex)
            {
                EditorWebView.NavigateToString(BuildEditorInitErrorHtml(ex.Message));
            }
        }

        public static string GetEditorDataRoot()
        {
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string root = Path.Combine(localAppData, "MEEY");
            Directory.CreateDirectory(root);
            return root;
        }

        public static string GetEditorRecordsPath()
        {
            string path = Path.Combine(GetEditorDataRoot(), "Assets", "EditorKayitlari");
            Directory.CreateDirectory(path);
            return path;
        }

        private static string BuildEditorHtml()
        {
          var scriptTag = "<script src=\"https://appassets/tinymce/tinymce.min.js\"></script>";

            return $@"<!doctype html>
<html lang='tr'>
<head>
  <meta charset='utf-8' />
  <meta name='viewport' content='width=device-width, initial-scale=1' />
  {scriptTag}
  <style>
    html, body {{ margin:0; padding:0; height:100%; background:#f8f9fa; }}
    #editor {{ height: calc(100vh - 10px); margin: 5px; }}

    /* Custom CSS injected from C# to override TinyMCE content styles dynamically */
    #custom-style {{ display: none; }}
  </style>
</head>
<body>
  <textarea id='editor'></textarea>
  <script>
    if (window.tinymce) {{
      tinymce.init({{
        selector: '#editor',
        base_url: 'https://appassets/tinymce',
        license_key: 'gpl',
        height: '100%',
        language: 'tr',
        language_url: 'https://appassets/tinymce/langs/tr.js',
        plugins: 'anchor autolink charmap codesample emoticons link lists media searchreplace table visualblocks wordcount',
        toolbar: 'undo redo | blocks fontfamily fontsize | bold italic underline strikethrough | link media table | align lineheight | numlist bullist indent outdent | emoticons charmap | removeformat',
        menubar: 'file edit view insert format page tools table help',
        menu: {{
            page: {{ title: 'Sayfa', items: 'page_size | page_margins' }},
            file: {{ title: 'Dosya', items: 'save_html_item save_pdf_item save_doc_item | print' }},
            edit: {{ title: 'Düzenle', items: 'undo redo | cut copy paste | selectall | searchreplace' }},
            view: {{ title: 'Görünüm', items: 'code | visualblocks | preview fullscreen' }},
            insert: {{ title: 'Ekle', items: 'link media | charmap emoticons' }},
            format: {{ title: 'Biçim', items: 'bold italic underline strikethrough | formats | align | removeformat' }},
            tools: {{ title: 'Araçlar', items: 'wordcount' }},
            table: {{ title: 'Tablo', items: 'inserttable | cell row column | tableprops deletetable' }},
            help: {{ title: 'Yardım', items: 'help' }}
        }},
        setup: function (editor) {{
            // Sayfa Boyutu Alt Menüsü
            editor.ui.registry.addNestedMenuItem('page_size', {{
                text: 'Sayfa Boyutu',
                icon: 'document-properties',
                getSubmenuItems: function () {{
                    return [
                        {{
                            type: 'menuitem',
                            text: 'A3 Dikey',
                            icon: 'orientation',
                            onAction: function () {{
                                editor.getDoc().body.style.maxWidth = '297mm';
                                editor.getDoc().body.style.minHeight = '420mm';
                            }}
                        }},
                        {{
                            type: 'menuitem',
                            text: 'A3 Yatay',
                            icon: 'orientation',
                            onAction: function () {{
                                editor.getDoc().body.style.maxWidth = '420mm';
                                editor.getDoc().body.style.minHeight = '297mm';
                            }}
                        }},
                        {{
                            type: 'menuitem',
                            text: 'A4 Dikey',
                            icon: 'orientation',
                            onAction: function () {{
                                editor.getDoc().body.style.maxWidth = '210mm';
                                editor.getDoc().body.style.minHeight = '297mm';
                            }}
                        }},
                        {{
                            type: 'menuitem',
                            text: 'A4 Yatay',
                            icon: 'orientation',
                            onAction: function () {{
                                editor.getDoc().body.style.maxWidth = '297mm';
                                editor.getDoc().body.style.minHeight = '210mm';
                            }}
                        }},
                        {{
                            type: 'menuitem',
                            text: 'A5 Dikey',
                            icon: 'orientation',
                            onAction: function () {{
                                editor.getDoc().body.style.maxWidth = '148mm';
                                editor.getDoc().body.style.minHeight = '210mm';
                            }}
                        }},
                        {{
                            type: 'menuitem',
                            text: 'A5 Yatay',
                            icon: 'orientation',
                            onAction: function () {{
                                editor.getDoc().body.style.maxWidth = '210mm';
                                editor.getDoc().body.style.minHeight = '148mm';
                            }}
                        }}
                    ];
                }}
            }});

            // Kenar Boşlukları Alt Menüsü
            editor.ui.registry.addNestedMenuItem('page_margins', {{
                text: 'Kenar Boşlukları',
                icon: 'ltr',
                getSubmenuItems: function () {{
                    return [
                        {{
                            type: 'menuitem',
                            text: 'Normal',
                            icon: 'ltr',
                            onAction: function () {{
                                editor.getDoc().body.style.padding = '15px';
                            }}
                        }},
                        {{
                            type: 'menuitem',
                            text: 'Dar',
                            icon: 'ltr',
                            onAction: function () {{
                                editor.getDoc().body.style.padding = '5px';
                            }}
                        }},
                        {{
                            type: 'menuitem',
                            text: 'Geniş',
                            icon: 'ltr',
                            onAction: function () {{
                                editor.getDoc().body.style.padding = '30px';
                            }}
                        }}
                    ];
                }}
            }});

            editor.ui.registry.addMenuItem('save_html_item', {{
                text: 'HTML formatında kaydet',
                icon: 'sourcecode',
                onAction: function () {{
                    var html = '<!DOCTYPE html><html>' + editor.getDoc().documentElement.innerHTML + '</html>';
                    window.chrome.webview.postMessage(JSON.stringify({{ action: 'save_html', content: html }}));
                }}
            }});
            editor.ui.registry.addMenuItem('save_pdf_item', {{
                text: 'PDF formatında kaydet',
                icon: 'document-properties',
                onAction: function () {{
                    var html = '<!DOCTYPE html><html>' + editor.getDoc().documentElement.innerHTML + '</html>';
                    window.chrome.webview.postMessage(JSON.stringify({{ action: 'save_pdf', content: html }}));
                }}
            }});
            editor.ui.registry.addMenuItem('save_doc_item', {{
                text: 'Word (DOC) formatında kaydet',
                icon: 'document-properties',
                onAction: function () {{
                    var html = '<!DOCTYPE html><html><head><meta charset=""utf-8""></head><body>' + editor.getContent() + '</body></html>';
                    window.chrome.webview.postMessage(JSON.stringify({{ action: 'save_doc', content: html }}));
                }}
            }});
        }},
        content_style: 'body {{ max-width: 210mm; width: 100%; min-height: 297mm; margin: 10px auto !important; padding: 15px; background: white; box-shadow: 0 4px 8px rgba(0,0,0,0.1); box-sizing: border-box; }} html {{ background: #f0f2f5; overflow-x: auto; overflow-y: scroll; }} @media print {{ @page {{ size: portrait; margin: 5mm; }} body {{ margin: 0; padding: 0; box-shadow: none; width: auto; height: auto; }} html {{ background: white; }} }}'
      }});
    }}
  </script>
</body>
</html>";
        }

        public async System.Threading.Tasks.Task SetHtmlContentAsync(string htmlContent)
        {
            if (EditorWebView.CoreWebView2 == null) return;
            
            // Javascript string formatından kaçış karakterleri
            var escapedHtml = htmlContent
                .Replace("\\", "\\\\")
                .Replace("'", "\\'")
                .Replace("\r", "\\r")
                .Replace("\n", "\\n");

            var script = $@"
                if(window.tinymce && tinymce.activeEditor) {{
                    var htmlString = '{escapedHtml}';
                    
                    // HTML'i JS ortamında parse edip HEAD'deki stilleri toplayalım.
                    var parser = new DOMParser();
                    var tempDoc = parser.parseFromString(htmlString, 'text/html');
                    
                    var styles = tempDoc.querySelectorAll('style');
                    var styleText = '';
                    styles.forEach(function(s) {{ styleText += s.innerHTML + '\n'; }});
                    
                    // Sadece body içerisindeki elementleri content olarak alıyoruz
                    var bodyContent = tempDoc.body ? tempDoc.body.innerHTML : htmlString;
                    
                    var editorDoc = tinymce.activeEditor.getDoc();
                    var customStyle = editorDoc.getElementById('custom-style');
                    if (!customStyle) {{
                        customStyle = editorDoc.createElement('style');
                        customStyle.id = 'custom-style';
                        editorDoc.head.appendChild(customStyle);
                    }}
                    customStyle.innerHTML = styleText;

                    tinymce.activeEditor.setContent(bodyContent);
                }}";
            await EditorWebView.CoreWebView2.ExecuteScriptAsync(script);
        }

        public async System.Threading.Tasks.Task SetPageOrientationAsync(string maxWidth, string minHeight)
        {
            if (EditorWebView.CoreWebView2 == null) return;
            var script = $@"
                if(window.tinymce && tinymce.activeEditor) {{
                    var doc = tinymce.activeEditor.getDoc();
                    if(doc) {{
                        var body = doc.body;
                        body.style.maxWidth = '{maxWidth}';
                        body.style.minHeight = '{minHeight}';
                    }}
                }}
            ";
            await EditorWebView.CoreWebView2.ExecuteScriptAsync(script);
        }

        public async System.Threading.Tasks.Task SetPagePaddingAsync(string padding)
        {
            if (EditorWebView.CoreWebView2 == null) return;
            var script = $@"
                if(window.tinymce && tinymce.activeEditor) {{
                    var doc = tinymce.activeEditor.getDoc();
                    if(doc) {{
                        var body = doc.body;
                        body.style.padding = '{padding}';
                    }}
                }}
            ";
            await EditorWebView.CoreWebView2.ExecuteScriptAsync(script);
        }

        private async void EditorWebView_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                var message = e.TryGetWebMessageAsString();
                if (string.IsNullOrEmpty(message)) return;

                var json = JsonDocument.Parse(message);
                var action = json.RootElement.GetProperty("action").GetString();
                var content = json.RootElement.GetProperty("content").GetString();

                if (action == "save_html")
                {
                    SaveHtmlDialog(content);
                }
                else if (action == "save_doc")
                {
                    SaveDocDialog(content);
                }
                else if (action == "save_pdf")
                {
                    await SavePdfDialogAsync(content);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"İşlem hatası: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveHtmlDialog(string htmlContent)
        {
            var saveDir = GetEditorRecordsPath();

            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "HTML Dosyası (*.html)|*.html",
                DefaultExt = ".html",
                FileName = "YeniBelge.html",
                InitialDirectory = saveDir
            };

            if (dialog.ShowDialog() == true)
            {
                System.IO.File.WriteAllText(dialog.FileName, htmlContent);
                MessageBox.Show("HTML olarak başarıyla kaydedildi!", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void SaveDocDialog(string htmlContent)
        {
            var saveDir = GetEditorRecordsPath();

            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Word Dosyası (*.doc)|*.doc",
                DefaultExt = ".doc",
                FileName = "YeniBelge.doc",
                InitialDirectory = saveDir
            };

            if (dialog.ShowDialog() == true)
            {
                File.WriteAllText(dialog.FileName, htmlContent);
                MessageBox.Show("Word belgesi başarıyla kaydedildi.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async System.Threading.Tasks.Task SavePdfDialogAsync(string htmlContent)
        {
            var saveDir = GetEditorRecordsPath();

            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "PDF Dosyası (*.pdf)|*.pdf",
                DefaultExt = ".pdf",
                FileName = "YeniBelge.pdf",
                InitialDirectory = saveDir
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    if (webViewEnvironment == null)
                    {
                        string webViewDataPath = Path.Combine(GetEditorDataRoot(), "WebView2");
                        Directory.CreateDirectory(webViewDataPath);
                        webViewEnvironment = await CoreWebView2Environment.CreateAsync(userDataFolder: webViewDataPath);
                    }

                    await HiddenWebView.EnsureCoreWebView2Async(webViewEnvironment);
                    
                    bool isNavigated = false;
                    EventHandler<CoreWebView2NavigationCompletedEventArgs> navHandler = null;
                    navHandler = (s, args) => { isNavigated = true; HiddenWebView.CoreWebView2.NavigationCompleted -= navHandler; };
                    HiddenWebView.CoreWebView2.NavigationCompleted += navHandler;
                    
                    HiddenWebView.NavigateToString(htmlContent);
                    
                    int waitCounter = 0;
                    while (!isNavigated && waitCounter < 50)
                    {
                        await System.Threading.Tasks.Task.Delay(100);
                        waitCounter++;
                    }
                    
                    await System.Threading.Tasks.Task.Delay(500); // Wait for potential rendering 

                    var printSettings = HiddenWebView.CoreWebView2.Environment.CreatePrintSettings();
                    printSettings.ShouldPrintBackgrounds = true;
                    printSettings.ShouldPrintHeaderAndFooter = false;

                    await HiddenWebView.CoreWebView2.PrintToPdfAsync(dialog.FileName, printSettings);
                    MessageBox.Show("PDF dosyası başarıyla oluşturuldu ve kaydedildi.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"PDF kaydetme hatası: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

                private static string BuildEditorInitErrorHtml(string errorMessage)
                {
                        var safeMessage = System.Net.WebUtility.HtmlEncode(errorMessage);
                        return $@"<!doctype html>
<html lang='tr'>
<head>
    <meta charset='utf-8' />
    <style>
        body {{ margin:0; font-family: Segoe UI, sans-serif; background:#f8f9fa; }}
        .wrap {{ height:100vh; display:flex; align-items:center; justify-content:center; color:#2c3e50; }}
        .card {{ background:white; border:1px solid #d0d7de; border-radius:10px; padding:24px; max-width:760px; }}
        .err {{ color:#b42318; }}
    </style>
</head>
<body>
    <div class='wrap'>
        <div class='card'>
            <h3>Metin Editörü başlatılamadı</h3>
            <p>WebView2 başlatılırken bir hata oluştu.</p>
            <p class='err'><b>Detay:</b> {safeMessage}</p>
        </div>
    </div>
</body>
</html>";
                }

        private static string BuildMissingAssetsHtml()
        {
            return @"<!doctype html>
<html lang='tr'>
<head>
  <meta charset='utf-8' />
  <style>
    body { margin:0; font-family: Segoe UI, sans-serif; background:#f8f9fa; }
    .wrap { height:100vh; display:flex; align-items:center; justify-content:center; color:#2c3e50; }
    .card { background:white; border:1px solid #d0d7de; border-radius:10px; padding:24px; max-width:700px; }
  </style>
</head>
<body>
  <div class='wrap'>
    <div class='card'>
      <h3>TinyMCE offline dosyaları bulunamadı</h3>
      <p><b>Assets/tinymce</b> klasörü kurulum paketine dahil edilmelidir.</p>
    </div>
  </div>
</body>
</html>";
        }

        private static string ResolveAssetsPath()
        {
            var candidates = new[]
            {
                Path.Combine(AppContext.BaseDirectory, "Assets"),
                Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Assets")),
                Path.Combine(Environment.CurrentDirectory, "MEEY", "Assets"),
                Path.Combine(Environment.CurrentDirectory, "Assets")
            };

            foreach (var path in candidates)
            {
                var tinyMceFolder = Path.Combine(path, "tinymce");
                if (Directory.Exists(tinyMceFolder))
                {
                    return path;
                }
            }

            return string.Empty;
        }
    }
}
