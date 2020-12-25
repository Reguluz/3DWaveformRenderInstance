using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Collections;
using UnityEngine;

public struct WaveformTick
{
    public float loudness;
    // public float time;
}
struct WaveformBlock
{
    float worldPosY;
    Vector3 color;
};
[RequireComponent(typeof(AudioSource))]
public class WaveformAnalyzer : MonoBehaviour
{
    [Range(0,4410)]public int Count;
    [Space] 
    [SerializeField] ComputeShader cs = default;
    [SerializeField] Material material = default;
    [SerializeField] Mesh mesh = default;
    private ComputeBuffer _waveformBuffer;
    private ComputeBuffer _waveformOutput;
    private AudioSource _audioSource;
    private NativeArray<WaveformTick> _dbList;
    private static int maxCount = 4410;

    private static readonly int
        waveformTicksId = Shader.PropertyToID("_WaveformTicks"),
        waveformOutputId = Shader.PropertyToID("_WaveformOutputs"),
        TimeId = Shader.PropertyToID("_Time");
    private void OnEnable()
    {
        _audioSource = GetComponent<AudioSource>();
        _dbList = new NativeArray<WaveformTick>(maxCount, Allocator.Persistent);
        _waveformBuffer = new ComputeBuffer(maxCount, Marshal.SizeOf(typeof(WaveformTick)));
        _waveformOutput = new ComputeBuffer(maxCount, Marshal.SizeOf(typeof(WaveformBlock)));
    }

    private void Update()
    {
        RefreshBuffer();
        RefreshShader();
        material.SetBuffer(waveformOutputId, _waveformOutput);
        var bounds = new Bounds(Vector3.zero, Vector3.one / 10);
        Graphics.DrawMeshInstancedProcedural(mesh, 0, material, bounds, Count);

    }

    private void OnDisable()
    {
        _dbList.Dispose();
        _waveformBuffer.Release();
        _waveformBuffer = null;
        _waveformOutput.Release();
        _waveformOutput = null;
    }

    void RefreshBuffer()
    {
        float intervalTime = _audioSource.time;
        float[] samples = new float[441000];
        _audioSource.clip.GetData(samples, (int)(intervalTime * 44100));
        for (int i = 0; i < maxCount; i++)
        {
            var intervalDb = _dbList[i];
            intervalDb.loudness = samples[100 * i];
            // intervalDb.time = intervalTime + (float)(10 * i) / 44100;
            _dbList[i] = intervalDb;
        }
        _waveformBuffer.SetData(_dbList);
    }

    void RefreshShader()
    {
        int kernal = cs.FindKernel("WaveformRenderer");
        cs.SetFloat(TimeId, _audioSource.time);
        cs.SetBuffer(kernal, waveformTicksId, _waveformBuffer);
        cs.SetBuffer(kernal, waveformOutputId, _waveformOutput);
        int groups = Mathf.CeilToInt(maxCount / 64f);
        cs.Dispatch(kernal, groups, 1, 1);
        
    }
}
