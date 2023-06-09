using System;
using TMPro;
using UnityEngine;

public class TimeController : MonoBehaviour
{
    [SerializeField] private Material _daySkybox;
    [SerializeField] private Material _nightSkybox;
    [SerializeField] private float _timeMultiplaier = 600;
    [SerializeField] private float _startHour = 12;
    [SerializeField] private TextMeshProUGUI _timeText;
    [SerializeField] private Light _sunLigth;
    [SerializeField] private float _sunriseHour = 7;
    [SerializeField] private float _sunsetHour = 20;
    [SerializeField] private Color _dayAmbientLight;
    [SerializeField] private Color _nightAmbientLight;
    [SerializeField] private AnimationCurve _lightChangeCurve;
    [SerializeField] private float _maxSunLightIntensity = 1f;
    [SerializeField] private Light _moonLight;
    [SerializeField] private float _maxMoonLightIntensity = 0.5f;

    public DateTime CurrentTime;
    private TimeSpan _sunriseTime;
    private TimeSpan _sunsetTime;

    // Start is called before the first frame update
    private void Start()
    {
        CurrentTime = DateTime.Now.Date + TimeSpan.FromHours(_startHour);

        _sunriseTime = TimeSpan.FromHours(_sunriseHour);
        _sunsetTime = TimeSpan.FromHours(_sunsetHour);
    }

    // Update is called once per frame
    private void Update()
    {
        UpdateTimeOfDay();
        RotateSun();
        UpdateLightSettings();
    }

    private void UpdateTimeOfDay()
    {
        CurrentTime = CurrentTime.AddSeconds(Time.deltaTime * _timeMultiplaier);
        if (_timeText != null)
        {
            _timeText.text = "Time: " + CurrentTime.ToString("HH:mm");
        }
    }

    private void RotateSun()
    {
        float sunLightRotation;
        double percentage;
        if (CurrentTime.TimeOfDay > _sunriseTime && CurrentTime.TimeOfDay < _sunsetTime)
        {
            TimeSpan sunriseToSunsetDuration = CalculateTimeDifference(_sunriseTime, _sunsetTime);
            TimeSpan timeSinceSunrise =
                CalculateTimeDifference(_sunriseTime, CurrentTime.TimeOfDay);

            percentage =
                timeSinceSunrise.TotalMinutes / sunriseToSunsetDuration.TotalMinutes;

            sunLightRotation = Mathf.Lerp(0, 180, (float)percentage);
            RenderSettings.skybox = _daySkybox;
        }
        else
        {
            TimeSpan sunsetToSunriseDuration = CalculateTimeDifference(_sunsetTime, _sunriseTime);
            TimeSpan timeSinceSunset = CalculateTimeDifference(_sunsetTime, CurrentTime.TimeOfDay);

            percentage = timeSinceSunset.TotalMinutes / sunsetToSunriseDuration.TotalMinutes;

            sunLightRotation = Mathf.Lerp(180, 360, (float)percentage);
            RenderSettings.skybox = _nightSkybox;
        }

        _sunLigth.transform.rotation = Quaternion.AngleAxis(sunLightRotation, Vector3.right);
    }

    private void UpdateLightSettings()
    {
        float dotProduct = Vector3.Dot(_sunLigth.transform.forward, Vector3.down);
        _sunLigth.intensity =
            Mathf.Lerp(0, _maxSunLightIntensity, _lightChangeCurve.Evaluate(dotProduct));
        _moonLight.intensity =
            Mathf.Lerp(_maxMoonLightIntensity, 0, _lightChangeCurve.Evaluate(dotProduct));
        RenderSettings.ambientLight = Color.Lerp(_nightAmbientLight, _dayAmbientLight,
            _lightChangeCurve.Evaluate(dotProduct));
    }

    private TimeSpan CalculateTimeDifference(TimeSpan fromTime, TimeSpan toTime)
    {
        TimeSpan difference = toTime - fromTime;
        if (difference.TotalSeconds < 0)
        {
            difference += TimeSpan.FromHours(24);
        }

        return difference;
    }
}
