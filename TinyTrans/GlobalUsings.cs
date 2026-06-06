// Resolve ambiguities between System.Windows (WPF) and System.Windows.Forms
global using Application = System.Windows.Application;
global using Brushes = System.Windows.Media.Brushes;
global using Clipboard = System.Windows.Clipboard;
global using KeyEventArgs = System.Windows.Input.KeyEventArgs;
global using TextBox = System.Windows.Controls.TextBox;
global using Visibility = System.Windows.Visibility;

// Missing implicit using for HttpClient
global using System.Net.Http;
