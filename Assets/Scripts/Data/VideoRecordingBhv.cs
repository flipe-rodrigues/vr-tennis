using UnityEngine;
using System.Diagnostics;
using System.IO;

public class VideoRecordingBhv : MonoBehaviour
{
    // Public fields
    public int samplingRate = 120;
    public bool recordVideo;

    // Read only fields
    [SerializeField, ReadOnly]
    private string _videoFilename;
    [SerializeField, ReadOnly]
    private float _samplingInterval;

    // Private fields
    private Camera _camera;
    private RenderTexture _renderTexture;
    private Texture2D frameTexture;
    private string _videoPath;
    private float _samplingTimer;
    private int _frameIndex;

    private Process ffmpeg;
    private Stream ffmpegStdin;
    private bool _hasStoppedRecording;

    private void OnValidate()
    {
        _samplingInterval = 1f / samplingRate;
    }

    private void Awake()
    {
        _camera = GetComponent<Camera>();

        _renderTexture = _camera.targetTexture;

        _videoFilename = DataManager.GetFilename(this.name, ".mp4");
        _videoPath = Path.Combine(DataManager.savePath, _videoFilename);
    }

    private void Start()
    {
        if (!recordVideo)
        {
            return;
        }

        _camera.enabled = false;

        frameTexture = new Texture2D(_renderTexture.width, _renderTexture.height, TextureFormat.RGB24, false);

        this.StartFFmpeg();
    }
    private void StartFFmpeg()
    {
        string args = $"-y -f rawvideo -vcodec rawvideo -pix_fmt rgb24 -s {_renderTexture.width}x{_renderTexture.height} -r {samplingRate} -i - -an -c:v libx264 -preset ultrafast -crf 18 -pix_fmt yuv420p \"{_videoPath}\"";

        var startInfo = new ProcessStartInfo
        {
            FileName = "ffmpeg",
            Arguments = args,
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            CreateNoWindow = true
        };

        ffmpeg = new Process();
        ffmpeg.StartInfo = startInfo;
        ffmpeg.Start();
        ffmpegStdin = ffmpeg.StandardInput.BaseStream;

        UnityEngine.Debug.Log("FFmpeg recording started.");
    }

    private void FixedUpdate()
    {
        if (!recordVideo)
        {
            return;
        }

        _samplingTimer += Time.fixedDeltaTime;

        if (_samplingTimer >= _samplingInterval)
        {
            this.RecordFrame2();

            _samplingTimer = 0f;
        }
    }

    private void RecordFrame()
    {
        // Render the camera manually
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = _renderTexture;
        _camera.Render();

        // Read the pixels
        frameTexture.ReadPixels(new Rect(0, 0, _renderTexture.width, _renderTexture.height), 0, 0);
        frameTexture.Apply();

        // Save as PNG or JPG
        byte[] bytes = frameTexture.EncodeToJPG(); // Or EncodeToPNG()
        string fileName = DataManager.GetFilename($"frame_{_frameIndex:D05}.jpg", ".jpg");
        string filePath = Path.Combine(_videoPath, fileName);
        File.WriteAllBytes(filePath, bytes);

        _frameIndex++;
        RenderTexture.active = currentRT;
    }

    private void RecordFrame2()
    {
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = _renderTexture;
        _camera.Render();

        frameTexture.ReadPixels(new Rect(0, 0, _renderTexture.width, _renderTexture.height), 0, 0);
        frameTexture.Apply();

        byte[] rawData = frameTexture.GetRawTextureData();
        ffmpegStdin.Write(rawData, 0, rawData.Length);

        _frameIndex++;
        RenderTexture.active = currentRT;
    }

    protected virtual void OnEnable()
    {
        ApplicationManager.onQuitStart += this.HandleQuitRequest;
    }

    protected virtual void OnDisable()
    {
        ApplicationManager.onQuitStart -= this.HandleQuitRequest;
    }

    private void HandleQuitRequest()
    {
        this.OnApplicationQuit();
    }

    private void OnApplicationQuit()
    {
        if (_hasStoppedRecording || !recordVideo)
        {
            return;
        }

        this.StopFFmpeg();
    }

    private void StopFFmpeg()
    {
        try
        {
            if (ffmpegStdin != null)
            {
                ffmpegStdin.Flush();
                ffmpegStdin.Close(); // Signals FFmpeg to finish
            }

            if (ffmpeg != null && !ffmpeg.HasExited)
            {
                ffmpeg.WaitForExit(); // Wait for FFmpeg to write the MP4 footer
            }

            ffmpeg?.Dispose();
            ffmpeg = null;

            _hasStoppedRecording = true;
        }
        catch (System.Exception ex)
        {
            UnityEngine.Debug.LogWarning($"FFmpeg stop error: {ex.Message}");
        }

        UnityEngine.Debug.Log("FFmpeg recording stopped.");
    }
}