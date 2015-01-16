using UnityEngine;
using System.Collections;

namespace Kvant {

[ExecuteInEditMode]
[AddComponentMenu("Kvant/Spray")]
public partial class Spray : MonoBehaviour
{
    #region Parameters Exposed To Editor

    [SerializeField] Mesh[] _shapes;
    [SerializeField] int _maxParticles = 1000;

    [SerializeField] Vector3 _emitterPosition = Vector3.zero;
    [SerializeField] Vector3 _emitterSize = Vector3.zero;

    [SerializeField] float _life = 3.0f;
    [SerializeField] float _randomness = 0.5f;

    [SerializeField] Vector3 _velocity = Vector3.forward * -10.0f;
    [SerializeField] float _noiseVelocity = 0.0f;
    [SerializeField] float _noiseDensity = 0.5f;

    [SerializeField] Color _color = Color.white;

    [SerializeField] bool _debug;

    #endregion

    #region Shader And Materials

    [SerializeField] Shader _kernelShader;
    [SerializeField] Shader _surfaceShader;
    [SerializeField] Shader _debugShader;

    Material _kernelMaterial;
    Material _surfaceMaterial;
    Material _debugMaterial;

    #endregion

    #region GPGPU Buffers

    RenderTexture _positionBuffer1;
    RenderTexture _positionBuffer2;
    RenderTexture _rotationBuffer1;
    RenderTexture _rotationBuffer2;

    #endregion

    #region Private Objects

    BulkMesh _bulkMesh;
    bool _needsReset = true;

    #endregion

    #region Resource Management

    public void NotifyConfigChanged()
    {
        _needsReset = true;
    }

    RenderTexture CreateBuffer()
    {
        var width = _bulkMesh.copyCount;
        var height = _maxParticles / width + 1;
        var buffer = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBFloat);
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

    void ApplyKernelParameters()
    {
        _kernelMaterial.SetVector("_EmitterPos", _emitterPosition);
        _kernelMaterial.SetVector("_EmitterSize", _emitterSize);

        _kernelMaterial.SetFloat("_Life", _life);
        _kernelMaterial.SetFloat("_Randomness", _randomness);

        _kernelMaterial.SetVector("_Velocity", _velocity);
        _kernelMaterial.SetVector("_Noise", new Vector2(_noiseDensity, _noiseVelocity));
    }

    void ResetResources()
    {
        // Mesh object.
        if (_bulkMesh == null)
            _bulkMesh = new BulkMesh(_shapes);
        else
            _bulkMesh.Rebuild(_shapes);

        // GPGPU buffers.
        if (_positionBuffer1) DestroyImmediate(_positionBuffer1);
        if (_positionBuffer2) DestroyImmediate(_positionBuffer2);
        if (_rotationBuffer1) DestroyImmediate(_rotationBuffer1);
        if (_rotationBuffer2) DestroyImmediate(_rotationBuffer2);

        _positionBuffer1 = CreateBuffer();
        _positionBuffer2 = CreateBuffer();
        _rotationBuffer1 = CreateBuffer();
        _rotationBuffer2 = CreateBuffer();

        // Shader materials.
        if (!_kernelMaterial)  _kernelMaterial  = CreateMaterial(_kernelShader );
        if (!_surfaceMaterial) _surfaceMaterial = CreateMaterial(_surfaceShader);
        if (!_debugMaterial)   _debugMaterial   = CreateMaterial(_debugShader  );

        // Initialization.
        ApplyKernelParameters();
        Graphics.Blit(null, _positionBuffer2, _kernelMaterial, 0);
        Graphics.Blit(null, _rotationBuffer2, _kernelMaterial, 1);

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

        // Swap the buffers.
        {
            var temp = _positionBuffer1;
            _positionBuffer1 = _positionBuffer2;
            _positionBuffer2 = temp;

            temp = _rotationBuffer1;
            _rotationBuffer1 = _rotationBuffer2;
            _rotationBuffer2 = temp;
        }

        // Apply the kernel shaders.
        ApplyKernelParameters();

        Graphics.Blit(_positionBuffer1, _positionBuffer2, _kernelMaterial, 2);
        Graphics.Blit(_rotationBuffer1, _rotationBuffer2, _kernelMaterial, 3);

        // Draw the meshes.
        _surfaceMaterial.SetTexture("_PositionTex", _positionBuffer2);
        _surfaceMaterial.SetTexture("_RotationTex", _rotationBuffer2);
        _surfaceMaterial.SetColor("_Color", _color);

        var block = new MaterialPropertyBlock();

        for (var i = 0; i < _positionBuffer2.height; i++)
        {
            block.AddFloat("_BufferOffset", (float)i / _positionBuffer2.height);
            Graphics.DrawMesh(_bulkMesh.mesh, transform.position, transform.rotation, _surfaceMaterial, 0, null, 0, block);
        }
    }

    void OnGUI()
    {
        if (_debug && Event.current.type.Equals(EventType.Repaint) && _debugMaterial)
        {
            var w = 64;
            var r1 = new Rect(0 * w, 0, w, w);
            var r2 = new Rect(1 * w, 0, w, w);
            if (_positionBuffer1) Graphics.DrawTexture(r1, _positionBuffer1, _debugMaterial);
            if (_rotationBuffer1) Graphics.DrawTexture(r2, _rotationBuffer2, _debugMaterial);
        }
    }

    #endregion
}

} // namespace Kvant
