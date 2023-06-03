using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AlertManager : MonoBehaviour
{
    private Timer errorAlert = new(5000);
    private Timer warningAlert = new(5000);

    private bool laneChanged = false;
    private bool inverseLaneChanged = false;
    private bool outOfRoad = false;
    private bool semaphoreFail = false;
    private bool noMovement = false;
    public bool LaneChanged { get => laneChanged; set => laneChanged = value; }
    public bool InverseLaneChanged { get => inverseLaneChanged; set => inverseLaneChanged = value; }
    public bool OutOfRoad { get => outOfRoad; set => outOfRoad = value; }
    public bool SemaphoreFail { get => semaphoreFail; set => semaphoreFail = value; }
    public bool NoMovement { get => noMovement; set => noMovement = value; }

    private bool reload = false;
    private void Start()
    {
        errorAlert.AutoReset = false;
        errorAlert.Elapsed += (sender, args) => reload = true;

        warningAlert.AutoReset = false;
        warningAlert.Elapsed += (sender, args) => Debug.Log("Warning ended");
    }


    private void Update()
    {
        if (reload)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
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
        if (!InverseLaneChanged) {

            StartErrorTimer(ref inverseLaneChanged, "Está circulando en sentido contrario. Vuelva a su carril.");
        }
        else
        {
            StopErrorTimer(ref inverseLaneChanged, "Sentido OK");
        }
    }
    public void AlertOutOfRoad()
    {
        if (!OutOfRoad) { 

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
        return OutOfRoad || InverseLaneChanged;
    }

    private void StartErrorTimer(ref bool alert, string message)
    {
        if(!AnyAlertPending())
        {
            Debug.Log(message);
            errorAlert.Start();
        }
        alert = true;
    }

    private void StopErrorTimer(ref bool alert, string message) {
        Debug.Log("OK");
        alert = false;
        if (!AnyAlertPending())
            errorAlert.Stop();
    }

    private void StartWarningTimer(ref bool alert, string message)
    {
        Debug.Log(message);
        if(!warningAlert.Enabled)
            warningAlert.Start();
        alert = true;
    }

    private void StopWarningTimer(ref bool alert, string message)
    {
        Debug.Log("OK");
        alert = false;
        if (warningAlert.Enabled)
            warningAlert.Stop();
    }

}
