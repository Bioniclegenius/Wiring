﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wiring
{
    public class Tile
    {
        /*
         * Wires
         * 0: Empty
         * 1-4: Endpoints. Left, up, right, down.
         * 5-8: Corners. Left/up, up/right, right/down, down/left.
         * 9-12: Ts. Up/down/left, left/right/up, up/down/right, left/right/down.
         * 13-14: Straights. Left/right, up/down.
         * 15: + bridge.
         * 16: + intersection.
         */
        public int type;
        public List<string> signal1Fingerprints;
        public List<string> signal2Fingerprints;
        public static int baseColor = 127;
        private const int powerStart = 48;
        private const int fingerprintEnd = 40;

        #region Constructors

        public Tile()
        {
            type = 0;
            signal1Fingerprints = new List<string>();
            signal2Fingerprints = new List<string>();
        }

        public Tile(Tile t)
        {
            type = t.type;
            signal1Fingerprints = new List<string>();
            for (int x = 0; x < t.signal1Fingerprints.Count(); x++)
                signal1Fingerprints.Add(t.signal1Fingerprints[x]);
            signal2Fingerprints = new List<string>();
            for (int x = 0; x < t.signal2Fingerprints.Count(); x++)
                signal2Fingerprints.Add(t.signal2Fingerprints[x]);
        }

        #endregion

        #region Power the tile

        public void power(List<string> fingerprints, int level = 0, bool remove = false)
        {
            for (int x = 0; x < fingerprints.Count(); x++)
                power(fingerprints[x], level, remove);
        }

        public void power(string fingerprint, int level = 0, bool remove = false)
        {
            if (fingerprint != "")
            {
                double fpPower = Convert.ToDouble(fingerprint.Substring(powerStart));
                if (fpPower > 3)
                    fingerprint = string.Format("{0}{1}", fingerprint.Substring(0, powerStart), 3);

                fingerprint = stepDown(fingerprint);

                bool contains = false;
                for (int x = 0; x < signal1Fingerprints.Count(); x++)
                    if (signal1Fingerprints[x].Substring(0, fingerprintEnd) == fingerprint.Substring(0, fingerprintEnd))
                    {
                        signal1Fingerprints[x] = fingerprint;
                        if (remove)
                        {
                            signal1Fingerprints.RemoveAt(x);
                            x--;
                        }
                        else
                        {
                            contains = true;
                            break;
                        }
                    }
                if (level != 2 && !contains)
                    signal1Fingerprints.Add(fingerprint);
                contains = false;
                for (int x = 0; x < signal2Fingerprints.Count(); x++)
                    if (signal2Fingerprints[x].Substring(0, fingerprintEnd) == fingerprint.Substring(0, fingerprintEnd))
                    {
                        signal2Fingerprints[x] = fingerprint;
                        if (remove)
                        {
                            signal2Fingerprints.RemoveAt(x);
                            x--;
                        }
                        else
                        {
                            contains = true;
                            break;
                        }
                    }
                if (!contains && level != 1)
                    signal2Fingerprints.Add(fingerprint);
            }
        }

        public void power(string fpl, string fpu, string fpr, string fpd, bool remove = false)
        {
            var left = new int[] { 1, 5, 8, 9, 10, 12, 13, 15 };
            var up = new int[] { 2, 5, 6, 9, 10, 11, 14, 15 };
            var right = new int[] { 3, 6, 7, 10, 11, 12, 13, 15 };
            var down = new int[] { 4, 7, 8, 9, 11, 12, 14, 15 };
            if (fpl != "")
                for (int x = 0; x < 8; x++)
                    if (type == left[x])
                        power(fpl, 1, remove);
            if (fpu != "")
                for (int x = 0; x < 8; x++)
                    if (type == up[x])
                        power(fpu, 1, remove);
            if (fpr != "")
                for (int x = 0; x < 8; x++)
                    if (type == right[x])
                        power(fpr, 1, remove);
            if (fpd != "")
                for (int x = 0; x < 8; x++)
                    if (type == down[x])
                        power(fpd, 1, remove);
            if (type == 16)
            {
                power(fpl, 1, remove);
                power(fpr, 1, remove);
                power(fpu, 2, remove);
                power(fpd, 2, remove);
            }
        }

        private string stepDown(string fingerprint)
        {
            UInt32 step = 0;
            UInt32.TryParse(fingerprint.Substring(fingerprintEnd, 8), NumberStyles.HexNumber, CultureInfo.CurrentCulture, out step);
            if (step > 0)
                step--;
            string fingerprintBuf = string.Format("{0}{1}{2}", fingerprint.Substring(0, fingerprintEnd), step.ToString("X8"), fingerprint.Substring(powerStart));
            return fingerprintBuf;
        }

        #endregion

        #region Get power

        public UInt32 getStep(string fingerprint,int axis=0)
        {
            UInt32 step = 0;
            string myFingerprint = getFingerprint(fingerprint, axis);
            UInt32.TryParse(myFingerprint.Substring(fingerprintEnd, 8), NumberStyles.HexNumber, CultureInfo.CurrentCulture, out step);
            return step;
        }

        public List<string> getPowerLists(int dir)
        {
            var left = new int[] { 1, 5, 8, 9, 10, 12, 13, 15 };
            var up = new int[] { 2, 5, 6, 9, 10, 11, 14, 15 };
            var right = new int[] { 3, 6, 7, 10, 11, 12, 13, 15 };
            var down = new int[] { 4, 7, 8, 9, 11, 12, 14, 15 };
            if (dir == 0)
            {
                for (int x = 0; x < 8; x++)
                    if (type == left[x])
                        return signal1Fingerprints;
            }
            if (dir == 1)
            {
                for (int x = 0; x < 8; x++)
                    if (type == up[x])
                        return signal1Fingerprints;
            }
            if (dir == 2)
            {
                for (int x = 0; x < 8; x++)
                    if (type == right[x])
                        return signal1Fingerprints;
            }
            if (dir == 3)
            {
                for (int x = 0; x < 8; x++)
                    if (type == down[x])
                        return signal1Fingerprints;
            }
            if (type == 16)
            {
                if (dir == 0 || dir == 2)
                    return signal1Fingerprints;
                if (dir == 1 || dir == 3)
                    return signal2Fingerprints;
            }
            return new List<string>();
        }

        public double getPowerFingerprint(int axis)
        {
            double maxPower = -1;
            if (axis == 1)
            {
                for (int x = 0; x < signal2Fingerprints.Count(); x++)
                {
                    double power = Convert.ToDouble(signal2Fingerprints[x].Substring(powerStart));
                    if (power > maxPower)
                        maxPower = power;
                }
                return maxPower;
            }
            for (int x = 0; x < signal1Fingerprints.Count(); x++)
            {
                double power = Convert.ToDouble(signal1Fingerprints[x].Substring(powerStart));
                if (power > maxPower)
                    maxPower = power;
            }
            return maxPower;
        }

        public bool hasFingerprint(string fingerprint, int axis = 0)
        {
            if (axis == 1)
                for (int x = 0; x < signal2Fingerprints.Count(); x++)
                    if (signal2Fingerprints[x].Substring(0, fingerprintEnd) == fingerprint.Substring(0, fingerprintEnd) &&
                        signal2Fingerprints[x].Substring(powerStart) == fingerprint.Substring(powerStart))
                        return true;
            for (int x = 0; x < signal1Fingerprints.Count(); x++)
                if (signal1Fingerprints[x].Substring(0, fingerprintEnd) == fingerprint.Substring(0, fingerprintEnd) &&
                    signal1Fingerprints[x].Substring(powerStart) == fingerprint.Substring(powerStart))
                    return true;
            return false;
        }

        public bool hasFingerprintSig(string fingerprint, int axis = 0)
        {
            for (int x = 0; x < signal1Fingerprints.Count(); x++)
                if (signal1Fingerprints[x].Substring(0, fingerprintEnd) == fingerprint.Substring(0, fingerprintEnd))
                    return true;
            if (axis != 0)
                for (int x = 0; x < signal2Fingerprints.Count(); x++)
                    if (signal2Fingerprints[x].Substring(0, fingerprintEnd) == fingerprint.Substring(0, fingerprintEnd))
                        return true;
            return false;
        }

        public string getFingerprint(string fingerprint, int axis = 0)
        {
            if (axis == 1)
                for (int x = 0; x < signal2Fingerprints.Count(); x++)
                    if (signal2Fingerprints[x].Substring(0, fingerprintEnd) == fingerprint.Substring(0, fingerprintEnd))
                        return signal2Fingerprints[x];
            for (int x = 0; x < signal1Fingerprints.Count(); x++)
                if (signal1Fingerprints[x].Substring(0, fingerprintEnd) == fingerprint.Substring(0, fingerprintEnd))
                    return signal1Fingerprints[x];
            return new string('0', powerStart + 1);
        }

        /// <summary>
        /// Returns the signal strength of this particular wire.
        /// </summary>
        /// <param name="dir">The direction that it outputs power. 0 = left, 1 = up, 2 = right, 3 = down.</param>
        /// <returns>Electrical strength here. 0 if not powered, 1 at full.</returns>
        public double getPower(int dir)
        {
            var left = new int[] { 1, 5, 8, 9, 10, 12, 13, 15 };
            var up = new int[] { 2, 5, 6, 9, 10, 11, 14, 15 };
            var right = new int[] { 3, 6, 7, 10, 11, 12, 13, 15 };
            var down = new int[] { 4, 7, 8, 9, 11, 12, 14, 15 };
            if (dir == 0)
            {
                for (int x = 0; x < 8; x++)
                    if (type == left[x])
                        return getPowerFingerprint(0);
            }
            if (dir == 1)
            {
                for (int x = 0; x < 8; x++)
                    if (type == up[x])
                        return getPowerFingerprint(0);
            }
            if (dir == 2)
            {
                for (int x = 0; x < 8; x++)
                    if (type == right[x])
                        return getPowerFingerprint(0);
            }
            if (dir == 3)
            {
                for (int x = 0; x < 8; x++)
                    if (type == down[x])
                        return getPowerFingerprint(0);
            }
            if (type == 16)
            {
                if (dir == 0 || dir == 2)
                    return getPowerFingerprint(0);
                if (dir == 1 || dir == 3)
                    return getPowerFingerprint(1);
            }
            return -1;
        }

        public double getPower()
        {
            return Math.Max(getPowerAxis(0), getPowerAxis(1));
        }

        public double getPowerAxis(int axis)
        {
            if (axis == 1)
                return Math.Max(getPower(1), getPower(3));
            return Math.Max(getPower(0), getPower(2));
        }

        #endregion

        #region canDir

        /// <summary>
        /// Returns if a tile goes a particular direction
        /// </summary>
        /// <param name="dir">Which direction to check. 0 = left, 1 = up, 2 = right, 3 = down.</param>
        public bool canDir(int dir)
        {
            var left = new int[] { 1, 5, 8, 9, 10, 12, 13, 15, 16 };
            var up = new int[] { 2, 5, 6, 9, 10, 11, 14, 15, 16 };
            var right = new int[] { 3, 6, 7, 10, 11, 12, 13, 15, 16 };
            var down = new int[] { 4, 7, 8, 9, 11, 12, 14, 15, 16 };
            if (dir == 0)
            {
                for (int x = 0; x < 9; x++)
                    if (left[x] == type)
                        return true;
            }
            else if (dir == 1)
            {
                for (int x = 0; x < 9; x++)
                    if (up[x] == type)
                        return true;
            }
            else if (dir == 2)
            {
                for (int x = 0; x < 9; x++)
                    if (right[x] == type)
                        return true;
            }
            else if (dir == 3)
            {
                for (int x = 0; x < 9; x++)
                    if (down[x] == type)
                        return true;
            }
            return false;
        }

        #endregion

        #region Modify tile wire connection directions

        public void changeDir(int dir, bool add = true)
        {
            if (add)
                addDir(dir);
            else
                subDir(dir);
        }
        public void addDir(int dir)
        {
            var leftin = new int[] { 0, 2, 3, 4, 6, 7, 11, 14 };
            var leftout = new int[] { 1, 5, 13, 8, 10, 12, 16, 9 };
            if (dir == 0)
                for (int x = 0; x < 8; x++)
                    if (leftin[x] == type)
                        type = leftout[x];
            var upin = new int[] { 0, 1, 3, 4, 7, 8, 12, 13 };
            var upout = new int[] { 2, 5, 6, 14, 11, 9, 16, 10 };
            if (dir == 1)
                for (int x = 0; x < 8; x++)
                    if (upin[x] == type)
                        type = upout[x];
            var rightin = new int[] { 0, 1, 2, 4, 5, 8, 9, 14 };
            var rightout = new int[] { 3, 13, 6, 7, 10, 12, 16, 11 };
            if (dir == 2)
                for (int x = 0; x < 8; x++)
                    if (rightin[x] == type)
                        type = rightout[x];
            var downin = new int[] { 0, 1, 2, 3, 5, 6, 10, 13 };
            var downout = new int[] { 4, 8, 14, 7, 9, 11, 16, 12 };
            if (dir == 3)
                for (int x = 0; x < 8; x++)
                    if (downin[x] == type)
                        type = downout[x];
        }
        public void subDir(int dir)
        {
            var leftin = new int[] { 1, 5, 13, 8, 10, 12, 15, 9, 16 };
            var leftout = new int[] { 0, 2, 3, 4, 6, 7, 11, 14, 11 };
            if (dir == 0)
                for (int x = 0; x < 9; x++)
                    if (leftin[x] == type)
                        type = leftout[x];
            var upin = new int[] { 2, 5, 6, 14, 11, 9, 15, 10, 16 };
            var upout = new int[] { 0, 1, 3, 4, 7, 8, 12, 13, 12 };
            if (dir == 1)
                for (int x = 0; x < 9; x++)
                    if (upin[x] == type)
                        type = upout[x];
            var rightin = new int[] { 3, 13, 6, 7, 10, 12, 15, 11, 16 };
            var rightout = new int[] { 0, 1, 2, 4, 5, 8, 9, 14, 9 };
            if (dir == 2)
                for (int x = 0; x < 9; x++)
                    if (rightin[x] == type)
                        type = rightout[x];
            var downin = new int[] { 4, 8, 14, 7, 9, 11, 15, 12, 16 };
            var downout = new int[] { 0, 1, 2, 3, 5, 6, 10, 13, 10 };
            if (dir == 3)
                for (int x = 0; x < 9; x++)
                    if (downin[x] == type)
                        type = downout[x];
        }

        #endregion

        #region Reset

        public void reset()
        {
            signal1Fingerprints = new List<string>();
            signal2Fingerprints = new List<string>();
        }

        #endregion

        #region Renders

        public void render(Graphics g, int x, int y, int zoomlevel)
        {
            SolidBrush b = new SolidBrush(Color.FromArgb(63, 63, 63));
            g.FillRectangle(b, x, y, zoomlevel, zoomlevel);
            b.Color = Color.FromArgb(0, 0, 0);
            g.FillRectangle(b, x + 1, y + 1, zoomlevel - 2, zoomlevel - 2);
            double signal = getPower();
            if (type == 16)
                signal = getPowerAxis(0);
            b.Color = Color.FromArgb((int)(baseColor + Math.Min(Math.Max(signal * (255 - baseColor), 0), 255 - baseColor)),
                                     (int)(Math.Min(Math.Max((signal - 1) * 255, 0), 255)),
                                     (int)(Math.Min(Math.Max((signal - 2) * 255, 0), 255)));
            switch (type)
            {
                case 1:
                    g.FillRectangle(b, x, y + (int)(zoomlevel / 2 - zoomlevel / 10), zoomlevel / 2 + zoomlevel / 5, zoomlevel / 5);
                    g.FillRectangle(b, x + (int)(zoomlevel / 2 - zoomlevel / 10), y + (int)(zoomlevel / 2 - zoomlevel / 5), zoomlevel / 5, 2 * zoomlevel / 5);
                    break;
                case 2:
                    g.FillRectangle(b, x + (int)(zoomlevel / 2 - zoomlevel / 10), y, zoomlevel / 5, zoomlevel / 2 + zoomlevel / 5);
                    g.FillRectangle(b, x + (int)(zoomlevel / 2 - zoomlevel / 5), y + (int)(zoomlevel / 2 - zoomlevel / 10), 2 * zoomlevel / 5, zoomlevel / 5);
                    break;
                case 3:
                    g.FillRectangle(b, x + (int)(zoomlevel / 2 - zoomlevel / 5), y + (int)(zoomlevel / 2 - zoomlevel / 10), zoomlevel / 2 + zoomlevel / 5, zoomlevel / 5);
                    g.FillRectangle(b, x + (int)(zoomlevel / 2 - zoomlevel / 10), y + (int)(zoomlevel / 2 - zoomlevel / 5), zoomlevel / 5, 2 * zoomlevel / 5);
                    break;
                case 4:
                    g.FillRectangle(b, x + (int)(zoomlevel / 2 - zoomlevel / 10), y + (int)(zoomlevel / 2 - zoomlevel / 5), zoomlevel / 5, zoomlevel / 2 + zoomlevel / 5);
                    g.FillRectangle(b, x + (int)(zoomlevel / 2 - zoomlevel / 5), y + (int)(zoomlevel / 2 - zoomlevel / 10), 2 * zoomlevel / 5, zoomlevel / 5);
                    break;
                case 5:
                    g.FillRectangle(b, x, y + (int)(zoomlevel / 2 - zoomlevel / 10), zoomlevel / 2 + zoomlevel / 10, zoomlevel / 5);
                    g.FillRectangle(b, x + (int)(zoomlevel / 2 - zoomlevel / 10), y, zoomlevel / 5, zoomlevel / 2 + zoomlevel / 10);
                    break;
                case 6:
                    g.FillRectangle(b, x + (int)(zoomlevel / 2 - zoomlevel / 10), y, zoomlevel / 5, zoomlevel / 2 + zoomlevel / 10);
                    g.FillRectangle(b, x + (int)(zoomlevel / 2 - zoomlevel / 10), y + (int)(zoomlevel / 2 - zoomlevel / 10), zoomlevel / 2 + zoomlevel / 10, zoomlevel / 5);
                    break;
                case 7:
                    g.FillRectangle(b, x + (int)(zoomlevel / 2 - zoomlevel / 10), y + (int)(zoomlevel / 2 - zoomlevel / 10), zoomlevel / 2 + zoomlevel / 10, zoomlevel / 5);
                    g.FillRectangle(b, x + (int)(zoomlevel / 2 - zoomlevel / 10), y + (int)(zoomlevel / 2 - zoomlevel / 10), zoomlevel / 5, zoomlevel / 2 + zoomlevel / 10);
                    break;
                case 8:
                    g.FillRectangle(b, x + (int)(zoomlevel / 2 - zoomlevel / 10), y + (int)(zoomlevel / 2 - zoomlevel / 10), zoomlevel / 5, zoomlevel / 2 + zoomlevel / 10);
                    g.FillRectangle(b, x, y + (int)(zoomlevel / 2 - zoomlevel / 10), zoomlevel / 2 + zoomlevel / 10, zoomlevel / 5);
                    break;
                case 9:
                    g.FillRectangle(b, x + (int)(zoomlevel / 2 - zoomlevel / 10), y, zoomlevel / 5, zoomlevel);
                    g.FillRectangle(b, x, y + (int)(zoomlevel / 2 - zoomlevel / 10), zoomlevel / 2, zoomlevel / 5);
                    break;
                case 10:
                    g.FillRectangle(b, x, y + (int)(zoomlevel / 2 - zoomlevel / 10), zoomlevel, zoomlevel / 5);
                    g.FillRectangle(b, x + (int)(zoomlevel / 2 - zoomlevel / 10), y, zoomlevel / 5, zoomlevel / 2);
                    break;
                case 11:
                    g.FillRectangle(b, x + (int)(zoomlevel / 2 - zoomlevel / 10), y, zoomlevel / 5, zoomlevel);
                    g.FillRectangle(b, x + (int)(zoomlevel / 2), y + (int)(zoomlevel / 2 - zoomlevel / 10), zoomlevel / 2, zoomlevel / 5);
                    break;
                case 12:
                    g.FillRectangle(b, x, y + (int)(zoomlevel / 2 - zoomlevel / 10), zoomlevel, zoomlevel / 5);
                    g.FillRectangle(b, x + (int)(zoomlevel / 2 - zoomlevel / 10), y + (int)(zoomlevel / 2), zoomlevel / 5, zoomlevel / 2);
                    break;
                case 13:
                    g.FillRectangle(b, x, y + (int)(zoomlevel / 2 - zoomlevel / 10), zoomlevel, zoomlevel / 5);
                    break;
                case 14:
                    g.FillRectangle(b, x + (int)(zoomlevel / 2 - zoomlevel / 10), y, zoomlevel / 5, zoomlevel);
                    break;
                case 15:
                    g.FillRectangle(b, x, y + (int)(zoomlevel / 2 - zoomlevel / 10), zoomlevel, zoomlevel / 5);
                    g.FillRectangle(b, x + (int)(zoomlevel / 2 - zoomlevel / 10), y, zoomlevel / 5, zoomlevel);
                    g.FillRectangle(b, x + (int)(zoomlevel / 2 - zoomlevel / 5), y + (int)(zoomlevel / 2 - zoomlevel / 5), 2 * zoomlevel / 5, 2 * zoomlevel / 5);
                    break;
                case 16:
                    g.FillRectangle(b, x, y + (int)(zoomlevel / 2 - zoomlevel / 10), zoomlevel, zoomlevel / 5);
                    g.FillRectangle(b, x + (int)(zoomlevel / 2 - zoomlevel / 10), y, zoomlevel / 5, zoomlevel);
                    g.FillRectangle(b, x + (int)(zoomlevel / 2 - zoomlevel / 5), y + (int)(zoomlevel / 2 - zoomlevel / 5), 2 * zoomlevel / 5, 2 * zoomlevel / 5);
                    b.Color = Color.FromArgb(0, 0, 0);
                    g.FillRectangle(b, x + (int)(zoomlevel / 2 - zoomlevel / 10), y + (int)(zoomlevel / 2 - zoomlevel / 10), zoomlevel / 5, zoomlevel / 5);
                    break;
                default:
                    break;
            }
        }

        public void renderNum(Graphics g, double number, int x, int y, int zoomlevel)
        {
            SolidBrush b = new SolidBrush(Color.FromArgb(255, 255, 255));
            Font f = new Font("Arial", 12);
            b.Color = Color.FromArgb(255, 255, 255);
            g.DrawString(string.Format("{0}", number), f, b, new PointF(x, y));
        }

        #endregion

    }
}
