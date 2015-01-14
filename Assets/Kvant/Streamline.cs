using UnityEngine;
using System.Collections;

namespace Kvant {

[ExecuteInEditMode]
[AddComponentMenu("Kvant/Streamline")]
public class Streamline : MonoBehaviour
{
    #region Private Settings

    const int bufferWidth = 512;
    const int bufferHeight = 48;

    #endregion

    #region Parameters Exposed To Editor

    [SerializeField] Color _color = new Color(1, 1, 1, 0.5f);
    [SerializeField] Vector3 _range = Vector3.one * 100;
    [SerializeField] Vector3 _velocity = Vector3.forward * -10;
    [SerializeField] float _noiseDensity = 0.5f;
    [SerializeField] float _noiseVelocity = 0.0f;
    [SerializeField] float _random = 0.5f;
    [SerializeField] float _life = 3;
    [SerializeField] bool _debug;

    #endregion

    #region Shader And Materials

    [SerializeField] Shader _deltaShader;
    [SerializeField] Shader _lineShader;
    [SerializeField] Shader _debugShader;

    Material _deltaMaterial;
    Material _lineMaterial;
    Material _debugMaterial;

    #endregion

    #region GPGPU Buffers

    RenderTexture _positionBuffer;
    RenderTexture _previousBuffer;

    #endregion

    #region Private Objects

    Mesh _mesh;
    bool _needsReset = true;

    #endregion

    #region Resource Management

    public void NotifyConfigChanged()
    {
        _needsReset = true;
    }

    RenderTexture CreateBuffer()
    {
        var buffer = new RenderTexture(bufferWidth, bufferHeight, 0, RenderTextureFormat.ARGBFloat);
        buffer.hideFlags = HideFlags.DontSave;
        buffer.filterMode = FilterMode.Point;
        buffer.wrapMode = TextureWrapMode.Repeat;
        return buffer;
    }

    Material CreateMaterial(Shader shader)
    {
        var material = new Material(shader);
        material.hideFlags = HideFlags.DontSave;
        return material;
    }

    Mesh CreateMesh()
    {
        // Create vertex arrays.
        var VA = new Vector3[bufferWidth * bufferHeight * 2];
        var TA = new Vector2[bufferWidth * bufferHeight * 2];

        var Ai = 0;
        for (var x = 0; x < bufferWidth; x++)
        {
            for (var y = 0; y < bufferHeight; y++)
            {
                VA[Ai + 0] = new Vector3(0, 0, 0);
                VA[Ai + 1] = new Vector3(1, 0, 0);

                var u = (float)x / bufferWidth;
                var v = (float)y / bufferHeight;
                TA[Ai] = TA[Ai + 1] = new Vector2(u, v);

                Ai += 2;
            }
        }

        // Index array.
        var IA = new int[VA.Length];
        for (Ai = 0; Ai < VA.Length; Ai++) IA[Ai] = Ai;

        // Create a mesh object.
        var mesh = new Mesh();
        mesh.vertices = VA;
        mesh.uv = TA;
        mesh.SetIndices(IA, MeshTopology.Lines, 0);
        mesh.Optimize();

        // Avoid being culled.
        mesh.bounds = new Bounds(Vector3.zero, _range);

        // This only for temporary use. Don't save.
        mesh.hideFlags = HideFlags.DontSave;

        return mesh;
    }

    void ResetResources()
    {
        // GPGPU buffers.
        if (_positionBuffer) DestroyImmediate(_positionBuffer);
        if (_previousBuffer) DestroyImmediate(_previousBuffer);

        _positionBuffer = CreateBuffer();
        _previousBuffer = CreateBuffer();

        // Shader materials.
        if (!_deltaMaterial) _deltaMaterial = CreateMaterial(_deltaShader);
        if (!_lineMaterial ) _lineMaterial  = CreateMaterial(_lineShader );
        if (!_debugMaterial) _debugMaterial = CreateMaterial(_debugShader);

        // Initialization.
        _deltaMaterial.SetVector("_Range", _range);
        _deltaMaterial.SetFloat("_Life", _life);
        Graphics.Blit(null, _previousBuffer, _deltaMaterial, 0);
        Graphics.Blit(null, _positionBuffer, _deltaMaterial, 0);

        // Mesh object.
        if (_mesh == null) _mesh = CreateMesh();

        _needsReset = false;
    }

    #endregion

    #region MonoBehaviour Functions

    void Reset()
    {
        _needsReset = true;
    }

    void Update()
    {
        if (_needsReset) ResetResources();

        // Swap the position buffers.
        {
            var temp = _previousBuffer;
            _previousBuffer = _positionBuffer;
            _positionBuffer = temp;
        }

        // Apply the delta shader.
        _deltaMaterial.SetVector("_Range", _range);
        _deltaMaterial.SetVector("_Velocity", _velocity);
        _deltaMaterial.SetFloat("_Random", _random);
        _deltaMaterial.SetFloat("_Life", _life);

        if (_noiseVelocity > 0)
        {
            _deltaMaterial.EnableKeyword("NOISE_ON");
            _deltaMaterial.SetVector("_Noise", new Vector2(_noiseDensity, _noiseVelocity));
        }
        else
        {
            _deltaMaterial.DisableKeyword("NOISE_ON");
        }

        Graphics.Blit(_previousBuffer, _positionBuffer, _deltaMaterial, 1);

        // Draw lines.
        _lineMaterial.SetColor("_Color", _color);
        _lineMaterial.SetTexture("_PreviousTex", _previousBuffer);
        _lineMaterial.SetTexture("_PositionTex", _positionBuffer);
        Graphics.DrawMesh(_mesh, transform.position, transform.rotation, _lineMaterial, 0);
    }

    void OnGUI()
    {
        if (_debug && Event.current.type.Equals(EventType.Repaint) && _debugMaterial)
        {
            var w = 64;
            var r1 = new Rect(0 * w, 0, w, w);
            var r2 = new Rect(1 * w, 0, w, w);
            if (_positionBuffer) Graphics.DrawTexture(r1, _positionBuffer, _debugMaterial);
            if (_previousBuffer) Graphics.DrawTexture(r2, _previousBuffer, _debugMaterial);
        }
    }

    #endregion
}

} // namespace Kvant
