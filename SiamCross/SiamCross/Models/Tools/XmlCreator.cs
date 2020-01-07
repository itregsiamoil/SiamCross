using SiamCross.DataBase.DataBaseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace SiamCross.Models.Tools
{
    public class XmlCreator
    {
        public string CreateDdim2XmlOld(Ddim2Measurement dbMeasurementItem)
        {
            string name = "";
            string number = "";
            foreach (char ch in dbMeasurementItem.Name)
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

            List<double> movement = new  List<double>();
            List<double> weight = new List<double>();
            var discrets = DgmConverter.GetXYs(
                dbMeasurementItem.DynGraph.ToList(),
                dbMeasurementItem.Step,
                dbMeasurementItem.WeightDiscr);
            for(int i = 0; i < discrets.Length; i++)
            {
                movement.Add(discrets[i, 0]);
                weight.Add(discrets[i, 1]);
            }

            StringBuilder builder = new StringBuilder();

            builder.Append("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
            builder.Append("\r\n");
            builder.Append("<Device_List>");
            builder.Append("\r\n  ");

            builder.Append("<Device DEVTID=\"siddos01\" DSTID=\"");
            builder.Append("ДДИМ2");                                                                 ///!!! Временное решение
            builder.Append("\" DEVSERIALNUMBER=\"");
            builder.Append(number.ToString());
            builder.Append("\">");
            builder.Append("\r\n    ");
            builder.Append("<Measurement_List>");
            builder.Append("\r\n      ");
            builder.Append("<Measurement>");
            builder.Append("\r\n        ");
            builder.Append("<Header MESTYPEID=\"dynamogram\" MESSTARTDATE=\"");

            builder.Append(dbMeasurementItem.DateTime.Date.ToString());
            builder.Append("T");
            builder.Append(builder.Append(dbMeasurementItem.DateTime.TimeOfDay.ToString()));
            builder.Append("\" MESDEVICEOPERATORID=\"");
            builder.Append(0);                                                                          /// ?
            builder.Append("\" MESDEVICEFIELDID=\"");
            builder.Append(dbMeasurementItem.Field.ToString());
            builder.Append("\" MESDEVICEWELLCLUSTERID=\"");
            builder.Append(dbMeasurementItem.Bush.ToString());
            builder.Append("\" MESDEVICEWELLID=\"");
            builder.Append(dbMeasurementItem.Well.ToString());
            builder.Append("\" MESDEVICEDEPARTMENTID=\"");
            builder.Append(dbMeasurementItem.Shop.ToString());

            builder.Append("\" MESDEVICEBUFFERPRESSUREID=\"");
            builder.Append(dbMeasurementItem.BufferPressure.ToString());

            builder.Append("\" />");
            builder.Append("\r\n        ");
            builder.Append("<Value_List>");
            builder.Append("\r\n          ");
            builder.Append("<Value MSVDICTIONARYID=\"dynmovement\" MSVDATA=\"");
            builder.Append(BinaryToBase64(movement.ToArray()));
            builder.Append("\" />");
            builder.Append("\r\n          ");
            builder.Append("<Value MSVDICTIONARYID=\"/*dynburden*/\" MSVDATA=\"");
            builder.Append(BinaryToBase64(weight.ToArray()));                                                                                           
            builder.Append("\" />");
            builder.Append("\r\n          ");
            builder.Append("<Value MSVDICTIONARYID=\"sidsensortype\" MSVINTEGER=\"");
            builder.Append(2.ToString()); //межтраверсный
            //  builder.Append(String.valueOf(wSensor));                                                                   
            builder.Append("\" />");
            builder.Append("\r\n          ");
            builder.Append("<Value MSVDICTIONARYID=\"sidsensorplacemanttype\" MSVINTEGER=\"");
            builder.Append(1.ToString()); // ? ddim/ddin?
            //builder.Append(String.valueOf(placementType));                                                
            builder.Append("\" />");
            builder.Append("\r\n          ");

            var cycle = 1; // copy from siamService; for ddim cycle == 1
            if (cycle >= 0)
            {
                builder.Append("<Value MSVDICTIONARYID=\"sidskippedcyclecount\" MSVINTEGER=\"");
                builder.Append(cycle.ToString());
                builder.Append("\" />");
                builder.Append("\r\n          ");
            }
            builder.Append("<Value MSVDICTIONARYID=\"sidtimediscrete\" MSVDOUBLE=\"");
            builder.Append(dbMeasurementItem.TimeDiscr.ToString()); // возможно нужно умножить или поделить на тысячу
            builder.Append("\" />");
            builder.Append("\r\n          ");

            var maxStaticW = weight.Max();
            var minStaticW = weight.Min();

            if (maxStaticW >= 0)
            {
                builder.Append("<Value MSVDICTIONARYID=\"dynbarweightupplace\" MSVDOUBLE=\"");
                builder.Append(maxStaticW.ToString());
                builder.Append("\" />");
                builder.Append("\r\n          ");
            }
            if (minStaticW >= 0)
            {
                builder.Append("<Value MSVDICTIONARYID=\"dynbarweightdownplace\" MSVDOUBLE=\"");
                builder.Append(minStaticW.ToString());
                builder.Append("\" />");
                builder.Append("\r\n          ");
            }

            var hole = dbMeasurementItem.ApertNumber; // не точно что этот параметр
            if (hole > 0)
            {
                builder.Append("<Value MSVDICTIONARYID=\"holeindex\" MSVINTEGER=\"");
                builder.Append(hole.ToString());
                builder.Append("\" />");
                builder.Append("\r\n          ");
            }

            //builder.Append("<Value MSVDICTIONARYID=\"dynbossdiameter\" MSVDOUBLE=\"");
            //builder.Append(dbMeasurementItem.);
            //builder.Append("\" />");
            //builder.Append("\r\n          "); // Параметр диаметр штока есть только у ддин2

            if (dbMeasurementItem.Travel >= 0)
            {
                builder.Append("<Value MSVDICTIONARYID=\"dynbosstravellength\" MSVDOUBLE=\"");
                builder.Append(dbMeasurementItem.Travel.ToString());
                builder.Append("\" />");
                builder.Append("\r\n          ");
            }

            if (dbMeasurementItem.MaxWeight >= 0)
            {
                builder.Append("<Value MSVDICTIONARYID=\"dynmaxbossburden\" MSVDOUBLE=\"");
                builder.Append(dbMeasurementItem.MaxWeight.ToString());
                builder.Append("\" />");
                builder.Append("\r\n          ");
            }
            if (dbMeasurementItem.MinWeight >= 0)
            {
                builder.Append("<Value MSVDICTIONARYID=\"dynminbossburden\" MSVDOUBLE=\"");
                builder.Append(dbMeasurementItem.MinWeight.ToString());
                builder.Append("\" />");
                builder.Append("\r\n          ");
            }
            if (dbMeasurementItem.Period >= 0)
            {
                builder.Append("<Value MSVDICTIONARYID=\"dynswingcount\" MSVDOUBLE=\"");
                builder.Append(dbMeasurementItem.Period.ToString());
                builder.Append("\" />");
                builder.Append("\r\n          ");
            }
            builder.Append("<Value MSVDICTIONARYID=\"sidtype\" MSVINTEGER=\"");
            builder.Append(1.ToString());
            builder.Append("\" />");
            builder.Append("\r\n        ");
            builder.Append("</Value_List>");
            builder.Append("\r\n      ");
            builder.Append("</Measurement>");
            builder.Append("\r\n    ");
            builder.Append("</Measurement_List>");
            builder.Append("\r\n  ");
            builder.Append("</Device>");
            //if (isSingle)
            //{
            //    builder.Append("\r\n");
            //    builder.Append("</Device_List>");
            //} В СиамСервисе всегда isSingle передается фолс => ???

            return builder.ToString();
        }

        public string CreateDdin2XmlOld(Ddin2Measurement dbMeasurementItem)
        {
            string name = "";
            string number = "";
            foreach (char ch in dbMeasurementItem.Name)
            {
                if (ch > 47 || ch < 58)
                {
                    name += ch;
                }
                else
                {
                    if(number.Length == 0 && ch == '0')
                    {
                        continue;
                    }
                    number += ch;
                }
            }

            List<double> movement = new List<double>();
            List<double> weight = new List<double>();
            var discrets = DgmConverter.GetXYs(
                dbMeasurementItem.DynGraph.ToList(),
                dbMeasurementItem.Step,
                dbMeasurementItem.WeightDiscr);
            for (int i = 0; i < discrets.GetUpperBound(0); i++)
            {
                movement.Add(discrets[i, 0]);
                weight.Add(discrets[i, 1]);
            }

            StringBuilder builder = new StringBuilder();

            builder.Append("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
            builder.Append("\r\n");
            builder.Append("<Device_List>");
            builder.Append("\r\n  ");

            builder.Append("<Device DEVTID=\"siddos01\" DSTID=\"");
            builder.Append("ДДИН2");                                                                 ///!!! Временное решение
            builder.Append("\" DEVSERIALNUMBER=\"");
            builder.Append(number.ToString());
            builder.Append("\">");
            builder.Append("\r\n    ");
            builder.Append("<Measurement_List>");
            builder.Append("\r\n      ");
            builder.Append("<Measurement>");
            builder.Append("\r\n        ");
            builder.Append("<Header MESTYPEID=\"dynamogram\" MESSTARTDATE=\"");

            builder.Append(dbMeasurementItem.DateTime.Date.ToString());
            builder.Append("T");
            builder.Append(builder.Append(dbMeasurementItem.DateTime.TimeOfDay.ToString()));
            builder.Append("\" MESDEVICEOPERATORID=\"");
            builder.Append(0);                                                                          /// ?
            builder.Append("\" MESDEVICEFIELDID=\"");
            builder.Append(dbMeasurementItem.Field.ToString());
            builder.Append("\" MESDEVICEWELLCLUSTERID=\"");
            builder.Append(dbMeasurementItem.Bush.ToString());
            builder.Append("\" MESDEVICEWELLID=\"");
            builder.Append(dbMeasurementItem.Well.ToString());
            builder.Append("\" MESDEVICEDEPARTMENTID=\"");
            builder.Append(dbMeasurementItem.Shop.ToString());

            builder.Append("\" MESDEVICEBUFFERPRESSUREID=\"");
            builder.Append(dbMeasurementItem.BufferPressure.ToString());

            builder.Append("\" />");
            builder.Append("\r\n        ");
            builder.Append("<Value_List>");
            builder.Append("\r\n          ");
            builder.Append("<Value MSVDICTIONARYID=\"dynmovement\" MSVDATA=\"");
            builder.Append(BinaryToBase64(movement.ToArray()));
            builder.Append("\" />");
            builder.Append("\r\n          ");
            builder.Append("<Value MSVDICTIONARYID=\"/*dynburden*/\" MSVDATA=\"");
            builder.Append(BinaryToBase64(weight.ToArray()));
            builder.Append("\" />");
            builder.Append("\r\n          ");
            builder.Append("<Value MSVDICTIONARYID=\"sidsensortype\" MSVINTEGER=\"");
            builder.Append(1.ToString()); //накладной
            //  builder.Append(String.valueOf(wSensor));                                                                    
            builder.Append("\" />");
            builder.Append("\r\n          ");
            builder.Append("<Value MSVDICTIONARYID=\"sidsensorplacemanttype\" MSVINTEGER=\"");
            builder.Append(1.ToString()); // ? ddim/ddin?
            //builder.Append(String.valueOf(placementType));                                                
            builder.Append("\" />");
            builder.Append("\r\n          ");

            var cycle = 1; // copy from siamService; for ddim cycle == 1
            if (cycle >= 0)
            {
                builder.Append("<Value MSVDICTIONARYID=\"sidskippedcyclecount\" MSVINTEGER=\"");
                builder.Append(cycle.ToString());
                builder.Append("\" />");
                builder.Append("\r\n          ");
            }
            builder.Append("<Value MSVDICTIONARYID=\"sidtimediscrete\" MSVDOUBLE=\"");
            builder.Append(dbMeasurementItem.TimeDiscr.ToString()); // возможно нужно умножить или поделить на тысячу
            builder.Append("\" />");
            builder.Append("\r\n          ");

            var maxStaticW = weight.Max();
            var minStaticW = weight.Min();

            if (maxStaticW >= 0)
            {
                builder.Append("<Value MSVDICTIONARYID=\"dynbarweightupplace\" MSVDOUBLE=\"");
                builder.Append(maxStaticW.ToString());
                builder.Append("\" />");
                builder.Append("\r\n          ");
            }
            if (minStaticW >= 0)
            {
                builder.Append("<Value MSVDICTIONARYID=\"dynbarweightdownplace\" MSVDOUBLE=\"");
                builder.Append(minStaticW.ToString());
                builder.Append("\" />");
                builder.Append("\r\n          ");
            }

            var hole = dbMeasurementItem.ApertNumber; // не точно что этот параметр
            if (hole > 0)
            {
                builder.Append("<Value MSVDICTIONARYID=\"holeindex\" MSVINTEGER=\"");
                builder.Append(hole.ToString());
                builder.Append("\" />");
                builder.Append("\r\n          ");
            }

            builder.Append("<Value MSVDICTIONARYID=\"dynbossdiameter\" MSVDOUBLE=\"");
            builder.Append(dbMeasurementItem.Rod);
            builder.Append("\" />");
            builder.Append("\r\n          "); 

            if (dbMeasurementItem.Travel >= 0)
            {
                builder.Append("<Value MSVDICTIONARYID=\"dynbosstravellength\" MSVDOUBLE=\"");
                builder.Append(dbMeasurementItem.Travel.ToString());
                builder.Append("\" />");
                builder.Append("\r\n          ");
            }

            if (dbMeasurementItem.MaxWeight >= 0)
            {
                builder.Append("<Value MSVDICTIONARYID=\"dynmaxbossburden\" MSVDOUBLE=\"");
                builder.Append(dbMeasurementItem.MaxWeight.ToString());
                builder.Append("\" />");
                builder.Append("\r\n          ");
            }
            if (dbMeasurementItem.MinWeight >= 0)
            {
                builder.Append("<Value MSVDICTIONARYID=\"dynminbossburden\" MSVDOUBLE=\"");
                builder.Append(dbMeasurementItem.MinWeight.ToString());
                builder.Append("\" />");
                builder.Append("\r\n          ");
            }
            if (dbMeasurementItem.Period >= 0)
            {
                builder.Append("<Value MSVDICTIONARYID=\"dynswingcount\" MSVDOUBLE=\"");
                builder.Append(dbMeasurementItem.Period.ToString());
                builder.Append("\" />");
                builder.Append("\r\n          ");
            }
            builder.Append("<Value MSVDICTIONARYID=\"sidtype\" MSVINTEGER=\"");
            builder.Append(1.ToString());
            builder.Append("\" />");
            builder.Append("\r\n        ");
            builder.Append("</Value_List>");
            builder.Append("\r\n      ");
            builder.Append("</Measurement>");
            builder.Append("\r\n    ");
            builder.Append("</Measurement_List>");
            builder.Append("\r\n  ");
            builder.Append("</Device>");
            //if (isSingle)
            //{
            //    builder.Append("\r\n");
            //    builder.Append("</Device_List>");
            //} В СиамСервисе всегда isSingle передается фолс => ???

            return builder.ToString();
        }

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

            List<double> movement = new List<double>();
            List<double> weight = new List<double>();
            var discrets = DgmConverter.GetXYs(
                dbDdimModel.DynGraph.ToList(),
                dbDdimModel.Step,
                dbDdimModel.WeightDiscr);
            for (int i = 0; i < discrets.Length; i++)
            {
                movement.Add(discrets[i, 0]);
                weight.Add(discrets[i, 1]);
            }

            var maxStaticW = weight.Max();
            var minStaticW = weight.Min();

            string date = dbDdimModel.DateTime.Date.ToString();
            string time = TimeSpan.Parse(string.Format("{0:HH:mm:ss}",
                dbDdimModel.DateTime.TimeOfDay)).ToString();

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
                                            new XAttribute("MSVDOUBLE", maxStaticW.ToString()),
                                            new XAttribute("MSVDICTIONARYID", "dynbarweightupplace")),

                                        new XElement("Value",
                                            new XAttribute("MSVDOUBLE", minStaticW.ToString()),
                                            new XAttribute("MSVDICTIONARYID", "dynbarweightdownplace")),

                                        new XElement("Value",
                                            new XAttribute("MSVINTEGER", dbDdimModel.ApertNumber.ToString()),
                                            new XAttribute("MSVDICTIONARYID", "holeindex")),

                                        //new XElement("Value",
                                        //    new XAttribute("MSVDOUBLE", "0.0"),
                                        //    new XAttribute("MSVDICTIONARYID", "dynbossdiameter")),                        // Данный параметр имеется только у прибора ддин2

                                        new XElement("Value",
                                            new XAttribute("MSVDOUBLE", dbDdimModel.Travel.ToString()),
                                            new XAttribute("MSVDICTIONARYID", "dynbosstravellength")),

                                        new XElement("Value",
                                            new XAttribute("MSVDOUBLE", dbDdimModel.MaxWeight.ToString()),
                                            new XAttribute("MSVDICTIONARYID", "dynmaxbossburden")),

                                        new XElement("Value",
                                            new XAttribute("MSVDOUBLE", dbDdimModel.MinWeight.ToString()),
                                            new XAttribute("MSVDICTIONARYID", "dynminbossburden")),

                                        new XElement("Value",
                                            new XAttribute("MSVDOUBLE", dbDdimModel.Period.ToString()),
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

            List<double> movement = new List<double>();
            List<double> weight = new List<double>();
            var discrets = DgmConverter.GetXYs(
                dbDdinModel.DynGraph.ToList(),
                dbDdinModel.Step,
                dbDdinModel.WeightDiscr);
            for (int i = 0; i < discrets.Length; i++)
            {
                movement.Add(discrets[i, 0]);
                weight.Add(discrets[i, 1]);
            }

            var maxStaticW = weight.Max();
            var minStaticW = weight.Min();

            string date = dbDdinModel.DateTime.Date.ToString();
            string time = TimeSpan.Parse(string.Format("{0:HH:mm:ss}",
                dbDdinModel.DateTime.TimeOfDay)).ToString();

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
                                            new XAttribute("MSVDATA", BinaryToBase64(movement.ToArray()))),                                    //!

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
                                            new XAttribute("MSVDOUBLE", dbDdinModel.TimeDiscr.ToString()),          //! возможно нужно поделить или умножить на 1К
                                            new XAttribute("MSVDICTIONARYID", "sidtimediscrete")),

                                        new XElement("Value",
                                            new XAttribute("MSVDOUBLE", maxStaticW.ToString()),
                                            new XAttribute("MSVDICTIONARYID", "dynbarweightupplace")),

                                        new XElement("Value",
                                            new XAttribute("MSVDOUBLE", minStaticW.ToString()),
                                            new XAttribute("MSVDICTIONARYID", "dynbarweightdownplace")),

                                        new XElement("Value",
                                            new XAttribute("MSVINTEGER", dbDdinModel.ApertNumber.ToString()),
                                            new XAttribute("MSVDICTIONARYID", "holeindex")),

                                        new XElement("Value",
                                            new XAttribute("MSVDOUBLE", dbDdinModel.Rod),
                                            new XAttribute("MSVDICTIONARYID", "dynbossdiameter")),                        

                                        new XElement("Value",
                                            new XAttribute("MSVDOUBLE", dbDdinModel.Travel.ToString()),
                                            new XAttribute("MSVDICTIONARYID", "dynbosstravellength")),

                                        new XElement("Value",
                                            new XAttribute("MSVDOUBLE", dbDdinModel.MaxWeight.ToString()),
                                            new XAttribute("MSVDICTIONARYID", "dynmaxbossburden")),

                                        new XElement("Value",
                                            new XAttribute("MSVDOUBLE", dbDdinModel.MinWeight.ToString()),
                                            new XAttribute("MSVDICTIONARYID", "dynminbossburden")),

                                        new XElement("Value",
                                            new XAttribute("MSVDOUBLE", dbDdinModel.Period.ToString()),
                                            new XAttribute("MSVDICTIONARYID", "dynswingcount")),

                                        new XElement("Value",
                                            new XAttribute("MSVINTEGER", "1"),                                              //?
                                            new XAttribute("MSVDICTIONARYID", "sidtype"))))))));

            return document;
        }

        private String BinaryToBase64(double[] array)
        {
            byte[] data = new byte[array.Length * 8];
            int key = 0;
            byte[] buffer;
            foreach (double anArray in array)
            {
                buffer = BitConverter.GetBytes(anArray);
                data[key] = buffer[7];
                data[key + 1] = buffer[6];
                data[key + 2] = buffer[5];
                data[key + 3] = buffer[4];
                data[key + 4] = buffer[3];
                data[key + 5] = buffer[2];
                data[key + 6] = buffer[1];
                data[key + 7] = buffer[0];
                key += 8;
            }

            return Convert.ToBase64String(data, Base64FormattingOptions.None);
        }
    }
}
