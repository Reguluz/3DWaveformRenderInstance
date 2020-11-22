using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UI;

public class SpectrumAnalyze : MonoBehaviour
{
    [Range(1,5)]public int Width2 = 4;
    [Range(0,3)]public int Height2 = 0;
    [Header("Controller")] 
    public GameObject CameraPivot;
    public RawImage _spectrumUI;
    public float Tempo;
    [Min(1)]public float SpectrumScale;
    public ComputeShader ComputeShader;
    private AudioSource _audioSource;
    private ComputeBuffer _waveFormBuffer;
    private Vector3[][] _colorList;
    private Vector2Int _textureSize;
    private float _oldSection;
    private RenderTexture _spectrum;
    private float[] samples;
    private int _audioSamples;
    private float _audioLength;
    private float[] waveform;
    private void Start()
    {
        if (!_spectrum)
        {
            _spectrum = RenderTexture.GetTemporary((int)Mathf.Pow(2,Width2+6), (int)Mathf.Pow(2,Height2+6), 0, RenderTextureFormat.Default,RenderTextureReadWrite.Default);//Texture2D(width, height, TextureFormat.RGBA32, false);
            _spectrum.enableRandomWrite = true;
            _spectrum.Create();
        }

        
        // _spectrumUI.texture = _spectrum;
        Init();
        DrawSpectrum();
    }

    private void Init()
    {
        _audioSource = GetComponent<AudioSource>();
        _textureSize = new Vector2Int((int)Mathf.Pow(2,Width2+6), (int)Mathf.Pow(2,Height2+6));
        samples = new float[_audioSource.clip.samples];
        _audioSource.clip.GetData(samples, 0);
        _audioLength = _audioSource.clip.length;
        _audioSamples = _audioSource.clip.samples;
        _waveFormBuffer = new ComputeBuffer((int)(60 / Tempo * 2/_audioSource.clip.length * SpectrumScale * _audioSamples),4);
        // _colorList = new Vector3[_textureSize.x][];
        // for (int i = 0; i < _textureSize.x; i++)
        // {
        //     _colorList[i] = new Vector3[_textureSize.y];
        // }
    }
    private void Update()
    {
        DrawSpectrum();
    }

    private void OnDisable()
    {
        RenderTexture.ReleaseTemporary(_spectrum);
        _waveFormBuffer.Release();
    }

    public void DrawSpectrum()
    {
        // _spectrumUI.texture = _spectrum;
            PaintWaveformSpectrum(/*_audioSource.clip, 1, */_textureSize.x, _textureSize.y, Color.green, 60 / Tempo * 2/_audioSource.clip.length * SpectrumScale, CameraPivot.transform.position.z);
    }

    private void SpectrumRender()
    {
        
        
        // Debug.Log(colors.Count);
        _waveFormBuffer.SetData(waveform);
        int KernelHandle = ComputeShader.FindKernel("SpectrumRenderer");
        ComputeShader.SetTexture(KernelHandle, "Result",_spectrum);
        ComputeShader.SetBuffer(KernelHandle, "Waveform",_waveFormBuffer);
        ComputeShader.SetInt("TotalSample", (int)(60 / Tempo * 2/_audioSource.clip.length * SpectrumScale * _audioSamples));
        Debug.Log(16*(int)Mathf.Pow(2,Width2) +"  "+ 32*(int)Mathf.Pow(2,Height2));
        ComputeShader.Dispatch(KernelHandle, 32, 2, 1);
        
        _spectrumUI.material.SetTexture("_MainTex", _spectrum);
    }
    public void PaintWaveformSpectrum(/*AudioClip audio, float saturation,*/ int width, int height, Color col, float section, float time)
    {
        if (section > _audioLength) section = _audioLength;
        int packSize = (int) (section / _audioLength * _audioSamples) + 1;//( _audioSamples / width ) + 1;
        width = (int) (section * _audioSamples / packSize + 1);
        // if (!_spectrum)
        // {
        //     _spectrum = RenderTexture.GetTemporary(width, height, 0, GraphicsFormat.R16G16B16_SFloat, 1, RenderTextureMemoryless.Color);//Texture2D(width, height, TextureFormat.RGBA32, false);
        // }
        // float[] samples = new float[_audioSamples];
         waveform = new float[width];
        
        int s = 0;
        for (int i = (int)((time/_audioLength - section/2) * _audioSamples ); i < (int)((time/_audioLength + section/2) * _audioSamples ); i += packSize*100)
        {
            int temp = i < 0 ? 0 : i;
            // float avg = 0;
            // for (int j = temp - packSize/2; j < temp + packSize/2; j++)
            // {
            //     int temp2 = Mathf.Clamp(j + temp, 0, _audioSamples);
            //     avg += Mathf.Abs(samples[temp2]);
            // }
            //
            // avg /= packSize;
            Debug.Log(waveform.Length+"  "+s +"  "+ samples.Length+"  "+temp);
            waveform[s] = samples[temp];//Mathf.Abs(samples[temp]);
            s++;
        }
        
        // for (int x = 0; x < width; x++) {
        //     for (int y = 0; y < height; y++)
        //     {
        //         _colorList[x][y] = Vector3.zero;
        //         // _spectrum.SetPixel(x, y, Color.black);
        //     }
        // }
        //
        // for (int x = 0; x < waveform.Length; x++) {
        //     if (Mathf.Abs(x - (int)(waveform.Length / 2))>2)
        //     {
        //         for (int y = 0; y <= waveform[x] * ((float)height * .75f); y++)
        //         {
        //             _colorList[x][(height / 2) + y] = new Vector3(col.r, col.g, col.b);
        //             _colorList[x][(height / 2) - y] = new Vector3(col.r, col.g, col.b);
        //             // _spectrum.SetPixel(x, ( height / 2 ) + y, col);
        //             // _spectrum.SetPixel(x, ( height / 2 ) - y, col);
        //         } 
        //     }
        //     else
        //     {
        //         for (int y = 0; y <= waveform[x] * ((float)height * .75f); y++) {
        //             _colorList[x][(height / 2) + y] = new Vector3(1,0,0);
        //             _colorList[x][(height / 2) - y] = new Vector3(1,0,0);
        //             // _spectrum.SetPixel(x, ( height / 2 ) + y, Color.red);
        //             // _spectrum.SetPixel(x, ( height / 2 ) - y, Color.red);
        //         }
        //     }
        //     
        // }
        //
        SpectrumRender();
        // _spectrum.Apply();
    }
}
