using SiamCross.DataBase.DataBaseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Xamarin.Forms.Internals;

namespace SiamCross.Models.Tools
{
    public class XmlCreator
    {
        public XDocument CreateDdim2Xml(Ddim2Measurement dbDdimModel)
        {
            #region Setup

            string name = "";
            string number = "";
            foreach (char ch in dbDdimModel.Name)
            {
                if (ch > 47 || ch < 58)
                {
                    name += ch;
                }
                else
                {
                    number += ch;
                }
            }

            number = dbDdimModel.Name.Substring(dbDdimModel.Name.Length - 4);
            if (number[0] == '0')
            {
                number.Substring(number.Length - 1);
            }

            List<double> movement = new List<double>();
            List<double> weight = new List<double>();
            var discrets = DgmConverter.GetXYs(
                dbDdimModel.DynGraph.ToList(),
                dbDdimModel.Step,
                dbDdimModel.WeightDiscr);
            for (int i = 0; i < discrets.GetUpperBound(0); i++)
            {
                movement.Add(discrets[i, 0]); 
                weight.Add(discrets[i, 1]);
            }

            var maxStaticW = weight.Max();
            var minStaticW = weight.Min();

            var month = dbDdimModel.DateTime.Date.Month.ToString();
            if (month.Length < 2)
            {
                month = "0" + month;
            }

            var day = dbDdimModel.DateTime.Date.Day.ToString();
            if (day.Length < 2)
            {
                day = "0" + day;
            }

            string date = dbDdimModel.DateTime.Date.Year.ToString() + "-" +
                month + "-" + day;
            string time = dbDdimModel.DateTime.TimeOfDay.ToString().Split('.')[0];

            string field = "";
            var tempField = dbDdimModel.Field.Split(':');
            if(tempField.Length > 1)
            {
                if(tempField[1].Length > 1)
                {
                    field = tempField[1].Remove(0, 1);
                }
            }

            #endregion

            XDocument document =
                new XDocument(
                new XElement("Device_List",
                    new XElement("Device",

                        new XAttribute("DEVTID", "siddos01"),
                        new XAttribute("DSTID", "ДДИМ-2"),
                        new XAttribute("DEVSERIALNUMBER", number),

                            new XElement("Measurement_List",
                                new XElement("Measurement",
                                    new XElement("Header",
                                        new XAttribute("MESTYPEID", "dynamogram"),
                                        new XAttribute("MESSTARTDATE", date + "T" + time),
                                        new XAttribute("MESDEVICEOPERATORID", "0"),                                             //?
                                        new XAttribute("MESDEVICEFIELDID", field),
                                        new XAttribute("MESDEVICEWELLCLUSTERID", dbDdimModel.Bush.ToString()),
                                        new XAttribute("MESDEVICEWELLID", dbDdimModel.Well.ToString()),
                                        new XAttribute("MESDEVICEDEPARTMENTID", dbDdimModel.Shop.ToString()),
                                        new XAttribute("MESDEVICEBUFFERPRESSUREID", dbDdimModel.BufferPressure.ToString())),

                                    new XElement("Value_List",

                                        new XElement("Value",
                                            new XAttribute("MSVDICTIONARYID", "dynmovement"),
                                            new XAttribute("MSVDATA", BinaryToBase64(Trim(movement.ToArray(), dbDdimModel.Period), 10))),                                    //!

                                        new XElement("Value",
                                            new XAttribute("MSVDICTIONARYID", "dynburden"),
                                            new XAttribute("MSVDATA", BinaryToBase64(Trim(weight.ToArray(), dbDdimModel.Period), 1000))),                                    //!

                                        new XElement("Value",
                                            new XAttribute("MSVINTEGER", "2"),                                      //Межтраверсный
                                            new XAttribute("MSVDICTIONARYID", "sidsensortype")),

                                         new XElement("Value",
                                            new XAttribute("MSVINTEGER", "1"),                                      //? 1 => ddim/ddin?
                                            new XAttribute("MSVDICTIONARYID", "sidsensorplacemanttype")),

                                        new XElement("Value",
                                            new XAttribute("MSVINTEGER", "1"),                                      // copy from siamService; for ddim cycle == 1
                                            new XAttribute("MSVDICTIONARYID", "sidskippedcyclecount")),

                                        new XElement("Value",
                                            new XAttribute("MSVDOUBLE", dbDdimModel.TimeDiscr.ToString()),          //! возможно нужно поделить или умножить на 1К
                                            new XAttribute("MSVDICTIONARYID", "sidtimediscrete")),

                                        new XElement("Value",
                                            new XAttribute("MSVDOUBLE", dbDdimModel.MaxBarbellWeight),
                                            new XAttribute("MSVDICTIONARYID", "dynbarweightupplace")),

                                        new XElement("Value",
                                            new XAttribute("MSVDOUBLE", dbDdimModel.MinBarbellWeight),
                                            new XAttribute("MSVDICTIONARYID", "dynbarweightdownplace")),

                                        new XElement("Value",
                                            new XAttribute("MSVINTEGER", dbDdimModel.ApertNumber.ToString()),
                                            new XAttribute("MSVDICTIONARYID", "holeindex")),

                                        new XElement("Value",
                                            new XAttribute("MSVDOUBLE", dbDdimModel.TravelLength.ToString("N3")),
                                            new XAttribute("MSVDICTIONARYID", "dynbosstravellength")),

                                        new XElement("Value",
                                            new XAttribute("MSVDOUBLE", dbDdimModel.MaxWeight),
                                            new XAttribute("MSVDICTIONARYID", "dynmaxbossburden")),

                                        new XElement("Value",
                                            new XAttribute("MSVDOUBLE", dbDdimModel.MinWeight),
                                            new XAttribute("MSVDICTIONARYID", "dynminbossburden")),

                                        new XElement("Value",
                                            new XAttribute("MSVDOUBLE", dbDdimModel.SwingCount.ToString("N3")),
                                            new XAttribute("MSVDICTIONARYID", "dynswingcount")),

                                        new XElement("Value",
                                            new XAttribute("MSVINTEGER", "1"),                                              //?
                                            new XAttribute("MSVDICTIONARYID", "sidtype"))))))));

            return document;
        }

        public XDocument CreateDdin2Xml(Ddin2Measurement dbDdinModel)
        {
            #region Setup

            string name = "";
            string number = "";
            foreach (char ch in dbDdinModel.Name)
            {
                if (ch > 47 || ch < 58)
                {
                    name += ch;
                }
                else
                {
                    number += ch;
                }
            }

            number = dbDdinModel.Name.Substring(dbDdinModel.Name.Length - 4);
            if(number[0] == '0')
            {
                number.Substring(number.Length - 1);
            }

            List<double> movement = new List<double>();
            List<double> weight = new List<double>();
            var discrets = DgmConverter.GetXYs(
                dbDdinModel.DynGraph.ToList(),
                dbDdinModel.Step,
                dbDdinModel.WeightDiscr);
            for (int i = 0; i < discrets.GetUpperBound(0); i++)
            {
                movement.Add(discrets[i, 0]);
                weight.Add(discrets[i, 1]);
            }

            var maxStaticW = weight.Max();
            var minStaticW = weight.Min();

            var month = dbDdinModel.DateTime.Date.Month.ToString();
            if(month.Length < 2)
            {
                month = "0" + month;
            }

            var day = dbDdinModel.DateTime.Date.Day.ToString();
            if (day.Length < 2)
            {
                day = "0" + day;
            }

            string date = dbDdinModel.DateTime.Date.Year.ToString() + "-" +
                month + "-" + day;
            string time = dbDdinModel.DateTime.TimeOfDay.ToString().Split('.')[0];

            string field = "";
            var tempField = dbDdinModel.Field.Split(':');
            if (tempField.Length > 1)
            {
                if (tempField[1].Length > 1)
                {
                    field = tempField[1].Remove(0, 1);
                }
            }

            #endregion

            XDocument document =
                new XDocument(
                new XElement("Device_List",
                    new XElement("Device",

                        new XAttribute("DEVTID", "siddos01"),
                        new XAttribute("DSTID", "ДДИН-2"),
                        new XAttribute("DEVSERIALNUMBER", number),

                            new XElement("Measurement_List",
                                new XElement("Measurement",
                                    new XElement("Header",
                                        new XAttribute("MESTYPEID", "dynamogram"),
                                        new XAttribute("MESSTARTDATE", date + "T" + time),
                                        new XAttribute("MESDEVICEOPERATORID", "0"),                                             //?
                                        new XAttribute("MESDEVICEFIELDID", field),
                                        new XAttribute("MESDEVICEWELLCLUSTERID", dbDdinModel.Bush.ToString()),
                                        new XAttribute("MESDEVICEWELLID", dbDdinModel.Well.ToString()),
                                        new XAttribute("MESDEVICEDEPARTMENTID", dbDdinModel.Shop.ToString()),
                                        new XAttribute("MESDEVICEBUFFERPRESSUREID", dbDdinModel.BufferPressure.ToString())),

                                    new XElement("Value_List",

                                        new XElement("Value",
                                            new XAttribute("MSVDICTIONARYID", "dynmovement"),
                                            new XAttribute("MSVDATA", BinaryToBase64(Trim(movement.ToArray(), dbDdinModel.Period), 10))),                                    //!

                                        new XElement("Value",
                                            new XAttribute("MSVDICTIONARYID", "dynburden"),
                                            new XAttribute("MSVDATA", BinaryToBase64(Trim(weight.ToArray(), dbDdinModel.Period), 1000))),                                    //!

                                        new XElement("Value",
                                            new XAttribute("MSVINTEGER", "1"),                                      //Накладной
                                            new XAttribute("MSVDICTIONARYID", "sidsensortype")),

                                         new XElement("Value",
                                            new XAttribute("MSVINTEGER", "1"),                                      //? 1 => ddim/ddin?
                                            new XAttribute("MSVDICTIONARYID", "sidsensorplacemanttype")),

                                        new XElement("Value",
                                            new XAttribute("MSVINTEGER", "1"),                                      // copy from siamService; for ddim cycle == 1
                                            new XAttribute("MSVDICTIONARYID", "sidskippedcyclecount")),

                                        new XElement("Value",
                                            new XAttribute("MSVDOUBLE", dbDdinModel.TimeDiscr.ToString()),         
                                            new XAttribute("MSVDICTIONARYID", "sidtimediscrete")),

                                        new XElement("Value",
                                            new XAttribute("MSVDOUBLE", dbDdinModel.MaxBarbellWeight),
                                            new XAttribute("MSVDICTIONARYID", "dynbarweightupplace")),

                                        new XElement("Value",
                                            new XAttribute("MSVDOUBLE", dbDdinModel.MinBarbellWeight),
                                            new XAttribute("MSVDICTIONARYID", "dynbarweightdownplace")),

                                        new XElement("Value",
                                            new XAttribute("MSVINTEGER", dbDdinModel.ApertNumber),
                                            new XAttribute("MSVDICTIONARYID", "holeindex")),

                                        new XElement("Value",
                                            new XAttribute("MSVDOUBLE", dbDdinModel.Rod),                    
                                            new XAttribute("MSVDICTIONARYID", "dynbossdiameter")),                        

                                        new XElement("Value",
                                            new XAttribute("MSVDOUBLE", dbDdinModel.TravelLength.ToString("N3")),
                                            new XAttribute("MSVDICTIONARYID", "dynbosstravellength")),

                                        new XElement("Value",
                                            new XAttribute("MSVDOUBLE", dbDdinModel.MaxWeight),
                                            new XAttribute("MSVDICTIONARYID", "dynmaxbossburden")),

                                        new XElement("Value",
                                            new XAttribute("MSVDOUBLE", dbDdinModel.MinWeight),
                                            new XAttribute("MSVDICTIONARYID", "dynminbossburden")),

                                        new XElement("Value",
                                            new XAttribute("MSVDOUBLE", dbDdinModel.SwingCount.ToString("N3")),
                                            new XAttribute("MSVDICTIONARYID", "dynswingcount")),

                                        new XElement("Value",
                                            new XAttribute("MSVINTEGER", "1"),                                              //?
                                            new XAttribute("MSVDICTIONARYID", "sidtype"))))))));

            return document;
        }

        public XDocument CreateSiddosA3MXml(SiddosA3MMeasurement dbSiddosA3MModel)
        {
            #region Setup

            string name = "";
            string number = "";
            foreach (char ch in dbSiddosA3MModel.Name)
            {
                if (ch > 47 || ch < 58)
                {
                    name += ch;
                }
                else
                {
                    number += ch;
                }
            }

            number = dbSiddosA3MModel.Name.Substring(dbSiddosA3MModel.Name.Length - 4);
            if (number[0] == '0')
            {
                number.Substring(number.Length - 1);
            }

            List<double> movement = new List<double>();
            List<double> weight = new List<double>();
            var discrets = DgmConverter.GetXYs(
                dbSiddosA3MModel.DynGraph.ToList(),
                dbSiddosA3MModel.Step,
                dbSiddosA3MModel.WeightDiscr);
            for (int i = 0; i < discrets.GetUpperBound(0); i++)
            {
                movement.Add(discrets[i, 0]);  
                weight.Add(discrets[i, 1]);
            }

            var maxStaticW = weight.Max();
            var minStaticW = weight.Min();

            var month = dbSiddosA3MModel.DateTime.Date.Month.ToString();
            if (month.Length < 2)
            {
                month = "0" + month;
            }

            var day = dbSiddosA3MModel.DateTime.Date.Day.ToString();
            if (day.Length < 2)
            {
                day = "0" + day;
            }

            string date = dbSiddosA3MModel.DateTime.Date.Year.ToString() + "-" +
                month + "-" + day;
            string time = dbSiddosA3MModel.DateTime.TimeOfDay.ToString().Split('.')[0];

            string field = "";
            var tempField = dbSiddosA3MModel.Field.Split(':');
            if (tempField.Length > 1)
            {
                if (tempField[1].Length > 1)
                {
                    field = tempField[1].Remove(0, 1);
                }
            }

            #endregion

            XDocument document =
                new XDocument(
                new XElement("Device_List",
                    new XElement("Device",

                        new XAttribute("DEVTID", "siddos01"),
                        new XAttribute("DSTID", "СИДДОС-АВТОМАТ 3М"),
                        new XAttribute("DEVSERIALNUMBER", number),

                            new XElement("Measurement_List",
                                new XElement("Measurement",
                                    new XElement("Header",
                                        new XAttribute("MESTYPEID", "dynamogram"),
                                        new XAttribute("MESSTARTDATE", date + "T" + time),
                                        new XAttribute("MESDEVICEOPERATORID", "0"),                                             //?
                                        new XAttribute("MESDEVICEFIELDID", field),
                                        new XAttribute("MESDEVICEWELLCLUSTERID", dbSiddosA3MModel.Bush.ToString()),
                                        new XAttribute("MESDEVICEWELLID", dbSiddosA3MModel.Well.ToString()),
                                        new XAttribute("MESDEVICEDEPARTMENTID", dbSiddosA3MModel.Shop.ToString()),
                                        new XAttribute("MESDEVICEBUFFERPRESSUREID", dbSiddosA3MModel.BufferPressure.ToString())),

                                    new XElement("Value_List",

                                        new XElement("Value",
                                            new XAttribute("MSVDICTIONARYID", "dynmovement"),
                                            new XAttribute("MSVDATA", BinaryToBase64(Trim(movement.ToArray(), dbSiddosA3MModel.Period), 10))),                                    //!

                                        new XElement("Value",
                                            new XAttribute("MSVDICTIONARYID", "dynburden"),
                                            new XAttribute("MSVDATA", BinaryToBase64(Trim(weight.ToArray(), dbSiddosA3MModel.Period), 1000))),                                    //!

                                        new XElement("Value",
                                            new XAttribute("MSVINTEGER", "0"),                                      //Домкрат
                                            new XAttribute("MSVDICTIONARYID", "sidsensortype")),

                                         new XElement("Value",
                                            new XAttribute("MSVINTEGER", "1"),                                      //? 1 => ddim/ddin?
                                            new XAttribute("MSVDICTIONARYID", "sidsensorplacemanttype")),

                                        new XElement("Value",
                                            new XAttribute("MSVINTEGER", "1"),                                      // copy from siamService; for ddim cycle == 1
                                            new XAttribute("MSVDICTIONARYID", "sidskippedcyclecount")),

                                        new XElement("Value",
                                            new XAttribute("MSVDOUBLE", dbSiddosA3MModel.TimeDiscr.ToString()),          //! возможно нужно поделить или умножить на 1К
                                            new XAttribute("MSVDICTIONARYID", "sidtimediscrete")),

                                        new XElement("Value",
                                            new XAttribute("MSVDOUBLE", dbSiddosA3MModel.MaxBarbellWeight),
                                            new XAttribute("MSVDICTIONARYID", "dynbarweightupplace")),

                                        new XElement("Value",
                                            new XAttribute("MSVDOUBLE", dbSiddosA3MModel.MinBarbellWeight),
                                            new XAttribute("MSVDICTIONARYID", "dynbarweightdownplace")),

                                        new XElement("Value",
                                            new XAttribute("MSVINTEGER", dbSiddosA3MModel.ApertNumber.ToString()),
                                            new XAttribute("MSVDICTIONARYID", "holeindex")),

                                        new XElement("Value",
                                            new XAttribute("MSVDOUBLE", dbSiddosA3MModel.TravelLength),
                                            new XAttribute("MSVDICTIONARYID", "dynbosstravellength")),

                                        new XElement("Value",
                                            new XAttribute("MSVDOUBLE", dbSiddosA3MModel.MaxWeight),
                                            new XAttribute("MSVDICTIONARYID", "dynmaxbossburden")),

                                        new XElement("Value",
                                            new XAttribute("MSVDOUBLE", dbSiddosA3MModel.MinWeight),
                                            new XAttribute("MSVDICTIONARYID", "dynminbossburden")),

                                        new XElement("Value",
                                            new XAttribute("MSVDOUBLE", dbSiddosA3MModel.SwingCount),
                                            new XAttribute("MSVDICTIONARYID", "dynswingcount")),

                                        new XElement("Value",
                                            new XAttribute("MSVINTEGER", "1"),                                              //?
                                            new XAttribute("MSVDICTIONARYID", "sidtype"))))))));

            return document;
        }

        public XDocument CreateDuXml(DuMeasurement dbDuModel)
        {
            #region Setup

            string name = "";
            string number = "";
            foreach (char ch in dbDuModel.Name)
            {
                if (ch > 47 || ch < 58)
                {
                    name += ch;
                }
                else
                {
                    number += ch;
                }
            }

            number = dbDuModel.Name.Substring(dbDuModel.Name.Length - 4);
            if (number[0] == '0')
            {
                number.Substring(number.Length - 1);
            }


            List<double> discretsY = new List<double>();
            var convertedEhogram = EchogramConverter.GetPoints(dbDuModel);
            for (int i = 0; i < convertedEhogram.GetUpperBound(0); i++)
            {
                discretsY.Add(convertedEhogram[i, 1]);
            }

            var month = dbDuModel.DateTime.Date.Month.ToString();
            if (month.Length < 2)
            {
                month = "0" + month;
            }

            var day = dbDuModel.DateTime.Date.Day.ToString();
            if (day.Length < 2)
            {
                day = "0" + day;
            }

            string date = dbDuModel.DateTime.Date.Year.ToString() + "-" +
                month + "-" + day;
            string time = dbDuModel.DateTime.TimeOfDay.ToString().Split('.')[0];

            string field = "";
            var tempField = dbDuModel.Field.Split(':');
            if (tempField.Length > 1)
            {
                if (tempField[1].Length > 1)
                {
                    field = tempField[1].Remove(0, 1);
                }
            }

            int measurementType = 0;
            if (dbDuModel.MeasurementType == Resource.StaticLevel)
            {
                measurementType = 1;
            }
            else if(dbDuModel.MeasurementType == Resource.DynamicLevel)
            {
                measurementType = 2;
            }

            var numberOfCorrectionTable = "0";

            var stringFragments = dbDuModel.SoundSpeedCorrection.Split(' ');
            if(stringFragments.Length == 2)
            {
                numberOfCorrectionTable = stringFragments[1];
            }

            #endregion

            XDocument document =
                new XDocument(
                new XElement("Device_List",
                    new XElement("Device",

                        new XAttribute("DEVTID", "siddos01"),
                        new XAttribute("DSTID", "ДУ-1"),
                        new XAttribute("DEVSERIALNUMBER", number),

                            new XElement("Measurement_List",
                                new XElement("Measurement",
                                    new XElement("Header",
                                        new XAttribute("MESTYPEID", "levelgage"),
                                        new XAttribute("MESSTARTDATE", date + "T" + time),
                                        new XAttribute("MESDEVICEOPERATORID", "0"),                                             
                                        new XAttribute("MESDEVICEFIELDID", field),
                                        new XAttribute("MESDEVICEWELLCLUSTERID", dbDuModel.Bush.ToString()),
                                        new XAttribute("MESDEVICEWELLID", dbDuModel.Well.ToString()),
                                        new XAttribute("MESDEVICEDEPARTMENTID", dbDuModel.Shop.ToString()),
                                        new XAttribute("MESDEVICEBUFFERPRESSUREID", dbDuModel.BufferPressure.ToString())),

                                    new XElement("Value_List",
                                    new XElement("Value",
                                            new XAttribute("MSVINTEGER", numberOfCorrectionTable),
                                            new XAttribute("MSVDICTIONARYID", "sudcorrectiontype")),
                                    new XElement("Value",
                                            new XAttribute("MSVINTEGER", numberOfCorrectionTable),
                                            new XAttribute("MSVDICTIONARYID", "sudcorrectiontypeud")),
                                    new XElement("Value",
                                            new XAttribute("MSVINTEGER", measurementType),
                                            new XAttribute("MSVDICTIONARYID", "sudresearchtype")),
                                    new XElement("Value",
                                            new XAttribute("MSVDOUBLE", dbDuModel.AnnularPressure),
                                            new XAttribute("MSVDICTIONARYID", "sudpressure")),
                                    new XElement("Value",
                                            new XAttribute("MSVDOUBLE", dbDuModel.FluidLevel),
                                            new XAttribute("MSVDICTIONARYID", "lglevel")),
                                        new XElement("Value",
                                            new XAttribute("MSVDOUBLE", dbDuModel.FluidLevel),
                                            new XAttribute("MSVDICTIONARYID", "lglevelud")),
                                        new XElement("Value",
                                            new XAttribute("MSVDOUBLE", dbDuModel.SoundSpeed),
                                            new XAttribute("MSVDICTIONARYID", "lgsoundspeed")),
                                        new XElement("Value",
                                            new XAttribute("MSVDOUBLE", dbDuModel.SoundSpeed),
                                            new XAttribute("MSVDICTIONARYID", "lgsoundspeedud")),
                                        new XElement("Value",
                                            new XAttribute("MSVINTEGER", dbDuModel.NumberOfReflections),
                                            new XAttribute("MSVDICTIONARYID", "lgreflectioncount")),
                                        new XElement("Value",
                                            new XAttribute("MSVDICTIONARYID", "lgtimediscrete"),
                                            new XAttribute("MSVDOUBLE", "0.00585938")),
                                        new XElement("Value",
                                            new XAttribute("MSVDICTIONARYID", "lgechogram"),
                                            new XAttribute("MSVDATA", BinaryToBase64(discretsY.ToArray(), 1)))))))));

            return document;
        }

        private const float arrayDivider = 10;

        private String BinaryToBase64(double[] array, int div) //!
        {
            return Convert.ToBase64String(array.SelectMany(n => 
            {
                return BitConverter.GetBytes(n / div);
            }).ToArray(), 
            Base64FormattingOptions.None);
        }

        private double[] Trim(double[] array, int period)
        {
            if (array != null)
            {
                if (array.Count() > 0)
                {
                    var trimedZerosList = new List<double>();
                    for (int i = 0; i < period; i++)
                    {
                        trimedZerosList.Add(array[i]);
                    }
                    array = trimedZerosList.ToArray();
                }
            }

            return array;
        }
    }
}
