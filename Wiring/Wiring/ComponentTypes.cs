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
        public const int And = 9;
        public const int Or = 10;
        public const int Chip = 11;

        public const int numTypes = 11;

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
                case ComponentTypes.Chip:
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
                case ComponentTypes.Power1:
                    image = "Power1";
                    break;
                case ComponentTypes.Power2:
                    image = "Power2";
                    break;
                case ComponentTypes.Power3:
                    image = "Power3";
                    break;
                case ComponentTypes.Or:
                    image = "Or";
                    break;
                case ComponentTypes.And:
                    image = "And";
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
                case ComponentTypes.Chip:
                    image = "Chip";
                    break;
                default:
                    break;
            }
            return string.Format("images\\{0}.png",image);
        }

    }
}
