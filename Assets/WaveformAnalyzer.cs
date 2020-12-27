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
    [Range(5,12)]public int CountLevel;
    [Space] 
    [SerializeField] ComputeShader cs = default;
    [SerializeField] Material material = default;
    [SerializeField] Mesh mesh = default;
    private ComputeBuffer _waveformBuffer;
    private ComputeBuffer _waveformOutput;
    private AudioSource _audioSource;
    private NativeArray<WaveformTick> _dbList;
    private static readonly int MAXCount = 4096;
    private float[] _samples;
    private List<float> _totalSampleList;
    private int _count;
    private List<float> _samplesClip;
    private static readonly int
        waveformTicksId = Shader.PropertyToID("_WaveformTicks"),
        waveformOutputId = Shader.PropertyToID("_WaveformOutputs"),
        TimeId = Shader.PropertyToID("_Time");
    private void OnEnable()
    {
        _audioSource = GetComponent<AudioSource>();
        _dbList = new NativeArray<WaveformTick>(MAXCount, Allocator.Persistent);
        _waveformBuffer = new ComputeBuffer(MAXCount, Marshal.SizeOf(typeof(WaveformTick)));
        _waveformOutput = new ComputeBuffer(MAXCount, Marshal.SizeOf(typeof(WaveformBlock)));
        _samples = new float[_audioSource.clip.samples];
        AudioClip clip;
        (clip = _audioSource.clip).GetData(_samples, 0);
        _totalSampleList = new List<float>(_samples);
        _samplesClip = new List<float>();
    }

    private void Update()
    {
        _count = (int)Mathf.Pow(2, CountLevel);
        RefreshBuffer();
        RefreshShader();
        material.SetBuffer(waveformOutputId, _waveformOutput);
        var bounds = new Bounds(Vector3.zero, Vector3.one / 10);
        Graphics.DrawMeshInstancedProcedural(mesh, 0, material, bounds, _count);

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
        //当前起始位置时间
        float currentTime = _audioSource.time;
        //起始位置的采样序号
        int currentSample = (int)(currentTime * 44100);
        //获取往后的40960（或更少）的采样
        int sampleClipLength = Mathf.Min(_samples.Length - currentSample, MAXCount * 10);
        //原曲采样转换成列表
        
        _samplesClip.Clear();
        //原曲采样截取至截取列表
        _samplesClip = _totalSampleList.GetRange((int)(currentTime * 44100), sampleClipLength);
        _samplesClip[0] = 0;
        //list补全至全长
        while (_samplesClip.Count < MAXCount * 10)
        {
            _samplesClip.Add(0);
        }

        int reverseLevel = (int)Mathf.Pow(2, (12 - CountLevel));
        // Debug.Log(reverseLevel + " "+_count);
        for (int i = 0; i < _count; i++)
        {
            var intervalDb = _dbList[i];
            intervalDb.loudness = _samplesClip[reverseLevel * i * 10];
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
        int groups = Mathf.CeilToInt(MAXCount / 64f);
        cs.Dispatch(kernal, groups, 1, 1);
        
    }
}
