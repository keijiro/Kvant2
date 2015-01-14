using UnityEngine;
using System.Collections;

namespace Kvant {

[ExecuteInEditMode]
[AddComponentMenu("Kvant/Tunnel")]
public class Tunnel : MonoBehaviour
{
    #region Parameters Exposed To Editor

    [SerializeField] float _radius = 5;
    [SerializeField] float _height = 20;

    [SerializeField] int _slices = 8;
    [SerializeField] int _stacks = 10;

    [SerializeField] float _offset = 0;
    [SerializeField] float _twist = 0;

    [SerializeField] int _density = 4;
    [SerializeField] float _bump = 0;
    [SerializeField] float _warp = 0;

    [SerializeField] Color _surfaceColor = new Color(0.8f, 0.8f, 0.8f, 1);
    [SerializeField] Color _lineColor = new Color(1, 1, 1, 0);

    [SerializeField] bool _debug;

    #endregion

    #region Public Properties

    public float radius {
        get { return _radius; }
        set { _radius = value; }
    }

    public float height {
        get { return _height; }
        set { _height = value; }
    }

    public int slices { get { return _slices; } }
    public int stacks { get { return _stacks; } }

    public float offset {
        get { return _offset; }
        set { _offset = value; }
    }

    public float twist {
        get { return _twist; }
        set { _twist = value; }
    }

    public int density {
        get { return _density; }
        set { _density = value; }
    }

    public float bump {
        get { return _bump; }
        set { _bump = value; }
    }

    public float warp {
        get { return _warp; }
        set { _warp = value; }
    }

    public Color surfaceColor {
        get { return _surfaceColor; }
        set { _surfaceColor = value; }
    }

    public Color lineColor {
        get { return _lineColor; }
        set { _lineColor = value; }
    }

    #endregion

    #region Shader And Materials

    [SerializeField] Shader _constructShader;
    [SerializeField] Shader _surfaceShader;
    [SerializeField] Shader _lineShader;
    [SerializeField] Shader _debugShader;

    Material _constructMaterial;
    Material _surfaceMaterial1;
    Material _surfaceMaterial2;
    Material _lineMaterial;
    Material _debugMaterial;

    #endregion

    #region GPGPU Buffers

    RenderTexture _positionBuffer;
    RenderTexture _normalBuffer1;
    RenderTexture _normalBuffer2;

    #endregion

    #region Private Objects

    Lattice _lattice;
    bool _needsReset = true;

    #endregion

    #region Resource Management

    public void NotifyConfigChanged()
    {
        _needsReset = true;
    }

    void SanitizeParameters()
    {
        _slices = Mathf.Clamp(_slices, 8, 255);
        _stacks = Mathf.Clamp(_stacks, 8, 1023);
    }

    RenderTexture CreateBuffer()
    {
        var buffer = new RenderTexture(_slices * 2, _stacks + 1, 0, RenderTextureFormat.ARGBFloat);
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

    void ResetResources()
    {
        SanitizeParameters();

        // GPGPU buffers.
        if (_positionBuffer) DestroyImmediate(_positionBuffer);
        if (_normalBuffer1 ) DestroyImmediate(_normalBuffer1 );
        if (_normalBuffer2 ) DestroyImmediate(_normalBuffer2 );

        _positionBuffer = CreateBuffer();
        _normalBuffer1  = CreateBuffer();
        _normalBuffer2  = CreateBuffer();

        // Shader materials.
        if (!_constructMaterial) _constructMaterial = CreateMaterial(_constructShader);
        if (!_surfaceMaterial1 ) _surfaceMaterial1  = CreateMaterial(_surfaceShader  );
        if (!_surfaceMaterial2 ) _surfaceMaterial2  = CreateMaterial(_surfaceShader  );
        if (!_lineMaterial     ) _lineMaterial      = CreateMaterial(_lineShader     );
        if (!_debugMaterial    ) _debugMaterial     = CreateMaterial(_debugShader    );

        _surfaceMaterial1.SetTexture("_PositionTex", _positionBuffer);
        _surfaceMaterial2.SetTexture("_PositionTex", _positionBuffer);
        _lineMaterial    .SetTexture("_PositionTex", _positionBuffer);

        _surfaceMaterial1.SetTexture("_NormalTex", _normalBuffer1);
        _surfaceMaterial2.SetTexture("_NormalTex", _normalBuffer2);

        // Mesh.
        if (_lattice == null)
            _lattice = new Lattice(_slices, _stacks);
        else
            _lattice.Rebuild(_slices, _stacks);

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

        var height = _height * (_stacks + 1) / _stacks;
        var vdensity = _density / (Mathf.PI * 2 * _radius);

        _constructMaterial.SetVector("_Size", new Vector2(_radius, height));
        _constructMaterial.SetVector("_Offset", new Vector2(_twist * density, _offset * vdensity));
        _constructMaterial.SetVector("_Period", new Vector3(1, 10000));
        _constructMaterial.SetVector("_Density", new Vector2(_density, vdensity * height));
        _constructMaterial.SetVector("_Displace", new Vector3(_bump, _warp, _warp));

        _surfaceMaterial1.SetColor("_Color", _surfaceColor);
        _surfaceMaterial2.SetColor("_Color", _surfaceColor);
        _lineMaterial.SetColor("_Color", _lineColor);

        Graphics.Blit(null, _positionBuffer, _constructMaterial, 0);
        Graphics.Blit(_positionBuffer, _normalBuffer1, _constructMaterial, 1);
        Graphics.Blit(_positionBuffer, _normalBuffer2, _constructMaterial, 2);

        foreach (var mesh in _lattice.meshes)
        {
            Graphics.DrawMesh(mesh, transform.position, transform.rotation, _surfaceMaterial1, 0, null, 0);
            Graphics.DrawMesh(mesh, transform.position, transform.rotation, _surfaceMaterial2, 0, null, 1);
            if (_lineColor.a > 0.0f)
                Graphics.DrawMesh(mesh, transform.position, transform.rotation, _lineMaterial, 0, null, 2);
        }
    }

    void OnGUI()
    {
        if (_debug && Event.current.type.Equals(EventType.Repaint) && _debugMaterial)
        {
            var w = 64;
            var r1 = new Rect(0 * w, 0, w, w);
            var r2 = new Rect(1 * w, 0, w, w);
            var r3 = new Rect(2 * w, 0, w, w);
            if (_positionBuffer) Graphics.DrawTexture(r1, _positionBuffer, _debugMaterial);
            if (_normalBuffer1 ) Graphics.DrawTexture(r2, _normalBuffer1,  _debugMaterial);
            if (_normalBuffer2 ) Graphics.DrawTexture(r3, _normalBuffer2,  _debugMaterial);
        }
    }

    #endregion
}

} // namespace Kvant
