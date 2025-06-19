using UnityEngine;

public static class ScreenshotUtil
{
    /// <summary>
    /// Renders the given camera into a RenderTexture, reads it back into a Texture2D,
    /// and returns it wrapped as a Sprite. Also disables the camera afterward.
    /// </summary>
    public static Sprite CaptureCameraView(Camera cam, int size = 256)
    {
        // 1. Backup camera settings
        var originalRT = cam.targetTexture;
        var originalClear = cam.clearFlags;
        var originalBG = cam.backgroundColor;
        var originalEnabled = cam.enabled;

        // 2. Setup a temporary RenderTexture
        var rt = new RenderTexture(size, size, 16);
        cam.targetTexture = rt;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.clear;
        cam.enabled = true;

        // 3. Render the camera and capture the image
        cam.Render();
        RenderTexture.active = rt;

        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.ReadPixels(new Rect(0, 0, size, size), 0, 0);
        tex.Apply();

        // 4. Restore camera settings
        cam.targetTexture = originalRT;
        cam.clearFlags = originalClear;
        cam.backgroundColor = originalBG;
        cam.enabled = originalEnabled;

        RenderTexture.active = null;
        Object.Destroy(rt);

        // 5. Return the captured image as a sprite
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }
}
