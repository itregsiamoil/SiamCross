﻿using SiamCross.Services;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;

namespace SiamCross.Models.Sensors.Du.Measurement
{
    public enum MeasureState
    {
          Ok = 0
        , IOError = 1
        , LogicError = 2
        , UnknownError = 10
    }
    public class DuMeasurementData
    {
        public UInt16 SrcFluidLevel { get; set; }
        public UInt16 SrcReflectionsCount { get; set; }

        public DuMeasurementSecondaryParameters SecondaryParameters => StartParam.SecondaryParameters;
        public byte[] Echogram { get; set; }

        public UInt16 FluidLevelMultipler
        {
            get
            {
                UInt16 level_multipler = 1;
                BitVector32 myBV = new BitVector32(SrcFluidLevel);
                if (true == myBV[0x4000])
                    level_multipler = 2;
                return level_multipler;
            }
        }

        public UInt16 FluidLevel 
        {
            get
            {
                BitVector32 myBV = new BitVector32(SrcFluidLevel);
                if(true == myBV[0x4000] )
                    myBV[0x4000] = false;
                UInt16 fluid_level = (ushort)(myBV.Data* FluidLevelMultipler);

                if (string.IsNullOrEmpty(StartParam.SecondaryParameters.SoundSpeed))
                {
                    var correctionTable = HandbookData.Instance.GetSoundSpeedList().Find(
                        x => x.ToString() == StartParam.SecondaryParameters.SoundSpeedCorrection);
                    StartParam.SecondaryParameters.SoundSpeed =
                        correctionTable.GetApproximatedSpeedFromTable(AnnularPressure).ToString();
                }
                fluid_level = (UInt16)(fluid_level *
                    float.Parse(StartParam.SecondaryParameters.SoundSpeed) / Constants.EhoFixedSoundSpeed);
                return fluid_level;
            }
        }
        public UInt16 NumberOfReflections 
        {
            get
            {
                UInt16 val = SrcReflectionsCount;
                const UInt16 mask = 0x000F;
                UInt16 dec = (UInt16)((((val >> 4)) & mask) * 10);
                UInt16 sig = (UInt16)(val & mask);
                int refect = dec + sig;
                return (refect > 99) ? (UInt16)99 : (UInt16)refect;
            }
        }
        public float AnnularPressure { get; set; }
        
        public DateTime Date { get; set; }
        MeasureState State { get; set; }
        public DuMeasurementStartParameters StartParam { get; }

        public DuMeasurementData(DateTime date
            , DuMeasurementStartParameters start_param
            , float annualPressure
            , UInt16 fluidLevel
            , UInt16 numberOfReflections
            , byte[] echogram
            , MeasureState state= MeasureState.UnknownError)
        {
            Date = date;
            StartParam = start_param;
            AnnularPressure = annualPressure;
            SrcFluidLevel = fluidLevel;
            SrcReflectionsCount = numberOfReflections;
            Echogram = echogram;
            State = state;
        }
    }
}
