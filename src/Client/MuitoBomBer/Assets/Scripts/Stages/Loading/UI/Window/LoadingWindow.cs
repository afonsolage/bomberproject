using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingWindow : UIComponent
{
    public UILabel _version;

    public UISlider _progressSlider;
    public UILabel _progressLabel;

    private float _targetValue = 0f;
    private bool _animationBar = false;

    public enum LoadingStatus
    {
        NONE,
        LOADING_PATCH_DATA,
        CHECKING_PATCH_DETAILS,
        CHECKING_PATCH_DATA,

        CHECKING_DOWNLOAD_PATCH_DATA,
        DOWNLOADING_PATCH_DATA,

        DONE,

        MAX
    }

    private LoadingStatus _status = LoadingStatus.NONE;

    void Update()
    {
        if(_animationBar)
        {
            _progressSlider.value = Mathf.Lerp(_progressSlider.value, _targetValue, Time.deltaTime * 5f);
        
            if (Mathf.Abs(_progressSlider.value - _targetValue) < 0.0002)
            {
                _progressSlider.value = _targetValue;
                _animationBar = false;
            }
        }
    }

    public void SetVersion(string version)
    {
        _version.gameObject.SetActive(true);
        _version.text = string.Format("Version: {0}", version);
    }

    public void SetStatus(LoadingStatus status)
    {
        if(!_progressLabel.gameObject.activeSelf) _progressLabel.gameObject.SetActive(true);

        switch(status)
        {
            case LoadingStatus.LOADING_PATCH_DATA:
                {
                    _progressSlider.value = 0f;
                    _progressLabel.text = GetStatusString(status);
                }
                break;
            case LoadingStatus.CHECKING_PATCH_DETAILS:
                {
                    _targetValue = 0.5f;
                    _animationBar = true;
                    _progressLabel.text = GetStatusString(status);
                }
                break;
            case LoadingStatus.CHECKING_PATCH_DATA:
                {
                    _targetValue = 1.0f;
                    _animationBar = true;
                    _progressLabel.text = GetStatusString(status);
                }
                break;
            case LoadingStatus.CHECKING_DOWNLOAD_PATCH_DATA:
                {
                    _targetValue = 1f;
                    _animationBar = true;
                    _progressLabel.text = GetStatusString(status);
                }
                break;
            case LoadingStatus.DONE:
                {
                    _progressSlider.value = 1f;
                    _progressLabel.text = GetStatusString(status);
                }
                break;
        }

        _status = status;
    }

    private string GetStatusString(LoadingStatus status)
    {
        switch (status)
        {
            case LoadingStatus.LOADING_PATCH_DATA:              return "Loading Patch Data...";
            case LoadingStatus.CHECKING_PATCH_DETAILS:          return "Checking out Patch Details...";
            case LoadingStatus.CHECKING_PATCH_DATA:             return "Checking out Patch Data...";
            case LoadingStatus.CHECKING_DOWNLOAD_PATCH_DATA:    return "Starting to download the new updates...";
            case LoadingStatus.DONE:                            return "Done...";
            default:                                            return "";
        }
    }

    public void SetDownloadStatus(float value, float maxValue)
    {
        float percent = (value / maxValue);

        if (percent < _progressSlider.value)
        {
            _progressSlider.value = percent;
        }
        else
        {
            _animationBar = true;
            _targetValue = percent;

            _progressSlider.value = percent;
        }

        string currentSize = Utils.SizeSuffix((long)value);
        string maxSize = Utils.SizeSuffix((long)maxValue);

        _progressLabel.text = string.Format("Downloading files...{0:0.00}% [sub][{1}/{2}][/sub]", percent * 100f, currentSize, maxSize);
    }
}
