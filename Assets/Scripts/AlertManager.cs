using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Timers;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AlertManager : MonoBehaviour
{
    [SerializeField] private ErrorNotifcation errorNotification;
    [SerializeField] private WarningNotification warningNotification;

    private Stopwatch errorWatch = new();
    private Timer errorAlert = new(5000);
    private Timer warningAlert = new(5000);

    private bool laneChanged = false;
    private bool inverseLaneChanged = false;
    private bool outOfRoad = false;
    private bool semaphoreFail = false;
    private bool noMovement = false;
    private bool collision = false;
    public bool LaneChanged { get => laneChanged; set => laneChanged = value; }
    public bool InverseLaneChanged { get => inverseLaneChanged; set => inverseLaneChanged = value; }
    public bool OutOfRoad { get => outOfRoad; set => outOfRoad = value; }
    public bool SemaphoreFail { get => semaphoreFail; set => semaphoreFail = value; }
    public bool NoMovement { get => noMovement; set => noMovement = value; }

    public bool Collision { get => collision; set => collision = value; }

    private bool reload = false;
    private bool hide = false;
    private void Start()
    {
        errorAlert.AutoReset = false;
        errorAlert.Elapsed += (sender, args) => reload = true;

        warningAlert.AutoReset = false;
        warningAlert.Elapsed += WarningEnded;
    }

    private void WarningEnded(object sender, ElapsedEventArgs e)
    {
        UnityEngine.Debug.Log("Warning has ended");
        hide = true;


    }

    private void Update()
    {
        if (reload)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        if (hide)
        {
            hide = false;
            warningNotification.gameObject.SetActive(false);
        }

        if (errorAlert.Enabled)
            errorNotification.SetTimer((errorAlert.Interval - errorWatch.ElapsedMilliseconds) / 1000);
    }

    private void OnDisable()
    {
        errorAlert.Dispose();
    }

    public void AlertLaneChange()
    {
        if (!LaneChanged)
        {
            StartWarningTimer(ref laneChanged, "Se ha producido un cambio de carril. Vuelva a su carril.");
        }
        else
        {
            StopWarningTimer(ref laneChanged, "Carril OK");
        }
    }
    public void AlertInverseLaneChange()
    {
        if (!InverseLaneChanged)
        {

            StartErrorTimer(ref inverseLaneChanged, "Está circulando en sentido contrario. Vuelva a su carril.");
        }
        else
        {
            if (AnyAlertPending())
                StopErrorTimer(ref inverseLaneChanged, "Sentido OK");
        }
    }
    public void AlertOutOfRoad()
    {
        if (!OutOfRoad)
        {

            StartErrorTimer(ref outOfRoad, "Está circulando fuera de la carretera. Vuelva a la calzada.");
        }
    }

    public void AlertRightPath()
    {
        OutOfRoad = false;
        StopErrorTimer(ref outOfRoad, "Calzada OK");
    }

    public void AlertSemaphoreFail()
    {
        if (!SemaphoreFail)
        {
            StartErrorTimer(ref semaphoreFail, "Tienes que detenerte en los semaforos en rojo.");
        }
    }

    public void AlertCollision()
    {
        if (!Collision)
        {
            StartErrorTimer(ref collision, "Se ha producido una colisión.");
        }
    }

    public void AlertNoMovement()
    {
        if (!NoMovement)
        {
            StartWarningTimer(ref noMovement, "Debe mantener una velocidad estable con la circulación");
        }
        else
        {
            noMovement = false;
        }
    }
    private bool AnyAlertPending()
    {
        return OutOfRoad || InverseLaneChanged || SemaphoreFail || Collision;
    }

    private void StartErrorTimer(ref bool alert, string message)
    {
        errorNotification.gameObject.SetActive(true);
        errorNotification.SetErrorText(message);

        if (!AnyAlertPending())
        {
            errorWatch.Restart();
            errorAlert.Start();
        }
        alert = true;
    }

    private void StopErrorTimer(ref bool alert, string message)
    {

        alert = false;
        if (!AnyAlertPending())
        {
            errorWatch.Stop();
            errorAlert.Stop();
            errorNotification.gameObject.SetActive(false);
        }
    }

    private void StartWarningTimer(ref bool alert, string message)
    {
        warningNotification.gameObject.SetActive(true);
        warningNotification.SetWarningText(message);

        if (!warningAlert.Enabled)
            warningAlert.Start();
        alert = true;
    }

    private void StopWarningTimer(ref bool alert, string message)
    {

        alert = false;
        if (warningAlert.Enabled)
        {
            warningAlert.Stop();

        }
    }

}
