using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wiring {
    public static class ComponentTypes {
        public const int Power1 = 0;
        public const int Power2 = 1;
        public const int Power3 = 2;
        public const int Toggle = 3;
        public const int BigToggle = 4;
        public const int Lamp1x1 = 5;
        public const int Lamp2x2 = 6;
        public const int Diode = 7;
        public const int Inverter = 8;
        public const int RisingEdge = 9;
        public const int And = 10;
        public const int Or = 11;
        public const int XOr = 12;
        public const int DLatch = 13;
        public const int SevSeg = 14;
        public const int BCDCounter = 15;
        public const int Chip = 16;

        public const int numTypes = 16;

        public static bool isInput(int type) {
            switch(type) {
                case ComponentTypes.Power1:
                case ComponentTypes.Power2:
                case ComponentTypes.Power3:
                case ComponentTypes.Toggle:
                case ComponentTypes.BigToggle:
                    return true;
                default:
                    return false;
            }
        }

        public static bool isOutput(int type) {
            switch(type) {
                case ComponentTypes.Lamp1x1:
                case ComponentTypes.Lamp2x2:
                    return true;
                default:
                    return false;
            }
        }

        public static bool isClickable(int type) {
            switch(type) {
                case ComponentTypes.Diode:
                case ComponentTypes.Inverter:
                case ComponentTypes.Or:
                case ComponentTypes.And:
                case ComponentTypes.Toggle:
                case ComponentTypes.BigToggle:
                case ComponentTypes.XOr:
                case ComponentTypes.RisingEdge:
                    return true;
                default:
                    return false;
            }
        }

        public static double powerOutput(int type) {
            switch(type) {
                case ComponentTypes.Inverter:
                case ComponentTypes.Power1:
                case ComponentTypes.Toggle:
                case ComponentTypes.BigToggle:
                case ComponentTypes.RisingEdge:
                case ComponentTypes.BCDCounter:
                    return 1;
                case ComponentTypes.Power2:
                    return 2;
                case ComponentTypes.Power3:
                    return 3;
                default:
                    return 0;
            }
        }

        public static string imageFileName(int type) {
            string image = "";
            switch(type) {
                case ComponentTypes.Diode:
                    image = "Diode";
                    break;
                case ComponentTypes.Inverter:
                    image = "Inverter";
                    break;
                case ComponentTypes.RisingEdge:
                    image = "RisingEdge";
                    break;
                case ComponentTypes.Power1:
                    image = "Power1";
                    break;
                case ComponentTypes.Power2:
                    image = "Power2";
                    break;
                case ComponentTypes.Power3:
                    image = "Power3";
                    break;
                case ComponentTypes.XOr:
                    image = "XOr";
                    break;
                case ComponentTypes.Or:
                    image = "Or";
                    break;
                case ComponentTypes.And:
                    image = "And";
                    break;
                case ComponentTypes.DLatch:
                    image = "DLatch";
                    break;
                case ComponentTypes.Lamp1x1:
                    image = "1x1Lamp";
                    break;
                case ComponentTypes.Lamp2x2:
                    image = "2x2Lamp";
                    break;
                case ComponentTypes.Toggle:
                    image = "Toggle";
                    break;
                case ComponentTypes.BigToggle:
                    image = "BigToggle";
                    break;
                case ComponentTypes.SevSeg:
                    image = "SevenSegmentDisplay";
                    break;
                case ComponentTypes.BCDCounter:
                    image = "BCDDecoder";
                    break;
                case ComponentTypes.Chip:
                    image = "Chip";
                    break;
                default:
                    break;
            }
            return string.Format("images\\{0}.png",image);
        }

    }

    public struct Point3FP {
        public int X, Y, Source;
        public string Fingerprint;
        public Point3FP(int x,int y,string fingerprint,int source) {
            X = x;
            Y = y;
            Source = source;
            Fingerprint = fingerprint;
        }
        public Point3FP(PointFP fp,int source) {
            X = fp.X;
            Y = fp.Y;
            Fingerprint = fp.Fingerprint;
            Source = source;
        }
    }

    public struct PointFP {
        public int X, Y;
        public string Fingerprint;
        public PointFP(int x,int y,string fingerprint) {
            X = x;
            Y = y;
            Fingerprint = fingerprint;
        }
    }
}
