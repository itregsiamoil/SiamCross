using SiamCross.DataBase.DataBaseModels;
using SiamCross.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SiamCross.Models.Tools
{
    public class XmlCreator
    {
        private static string ExtractName(string str)
        {
            int pos = str.IndexOf('№', 0);
            if (0 < pos)
                return str.Substring(0, pos);

            pos = -1;
            for (int i = 0; i < str.Length && -1 == pos; ++i)
            {
                if (!char.IsLetter(str[i]))
                    pos = i;
            }
            if (0 < pos)
                return str.Substring(0, pos);

            return str;
        }

        private static string ExtractNumber(string str)
        {
            int pos = str.IndexOf('№', 0);
            if (0 < pos)
                return str.Substring(pos + 1, str.Length - pos - 1);

            pos = -1;
            for (int i = 0; i < str.Length && -1 == pos; ++i)
            {
                if (!char.IsLetter(str[i]))
                    pos = i;
            }
            if (0 < pos)
                return str.Substring(pos, str.Length - pos);

            return str;
        }

        public XDocument CreateDdin2Xml(Ddin2Measurement dbDdinModel)
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            #region Setup

            string name = ExtractName(dbDdinModel.Name);
            string number = ExtractNumber(dbDdinModel.Name);

            List<double> movement = new List<double>();
            List<double> weight = new List<double>();
            double[,] discrets = DgmConverter.GetXYs(
                dbDdinModel.DynGraph.ToList(),
                dbDdinModel.Step,
                dbDdinModel.WeightDiscr);
            for (int i = 0; i < discrets.GetUpperBound(0); i++)
            {
                movement.Add(discrets[i, 0]);
                weight.Add(discrets[i, 1]);
            }

            double maxStaticW = weight.Max();
            double minStaticW = weight.Min();

            string month = dbDdinModel.DateTime.Date.Month.ToString();
            if (month.Length < 2)
            {
                month = "0" + month;
            }

            string day = dbDdinModel.DateTime.Date.Day.ToString();
            if (day.Length < 2)
            {
                day = "0" + day;
            }

            string date = dbDdinModel.DateTime.Date.Year.ToString() + "-" +
                month + "-" + day;
            string time = dbDdinModel.DateTime.TimeOfDay.ToString().Split('.')[0];

            string field = "";
            string[] tempField = dbDdinModel.Field.Split(':');
            if (tempField.Length > 1)
            {
                if (tempField[1].Length > 1)
                {
                    field = tempField[1].Remove(0, 1);
                }
            }

            #endregion

            int sens_type = 0;
            string lwname = name.ToLower();
            if (lwname.Contains("siddos") || lwname.Contains("сиддос"))
                sens_type = 0;
            else if (lwname.Contains("ddin") || lwname.Contains("ддин"))
                sens_type = 1;
            else if (lwname.Contains("ddim") || lwname.Contains("ддим"))
                sens_type = 2;

            XDocument document =
                new XDocument(
                new XElement("Device_List",
                    new XElement("Device",

                        new XAttribute("DEVTID", "siddos01"),
                        new XAttribute("DSTID", name),
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
                                        new XAttribute("MESDEVICEDEPARTMENTID", dbDdinModel.Shop.ToString())),

                                    new XElement("Value_List",
                                        new XElement("Value",
                                            new XAttribute("MSVDOUBLE", dbDdinModel.BufferPressure.ToString("F3")),
                                            new XAttribute("MSVDICTIONARYID", "bufferpressure")),

                                        new XElement("Value",
                                            new XAttribute("MSVDICTIONARYID", "dynmovement"),
                                            new XAttribute("MSVDATA", BinaryToBase64(Trim(movement.ToArray(), dbDdinModel.Period), 10))),                                    //!

                                        new XElement("Value",
                                            new XAttribute("MSVDICTIONARYID", "dynburden"),
                                            new XAttribute("MSVDATA", BinaryToBase64(Trim(weight.ToArray(), dbDdinModel.Period), 1000))),                                    //!

                                        new XElement("Value",
                                            new XAttribute("MSVINTEGER", sens_type.ToString()),                       //Накладной
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
        public XDocument CreateDuXml(DuMeasurement dbDuModel)
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            #region Setup

            string name = ExtractName(dbDuModel.Name);
            string number = ExtractNumber(dbDuModel.Name);

            List<double> discretsY = new List<double>();
            float minX;
            float minY;
            float maxX;
            float maxY;

            double[,] convertedEhogram = EchogramConverter.GetPoints(dbDuModel
                , out minX, out maxX, out minY, out maxY);
            for (int i = 0; i < convertedEhogram.GetLength(0); i++)
            {
                discretsY.Add(convertedEhogram[i, 1]);
            }

            string month = dbDuModel.DateTime.Date.Month.ToString();
            if (month.Length < 2)
            {
                month = "0" + month;
            }

            string day = dbDuModel.DateTime.Date.Day.ToString();
            if (day.Length < 2)
            {
                day = "0" + day;
            }

            string date = dbDuModel.DateTime.Date.Year.ToString() + "-" +
                month + "-" + day;
            string time = dbDuModel.DateTime.TimeOfDay.ToString().Split('.')[0];

            string field = "";
            string[] tempField = dbDuModel.Field.Split(':');
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
            else if (dbDuModel.MeasurementType == Resource.DynamicLevel)
            {
                measurementType = 2;
            }

            string numberOfCorrectionTable = "0";

            string[] stringFragments = dbDuModel.SoundSpeedCorrection.Split(' ');
            if (stringFragments.Length == 2)
            {
                numberOfCorrectionTable = stringFragments[1];
            }

            #endregion

            XDocument document =
                new XDocument(
                new XElement("Device_List",
                    new XElement("Device",

                        new XAttribute("DEVTID", "sudos01"),
                        new XAttribute("DSTID", name),
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
                                        new XAttribute("MESDEVICEDEPARTMENTID", dbDuModel.Shop.ToString())),

                                    new XElement("Value_List",

                                    new XElement("Value",
                                            new XAttribute("MSVDOUBLE", dbDuModel.BufferPressure.ToString("F3")),
                                            new XAttribute("MSVDICTIONARYID", "bufferpressure")),
                                    new XElement("Value",
                                            new XAttribute("MSVDOUBLE", dbDuModel.PumpDepth.ToString("N3")),
                                            new XAttribute("MSVDICTIONARYID", "pumpdepth")),
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

        public XDocument CreateXml(MeasureData md)
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

            XDocument document = new XDocument();
            XElement deviceList = new XElement("Device_List");

            XElement device = new XElement("Device");
            device.Add(new XAttribute("DEVTID", "umt01"));
            device.Add(new XAttribute("DSTID", md.Device.Name));
            device.Add(new XAttribute("DEVSERIALNUMBER", md.Device.Number.ToString()));

            XElement measurementList = new XElement("Measurement_List");

            XElement measurement = new XElement("Measurement");

            XElement header = new XElement("Header");
            header.Add(new XAttribute("MESTYPEID", "mtmeter"));
            header.Add(new XAttribute("MESSTARTDATE", md.Measure.BeginTimestamp.ToString("yyyy-MM-ddTHH:mm:ss")));
            header.Add(new XAttribute("MESDEVICEOPERATORID", "0"));
            header.Add(new XAttribute("MESDEVICEFIELDID", md.Position.Field.ToString()));
            header.Add(new XAttribute("MESDEVICEWELLCLUSTERID", md.Position.Bush));
            header.Add(new XAttribute("MESDEVICEWELLID", md.Position.Well));
            header.Add(new XAttribute("MESDEVICEDEPARTMENTID", md.Position.Shop.ToString()));

            var attrByTitle = Repo.AttrDir.ByTitle;
            XElement ValueList = new XElement("Value_List");
            ValueList.Add(MakeValue("umttype", md.Measure.DataInt[attrByTitle["umttype"]]));

            var ts = TimeSpan.FromSeconds(md.Measure.DataInt[attrByTitle["PeriodSec"]]);
            var strts = (DateTime.MinValue + ts).ToString("yyyy-MM-ddTHH:mm:ss");
            ValueList.Add(MakeValue("mtinterval", strts, "MSVDATE"));

            ValueList.Add(MakeBase64Value("mtpressure", md.Measure.DataBlob[attrByTitle["mtpressure"]]));

            string fileName;
            if (md.Measure.DataBlob.TryGetValue(attrByTitle["mttemperature"], out fileName))
                ValueList.Add(MakeBase64Value("mttemperature", fileName));
            if (md.Measure.DataBlob.TryGetValue(attrByTitle["umttemperatureex"], out fileName))
                ValueList.Add(MakeBase64Value("mttemperature", fileName));

            measurement.Add(header);
            measurement.Add(ValueList);
            measurementList.Add(measurement);
            device.Add(measurementList);
            deviceList.Add(device);
            document.Add(deviceList);


            return document;
        }

        XElement MakeBase64Value(string name, string fileName)
        {
            var path = Path.Combine(
                    Environment.GetFolderPath(
                    Environment.SpecialFolder.Personal), "bin");
            path = Path.Combine(path, fileName);

            var file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            using (file)
            {
                float val;
                byte[] arr4 = new byte[4];
                byte[] arr = new byte[file.Length * 2];
                UInt32 i = 0;
                while (0 < file.Read(arr4, 0, 4))
                {
                    val = BitConverter.ToSingle(arr4, 0);

                    BitConverter.GetBytes(((double)val)).CopyTo(arr, i * 8);
                    //arr4.CopyTo(arr, i*8);
                    i++;
                }
                return MakeValue(name, arr);
            }
        }
        XElement MakeValue(string name, byte[] val)
        {
            return MakeValue(name, Convert.ToBase64String(val), "MSVDATA");
        }
        XElement MakeValue(string name, DateTime val)
        {
            return MakeValue(name, val.ToString(), "MSVDATE");
        }
        XElement MakeValue(string name, long val)
        {
            return MakeValue(name, val.ToString(), "MSVINTEGER");
        }
        XElement MakeValue(string name, string val, string kind)
        {
            return new XElement("Value",
                new XAttribute(kind, val),
                new XAttribute("MSVDICTIONARYID", name));
        }



        private const float arrayDivider = 10;

        private string BinaryToBase64(double[] array, int div) //!
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
                    List<double> trimedZerosList = new List<double>();
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
