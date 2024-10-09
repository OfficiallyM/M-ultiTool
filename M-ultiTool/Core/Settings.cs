using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiTool.Core
{
    internal class Settings
    {
        public static bool s_deleteMode = false;
        public bool deleteMode
        {
            get { return s_deleteMode; }
            set { s_deleteMode = value; }
        }

        public static bool s_godMode = false;
        public bool godMode
        {
            get { return s_godMode; }
            set { s_godMode = value; }
        }

        public static bool s_noclip = false;
        public bool noclip
        {
            get { return s_noclip; }
            set { s_noclip = value; }
        }

        public static bool s_spawnWithFuel = true;
        public bool spawnWithFuel
        {
            get { return s_spawnWithFuel; }
            set { s_spawnWithFuel = value; }
        }

        public static string s_mode = null;
        public string mode
        {
            get { return s_mode; }
            set { s_mode = value; }
        }

        public static carscript s_car = null;
        public carscript car
        {
            get { return s_car; }
            set { s_car = value; }
        }

        public static string s_slotStage = null;
        public string slotStage
        {
            get { return s_slotStage; }
            set { s_slotStage = value; }
        }

        public static bool s_showCoords = false;
        public bool showCoords
        {
            get { return s_showCoords; }
            set { s_showCoords = value; }
        }

        public static bool s_objectDebug = false;
        public bool objectDebug
        {
            get { return s_objectDebug; }
            set { s_objectDebug = value; }
        }

        public static bool s_advancedObjectDebug = false;
        public bool advancedObjectDebug
        {
            get { return s_advancedObjectDebug; }
            set { s_advancedObjectDebug = value; }
        }

        public static bool s_objectDebugShowUnity = true;
        public bool objectDebugShowUnity
        {
            get { return s_objectDebugShowUnity; }
            set { s_objectDebugShowUnity = value; }
        }

        public static bool s_objectDebugShowCore = true;
        public bool objectDebugShowCore
        {
            get { return s_objectDebugShowCore; }
            set { s_objectDebugShowCore = value; }
        }

        public static bool s_objectDebugShowChildren = true;
        public bool objectDebugShowChildren
        {
            get { return s_objectDebugShowChildren; }
            set { s_objectDebugShowChildren = value; }
        }

        public static bool s_showColliders = false;
        public bool showColliders
        {
            get { return s_showColliders; }
            set { s_showColliders = value; }
        }

        public static bool s_showColliderHelp = false;
        public bool showColliderHelp
        {
            get { return s_showColliderHelp; }
            set { s_showColliderHelp = value; }
        }
    }
}
