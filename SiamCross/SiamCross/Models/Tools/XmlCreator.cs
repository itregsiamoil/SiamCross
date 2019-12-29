using SiamCross.DataBase.DataBaseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiamCross.Models.Tools
{
    public class XmlCreator
    {
        public string CreateDdim2Xml(Ddim2Measurement dbMeasurementItem)
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

        public string CreateDdin2Xml(Ddin2Measurement dbMeasurementItem)
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

            return Convert.ToBase64String(data);
        }
    }
}
