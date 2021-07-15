using SiamCross.Models.Connection;
using SiamCross.Models.Connection.Protocol;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.ObjectModel;

namespace SiamCross.Models.Sensors
{
    public class SensorModel : IDisposable
    {
        public readonly DeviceInfo Device;
        public readonly StatusModel Status = new StatusModel();

        public readonly IProtocolConnection Connection;
        public readonly TaskManager Manager;
        public readonly ConnectionHolder ConnHolder;

        public readonly SensorPosition Position;
        public readonly List<BaseSurveyModel> Surveys;
        public readonly CommonInfo Info;
        public BaseStorageModel Storage { get; set; }

        public ITask TaskWait { get; set; }
        public readonly List<ITask> OnConnectQueue = new List<ITask>();

        public SensorModel(IProtocolConnection conn, DeviceInfo deviceInfo)
        {
            Device = deviceInfo;
            Connection = conn;
            Manager = new TaskManager();
            ConnHolder = new ConnectionHolder(Manager, Connection);

            Position = new SensorPosition(this);
            Surveys = new List<BaseSurveyModel>();
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
            Position?.ResetSaved();
            foreach (var surveyModel in Surveys)
                surveyModel.Config?.ResetSaved();
        }
        async Task OnConnect()
        {
            if (null != TaskWait)
                await Manager.Execute(TaskWait);
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
}
