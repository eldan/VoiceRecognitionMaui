using CommunityToolkit.Maui.Media;
using CommunityToolkit.Maui.Alerts;
using System.Globalization;

namespace VoiceRecognitionMaui
{
  public partial class MainPage : ContentPage
  {
    private readonly ISpeechToText _speechToText;
    private bool _isListening = false;
    private CancellationTokenSource? _cancellationTokenSource;
    //private static Dictionary<string, string> LanguageCodes = new()
    //{
    //  { "English (United States)", "en-US" },
    //  { "Spanish (Spain)", "es-ES" },
    //  { "French (France)", "fr-FR" },
    //  { "German (Germany)", "de-DE" },
    //  { "Chinese (Simplified)", "zh-CN" },
    //  { "Japanese (Japan)", "ja-JP" },
    //  { "Korean (South Korea)", "ko-KR" },
    //  { "Portuguese (Brazil)", "pt-BR" },
    //  { "Russian (Russia)", "ru-RU" },
    //  { "Italian (Italy)", "it-IT" },
    //  { "Hebrew (Israel)", "he-IL" }
    //};
    private string _selectedLanguage = "fr-FR";

    public MainPage()
    {
      InitializeComponent();
      _speechToText = SpeechToText.Default;
      btnRecord.Clicked += OnRecordButtonClicked;
    }

    private async void OnRecordButtonClicked(object? sender, EventArgs e)
    {
      if (_isListening)
      {
        await StopListening();
      }
      else
      {
        await StartListening();
      }
    }

    private async Task StartListening()
    {
      try
      {
        var isGranted = await CheckAndRequestMicrophonePermission();
        if (!isGranted)
        {
          await DisplayAlert("Permission Required", "Microphone permission is required for voice recognition.", "OK");
          return;
        }

        _cancellationTokenSource = new CancellationTokenSource();
        _isListening = true;
        btnRecord.Text = "Stop Recording";
        lblRecognition.Text = $"Listening in {_selectedLanguage}...";

        var culture = new CultureInfo(_selectedLanguage);
        
        System.Diagnostics.Debug.WriteLine($"=== SPEECH RECOGNITION DEBUG ===");
        System.Diagnostics.Debug.WriteLine($"Selected language: {_selectedLanguage}");
        System.Diagnostics.Debug.WriteLine($"Culture created: {culture.Name}");
        System.Diagnostics.Debug.WriteLine($"Culture display name: {culture.DisplayName}");
        System.Diagnostics.Debug.WriteLine($"Platform: {DeviceInfo.Platform}");
        System.Diagnostics.Debug.WriteLine($"================================");

        var recognitionResult = await _speechToText.ListenAsync(
          culture,
          new Progress<string>(partialText =>
          {
            lblRecognition.Text = partialText;
          }),
          _cancellationTokenSource.Token);

        if (recognitionResult.IsSuccessful)
        {
          lblRecognition.Text = $"[{_selectedLanguage}] {recognitionResult.Text}";
          System.Diagnostics.Debug.WriteLine($"Result: {recognitionResult.Text}");
        }
        else
        {
          lblRecognition.Text = $"Error: {recognitionResult.Exception?.Message ?? "Recognition failed"}";
        }
      }
      catch (Exception ex)
      {
        lblRecognition.Text = $"Error: {ex.Message}";
        System.Diagnostics.Debug.WriteLine($"Exception: {ex}");
      }
      finally
      {
        _isListening = false;
        btnRecord.Text = "Start Recording";
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null;
      }
    }

    private async Task StopListening()
    {
      _cancellationTokenSource?.Cancel();
      await Task.Delay(100);
    }

    private async Task<bool> CheckAndRequestMicrophonePermission()
    {
      var status = await Permissions.CheckStatusAsync<Permissions.Microphone>();

      if (status == PermissionStatus.Granted)
        return true;

      if (status == PermissionStatus.Denied && DeviceInfo.Platform == DevicePlatform.iOS)
      {
        return false;
      }

      status = await Permissions.RequestAsync<Permissions.Microphone>();

      return status == PermissionStatus.Granted;
    }
  }
}
