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
    [Preserve(AllMembers = true)]
    public class XmlCreator
    {
        public XDocument CreateDdim2Xml(Ddim2Measurement dbDdimModel)
        {
            //////////////////////////////////////////////////////////////////////////////////////// Setup

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
                movement.Add(discrets[i, 0] * 100);  //////для УМИ умножается на 100
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

            ////////////////////////////////////////////////////////////////////////////////////////////

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
                                        new XAttribute("MESDEVICEFIELDID", dbDdimModel.Field.ToString()),
                                        new XAttribute("MESDEVICEWELLCLUSTERID", dbDdimModel.Bush.ToString()),
                                        new XAttribute("MESDEVICEWELLID", dbDdimModel.Well.ToString()),
                                        new XAttribute("MESDEVICEDEPARTMENTID", dbDdimModel.Shop.ToString()),
                                        new XAttribute("MESDEVICEBUFFERPRESSUREID", dbDdimModel.BufferPressure.ToString())),

                                    new XElement("Value_List",

                                        new XElement("Value",
                                            new XAttribute("MSVDICTIONARYID", "dynmovement"),
                                            new XAttribute("MSVDATA", BinaryToBase64(movement.ToArray()))),                                    //!

                                        new XElement("Value",
                                            new XAttribute("MSVDICTIONARYID", "dynburden"),
                                            new XAttribute("MSVDATA", BinaryToBase64(weight.ToArray()))),                                    //!

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
                                            new XAttribute("MSVDOUBLE", dbDdimModel.TravelLength),
                                            new XAttribute("MSVDICTIONARYID", "dynbosstravellength")),

                                        new XElement("Value",
                                            new XAttribute("MSVDOUBLE", dbDdimModel.MaxWeight),
                                            new XAttribute("MSVDICTIONARYID", "dynmaxbossburden")),

                                        new XElement("Value",
                                            new XAttribute("MSVDOUBLE", dbDdimModel.MinWeight),
                                            new XAttribute("MSVDICTIONARYID", "dynminbossburden")),

                                        new XElement("Value",
                                            new XAttribute("MSVDOUBLE", dbDdimModel.SwingCount),
                                            new XAttribute("MSVDICTIONARYID", "dynswingcount")),

                                        new XElement("Value",
                                            new XAttribute("MSVINTEGER", "1"),                                              //?
                                            new XAttribute("MSVDICTIONARYID", "sidtype"))))))));

            return document;
        }

        public XDocument CreateDdin2Xml(Ddin2Measurement dbDdinModel)
        {
            //////////////////////////////////////////////////////////////////////////////////////// Setup

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

            var maxStaticW = weight.Max() / 1000;
            var minStaticW = weight.Min() / 1000;

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

            ////////////////////////////////////////////////////////////////////////////////////////////

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
                                        new XAttribute("MESDEVICEFIELDID", dbDdinModel.Field.ToString()),
                                        new XAttribute("MESDEVICEWELLCLUSTERID", dbDdinModel.Bush.ToString()),
                                        new XAttribute("MESDEVICEWELLID", dbDdinModel.Well.ToString()),
                                        new XAttribute("MESDEVICEDEPARTMENTID", dbDdinModel.Shop.ToString()),
                                        new XAttribute("MESDEVICEBUFFERPRESSUREID", dbDdinModel.BufferPressure.ToString())),

                                    new XElement("Value_List",

                                        new XElement("Value",
                                            new XAttribute("MSVDICTIONARYID", "dynmovement"),
                                            new XAttribute("MSVDATA", BinaryToBase64(movement.ToArray()))),                                    //!

                                        new XElement("Value",
                                            new XAttribute("MSVDICTIONARYID", "dynburden"),
                                            new XAttribute("MSVDATA", BinaryToBase64(weight.ToArray()))),                                    //!

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
                                            new XAttribute("MSVDOUBLE", dbDdinModel.TravelLength),
                                            new XAttribute("MSVDICTIONARYID", "dynbosstravellength")),

                                        new XElement("Value",
                                            new XAttribute("MSVDOUBLE", dbDdinModel.MaxWeight),
                                            new XAttribute("MSVDICTIONARYID", "dynmaxbossburden")),

                                        new XElement("Value",
                                            new XAttribute("MSVDOUBLE", dbDdinModel.MinWeight),
                                            new XAttribute("MSVDICTIONARYID", "dynminbossburden")),

                                        new XElement("Value",
                                            new XAttribute("MSVDOUBLE", dbDdinModel.SwingCount),
                                            new XAttribute("MSVDICTIONARYID", "dynswingcount")),

                                        new XElement("Value",
                                            new XAttribute("MSVINTEGER", "1"),                                              //?
                                            new XAttribute("MSVDICTIONARYID", "sidtype"))))))));

            return document;
        }

        public XDocument CreateSiddosA3MXml(SiddosA3MMeasurement dbSiddosA3MModel)
        {
            //////////////////////////////////////////////////////////////////////////////////////// Setup

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
                movement.Add(discrets[i, 0] * 100);  //////для УМИ умножается на 100
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

            ////////////////////////////////////////////////////////////////////////////////////////////

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
                                        new XAttribute("MESDEVICEFIELDID", dbSiddosA3MModel.Field.ToString()),
                                        new XAttribute("MESDEVICEWELLCLUSTERID", dbSiddosA3MModel.Bush.ToString()),
                                        new XAttribute("MESDEVICEWELLID", dbSiddosA3MModel.Well.ToString()),
                                        new XAttribute("MESDEVICEDEPARTMENTID", dbSiddosA3MModel.Shop.ToString()),
                                        new XAttribute("MESDEVICEBUFFERPRESSUREID", dbSiddosA3MModel.BufferPressure.ToString())),

                                    new XElement("Value_List",

                                        new XElement("Value",
                                            new XAttribute("MSVDICTIONARYID", "dynmovement"),
                                            new XAttribute("MSVDATA", BinaryToBase64(movement.ToArray()))),                                    //!

                                        new XElement("Value",
                                            new XAttribute("MSVDICTIONARYID", "dynburden"),
                                            new XAttribute("MSVDATA", BinaryToBase64(weight.ToArray()))),                                    //!

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

        private String BinaryToBase64(double[] array)
        {          
            return Convert.ToBase64String(array.SelectMany(n => {
                return BitConverter.GetBytes(n / 1000f);
            }).ToArray(), 
            Base64FormattingOptions.None);
        }
    }
}
