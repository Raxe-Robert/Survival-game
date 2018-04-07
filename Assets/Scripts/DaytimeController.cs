﻿using UnityEngine;
using System.Collections;
using System;

public class DaytimeController : Photon.MonoBehaviour, IPunObservable
{
    public const int MINUTESPERDAY = 24 * 60; 
    
    [Range(0, 45)]
    [SerializeField] private float rotationCurveOffset = 0f;

    [Tooltip("Ticks per second, a tick is 1 MINUTE in gametime.")]
    [SerializeField] int tickRate = 1;

    [Header("Time Settings")]
    [Tooltip("Hours/Minutes/Seconds")]
    [SerializeField] private Vector3 initialStartTime = new Vector3(12, 0, 0);

    [Range(1, 10)] [SerializeField] private int dayStartHour = 7;
    [Range(16, 24)] [SerializeField] private int nightStartHour = 23;

    [Header("Lightning")]
    [SerializeField] private Light sunLight = null;
    [SerializeField] private float sunIntensityMax = 1f;
    [SerializeField] private float sunIntensityMin = 0f;
    [SerializeField] private float sunIntensityFadeSpeed = 1f;

    [SerializeField] private Light moonLight = null;
    [SerializeField] private float moonIntensityMax = 1f;
    [SerializeField] private float moonIntensityMin = 0f;
    [SerializeField] private float moonIntensityFadeSpeed = 1f;

    public delegate void TimeChangedHandler(TimeSpan currentTime);
    public static event TimeChangedHandler OnTimeChangedCallback;

    private TimeSpan currentTime;
    public TimeSpan CurrentTime
    {
        get { return currentTime; }
        private set
        {
            currentTime = value;
            TargetAngle = CalculateNextSunAngle();

            OnTimeChangedCallback?.Invoke(currentTime);
        }
    }
    /// <summary>
    /// Returns the duration of the dayTime in hours.
    /// </summary>
    public int DayDuration => nightStartHour - dayStartHour;
    /// <summary>
    /// Returns the duration of the nightTime in hours.
    /// </summary>
    public int NightDuration => 24 - DayDuration;

    private float currentSunAngle = 0f;
    public float CurrentSunAngle
    {
        get { return currentSunAngle; }
        private set { currentSunAngle = AngleMod(value); }
    }

    private float targetAngle = 0f;
    private float TargetAngle
    {
        get { return targetAngle; }
        set { targetAngle = AngleMod(value); }
    }

    private void Start()
    {
        CurrentTime = VectorToTimeSpan(initialStartTime);

        moonLight.transform.eulerAngles = new Vector3(90f, 0f, 0f);
    }

    private void OnEnable()
    {
        StartCoroutine(TimeProgression());
    }

    private void OnDisable()
    {
        StopCoroutine(TimeProgression());
    }

    private void FixedUpdate()
    {
        ApplyAngle();
    }

    private IEnumerator TimeProgression()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(1f);

        while (true)
        {
            if (PhotonNetwork.isMasterClient)
            {
                CurrentTime = CurrentTime.Add(TimeSpan.FromMinutes(tickRate));
            }
            yield return waitForSeconds;
        }
    }

    /// <summary>
    /// Calculates the sun position based on the <see cref="CurrentTime"/>, <seealso cref="dayStartHour"/> and <seealso cref="nightStartHour"/>.
    /// </summary>
    private float CalculateNextSunAngle()
    {
        float dayMinutes = CurrentTime.Hours > 0 ? CurrentTime.Hours * 60 + CurrentTime.Minutes : CurrentTime.Minutes;
        dayMinutes += tickRate; // Add the tickrate to the current amount of minutes to get the total of minutes on the next tick

        if (dayMinutes > MINUTESPERDAY)
            dayMinutes = Mathf.Abs(MINUTESPERDAY - dayMinutes);
        
        if (dayStartHour * 60 <= dayMinutes && nightStartHour * 60 > dayMinutes)
        {
            float dayPercentage = (dayMinutes - dayStartHour * 60) / (DayDuration * 60) * 100;

            return 180f / 100f * dayPercentage;
        }
        else
        {
            float nightPercentage = (dayMinutes - nightStartHour * 60) / (NightDuration * 60) * 100;

            // If we reached midnight we should calculate the percentage differently
            if (nightPercentage < 0)
                nightPercentage = (dayMinutes + (MINUTESPERDAY - nightStartHour * 60)) / (NightDuration * 60) * 100;
            
            return 180f / 100f * nightPercentage + 180;
        }
    }

    private void ApplyAngle()
    {
        CurrentSunAngle = AngleMod(Mathf.LerpAngle(CurrentSunAngle, TargetAngle, Time.deltaTime)); // Smooth the sun angle

        sunLight.transform.rotation = Quaternion.identity;
        sunLight.transform.Rotate(Vector3.forward, rotationCurveOffset);
        sunLight.transform.rotation *= Quaternion.AngleAxis(CurrentSunAngle, Vector3.right);

        // Adjusts the intensity of the sun based on the angle of the sun
        sunLight.intensity = CurrentSunAngle >= 0 && CurrentSunAngle <= 180 ? Mathf.Lerp(sunLight.intensity, sunIntensityMax, Time.deltaTime * sunIntensityFadeSpeed) : Mathf.Lerp(sunLight.intensity, sunIntensityMin, Time.deltaTime * sunIntensityFadeSpeed);
        
        // Adjusts the intensity of the moon based on the angle of the sun
        moonLight.intensity = CurrentSunAngle >= 10 && CurrentSunAngle <= 170 ? Mathf.Lerp(moonLight.intensity, moonIntensityMin, Time.deltaTime * moonIntensityFadeSpeed) : Mathf.Lerp(moonLight.intensity, moonIntensityMax, Time.deltaTime * moonIntensityFadeSpeed);

    }

    private TimeSpan VectorToTimeSpan(Vector3 vector)
    {
        return new TimeSpan((int)vector.x, (int)vector.y, (int)vector.z);
    }

    private float AngleMod(float angle)
    {
        return (angle % 360f + 360f) % 360f;
    }

    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            stream.SendNext(CurrentTime.Ticks);
        }
        else
        {
            CurrentTime = TimeSpan.FromTicks((long)stream.ReceiveNext());
        }
    }
}
