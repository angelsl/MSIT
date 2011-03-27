// This file is part of MSIT. This file may have been taken from other applications and libraries.
// 
// MSIT is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// MSIT is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MSIT.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Collections.Generic;
using System.Drawing;
using MapleLib.Helpers;
using MapleLib.WzLib.WzStructure.Data;
//using HaCreator.Helpers;

namespace MapleLib.WzLib.WzStructure
{
    public class MapInfo //Credits to Bui
    {
        public static MapInfo Default = new MapInfo();

        public string MapMark = "None";
        public Rectangle? VR;
        public MapleBool VRLimit = null; //use vr's as limits?
        public List<IWzImageProperty> additionalProps = new List<IWzImageProperty>();
        public MapleBool allMoveCheck = null; //another JQ hack protection
        public List<int> allowedItems;
        public AutoLieDetector? autoLieDetector;
        public string bgm = "Bgm00/GoPicnic";
        public MapleBool blockPBossChange = null; //something with monster carnival
        public bool cloud;
        public MapleBool consumeItemCoolTime = null; //cool time of consume item
        public int? createMobInterval; //used for massacre pqs
        public MapleBool damageCheckFree = null; //something with fishing event
        public int? decHP;
        public int? decInterval;
        public int? dropExpire; //in seconds
        public float? dropRate;
        public string effect; //Bubbling; 610030550 and many others
        public MapleBool entrustedShop = null;
        public MapleBool everlast = null; //something with bonus stages of PQs
        public MapleBool expeditionOnly = null;
        public FieldLimit fieldLimit = FieldLimit.FIELDOPT_NONE;
        public FieldType? fieldType;
        public int? fixedMobCapacity; //mob capacity to target (used for massacre pqs)
        public MapleBool fly = null;
        public int forcedReturn = 999999999;
        public float? fs; //slip on ice speed, default 0.2
        public string help; //help string
        public bool hideMinimap;
        public int id;
        public int? lvForceMove; //limit FROM value
        public int? lvLimit;

        //Unknown optional
        public string mapDesc;
        public string mapName;
        public MapType mapType = MapType.RegularMap;
        public MapleBool miniMapOnOff = null;
        public float mobRate = 1.5f;
        public int? moveLimit;
        public MapleBool needSkillForFly = null;
        public MapleBool noMapCmd = null;
        public MapleBool noRegenMap = null; //610030400
        public string onFirstUserEnter;
        public string onUserEnter;
        public MapleBool partyOnly = null;
        public MapleBool personalShop = null;
        public int? protectItem; //ID, item protecting from cold
        public MapleBool rain = null;
        public MapleBool reactorShuffle = null;
        public string reactorShuffleName;
        public float? recovery; //recovery rate, like in sauna (3)
        public int returnMap = 999999999;
        public MapleBool scrollDisable = null;
        public MapleBool snow = null;
        public string strMapName = "<Untitled>";
        public string strStreetName = "<Untitled>";
        public string streetName;
        public bool swim;
        public int? timeLimit;
        public TimeMob? timeMob;
        public bool town;
        public int version = 10;
        public MapleBool zakum2Hack = null; //JQ hack protection

        public MapInfo()
        {
        }

        public MapInfo(WzImage image, string strMapName, string strStreetName)
        {
            int? startHour;
            int? endHour;
            this.strMapName = strMapName;
            this.strStreetName = strStreetName;
            foreach (IWzImageProperty prop in image["info"].WzProperties)
                switch (prop.Name)
                {
                    case "bgm":
                        bgm = InfoTool.GetString(prop);
                        break;
                    case "cloud":
                        cloud = InfoTool.GetBool(prop);
                        break;
                    case "swim":
                        swim = InfoTool.GetBool(prop);
                        break;
                    case "forcedReturn":
                        forcedReturn = InfoTool.GetInt(prop);
                        break;
                    case "hideMinimap":
                        hideMinimap = InfoTool.GetBool(prop);
                        break;
                    case "mapDesc":
                        mapDesc = InfoTool.GetString(prop);
                        break;
                    case "mapMark":
                        MapMark = InfoTool.GetString(prop);
                        break;
                    case "mobRate":
                        mobRate = InfoTool.GetFloat(prop);
                        break;
                    case "moveLimit":
                        moveLimit = InfoTool.GetInt(prop);
                        break;
                    case "returnMap":
                        returnMap = InfoTool.GetInt(prop);
                        break;
                    case "town":
                        town = InfoTool.GetBool(prop);
                        break;
                    case "version":
                        version = InfoTool.GetInt(prop);
                        break;
                    case "fieldLimit":
                        int fl = InfoTool.GetInt(prop);
                        if (fl >= (int) Math.Pow(2, 23))
                            fl = fl & ((int) Math.Pow(2, 23) - 1);
                        fieldLimit = (FieldLimit) fl;
                        break;
                    case "VRTop":
                    case "VRBottom":
                    case "VRLeft":
                    case "VRRight":
                        break;
                    case "link":
                        //link = InfoTool.GetInt(prop);
                        break;
                    case "timeLimit":
                        timeLimit = InfoTool.GetInt(prop);
                        break;
                    case "lvLimit":
                        lvLimit = InfoTool.GetInt(prop);
                        break;
                    case "onFirstUserEnter":
                        onFirstUserEnter = InfoTool.GetString(prop);
                        break;
                    case "onUserEnter":
                        onUserEnter = InfoTool.GetString(prop);
                        break;
                    case "fly":
                        fly = InfoTool.GetBool(prop);
                        break;
                    case "noMapCmd":
                        noMapCmd = InfoTool.GetBool(prop);
                        break;
                    case "partyOnly":
                        partyOnly = InfoTool.GetBool(prop);
                        break;
                    case "fieldType":
                        int ft = InfoTool.GetInt(prop);
                        if (!Enum.IsDefined(typeof (FieldType), ft))
                            ft = 0;
                        fieldType = (FieldType) ft;
                        break;
                    case "miniMapOnOff":
                        miniMapOnOff = InfoTool.GetBool(prop);
                        break;
                    case "reactorShuffle":
                        reactorShuffle = InfoTool.GetBool(prop);
                        break;
                    case "reactorShuffleName":
                        reactorShuffleName = InfoTool.GetString(prop);
                        break;
                    case "personalShop":
                        personalShop = InfoTool.GetBool(prop);
                        break;
                    case "entrustedShop":
                        entrustedShop = InfoTool.GetBool(prop);
                        break;
                    case "effect":
                        effect = InfoTool.GetString(prop);
                        break;
                    case "lvForceMove":
                        lvForceMove = InfoTool.GetInt(prop);
                        break;
                    case "timeMob":
                        startHour = InfoTool.GetOptionalInt(prop["startHour"]);
                        endHour = InfoTool.GetOptionalInt(prop["endHour"]);
                        int? id = InfoTool.GetOptionalInt(prop["id"]);
                        string message = InfoTool.GetOptionalString(prop["message"]);
                        if (id == null || message == null || (startHour == null ^ endHour == null))
                        {
                            //System.Windows.Forms.MessageBox.Show("Warning", "Warning - incorrect timeMob structure in map data. Skipped and error log was saved.");
                            var file = (WzFile) image.WzFileParent;
                            if (file != null)
                                ErrorLogger.Log(ErrorLevel.IncorrectStructure,
                                                "timeMob, map " + image.Name + " of version " + Enum.GetName(typeof (WzMapleVersion), file.MapleVersion) + ", v" + file.Version);
                        }
                        else
                            timeMob = new TimeMob(startHour, endHour, (int) id, message);
                        break;
                    case "help":
                        help = InfoTool.GetString(prop);
                        break;
                    case "snow":
                        snow = InfoTool.GetBool(prop);
                        break;
                    case "rain":
                        rain = InfoTool.GetBool(prop);
                        break;
                    case "dropExpire":
                        dropExpire = InfoTool.GetInt(prop);
                        break;
                    case "decHP":
                        decHP = InfoTool.GetInt(prop);
                        break;
                    case "decInterval":
                        decInterval = InfoTool.GetInt(prop);
                        break;
                    case "autoLieDetector":
                        startHour = InfoTool.GetOptionalInt(prop["startHour"]);
                        endHour = InfoTool.GetOptionalInt(prop["endHour"]);
                        int? interval = InfoTool.GetOptionalInt(prop["interval"]);
                        int? propInt = InfoTool.GetOptionalInt(prop["prop"]);
                        if (startHour == null || endHour == null || interval == null || propInt == null)
                        {
                            //System.Windows.Forms.MessageBox.Show("Warning", "Warning - incorrect autoLieDetector structure in map data. Skipped and error log was saved.");
                            var file = (WzFile) image.WzFileParent;
                            if (file != null)
                                ErrorLogger.Log(ErrorLevel.IncorrectStructure,
                                                "autoLieDetector, map " + image.Name + " of version " + Enum.GetName(typeof (WzMapleVersion), file.MapleVersion) + ", v" + file.Version);
                        }
                        else
                            autoLieDetector = new AutoLieDetector((int) startHour, (int) endHour, (int) interval, (int) propInt);
                        break;
                    case "expeditionOnly":
                        expeditionOnly = InfoTool.GetBool(prop);
                        break;
                    case "fs":
                        fs = InfoTool.GetFloat(prop);
                        break;
                    case "protectItem":
                        protectItem = InfoTool.GetInt(prop);
                        break;
                    case "createMobInterval":
                        createMobInterval = InfoTool.GetInt(prop);
                        break;
                    case "fixedMobCapacity":
                        fixedMobCapacity = InfoTool.GetInt(prop);
                        break;
                    case "streetName":
                        streetName = InfoTool.GetString(prop);
                        break;
                    case "noRegenMap":
                        noRegenMap = InfoTool.GetBool(prop);
                        break;
                    case "allowedItems":
                        allowedItems = new List<int>();
                        if (prop.WzProperties != null && prop.WzProperties.Count > 0)
                            foreach (IWzImageProperty item in prop.WzProperties)
                                allowedItems.Add((int) item);
                        break;
                    case "recovery":
                        recovery = InfoTool.GetFloat(prop);
                        break;
                    case "blockPBossChange":
                        blockPBossChange = InfoTool.GetBool(prop);
                        break;
                    case "everlast":
                        everlast = InfoTool.GetBool(prop);
                        break;
                    case "damageCheckFree":
                        damageCheckFree = InfoTool.GetBool(prop);
                        break;
                    case "dropRate":
                        dropRate = InfoTool.GetFloat(prop);
                        break;
                    case "scrollDisable":
                        scrollDisable = InfoTool.GetBool(prop);
                        break;
                    case "needSkillForFly":
                        needSkillForFly = InfoTool.GetBool(prop);
                        break;
                    case "zakum2Hack":
                        zakum2Hack = InfoTool.GetBool(prop);
                        break;
                    case "allMoveCheck":
                        allMoveCheck = InfoTool.GetBool(prop);
                        break;
                    case "VRLimit":
                        VRLimit = InfoTool.GetBool(prop);
                        break;
                    case "consumeItemCoolTime":
                        consumeItemCoolTime = InfoTool.GetBool(prop);
                        break;
                    default:
                        additionalProps.Add(prop);
                        break;
                }
            if (image["info"]["VRLeft"] != null)
            {
                IWzImageProperty info = image["info"];
                int left = InfoTool.GetInt(info["VRLeft"]);
                int right = InfoTool.GetInt(info["VRRight"]);
                int top = InfoTool.GetInt(info["VRTop"]);
                int bottom = InfoTool.GetInt(info["VRBottom"]);
                VR = new Rectangle(left, top, right - left, bottom - top);
            }
        }

        #region Nested type: AutoLieDetector

        public struct AutoLieDetector
        {
            public int endHour, interval, prop; //interval in mins, prop default = 80
            public int startHour; //interval in mins, prop default = 80

            public AutoLieDetector(int startHour, int endHour, int interval, int prop)
            {
                this.startHour = startHour;
                this.endHour = endHour;
                this.interval = interval;
                this.prop = prop;
            }
        }

        #endregion

        #region Nested type: TimeMob

        public struct TimeMob
        {
            public int? endHour;
            public int id;
            public string message;
            public int? startHour;

            public TimeMob(int? startHour, int? endHour, int id, string message)
            {
                this.startHour = startHour;
                this.endHour = endHour;
                this.id = id;
                this.message = message;
            }
        }

        #endregion
    }
}