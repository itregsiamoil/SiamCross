using SiamCross.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace SiamCross.Models.Sensors
{
    public interface ISurvey
    {
        ICommand CmdStart { get; set; }
        ICommand CmdCancel { get; set; }
        ICommand CmdUpdate{ get; set; }
    }
    public class BaseSurvey : BaseVM, ISurvey
    {
        public ICommand CmdStart { get; set; }
        public ICommand CmdCancel { get; set; }
        public ICommand CmdUpdate { get; set; }
    }
    public class BaseSurveyVM : BaseVM, ISurvey
    {
        public ISurvey Model { get; }
        public ICommand CmdStart { get; set; }
        public ICommand CmdCancel { get; set; }
        public ICommand CmdUpdate { get; set; }
        public BaseSurveyVM(ISurvey model)
        {
            Model = model;
            CmdStart = Model?.CmdStart;
            CmdCancel = Model?.CmdCancel;
            CmdUpdate = Model?.CmdUpdate;
        }
    }
}
