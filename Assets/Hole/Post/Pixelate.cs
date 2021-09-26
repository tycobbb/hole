using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(UnityEngine.Camera))]
public class Pixelate: MonoBehaviour {
    // -- constants --
    static readonly int sDensityId = Shader.PropertyToID("_Density");
    static readonly int sAspectRatioId = Shader.PropertyToID("_AspectRatio");

    // -- config --
    /// the pixelate material
    [SerializeField] Material mMaterial = null;

    /// the pixel density
    [SerializeField] int mDensity = 80;

    // -- lifecycle --
    void OnRenderImage(RenderTexture src, RenderTexture dst) {
        // determine pixel aspect ratio
        var aspect = new Vector2(1.0f, 1.0f);
        if (Screen.height > Screen.width) {
            aspect.x = (float)Screen.width / Screen.height;
        } else {
            aspect.y = (float)Screen.height / Screen.width;
        }

        // set uniforms
        mMaterial.SetInt(sDensityId, mDensity);
        mMaterial.SetVector(sAspectRatioId, aspect);

        // apply effect
        Graphics.Blit(src, dst, mMaterial);
    }
}