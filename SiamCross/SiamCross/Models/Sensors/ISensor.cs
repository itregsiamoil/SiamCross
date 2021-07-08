﻿using SiamCross.Models.Connection;
using SiamCross.Models.Connection.Protocol;
using SiamCross.Models.Scanners;
using SiamCross.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.CommunityToolkit.ObjectModel;

namespace SiamCross.Models.Sensors
{
    public class SensorModel : IDisposable
    {
        public readonly DeviceInfo Device;
        public readonly StatusModel Status = new StatusModel();

        public readonly List<ITask> OnConnectQueue = new List<ITask>();

        public readonly IProtocolConnection Connection;
        public readonly TaskManager Manager;
        public readonly ConnectionHolder ConnHolder;

        public readonly SensorPosition Position;
        public readonly List<ISurvey> Surveys;
        public readonly CommonInfo Info;
        public IStorage Storage { get; set; }
        public ISurveyCfg SurveyCfg { get; set; }

        public SensorModel(IProtocolConnection conn, DeviceInfo deviceInfo)
        {
            Device = deviceInfo;
            Connection = conn;
            Manager = new TaskManager();
            ConnHolder = new ConnectionHolder(Manager, Connection);

            Position = new SensorPosition(this);
            Surveys = new List<ISurvey>();
            Info = new CommonInfo();

            //OnConnectQueue.Add(SurveyCfg.TaskWait);
            //OnConnectQueue.Add(Position.TaskLoad);
            OnConnectQueue.Add(new TaskUpdateInfoSiam(this));

            Connection.PropertyChanged += OnConnectionChange;
        }
        async void OnConnectionChange(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                if (null == sender || "State" != e.PropertyName)
                    return;
                if (ConnectionState.Connected == Connection.State)
                    await OnConnect();
                else if (ConnectionState.Disconnected == Connection.State)
                    OnDisconnect();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("WARNING exception "
                + ex.Message + " "
                + ex.GetType() + " "
                + ex.StackTrace + "\n");
            }
        }
        void OnDisconnect()
        {
            Position.ResetSaved();
            SurveyCfg.ResetSaved();
        }
        async Task OnConnect()
        {
            if (null != SurveyCfg && null != SurveyCfg.TaskWait)
                await Manager.Execute(SurveyCfg.TaskWait);
            var result = await Manager.Execute(new TaskQueue(OnConnectQueue));
            if (JobStatus.Сomplete == result && ConnHolder.CmdUpdateStatus is AsyncCommand asyncCmd)
                await asyncCmd.ExecuteAsync();
            else
                await this.Connection.Disconnect();
        }

        public virtual async void Dispose()
        {
            await Connection.Disconnect();
            Connection.PropertyChanged -= OnConnectionChange;

            ConnHolder?.Dispose();
            Manager?.Dispose();
            Connection.Dispose();
        }
    }


    public interface ISensor : IDisposable, INotifyPropertyChanged
    {
        SensorModel Model { get; }

        IProtocolConnection Connection { get; }
        TaskManagerVM TaskManager { get; set; }
        BaseStorageVM StorageVM { get; set; }
        SurveysCollectionVM SurveysVM { get; }
        SensorPositionVM PositionVM { get; }
        //IViewModel FactoryConfigVM { get; set; }
        //IViewModel UserConfigVM { get; set; }
        //IViewModel StateVM { get; set; }
        ICommand ShowDetailViewCommand { get; set; }
        ICommand ShowSurveysViewCommand { get; set; }
        ICommand ShowInfoViewCommand { get; set; }

        string ConnStateStr { get; }
        bool Activate { get; set; }
        bool IsMeasurement { get; }
        bool IsEnableQickInfo { get; set; }
        float MeasureProgress { get; set; }
        Task<bool> QuickReport(CancellationToken cancelToken);
        Task StartMeasurement(object measurementParameters);
        ScannedDeviceInfo ScannedDeviceInfo { get; }

        string Name { get; }
        string Type { get; }
        string Number { get; }
        string Label { get; }
        string Firmware { get; }
        string Battery { get; }
        string Temperature { get; }
        string RadioFirmware { get; }
        string Status { get; set; }
        Guid Id { get; }

        bool IsNewStatus { get; }
        bool IsOldStatus { get; }
    }
}
