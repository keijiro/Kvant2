using UnityEngine;
using System.Collections;

namespace Kvant {

[ExecuteInEditMode]
[DisallowMultipleComponent]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[AddComponentMenu("Kvant/Fractal Tunnel")]
public class Tunnel : MonoBehaviour
{
    #region Parameters Exposed To Editor

    [SerializeField] float _radius = 5;
    [SerializeField] float _height = 10;

    [SerializeField] int _slices = 40;
    [SerializeField] int _stacks = 40;

    [SerializeField] float _offset = 1;
    [SerializeField] int _repeat = 100;

    [SerializeField] int _density = 1;
    [SerializeField] float _bump = 1;
    [SerializeField] float _warp = 1;

    [SerializeField] Color _surfaceColor = Color.white;
    [SerializeField] Color _lineColor = Color.white;

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

    public int repeat {
        get { return _repeat; }
        set { _repeat = value; }
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

    #region Misc Variables

    bool needsReset = true;

    #endregion

    #region Resource Management

    public void NotifyConfigChanged()
    {
        needsReset = true;
    }

    void SanitizeParameters()
    {
        _slices = Mathf.Clamp(_slices, 8, 100);
        _stacks = Mathf.Clamp(_stacks, 8, 100);
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

        // Mesh filter.
        var meshFilter = GetComponent<MeshFilter>();
        if (meshFilter.sharedMesh) DestroyImmediate(meshFilter.sharedMesh);
        meshFilter.sharedMesh = Lattice.Build(_slices, _stacks);
        meshFilter.sharedMesh.hideFlags = HideFlags.DontSave;

        // Mesh renderer.
        renderer.sharedMaterials = new Material[3] {
            _surfaceMaterial1,
            _surfaceMaterial2,
            _lineMaterial
        };

        needsReset = false;
    }

    #endregion

    #region MonoBehaviour Functions

    void Awake()
    {
        GetComponent<MeshFilter>().sharedMesh = null;
        renderer.sharedMaterials = new Material[1] { null };
    }

    void Update()
    {
        if (needsReset) ResetResources();

        var sy = (float)(_stacks + 1) / _stacks;
        _constructMaterial.SetVector("_Size", new Vector2(_radius, _height * sy));
        _constructMaterial.SetVector("_Offset", new Vector2(0, _offset));
        _constructMaterial.SetVector("_Repeat", new Vector3(1, _repeat));
        _constructMaterial.SetVector("_Density", new Vector2(_density, _density * sy));
        _constructMaterial.SetVector("_Displace", new Vector3(_bump, _warp, _warp));

        _surfaceMaterial1.SetColor("_Color", _surfaceColor);
        _surfaceMaterial2.SetColor("_Color", _surfaceColor);
        _lineMaterial.SetColor("_Color", _lineColor);

        Graphics.Blit(null, _positionBuffer, _constructMaterial, 0);
        Graphics.Blit(_positionBuffer, _normalBuffer1, _constructMaterial, 1);
        Graphics.Blit(_positionBuffer, _normalBuffer2, _constructMaterial, 2);
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
